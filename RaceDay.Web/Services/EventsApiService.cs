
using System.Net.Http.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services
{
    public class EventsApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EventsApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<Event>> GetEventsAsync()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var events = await client.GetFromJsonAsync<List<Event>>(
                "api/Events");

            return events ?? new List<Event>();
        }
    }
}

