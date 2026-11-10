
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services
{
    public class EnrolmentsApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EnrolmentsApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        // Forward the current user's session cookie per request.
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

        // Retrieve enrolments for a participant.
        public async Task<List<Enrolment>> GetParticipantEnrolmentsAsync(
            int participantId)
        {
            return await GetEnrolmentsAsync(
                $"api/Enrolments?participantId={participantId}");
        }

        // Retrieve enrolments belonging to an event.
        public async Task<List<Enrolment>> GetEventEnrolmentsAsync(
            int eventId)
        {
            return await GetEnrolmentsAsync(
                $"api/Enrolments?eventId={eventId}");
        }

        private async Task<List<Enrolment>> GetEnrolmentsAsync(
            string url)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = CreateSessionRequest(
                HttpMethod.Get, url);

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Enrolment>>()
                ?? new List<Enrolment>();
        }

        // Create a participant enrolment.
        public async Task<string?> CreateEnrolmentAsync(
            int participantId,
            int eventId,
            int categoryId)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = CreateSessionRequest(
                HttpMethod.Post, "api/Enrolments");

            request.Content = JsonContent.Create(new
            {
                ParticipantId = participantId,
                EventId = eventId,
                CategoryId = categoryId,
                StatusId = 1
            });

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode
                ? null
                : await GetErrorMessageAsync(response);
        }

        // Approve or reject a participant enrolment.
        public async Task<string?> UpdateEnrolmentStatusAsync(
            int enrolmentId,
            int statusId)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = CreateSessionRequest(
                HttpMethod.Patch,
                $"api/Enrolments/{enrolmentId}/status");

            request.Content = JsonContent.Create(new
            {
                StatusId = statusId
            });

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode
                ? null
                : await GetErrorMessageAsync(response);
        }

        // Delete an enrolment.
        public async Task<string?> DeleteEnrolmentAsync(
            int enrolmentId)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = CreateSessionRequest(
                HttpMethod.Delete,
                $"api/Enrolments/{enrolmentId}");

            using var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode
                ? null
                : await GetErrorMessageAsync(response);
        }

        // Read an API error message.
        private static async Task<string> GetErrorMessageAsync(
            HttpResponseMessage response)
        {
            var content =
                await response.Content.ReadAsStringAsync();

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
                // The response was not JSON.
            }

            return string.IsNullOrWhiteSpace(content)
                ? "The request could not be completed."
                : content;
        }
    }
}
