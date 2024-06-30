using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using static FlightManagementServer.Models.DBEntities;

namespace FlightManagementServer.Controllers
{
    [Authorize]
    [Route("api/FlightManagement/")]
    [ApiController]
    public class FlightManagementController : ControllerBase
    {
        private readonly ILogger<FlightManagementController> _logger;
        private readonly IConfiguration iconfiguration;
        private readonly GeneralSqlRepository _generalRepository;
        private readonly IHubContext<FlightHub> _hubContext;

        public FlightManagementController(
            ILogger<FlightManagementController> logger,
            IConfiguration config,
            IHubContext<FlightHub> hubContext,
            GeneralSqlRepository generalRepository)
        {
            _logger = logger;
            iconfiguration = config;
            _hubContext = hubContext;
            _generalRepository = generalRepository;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("getflights")]
        public IList<Flight> GetFlights(FilterFlight filter)
        {
            _logger.LogInformation("getflights endpoint hit");
            return _generalRepository.GetFlights(filter);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("getairports")]
        public IList<Airport> GetAirports()
        {
            _logger.LogInformation("getairports endpoint hit");
            return _generalRepository.GetAirports();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("saveflight")]
        public bool SaveFlight(Flight flight)
        {
            _logger.LogInformation("saveflight endpoint hit");
            bool result = _generalRepository.SaveFlight(flight);

            if (result)
            {
                // Notify clients of the updated flight
                _hubContext.Clients.All.SendAsync("flightUpdated", flight);
            }

            return result;
        }
    }
}