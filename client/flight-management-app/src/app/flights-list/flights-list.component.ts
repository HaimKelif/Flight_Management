import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { FlightService } from '../flight.service';
import { Flight, FilterFlight } from '../flight.model';
import { EditFlightDialogComponent } from '../edit-flight-dialog/edit-flight-dialog.component';
import { WebSocketService } from '../web-socket.service';


@Component({
  selector: 'app-flights-list',
  templateUrl: './flights-list.component.html',
  styleUrls: ['./flights-list.component.scss']
})
export class FlightsListComponent implements OnInit {
  flights: Flight[] = [];
  displayedColumns: string[] = ['flightNumber', 'takeoffAirport', 'landingAirport', 'status', 'takeoffTime', 'landingTime', 'edit'];
  filter = {} as FilterFlight;
  constructor(private flightService: FlightService, public dialog: MatDialog, private socketService: WebSocketService) { }

  ngOnInit(): void {
    this.getFlights();
    this.socketService.onEvent('flight-update').subscribe((updatedFlight: Flight) => {
      // Update flights list with updatedFlight
      const index = this.flights.findIndex(flight => flight.flightNumber === updatedFlight.flightNumber);
      if (index !== -1) {
        this.flights[index] = updatedFlight;
      } else {
        this.flights.push(updatedFlight);
      }
    });

  }



  getFlights(): void {
    this.flightService.getAllFlights(this.filter).subscribe(
      flights => {
        this.flights = flights;
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
