using System.Data.SqlClient;
using FlightManagementServer.Models;
using static FlightManagementServer.Models.DBEntities;

namespace FlightManagementServer.Models
{
    public interface IGeneralRepository
    {
        IList<Flight> GetFlights(FilterFlight filter);
        IList<Airport> GetAirports();
        bool SaveFlight(Flight flight);
    }

}

public class GeneralSqlRepository : BaseRepository, IGeneralRepository
{
    private readonly ILogger<GeneralSqlRepository> _logger;

    public GeneralSqlRepository(SqlConnection connection) : base(connection)
    {
        var factory = LoggerFactory.Create(b => b.AddConsole());
            _logger = factory.CreateLogger<GeneralSqlRepository>();
    }

    public bool SaveFlight(Flight flight)
    {
        try
        {
            int result = 0;
            result = SaveData(flight, "SaveNewOrUpdateFlight");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(" generalSQLRepository SaveFlight: " + ex.Message.ToString() + " Ex: " + ex.ToString());
            base.CloseConnection();
            return false;
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
        try
        {
            IList<Airport> Airports;
            List<Param> paramList = new List<Param> {};
            using (SqlDataReader rdr = GetData("GetAllAirports", paramList))
            {
                Airports = new List<Airport>();
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
            return null;
        }
        finally
        {
            base.CloseConnection();
        }
    } 
}

