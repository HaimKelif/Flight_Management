# Flight Management

## Overview

The Flight Management project is designed to monitor and manage real-time data from flights, including takeoff and landing times, airports, and flight statuses. The system is built with a modern tech stack to ensure performance, scalability, and ease of use.

## Features

- **Real-Time Data Monitoring**: Track real-time flight data such as takeoff and landing times, airports, and status updates.
- **User-Friendly Interface**: Access and manage flight information through a responsive web application.
- **Notifications**: Receive alerts for important events such as delays or cancellations.
- **Data Visualization**: Visualize flight routes and statuses using interactive charts and maps.

## Tech Stack

- **Frontend**: Angular, TypeScript, HTML, SCSS, Signals, Angular Services, Angular Material
- **Backend**: .NET with C#, MVC architecture, SignalR for real-time updates
- **Database**: SQL SERVER
- **Deployment**: CI/CD pipelines, IIS
- **Other Tools**: GStreamer, ffmpeg

## Prerequisites

- **Node.js** and **npm**: For managing frontend dependencies and running the Angular application.
- **.NET SDK**: To build and run the backend application.
- **SQL SERVER**: Database server to store flight data.
- **Visual Studio**: Recommended IDE for development.

## Installation

### Backend Setup

1. **Clone the repository**:
   ```sh
   git clone https://github.com/HaimKelif/Flight_Management.git
   cd Flight_Management
   ```

2. **Restore .NET dependencies**:
   ```sh
   dotnet restore
   ```

3. **Update database connection string**:
   - Open `appsettings.json` and update the connection string for your SQL server.

4. **Apply migrations and update the database**:
   ```sh
   dotnet ef database update
   ```

5. **Run the backend application**:
   ```sh
   dotnet run
   ```

### Frontend Setup

1. **Navigate to the frontend directory**:
   ```sh
   cd Flight_Management/client
   ```

2. **Install dependencies**:
   ```sh
   npm install
   ```

3. **Run the Angular application**:
   ```sh
   ng serve
   ```

4. **Access the application**:
   - Open your browser and navigate to `http://localhost:4200`.

## Usage

1. **Add Flights**:
   - Use the interface to add new flights, specifying details such as flight number, takeoff and landing airports, and scheduled times.

2. **Monitor Flights**:
   - View real-time updates on flight statuses, including delays and cancellations.

3. **Visualize Data**:
   - Use the dashboard to see visual representations of flight routes and statuses.

## Contributing

1. **Fork the repository**.
2. **Create a new branch**:
   ```sh
   git checkout -b feature-branch
   ```
3. **Make your changes**.
4. **Commit your changes**:
   ```sh
   git commit -m "Description of changes"
   ```
5. **Push to the branch**:
   ```sh
   git push origin feature-branch
   ```
6. **Create a pull request**.



## Contact

For any inquiries or feedback, please contact:

- Name: Haim
- Email: haimklif77@gmail.com

---

This `README.md` provides a comprehensive guide to understanding, installing, and contributing to the Flight Management project. Make sure to replace placeholder URLs and email addresses with actual values specific to your project.
