
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventsApiService _eventsApiService;

        public EventsController(EventsApiService eventsApiService)
        {
            _eventsApiService = eventsApiService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _eventsApiService.GetEventsAsync();

                return View(events);
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "Unable to retrieve events. Please try again later.";

                return View(new List<RaceDay.Web.Models.Event>());
            }
        }
    }
}
