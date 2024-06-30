CREATE PROCEDURE SaveNewOrUpdateFlight
@NEW_ID INT OUTPUT,
    @FlightNumber VARCHAR(10) = NULL,
    @TakeoffAirport VARCHAR(10),
    @LandingAirport VARCHAR(10),
    @Status VARCHAR(11),
    @TakeoffTime DATETIME,
    @LandingTime DATETIME
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
            LandingTime = @LandingTime
        WHERE FlightNumber = @FlightNumber;

        -- Return success message for update
        SELECT 'Flight updated successfully.' AS Message, @FlightNumber AS FlightNumber;
    END
    ELSE
    BEGIN
        -- Insert the new flight record
        INSERT INTO Flights (FlightNumber, TakeoffAirport, LandingAirport, Status, TakeoffTime, LandingTime)
        VALUES (@FlightNumber, @TakeoffAirport, @LandingAirport, @Status, @TakeoffTime, @LandingTime);

        -- Return success message for insert
        SELECT 'Flight added successfully.' AS Message, @FlightNumber AS FlightNumber;
    END

	SET @NEW_ID = SCOPE_IDENTITY();
END