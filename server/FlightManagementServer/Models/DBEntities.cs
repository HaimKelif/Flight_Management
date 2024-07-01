using System;
using FlightManagementServer.Models;

namespace FlightManagementServer.Models
{
  
    public class DBEntities
    {
        [DBAttribute("FLIGHT")]
        public class Flight : BaseEntity
        {
            [DBAttribute("FLIGHTNUMBER")]
            public string? FlightNumber { get; set; }
            [DBAttribute("TAKEOFFAIRPORT")]
            public string TakeoffAirport { get; set; }
            [DBAttribute("LANDINGAIRPORT")]
            public string LandingAirport { get; set; }
            [DBAttribute("STATUS")]
            public string Status { get; set; }
            [DBAttribute("TAKEOFFTIME")]
            public DateTime TakeoffTime { get; set; }
            [DBAttribute("LANDINGTIME")]
            public DateTime LandingTime { get; set; }
            [DBAttribute("DELAYMINUTES ")]
            public int DelayMinutes { get; set; }
        }

        [DBAttribute("AIRPORT")]
        public class Airport : BaseEntity
        {
            [DBAttribute("AIRPORTCODE")]
            public string AirportCode { get; set; }
            [DBAttribute("AIRPORTNAME")]
            public string AirportName { get; set; }
        }

        [DBAttribute("FILTERFLIGHT")]
        public class FilterFlight : BaseEntity
        {
            [DBAttribute("FLIGHTNUMBER")]
            public string? FlightNumber { get; set; }
            [DBAttribute("TAKEOFFAIRPORT")]
            public string? TakeoffAirport { get; set; }
            [DBAttribute("LANDINGAIRPORT")]
            public string? LandingAirport { get; set; }
        }
    } 
}

