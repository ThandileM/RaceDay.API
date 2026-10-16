# RaceDay.API
# RaceDay API

## Project Overview

RaceDay API is a web API developed using ASP.NET Core and .NET 8 to support race event management. The application provides functionality for user registration and login, event management, and participant enrolment.

The project demonstrates RESTful API development, data management with Entity Framework Core, role-based access control, automated testing, and continuous integration using GitHub Actions.

## Objectives

- Implement user registration and login.
- Support race event management by authorised organisers.
- Restrict event management operations according to user roles.
- Support participant enrolment and data persistence.
- Test API functionality using automated tests.
- Automate building and testing through GitHub Actions.

## Technologies Used

- C#
- ASP.NET Core Web API
- .NET 8
- Entity Framework Core
- SQL Server for application data
- xUnit for automated testing
- Git and GitHub
- GitHub Actions for continuous integration
- Swagger/OpenAPI for API documentation and testing

## Main Features

### User Registration and Login
Users can register and log in to the application. Authentication-related functionality is tested to verify successful and unsuccessful requests.

### Event Management
Organisers can create race events. Access control prevents unauthenticated users and participants from creating events. Organisers are also restricted from modifying or deleting events belonging to other organisers.

### Participant Enrolment
The API supports participant enrolment functionality and is designed to persist application data using the configured database.

### Automated Testing
The project includes automated tests using xUnit and ASP.NET Core integration testing tools. The tests verify important API behaviour, including registration, login, and event access permissions.

### Continuous Integration
GitHub Actions automatically restores dependencies, builds the application, and runs the automated tests when changes are pushed to the configured branches or submitted through pull requests.

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio or another compatible C# IDE
- SQL Server and the required database configuration
- Git

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/ThandileM/RaceDay.API.git
   ```

2. Open the solution in Visual Studio.

3. Configure the database connection string using your local configuration or user secrets. Do not commit passwords or other secrets to GitHub.

4. Restore the dependencies:

   ```bash
   dotnet restore
   ```

5. Apply any required Entity Framework Core migrations, if configured:

   ```bash
   dotnet ef database update
   ```

6. Run the API project:

   ```bash
   dotnet run --project RaceDay.API/RaceDay.API.csproj
   ```

7. Open the Swagger URL displayed in the terminal, if Swagger is enabled for the current environment.

> Note: The commands above assume the API project is located at `RaceDay.API/RaceDay.API.csproj` in the repository.

## Running the Automated Tests

Run the test project with:

```bash
dotnet test ./RaceDay.API/RaceDay.Api.Tests/RaceDay.Api.Tests.csproj
```

The tests check important application behaviour, including authentication and event access control.

The test project uses an in-memory database for integration tests where configured, allowing tests to run without depending on the production database.

## GitHub Actions

The repository includes a GitHub Actions workflow that:

1. Checks out the source code.
2. Sets up the .NET 8 SDK.
3. Restores project dependencies.
4. Builds the project and runs automated tests.

The workflow has completed successfully, demonstrating that the automated build-and-test process works in GitHub's environment.

## YouTube Link

## Repository

GitHub: https://github.com/ThandileM/RaceDay.API

## Conclusion

RaceDay API demonstrates the development of a .NET Web API with authentication, event management, access control, database integration, automated testing, and continuous integration. The project applies software development and testing practices to build a maintainable application.

## Author

Developed as part of an academic software development assignment.
