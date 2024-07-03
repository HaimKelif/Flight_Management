export interface  Flight {
  flightNumber: string;
  takeoffAirport: string;
  landingAirport: string;
  status: string;
  takeoffTime: Date;
  landingTime: Date;
  delayMinutes: number;
}

export interface  FilterFlight {
  FlightNumber: string;
  TakeoffAirport: string;
  LandingAirport: string;
}


export interface  Airport {
  airportCode: string;
  airportName: string;
}


