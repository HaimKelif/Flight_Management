-- Creating Airports Table
CREATE TABLE Airports (
    AirportCode VARCHAR(10) PRIMARY KEY,
    AirportName VARCHAR(100)
);

-- Creating Flights Table
CREATE TABLE Flights (
    FlightNumber VARCHAR(10) PRIMARY KEY,
    LandingAirport VARCHAR(10) NOT NULL,
    TakeoffAirport VARCHAR(10) NOT NULL,
    Status VARCHAR(11) CHECK (Status IN ('hangar', 'airborne', 'malfunction')),
    TakeoffTime DATETIME,
    LandingTime DATETIME,
    CONSTRAINT FK_LandingAirport FOREIGN KEY (LandingAirport) REFERENCES Airports(AirportCode),
    CONSTRAINT FK_TakeoffAirport FOREIGN KEY (TakeoffAirport) REFERENCES Airports(AirportCode)
);