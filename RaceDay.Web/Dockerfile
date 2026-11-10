FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RaceDay.Web/RaceDay.Web.csproj", "RaceDay.Web/"]
RUN dotnet restore "RaceDay.Web/RaceDay.Web.csproj"

COPY . .
WORKDIR /src/RaceDay.Web

RUN dotnet publish "RaceDay.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]
