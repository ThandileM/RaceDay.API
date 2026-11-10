
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore the API project
COPY ["RaceDay.API/RaceDay.API.csproj", "RaceDay.API/"]
RUN dotnet restore "RaceDay.API/RaceDay.API.csproj"

# Copy the source code
COPY . .

# Publish the API
WORKDIR /src/RaceDay.API
RUN dotnet publish "RaceDay.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "RaceDay.API.dll"]
