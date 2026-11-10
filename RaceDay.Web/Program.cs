using RaceDay.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;


var builder = WebApplication.CreateBuilder(args);

// Configure connection to RaceDay API
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];

if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException("API base URL is not configured. Check appsettings.json.");
}
builder.Services.AddHttpClient("RaceDayApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl!);
})

.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
 {
     UseCookies = false
 });

builder.Services.AddScoped<EventsApiService>();

builder.Services.AddScoped<CategoriesApiService>();

builder.Services.AddScoped<AuthApiService>();

builder.Services.AddScoped<EnrolmentsApiService>();

//Add services to access the current browser request and response.
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
