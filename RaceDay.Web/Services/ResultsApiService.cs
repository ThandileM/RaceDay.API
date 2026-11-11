
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services
{
    public class ResultsApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResultsApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpRequestMessage CreateSessionRequest(
            HttpMethod method,
            string url)
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

        // Retrieve results permitted by the logged-in user's role.
        // Organisers can optionally filter by event.
        // Participants receive only their own results.
        public async Task<List<Result>> GetResultsAsync(
            int? eventId = null)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var url = eventId.HasValue
                ? $"api/Results?eventId={eventId.Value}"
                : "api/Results";

            using var request = CreateSessionRequest(
                HttpMethod.Get, url);

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<Result>>()
                ?? new List<Result>();
        }

        // Record a result for an enrolment.
        public async Task<string?> CreateResultAsync(
            int enrolmentId,
            int recordedById,
            TimeSpan finishTime,
            int finishingPosition,
            int totalFinishers)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = CreateSessionRequest(
                HttpMethod.Post, "api/Results");

            request.Content = JsonContent.Create(new
            {
                EnrolmentId = enrolmentId,
                RecordedById = recordedById,
                FinishTime = finishTime,
                FinishingPosition = finishingPosition,
                TotalFinishers = totalFinishers
            });

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode
                ? null
                : await GetErrorMessageAsync(response);
        }

        // Delete a result.
        public async Task<string?> DeleteResultAsync(int resultId)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = CreateSessionRequest(
                HttpMethod.Delete,
                $"api/Results/{resultId}");

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode
                ? null
                : await GetErrorMessageAsync(response);
        }

        private static async Task<string> GetErrorMessageAsync(
            HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                using var document = JsonDocument.Parse(content);

                if (document.RootElement.TryGetProperty(
                    "message", out var message))
                {
                    return message.GetString()
                        ?? "The request could not be completed.";
                }
            }
            catch (JsonException)
            {
                // The API response was not JSON.
            }

            return string.IsNullOrWhiteSpace(content)
                ? "The request could not be completed."
                : content;
        }
    }
}
