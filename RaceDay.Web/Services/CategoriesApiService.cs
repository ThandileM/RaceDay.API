
using System.Net.Http.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services
{
    public class CategoriesApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CategoriesApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        // Forward the logged-in user's API session cookie.
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

        // Retrieve categories belonging to an event.

        public async Task<List<Category>> GetCategoriesByEventAsync(int eventId)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var response = await client.GetAsync(
                $"api/Categories?eventId={eventId}");

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Categories API returned {(int)response.StatusCode}: {responseBody}");
            }

            return System.Text.Json.JsonSerializer.Deserialize<List<Category>>(
                responseBody,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Category>();
        }


        // Create a category for an event.
        public async Task<string?> CreateCategoryAsync(
            int eventId,
            string name,
            string? description)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = CreateSessionRequest(
                HttpMethod.Post, "api/Categories");

            request.Content = JsonContent.Create(new
            {
                EventId = eventId,
                Name = name,
                Description = description
            });

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }

        // Delete an existing category.
        public async Task<string?> DeleteCategoryAsync(int categoryId)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = CreateSessionRequest(
                HttpMethod.Delete, $"api/Categories/{categoryId}");

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }
    }
}
