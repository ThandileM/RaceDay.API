# RaceDay – Race Event Management System

## Project Overview

RaceDay is a web-based race event management system developed using ASP.NET Core and .NET 8. It consists of a RESTful Web API and an MVC web application.

The system supports race organisers in managing sporting events, categories, participant enrolments and race results. Participants can browse events, enrol in races, view their enrolments and access their results.

The project demonstrates API development, MVC architecture, Entity Framework Core, SQL Server, role-based access control, Azure Blob Storage, Docker containerisation and automated testing.

## Technologies Used

- C# and ASP.NET Core
- .NET 8 Web API and MVC
- Entity Framework Core
- SQL Server
- xUnit integration testing
- Azure Blob Storage
- Docker and Docker Compose
- Bootstrap and Razor Views
- Git and GitHub
- GitHub Actions
- Swagger / OpenAPI

## Main Features

### Authentication and Authorisation
- User registration and login.
- Participant and Organiser roles.
- Session-based authentication.
- Role-specific dashboards and navigation.
- Protected endpoints and pages.
- Logout functionality.

### Organiser Features
- View and manage organised events.
- Create, edit and delete events.
- Manage event categories.
- View participant enrolments.
- Update enrolment statuses.
- Record and delete race results.
- Upload event banner images.

### Participant Features
- Browse available race events.
- View event details.
- Enrol in events.
- View personal enrolments and their statuses.
- View personal race results.

### Azure Blob Storage
Azure Blob Storage is integrated through the Web API.

Organisers can upload event banner images when creating or editing events. Images are stored in an Azure Blob Storage container and displayed through the MVC application.

The MVC application communicates with Azure indirectly through the API rather than connecting to Azure Storage itself.

### Docker
The application uses Docker Compose to run three services:

- SQL Server database.
- RaceDay Web API.
- RaceDay MVC web application.

Docker Compose manages the services and their communication.

### Automated Testing
The API includes xUnit integration tests using an in-memory database.

The tests cover authentication, event creation and role-based access restrictions.

## Project Structure

```text
RaceDay.API/
│
├── RaceDay.API/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Services/
│   └── Program.cs
│
├── RaceDay.Web/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Views/
│   ├── ViewsModels/
│   └── Program.cs
│
├── RaceDay.API/
│   └── RaceDay.Api.Tests/
│
├── docker-compose.yml
├── RaceDay.sln
└── README.md
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio or another compatible C# IDE
- Docker Desktop
- SQL Server (if running without Docker)
- Azure Storage account for banner uploads

### Running with Docker

1. Clone the repository:

```bash
git clone https://github.com/ThandileM/RaceDay.API.git
```

2. Open the project directory:

```bash
cd RaceDay.API
```

3. Create a `.env` file in the repository root.

4. Configure the required environment variables in `.env`, including the SQL Server password and Azure Storage connection string. Never commit this file or expose its secrets.

5. Build and start the containers:

```bash
docker compose up -d --build
```

6. Access the applications:

- MVC application: http://localhost:7051
- API: http://localhost:7158
- Swagger: http://localhost:7158/swagger

### Running Without Docker

Restore dependencies:

```bash
dotnet restore RaceDay.sln
```

Build the solution:

```bash
dotnet build RaceDay.sln
```

Configure the SQL Server connection string and Azure Storage environment variable.

Run the API:

```bash
dotnet run --project RaceDay.API/RaceDay.API.csproj
```

Run the MVC application:

```bash
dotnet run --project RaceDay.Web/RaceDay.Web.csproj
```

## Running Automated Tests

Run the complete solution test suite:

```bash
dotnet test RaceDay.sln
```

The integration tests use an in-memory database to avoid depending on the production database.

## GitHub Actions and Continuous Integration

The project uses GitHub Actions to automate the build and test process.

The workflow is intended to:

1. Check out the repository.
2. Set up the .NET 8 SDK.
3. Restore project dependencies.
4. Build the API and MVC applications.
5. Run the automated tests.

## YouTube Demonstration

https://youtu.be/__cFVilh85c

## GitHub Repository

https://github.com/ThandileM/RaceDay.API

## Conclusion

RaceDay demonstrates a full-stack race event management application using ASP.NET Core Web API and MVC. It combines role-based functionality, database integration, cloud-based image storage, Docker deployment and automated testing to provide a system for managing sporting events and participants.
