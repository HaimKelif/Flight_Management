import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { Flight, } from './flight.model';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection;
  private flightUpdatedSubject = new BehaviorSubject<Flight | null>(null);
  public flightUpdated$ = this.flightUpdatedSubject.asObservable();

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7241/flightHub')
      .build();

    this.hubConnection.on('flightUpdated', (flight) => {
      this.flightUpdatedSubject.next(flight);
    });

    this.hubConnection.start().catch(err => console.error(err.toString()));
  }
}




