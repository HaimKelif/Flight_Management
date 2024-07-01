import { Component, OnInit } from '@angular/core';
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
  //flights: Flight[] = [];
  displayedColumns: string[] = ['flightNumber', 'takeoffAirport', 'landingAirport', 'status', 'takeoffTime', 'landingTime', 'DelayTime', 'edit'];
  filter = {} as FilterFlight;
  constructor(private flightService: FlightService, public dialog: MatDialog, private signalRService: SignalRService) { }

  ngOnInit(): void {
    this.getFlights();
    this.signalRService.flightUpdated$.subscribe(flight => {

      if (flight) {

        // Update your flights array with the new flight data
        const index = this.flights.data.findIndex(f => f.flightNumber === flight.flightNumber);

        if (index > -1) {
          console.log("id: " + flight.flightNumber + ", index: " + index + ", delay: " + flight.delayMinutes)
          this.flights.data[index] = flight;
          console.log("id: " + this.flights.data[index].flightNumber + ", index: " + index + ", delay: " + this.flights.data[index].delayMinutes)
        } else {
          if((this.filter.FlightNumber === '' || this.filter.FlightNumber === 'null'
             || this.filter.FlightNumber == flight.flightNumber)
             && (this.filter.LandingAirport === '' || this.filter.LandingAirport === 'null'
             || this.filter.LandingAirport == flight.LandingAirport)
             && (this.filter.TakeoffAirport === '' || this.filter.TakeoffAirport === 'null'
             || this.filter.TakeoffAirport == flight.TakeoffAirport)){
            this.flights.data.push(flight);
          }
        }
        this.flights.data = [...this.flights.data];
      }
    });

  }



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
