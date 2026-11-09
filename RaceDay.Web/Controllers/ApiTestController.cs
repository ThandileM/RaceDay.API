
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers
{
    public class ApiTestController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiTestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            try
            {
                var response = await client.GetAsync("api/Events");
                var content = await response.Content.ReadAsStringAsync();

                var result = $"API connection status: {(int)response.StatusCode} " +
                             $"{response.StatusCode}\n\n" +
                             $"API response:\n{content}";

                return Content(result, "text/plain");
            }
            catch (HttpRequestException ex)
            {
                return Content(
                    $"Could not connect to RaceDay API.\n\nError: {ex.Message}",
                    "text/plain");
            }
        }
    }
}
