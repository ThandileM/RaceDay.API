
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.ViewComponents
{
    public class UserSessionViewComponent : ViewComponent
    {
        private readonly AuthApiService _authApiService;

        public UserSessionViewComponent(AuthApiService authApiService)
        {
            _authApiService = authApiService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var session = await _authApiService.GetSessionAsync();
                return View(session);
            }
            catch (HttpRequestException)
            {
                return View(null);
            }
        }
    }
}
