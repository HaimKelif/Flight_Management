import { Component, OnInit, Renderer2  } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { FlightService } from '../flight.service';
import { Flight, FilterFlight } from '../flight.model';
import { EditFlightDialogComponent } from '../edit-flight-dialog/edit-flight-dialog.component';
import { SignalRService  } from '../web-socket.service';
import { MatTableDataSource } from '@angular/material/table';


@Component({
  selector: 'app-flights-list',
  templateUrl: './flights-list.component.html',
  styleUrls: ['./flights-list.component.scss']
})
export class FlightsListComponent implements OnInit {
  flights: MatTableDataSource<Flight> = new MatTableDataSource<Flight>();
  flickedFlight: string = '';
  displayedColumns: string[] = ['flightNumber', 'takeoffAirport', 'landingAirport', 'status',
     'takeoffTime', 'landingTime', 'DelayTime', 'edit'];
  filter = {} as FilterFlight;
  constructor(private flightService: FlightService, public dialog: MatDialog,
     private signalRService: SignalRService, private renderer: Renderer2) { }

  ngOnInit(): void {
    this.getFlights();
    this.signalRService.flightUpdated$.subscribe(flight => {
      if (flight) {
        const index = this.flights.data.findIndex(f => f.flightNumber === flight.flightNumber);
        if (index > -1) {
          this.flights.data[index] = flight;
          this.flickedFlight = this.flights.data[index].flightNumber
          //this.triggerFlickerEffect(flight.flightNumber);
        } else {
          if((this.filter.FlightNumber === '' || this.filter.FlightNumber === 'null'
             || this.filter.FlightNumber == flight.flightNumber)
             && (this.filter.LandingAirport === '' || this.filter.LandingAirport === 'null'
             || this.filter.LandingAirport == flight.LandingAirport)
             && (this.filter.TakeoffAirport === '' || this.filter.TakeoffAirport === 'null'
             || this.filter.TakeoffAirport == flight.TakeoffAirport)){
            this.flights.data.push(flight);
            this.flickedFlight = flight.flightNumber;
          }
        }
        this.flights.data = [...this.flights.data];
      }
    });

  }


  // private triggerFlickerEffect(flightNumber: string): void {
  //   console.log("id: " + flightNumber)
  //   const flightElement = document.getElementById(`flight-${flightNumber}`);
  //   if (flightElement) {
  //     console.log("flightElement: " + flightNumber)
  //     this.renderer.addClass(flightElement, 'flicker');
  //     // setTimeout(() => {
  //     //   this.renderer.removeClass(flightElement, 'flicker');
  //     // }, 10000); // 10 seconds
  //   }
  // }



  getFlights(): void {
    this.flightService.getAllFlights(this.filter).subscribe(
      flights => {
        this.flights = new MatTableDataSource(flights);
      },
      error => {
        console.error('Error loading flights:', error);
      }
    );
  }

  editFlight(flight: Flight): void {
    const dialogRef = this.dialog.open(EditFlightDialogComponent, {
      width: '250px',
      data: flight
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.flightService.createFlight(result).subscribe(() => {
          this.getFlights();
        });
      }
    });
  }


  openNewFlightDialog(): void {
    const dialogRef = this.dialog.open(EditFlightDialogComponent, {
      width: '300px',
      data: {}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.flightService.createFlight(result).subscribe(() => {
          this.getFlights();
        });
      }
    });
  }

  applyFilter() {
    this.getFlights(); // Call getFlights to apply the filter
  }
}
