using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly AuthApiService _authApiService;
        private readonly EventsApiService _eventsApiService;
        private readonly CategoriesApiService _categoriesApiService;
        private readonly EnrolmentsApiService _enrolmentsApiService;
        private readonly ResultsApiService _resultsApiService;

        public OrganiserController(
            AuthApiService authApiService,
            EventsApiService eventsApiService,
            CategoriesApiService categoriesApiService,
            EnrolmentsApiService enrolmentsApiService,
            ResultsApiService resultsApiService)
        {
            _authApiService = authApiService;
            _eventsApiService = eventsApiService;
            _categoriesApiService = categoriesApiService;
            _enrolmentsApiService = enrolmentsApiService;
            _resultsApiService = resultsApiService;
        }

        public async Task<IActionResult> Index()
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            var events = await _eventsApiService.GetEventsAsync();

            var myEvents = events
                .Where(e => e.OrganiserId == session.UserId)
                .ToList();

            ViewBag.UserRole = session.UserRole;
            ViewBag.LoginCount = session.LoginCount;

            return View(myEvents);
        }

        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            return View(new CreateEventViewModel
            {
                EventDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(
            CreateEventViewModel model)
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View(model);

            var error = await _eventsApiService.CreateEventAsync(
                session.UserId, model);

            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            TempData["SuccessMessage"] =
                "Your event was created successfully!";

            return RedirectToAction("Index");
        }

        // GET: Load an event for editing.
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            var events = await _eventsApiService.GetEventsAsync();

            var selectedEvent = events.FirstOrDefault(
                e => e.EventId == id &&
                     e.OrganiserId == session.UserId);

            if (selectedEvent == null)
                return NotFound();

            var model = new CreateEventViewModel
            {
                Name = selectedEvent.Name,
                Description = selectedEvent.Description,
                EventDate = selectedEvent.EventDate,
                Location = selectedEvent.Location,
                Distance = selectedEvent.Distance,
                ExistingBannerImageUrl = string.IsNullOrWhiteSpace(selectedEvent.BannerImageUrl)
                        ? null
                        : Url.Action(
                            "EventBanner",
                            "Organiser",
                            new { id = selectedEvent.EventId })
            };

            return View(model);
        }

        // POST: Save the updated event.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(
            int id, CreateEventViewModel model)
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            var events = await _eventsApiService.GetEventsAsync();

            var selectedEvent = events.FirstOrDefault(
                e => e.EventId == id &&
                     e.OrganiserId == session.UserId);

            if (selectedEvent == null)
                return NotFound();

            model.ExistingBannerImageUrl =
     string.IsNullOrWhiteSpace(selectedEvent.BannerImageUrl)
         ? null
         : Url.Action(
             "EventBanner",
             "Organiser",
             new { id = selectedEvent.EventId });

            if (!ModelState.IsValid)
                return View(model);

            var error = await _eventsApiService.UpdateEventAsync(
                id, session.UserId, model);

            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            TempData["SuccessMessage"] =
                "Your event was updated successfully!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            var events = await _eventsApiService.GetEventsAsync();

            var selectedEvent = events.FirstOrDefault(
                e => e.EventId == id &&
                     e.OrganiserId == session.UserId);

            if (selectedEvent == null)
                return NotFound();

            var error = await _eventsApiService.DeleteEventAsync(id);

            if (error != null)
            {
                TempData["ErrorMessage"] =
                    "Unable to delete this event. It may have existing categories or enrolments.";

                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] =
                "Your event was deleted successfully!";

            return RedirectToAction("Index");
        }
        // GET: Display categories for an organiser's event.
        [HttpGet]
        public async Task<IActionResult> ManageCategories(int eventId)
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            var events = await _eventsApiService.GetEventsAsync();

            var selectedEvent = events.FirstOrDefault(
                e => e.EventId == eventId &&
                     e.OrganiserId == session.UserId);

            if (selectedEvent == null)
                return NotFound();

            var categories =
                await _categoriesApiService.GetCategoriesByEventAsync(
                    eventId);

            var model = new ManageCategoriesViewModel
            {
                EventId = selectedEvent.EventId,
                EventName = selectedEvent.Name,
                Categories = categories
            };

            return View(model);
        }

        // POST: Add a category to an event.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(
            ManageCategoriesViewModel model)
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            var events = await _eventsApiService.GetEventsAsync();

            var selectedEvent = events.FirstOrDefault(
                e => e.EventId == model.EventId &&
                     e.OrganiserId == session.UserId);

            if (selectedEvent == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.EventName = selectedEvent.Name;
                model.Categories =
                    await _categoriesApiService.GetCategoriesByEventAsync(
                        model.EventId);

                return View("ManageCategories", model);
            }

            var error = await _categoriesApiService.CreateCategoryAsync(
                model.EventId,
                model.CategoryName.Trim(),
                model.Description);

            if (error != null)
            {
                TempData.Remove("SuccessMessage");

                TempData["ErrorMessage"] =
                    "Unable to add the category. " + error;
            }
            else
            {
                TempData.Remove("ErrorMessage");

                TempData["SuccessMessage"] =
                    "Category added successfully!";
            }

            return RedirectToAction(
                "ManageCategories",
                new { eventId = model.EventId });
        }

        // POST: Delete a category.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(
            int categoryId, int eventId)
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            var events = await _eventsApiService.GetEventsAsync();

            var selectedEvent = events.FirstOrDefault(
                e => e.EventId == eventId &&
                     e.OrganiserId == session.UserId);

            if (selectedEvent == null)
                return NotFound();

            var categories =
                await _categoriesApiService.GetCategoriesByEventAsync(
                    eventId);

            if (!categories.Any(c => c.CategoryId == categoryId))
                return NotFound();

            var error =
                await _categoriesApiService.DeleteCategoryAsync(
                    categoryId);

            if (error != null)
            {
                TempData.Remove("SuccessMessage");

                TempData["ErrorMessage"] =
                    "Unable to delete the category. It may already be used by an enrolment.";
            }
            else
            {
                TempData.Remove("ErrorMessage");

                TempData["SuccessMessage"] =
                    "Category deleted successfully!";
            }

            return RedirectToAction(
                "ManageCategories",
                new { eventId });
        }

        // GET: View participants enrolled in an organiser's event.
        [HttpGet]
        public async Task<IActionResult> ManageEnrolments(int eventId)
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            var events = await _eventsApiService.GetEventsAsync();

            var selectedEvent = events.FirstOrDefault(
                e => e.EventId == eventId &&
                     e.OrganiserId == session.UserId);

            if (selectedEvent == null)
                return NotFound();

            var enrolments =
                await _enrolmentsApiService.GetEventEnrolmentsAsync(eventId);

            ViewBag.EventId = selectedEvent.EventId;
            ViewBag.EventName = selectedEvent.Name;

            return View(enrolments);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEnrolmentStatus(
            int enrolmentId,
            int eventId,
            int statusId)
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            // Only allow Confirmed (2) or Cancelled (3).
            if (statusId != 2 && statusId != 3)
                return BadRequest();

            // Verify that this event belongs to the organiser.
            var events = await _eventsApiService.GetEventsAsync();

            var selectedEvent = events.FirstOrDefault(
                e => e.EventId == eventId &&
                     e.OrganiserId == session.UserId);

            if (selectedEvent == null)
                return NotFound();

            // Verify that the enrolment belongs to this event.
            var enrolments =
                await _enrolmentsApiService.GetEventEnrolmentsAsync(eventId);

            if (!enrolments.Any(e =>
                e.EnrolmentId == enrolmentId))
            {
                return NotFound();
            }

            var error =
                await _enrolmentsApiService.UpdateEnrolmentStatusAsync(
                    enrolmentId, statusId);

            if (error != null)
            {
                TempData.Remove("SuccessMessage");
                TempData["ErrorMessage"] =
                    "Unable to update the enrolment status. " + error;
            }
            else
            {
                TempData.Remove("ErrorMessage");

                TempData["SuccessMessage"] =
                    statusId == 2
                        ? "Participant enrolment confirmed successfully!"
                        : "Participant enrolment cancelled successfully!";
            }

            return RedirectToAction(
                "ManageEnrolments",
                new { eventId });
        }


        // GET: Display confirmed participants and results for an event.
        [HttpGet]
        public async Task<IActionResult> ManageResults(int eventId)
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            var events = await _eventsApiService.GetEventsAsync();

            var selectedEvent = events.FirstOrDefault(
                e => e.EventId == eventId &&
                     e.OrganiserId == session.UserId);

            if (selectedEvent == null)
                return NotFound();

            var enrolments =
                await _enrolmentsApiService.GetEventEnrolmentsAsync(eventId);

            var results =
                await _resultsApiService.GetResultsAsync(eventId);

            var model = new ManageResultsViewModel
            {
                EventId = selectedEvent.EventId,
                EventName = selectedEvent.Name,
                Enrolments = enrolments
                    .Where(e => e.StatusId == 2)
                    .ToList(),
                Results = results
            };

            return View(model);
        }

        // POST: Record a participant's finishing result.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(
            ManageResultsViewModel model)
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            var events = await _eventsApiService.GetEventsAsync();

            var selectedEvent = events.FirstOrDefault(
                e => e.EventId == model.EventId &&
                     e.OrganiserId == session.UserId);

            if (selectedEvent == null)
                return NotFound();

            var enrolments =
                await _enrolmentsApiService.GetEventEnrolmentsAsync(
                    model.EventId);

            var selectedEnrolment = enrolments.FirstOrDefault(
                e => e.EnrolmentId == model.EnrolmentId &&
                     e.StatusId == 2);

            if (selectedEnrolment == null)
            {
                TempData["ErrorMessage"] =
                    "Please select a confirmed participant for this event.";

                return RedirectToAction(
                    "ManageResults",
                    new { eventId = model.EventId });
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please enter valid finishing details.";

                return RedirectToAction(
                    "ManageResults",
                    new { eventId = model.EventId });
            }

            var error = await _resultsApiService.CreateResultAsync(
                model.EnrolmentId,
                session.UserId,
                model.FinishTime,
                model.FinishingPosition,
                model.TotalFinishers);

            if (error != null)
            {
                TempData["ErrorMessage"] = error;
            }
            else
            {
                TempData["SuccessMessage"] =
                    "Participant result recorded successfully!";
            }

            return RedirectToAction(
                "ManageResults",
                new { eventId = model.EventId });
        }

        // POST: Delete an incorrectly recorded result.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResult(
            int resultId,
            int eventId)
        {
            var session = await _authApiService.GetSessionAsync();

            if (session == null)
                return RedirectToAction("Login", "Auth");

            if (session.UserRole != "Organiser")
                return RedirectToAction("Index", "Home");

            var events = await _eventsApiService.GetEventsAsync();

            var selectedEvent = events.FirstOrDefault(
                e => e.EventId == eventId &&
                     e.OrganiserId == session.UserId);

            if (selectedEvent == null)
                return NotFound();

            var results =
                await _resultsApiService.GetResultsAsync(eventId);

            if (!results.Any(r => r.ResultId == resultId))
                return NotFound();

            var error =
                await _resultsApiService.DeleteResultAsync(resultId);

            if (error != null)
                TempData["ErrorMessage"] = error;
            else
                TempData["SuccessMessage"] =
                    "Result deleted successfully!";

            return RedirectToAction(
                "ManageResults",
                new { eventId });
        }


        [HttpGet]
        public async Task<IActionResult> EventBanner(int id)
        {
            var result = await _eventsApiService.GetBannerAsync(id);

            if (result.Content == null)
                return NotFound();

            return File(
                result.Content,
                result.ContentType ?? "application/octet-stream");
        }

    }
}