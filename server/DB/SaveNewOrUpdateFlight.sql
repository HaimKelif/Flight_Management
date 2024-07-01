ALTER PROCEDURE SaveNewOrUpdateFlight
    @OUT VARCHAR(10) OUTPUT,
    @FlightNumber VARCHAR(10) = null,
    @TakeoffAirport VARCHAR(10),
    @LandingAirport VARCHAR(10),
    @Status VARCHAR(11),
    @TakeoffTime DATETIME,
    @LandingTime DATETIME,
	@DELAYMINUTES int
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @GeneratedFlightNumber VARCHAR(10);

    -- If FlightNumber is NULL, generate a new one
    IF @FlightNumber IS NULL
    BEGIN
        DECLARE @Counter INT = 1;
        WHILE 1 = 1
        BEGIN
            -- Generate a flight number in the format AB1234
            SET @GeneratedFlightNumber = CHAR(65 + (@Counter / 10000) % 26)
                            + CHAR(65 + (@Counter / 1000) % 26)
							+ RIGHT('0000' + CAST(@Counter AS VARCHAR(4)), 4);

            -- Check if the generated flight number already exists
            IF NOT EXISTS (SELECT 1 FROM Flights WHERE FlightNumber = @GeneratedFlightNumber)
            BEGIN
                SET @FlightNumber = @GeneratedFlightNumber;
                BREAK;
            END

            -- Increment the counter
            SET @Counter = @Counter + 1;
        END
    END
	

    -- Check if the flight number already exists
    IF EXISTS (SELECT 1 FROM Flights WHERE FlightNumber = @FlightNumber)
    BEGIN
        -- If the flight number exists, update the existing flight
        UPDATE Flights
        SET TakeoffAirport = @TakeoffAirport,
            LandingAirport = @LandingAirport,
            Status = @Status,
            TakeoffTime = @TakeoffTime,
            LandingTime = @LandingTime,
			DELAYMINUTES = @DELAYMINUTES
        WHERE FlightNumber = @FlightNumber;

        -- Return success message for update
        
    END
    ELSE
    BEGIN
        -- Insert the new flight record
        INSERT INTO Flights (FlightNumber, TakeoffAirport, LandingAirport, Status, TakeoffTime, LandingTime, DELAYMINUTES)
        VALUES (@FlightNumber, @TakeoffAirport, @LandingAirport, @Status, @TakeoffTime, @LandingTime, @DELAYMINUTES);

        -- Return success message for insert
        
    END
	set @OUT = @FlightNumber
	--SELECT 'Flight updated successfully.' AS Message, @GeneratedFlightNumber AS FlightNumber;


END