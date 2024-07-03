import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { Flight, FilterFlight, Airport } from './flight.model';
import { SignalRService } from './web-socket.service';

@Injectable({
  providedIn: 'root'
})
export class FlightService {
  private apiUrl = 'https://localhost:7241/api/FlightManagement';

  private flightsSubject = new BehaviorSubject<Flight[]>([]);
  flights$ = this.flightsSubject.asObservable();
  flickedFlightSubject = new BehaviorSubject<string>('');
  flickedFlight$ = this.flickedFlightSubject.asObservable();
  filter: FilterFlight = {} as FilterFlight;

  constructor(private http: HttpClient, private signalRService: SignalRService) {}

  getAllFlights(): void {
    this.http.post<Flight[]>(`${this.apiUrl}/getflights`, this.filter).subscribe(
      flights => {
        this.flightsSubject.next(flights);
      },
      error => {
        console.error('Error loading flights:', error);
      }
    );
  }

  createFlight(flight: Flight): Observable<any> {
    return this.http.post(`${this.apiUrl}/saveflight`, flight);
  }

  getAirports(): Observable<Airport[]> {
    return this.http.post<Airport[]>(`${this.apiUrl}/getairports`, {});
  }

  checkFilter(flight: Flight): boolean {
    if((this.filter.FlightNumber === '' || this.filter.FlightNumber === 'null' || this.filter.FlightNumber === flight.flightNumber)
      && (this.filter.LandingAirport === '' || this.filter.LandingAirport === 'null' || this.filter.LandingAirport === flight.landingAirport)
      && (this.filter.TakeoffAirport === '' || this.filter.TakeoffAirport === 'null' || this.filter.TakeoffAirport === flight.takeoffAirport)){
        return true;
      }
      return false;
  }

  initSignalR(): void {
    this.signalRService.flightUpdated$.subscribe(flight => {
      if (flight) {
        const flights = this.flightsSubject.getValue();
        const index = flights.findIndex(f => f.flightNumber === flight.flightNumber);
        if (index > -1) {
          flights[index] = flight;
          this.flickedFlightSubject.next(flight.flightNumber);
        } else {
          if (this.checkFilter(flight)) {
            flights.push(flight);
            this.flickedFlightSubject.next(flight.flightNumber);
          }
        }
        this.flightsSubject.next([...flights]);
      }
    });
  }

  applyFilter(newFilter: FilterFlight): void {
    this.filter = newFilter;
    this.getAllFlights();
  }
}
