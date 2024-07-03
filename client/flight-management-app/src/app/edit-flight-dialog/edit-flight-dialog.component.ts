import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Flight, Airport } from '../flight.model';
import { FlightService } from '../flight.service';
import { Observable } from 'rxjs';
import { FormControl } from '@angular/forms';
import { startWith, map } from 'rxjs/operators';

@Component({
  selector: 'app-edit-flight-dialog',
  templateUrl: './edit-flight-dialog.component.html',
  styleUrls: ['./edit-flight-dialog.component.scss']
})
export class EditFlightDialogComponent implements OnInit {
  airports: Airport[] = [];
  filteredAirports!: Observable<Airport[]>;
  tackoffFilterControl = new FormControl('');
  landingFilterControl = new FormControl('');

  constructor(
    public dialogRef: MatDialogRef<EditFlightDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Flight,
    private flightService: FlightService) {}

  ngOnInit(): void {

    this.flightService.getAirports().subscribe(airports => {
      this.airports = airports;
      this.filteredAirports = this.tackoffFilterControl.valueChanges
      .pipe(
        startWith(''),
        map(value => this._filterAirports(value || ''))
      );
      this.filteredAirports = this.landingFilterControl.valueChanges
      .pipe(
        startWith(''),
        map(value => this._filterAirports(value || ''))
      );
    });

  }

  private _filterAirports(value: string): Airport[] {
    const filterValue = value.toLowerCase();
    return this.airports.filter(airport =>
      airport.airportCode.toLowerCase().includes(filterValue)
    );
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
