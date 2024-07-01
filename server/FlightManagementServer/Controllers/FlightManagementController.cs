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

        public FlightManagementController(
            ILogger<FlightManagementController> logger,
            IConfiguration config,
            GeneralSqlRepository generalRepository)
        {
            _logger = logger;
            iconfiguration = config;  
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
        public string SaveFlight(Flight flight)
        {
            _logger.LogInformation("saveflight endpoint hit");
            string result = _generalRepository.SaveFlight(flight);

            return result;
        }
    }
}