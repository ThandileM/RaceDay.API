
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly AuthApiService _authApiService;
        private readonly EventsApiService _eventsApiService;
        private readonly CategoriesApiService _categoriesApiService;
        private readonly EnrolmentsApiService _enrolmentsApiService;

        public ParticipantController(
            AuthApiService authApiService,
            EventsApiService eventsApiService,
            CategoriesApiService categoriesApiService,
            EnrolmentsApiService enrolmentsApiService)
        {
            _authApiService = authApiService;
            _eventsApiService = eventsApiService;
            _categoriesApiService = categoriesApiService;
            _enrolmentsApiService = enrolmentsApiService;
        }

        private async Task<UserSessionViewModel?> GetParticipantSessionAsync()
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null || session.UserRole != "Participant")
            {
                return null;
            }

            return session;
        }

        public async Task<IActionResult> Index()
        {
            var session = await GetParticipantSessionAsync();

            if (session == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var events = await _eventsApiService.GetEventsAsync();

            ViewBag.UserRole = session.UserRole;
            ViewBag.LoginCount = session.LoginCount;

            return View(events);
        }

        [HttpGet]
        public async Task<IActionResult> Enrol(int id)
        {
            var session = await GetParticipantSessionAsync();

            if (session == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var events = await _eventsApiService.GetEventsAsync();
            var selectedEvent = events.FirstOrDefault(e => e.EventId == id);

            if (selectedEvent == null)
            {
                return NotFound();
            }

            var categories =
                await _categoriesApiService.GetCategoriesByEventAsync(id);

            if (!categories.Any())
            {
                TempData["ErrorMessage"] =
                    "There are no categories available for this event.";

                return RedirectToAction(nameof(Index));
            }

            var model = new CreateEnrolmentViewModel
            {
                EventId = id,
                EventName = selectedEvent.Name,
                Categories = categories.Select(c =>
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.Name
                    }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(CreateEnrolmentViewModel model)
        {
            var session = await GetParticipantSessionAsync();

            if (session == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var events = await _eventsApiService.GetEventsAsync();
            var selectedEvent = events.FirstOrDefault(
                e => e.EventId == model.EventId);

            if (selectedEvent == null)
            {
                return NotFound();
            }

            var categories =
                await _categoriesApiService.GetCategoriesByEventAsync(
                    model.EventId);

            if (!categories.Any(c => c.CategoryId == model.CategoryId))
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Please select a valid category for this event.");
            }

            if (!ModelState.IsValid)
            {
                model.EventName = selectedEvent.Name;
                model.Categories = categories.Select(c =>
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.Name
                    }).ToList();

                return View(model);
            }

            var error = await _enrolmentsApiService.CreateEnrolmentAsync(
                session.UserId,
                model.EventId,
                model.CategoryId);

            if (error != null)
            {
                TempData["ErrorMessage"] = error;
                return RedirectToAction(nameof(Enrol), new { id = model.EventId });
            }

            TempData["SuccessMessage"] =
                "You have successfully enrolled in the event!";

            return RedirectToAction(nameof(MyEnrolments));
        }

        public async Task<IActionResult> MyEnrolments()
        {
            var session = await GetParticipantSessionAsync();

            if (session == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var enrolments =
                await _enrolmentsApiService.GetParticipantEnrolmentsAsync(
                    session.UserId);

            return View(enrolments);
        }
    }
}
