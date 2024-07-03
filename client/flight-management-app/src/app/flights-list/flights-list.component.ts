import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { FlightService } from '../flight.service';
import { Flight, FilterFlight } from '../flight.model';
import { EditFlightDialogComponent } from '../edit-flight-dialog/edit-flight-dialog.component';
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
  filter: FilterFlight = {} as FilterFlight;

  constructor(
    private flightService: FlightService,
    public dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.flightService.flights$.subscribe(flights => {
      this.flights = new MatTableDataSource(flights);
    });

    this.flightService.flickedFlight$.subscribe(flightNumber => {
      this.flickedFlight = flightNumber;
    });

    this.flightService.getAllFlights();
    this.flightService.initSignalR();
  }

  editFlight(flight: Flight): void {
    const dialogRef = this.dialog.open(EditFlightDialogComponent, {
      width: '250px',
      data: flight
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.flightService.createFlight(result).subscribe(() => {
          this.flightService.getAllFlights();
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
          this.flightService.getAllFlights();
        });
      }
    });
  }

  applyFilter() {
    this.flightService.applyFilter(this.filter);
  }
}
