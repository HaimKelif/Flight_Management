CREATE PROCEDURE GetFlightsByFilters
    @FlightNumber VARCHAR(10) = NULL,
    @TakeoffAirport VARCHAR(10) = NULL,
    @LandingAirport VARCHAR(10) = NULL
AS
BEGIN
    SELECT *
    FROM Flights
    WHERE (@FlightNumber IS NULL OR @FlightNumber = '' OR  FlightNumber = @FlightNumber)
      AND (@TakeoffAirport IS NULL OR @TakeoffAirport = '' OR TakeoffAirport = @TakeoffAirport)
      AND (@LandingAirport IS NULL OR @LandingAirport = '' OR LandingAirport = @LandingAirport)
	  order by TakeoffTime;
END