using System.Data.SqlClient;
using FlightManagementServer.Models;
using Microsoft.AspNetCore.SignalR;
using static FlightManagementServer.Models.DBEntities;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace FlightManagementServer.Models
{
    public interface IGeneralRepository
    {
        IList<Flight> GetFlights(FilterFlight filter);
        IList<Airport> GetAirports();
        string SaveFlight(Flight flight);
    }

}

public class GeneralSqlRepository : BaseRepository, IGeneralRepository
{
    private readonly ILogger<GeneralSqlRepository> _logger;
    private readonly IHubContext<FlightHub> _hubContext;
    

    public GeneralSqlRepository(SqlConnection connection, IHubContext<FlightHub> hubContext) : base(connection)
    {
        var factory = LoggerFactory.Create(b => b.AddConsole());
        _hubContext = hubContext;
        _logger = factory.CreateLogger<GeneralSqlRepository>();
    }

    public string SaveFlight(Flight flight)
    {
        try
        {
            string result;
            result = SaveData(flight, "SaveNewOrUpdateFlight");
            if (result != null)
            {
                flight.FlightNumber = result;
                // Notify clients of the updated flight
                _hubContext.Clients.All.SendAsync("flightUpdated", flight);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(" generalSQLRepository SaveFlight: " + ex.Message.ToString() + " Ex: " + ex.ToString());
            base.CloseConnection();
            return null;
        }
        finally
        {
            base.CloseConnection();
        }
    }

    public IList<Flight> GetFlights(FilterFlight filter)
    {
        try
        {
            IList<Flight> Flights;
            List<Param> paramList = new List<Param> { 
                new Param("FlightNumber", filter.FlightNumber),
                new Param("TakeoffAirport", filter.TakeoffAirport),
                new Param("LandingAirport", filter.LandingAirport)};
            using (SqlDataReader rdr = GetData("GetFlightsByFilters", paramList))
            {
                Flights = new List<Flight>();
                while (rdr.Read())
                {
                    var Flight = new Flight();
                    Flight.FillAll(rdr);
                    Flights.Add(Flight);
                }
                return Flights;
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(" generalSQLRepository GetAirports: " + ex.Message.ToString() + " Ex: " + ex.ToString());
            base.CloseConnection();
            return null;
        }
        finally
        {
            base.CloseConnection();
        }
    }

    public IList<Airport> GetAirports()
    {
        IList<Airport> Airports = new List<Airport>();
        try
        {
            List<Param> paramList = new List<Param> {};
            using (SqlDataReader rdr = GetData("GetAllAirports", paramList))
            {
                while (rdr.Read())
                {
                    var Airport = new Airport();
                    Airport.FillAll(rdr);
                    Airports.Add(Airport);
                }
                return Airports;
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(" generalSQLRepository GetAirports: " + ex.Message.ToString() + " Ex: " + ex.ToString());
            base.CloseConnection();
            return Airports;
        }
        finally
        {
            base.CloseConnection();
        }
    } 
}

