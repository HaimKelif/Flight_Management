import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Flight, FilterFlight, Airport  } from './flight.model';

@Injectable({
  providedIn: 'root'
})
export class FlightService {
  private apiUrl = 'https://localhost:7241/api/FlightManagement'; // Replace with your API endpoint

  constructor(private http: HttpClient) {}

  getAllFlights(filter: FilterFlight): Observable<any> {
    return this.http.post(`${this.apiUrl}/getflights`, filter);
  }

  createFlight(flight: Flight): Observable<any> {
    return this.http.post(`${this.apiUrl}/saveflight`, flight);
  }

  getAirports(): Observable<Airport[]> {
    return this.http.post<Airport[]>(`${this.apiUrl}/getairports`, {});
  }
}



