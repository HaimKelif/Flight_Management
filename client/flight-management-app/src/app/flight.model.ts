export class Flight {
  flightNumber: string = '';
  takeoffAirport: string = '';
  landingAirport: string = '';
  status: string = '';
  takeoffTime: Date = new Date();
  landingTime: Date = new Date();
  delayMinutes: number = 0;
}

export class FilterFlight {
  FlightNumber: string = '';
  TakeoffAirport: string = '';
  LandingAirport: string = '';
}


export class Airport {
  airportCode: string = '';
  airportName: string = '';
}


