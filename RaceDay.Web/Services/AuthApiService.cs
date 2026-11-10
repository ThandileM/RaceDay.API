
using System.Net.Http.Json;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Services
{
    public class AuthApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string endpoint,
            object? body = null)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = new HttpRequestMessage(method, endpoint);

            // Forward the current browser's API session cookie.
            var incomingCookie =
                _httpContextAccessor.HttpContext?.Request.Headers.Cookie.ToString();

            if (!string.IsNullOrWhiteSpace(incomingCookie))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie", incomingCookie);
            }

            // Attach request data when supplied.
            if (body != null)
            {
                request.Content = JsonContent.Create(body);
            }

            var response = await client.SendAsync(request);

            // Pass API session cookies back to the browser.
            if (response.Headers.TryGetValues(
                "Set-Cookie", out var cookies))
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext != null)
                {
                    foreach (var cookie in cookies)
                    {
                        httpContext.Response.Headers.Append(
                            "Set-Cookie", cookie);
                    }
                }
            }

            return response;
        }

        public Task<HttpResponseMessage> RegisterAsync(
            RegisterViewModel model)
        {
            return SendAsync(
                HttpMethod.Post,
                "api/Auth/Register",
                model);
        }

        public Task<HttpResponseMessage> LoginAsync(
            LoginViewModel model)
        {
            return SendAsync(
                HttpMethod.Post,
                "api/Auth/login",
                model);
        }

        public Task<HttpResponseMessage> LogoutAsync()
        {
            return SendAsync(
                HttpMethod.Post,
                "api/Auth/Logout");
        }

        public async Task<UserSessionViewModel?> GetSessionAsync()
        {
            using var response = await SendAsync(
                HttpMethod.Get,
                "api/Auth/session");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<UserSessionViewModel>();
        }

    }
}
