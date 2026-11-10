
using System.Net.Http.Json;
using RaceDay.Web.Models;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Services
{
    public class EventsApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EventsApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpRequestMessage CreateSessionRequest(
            HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            var cookie = _httpContextAccessor.HttpContext?
                .Request.Headers.Cookie.ToString();

            if (!string.IsNullOrWhiteSpace(cookie))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie", cookie);
            }

            return request;
        }

        public async Task<List<Event>> GetEventsAsync()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            return await client.GetFromJsonAsync<List<Event>>(
                "api/Events") ?? new List<Event>();
        }

        public async Task<string?> CreateEventAsync(
            int organiserId,
            CreateEventViewModel model)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = CreateSessionRequest(
                HttpMethod.Post, "api/Events");

            request.Content = JsonContent.Create(new
            {
                OrganiserId = organiserId,
                model.Name,
                model.Description,
                model.EventDate,
                model.Location,
                model.Distance,
                model.BannerImageUrl
            });

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string?> UpdateEventAsync(
            int eventId,
            int organiserId,
            CreateEventViewModel model)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = CreateSessionRequest(
                HttpMethod.Put, $"api/Events/{eventId}");

            request.Content = JsonContent.Create(new
            {
                OrganiserId = organiserId,
                model.Name,
                model.Description,
                model.EventDate,
                model.Location,
                model.Distance,
                model.BannerImageUrl
            });

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string?> DeleteEventAsync(int eventId)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = CreateSessionRequest(
                HttpMethod.Delete, $"api/Events/{eventId}");

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }

    }
}
