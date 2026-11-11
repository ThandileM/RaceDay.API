
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
                model.Distance
            });

            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            // Read the newly created event ID from the API response.
            using var document = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());

            if (!document.RootElement.TryGetProperty(
                "eventId", out var eventIdElement))
            {
                return "The event was created, but its ID could not be read. The banner was not uploaded.";
            }

            var eventId = eventIdElement.GetInt32();

            if (model.BannerImage != null &&
                model.BannerImage.Length > 0)
            {
                var uploadError = await UploadBannerAsync(
                    eventId, model.BannerImage);

                if (uploadError != null)
                {
                    return "The event was created, but the banner upload failed: "
                        + uploadError;
                }
            }

            return null;
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
                model.Distance
            });

            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            // Only replace the banner when a new file was selected.
            if (model.BannerImage != null &&
                model.BannerImage.Length > 0)
            {
                return await UploadBannerAsync(
                    eventId, model.BannerImage);
            }

            return null;
        }

        private async Task<string?> UploadBannerAsync(
            int eventId, IFormFile file)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = CreateSessionRequest(
                HttpMethod.Post,
                $"api/Events/{eventId}/banner");

            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();

            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.FileName);

            request.Content = content;

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }

        // Retrieves a banner through the API for the MVC image proxy.
        public async Task<(byte[]? Content, string? ContentType)>
            GetBannerAsync(int eventId)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var response = await client.GetAsync(
                $"api/Events/{eventId}/banner");

            if (!response.IsSuccessStatusCode)
                return (null, null);

            var content = await response.Content.ReadAsByteArrayAsync();

            var contentType = response.Content.Headers.ContentType?
                .MediaType ?? "application/octet-stream";

            return (content, contentType);
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
