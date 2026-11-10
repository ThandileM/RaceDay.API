
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiService _authApiService;

        public AuthController(AuthApiService authApiService)
        {
            _authApiService = authApiService;
        }

        // Display registration page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Process registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using var response =
                    await _authApiService.RegisterAsync(model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Registration successful! You can now log in.";

                    return RedirectToAction("Login");
                }

                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(error)
                        ? "Registration failed. Please check your details."
                        : error);

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the RaceDay API. Please try again.");

                return View(model);
            }
        }

        // Display login page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Process login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using var response =
                    await _authApiService.LoginAsync(model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Welcome! You have logged in successfully.";

                    if (model.Role == "Participant")
                    {
                        return RedirectToAction("Index", "Participant");
                    }

                    if (model.Role == "Organiser")
                    {
                        return RedirectToAction("Index", "Organiser");
                    }

                    return RedirectToAction("Login", "Auth");

                }

                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(error)
                        ? "Invalid email, password or role."
                        : error);

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the RaceDay API. Please try again.");

                return View(model);
            }
        }

        // Log out
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                using var response =
                    await _authApiService.LogoutAsync();

                TempData["SuccessMessage"] =
                    response.IsSuccessStatusCode
                        ? "You have logged out successfully."
                        : "Logout could not be confirmed.";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to contact the API to log out.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
