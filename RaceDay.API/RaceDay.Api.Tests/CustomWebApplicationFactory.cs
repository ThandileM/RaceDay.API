
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        $"RaceDayTests_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["AZURE_STORAGE_CONNECTION_STRING"] =
                        "UseDevelopmentStorage=true"
                });
        });
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<RaceDayDbContext>();
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();

            services.AddDbContext<RaceDayDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
