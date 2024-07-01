using static FlightManagementServer.Models.DBEntities;

public class FlightUpdateService : BackgroundService
{
    private readonly ILogger<FlightUpdateService> _logger;
    private readonly Random _random = new Random();
    private readonly IServiceScopeFactory _scopeFactory;

    public FlightUpdateService(ILogger<FlightUpdateService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(300, stoppingToken);

            using (var scope = _scopeFactory.CreateScope())
            {
                var generalRepository = scope.ServiceProvider.GetRequiredService<GeneralSqlRepository>();
                FilterFlight filter = new FilterFlight();
                var flights = generalRepository.GetFlights(filter);

                if (flights.Count > 0)
                {
                    var flight = flights[_random.Next(flights.Count)];
                    var updateType = _random.Next(3);

                    switch (updateType)
                    {
                        case 0:
                            UpdateStatus(flight, generalRepository);
                            break;
                        case 1:
                            UpdateTimeDelay(flight, generalRepository);
                            break;
                        case 2:
                            UpdateDestination(flight, generalRepository);
                            break;
                    }

                    _logger.LogInformation("Flight {FlightNumber} updated.", flight.FlightNumber);
                }
                else
                {
                    _logger.LogWarning("No flights found in the database.");
                }
            }
        }
    }

    private void UpdateStatus(Flight flight, GeneralSqlRepository generalRepository)
    {
        var statuses = new[] { "hangar", "airborne", "malfunction" };
        flight.Status = statuses[_random.Next(statuses.Length)];
        generalRepository.SaveFlight(flight);
    }

    private void UpdateTimeDelay(Flight flight, GeneralSqlRepository generalRepository)
    {
        var delay = _random.Next(0 - flight.DelayMinutes, 120 - flight.DelayMinutes);
        flight.TakeoffTime = flight.TakeoffTime.AddMinutes(delay);
        flight.LandingTime = flight.LandingTime.AddMinutes(delay);
        flight.DelayMinutes += delay;
        generalRepository.SaveFlight(flight);
    }

    private void UpdateDestination(Flight flight, GeneralSqlRepository generalRepository)
    {
        var airports = generalRepository.GetAirports();
        if (airports == null || airports.Count == 0)
        {
            _logger.LogWarning("No airports available for destination update.");
            return;
        }
        flight.LandingAirport = airports[_random.Next(airports.Count)].AirportCode;
        generalRepository.SaveFlight(flight);
    }
}