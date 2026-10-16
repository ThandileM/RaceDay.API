
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController(RaceDayDbContext db) : ControllerBase
    {
        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        private bool IsOrganiser =>
            string.Equals(
                HttpContext.Session.GetString("UserRole"),
                "Organiser",
                StringComparison.OrdinalIgnoreCase);

        private IActionResult? CheckOrganiserAccess()
        {
            if (CurrentUserId is null || string.IsNullOrEmpty(
                    HttpContext.Session.GetString("UserRole")))
            {
                return Unauthorized(new
                {
                    message = "Please log in before managing events."
                });
            }

            if (!IsOrganiser)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only organisers can manage events."
                });
            }

            return null;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAll()
        {
            var events = await db.Events
                .AsNoTracking()
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(events.Select(ToDto).ToList());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventResponseDto>> Get(int id)
        {
            var e = await db.Events.FindAsync(id);

            return e is null ? NotFound() : Ok(ToDto(e));
        }


        [HttpPost]
        public async Task<ActionResult<EventResponseDto>> Create(
            CreateEventDto dto)
        {
            var accessError = CheckOrganiserAccess();

            if (accessError is not null)
            {
                return new ObjectResult(
                    ((ObjectResult)accessError).Value)
                {
                    StatusCode = accessError switch
                    {
                        UnauthorizedObjectResult => StatusCodes.Status401Unauthorized,
                        _ => StatusCodes.Status403Forbidden
                    }
                };
            }

            // Use the logged-in organiser's ID.
            var e = new Event
            {
                OrganiserId = CurrentUserId!.Value,
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate.Date,
                Location = dto.Location,
                Distance = dto.Distance,
                BannerImageUrl = dto.BannerImageUrl
            };

            db.Events.Add(e);
            await db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = e.EventId },
                ToDto(e));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateEventDto dto)
        {
            var accessError = CheckOrganiserAccess();
            if (accessError is not null)
                return accessError;

            var e = await db.Events.FindAsync(id);
            if (e is null)
                return NotFound();

            if (e.OrganiserId != CurrentUserId!.Value)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only update your own events."
                });
            }

            e.Name = dto.Name;
            e.Description = dto.Description;
            e.EventDate = dto.EventDate.Date;
            e.Location = dto.Location;
            e.Distance = dto.Distance;
            e.BannerImageUrl = dto.BannerImageUrl;

            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var accessError = CheckOrganiserAccess();
            if (accessError is not null)
                return accessError;

            var e = await db.Events.FindAsync(id);
            if (e is null)
                return NotFound();

            if (e.OrganiserId != CurrentUserId!.Value)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only delete your own events."
                });
            }

            db.Events.Remove(e);

            try
            {
                await db.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "Event has categories or enrolments and cannot be deleted."
                });
            }
        }

        private static EventResponseDto ToDto(Event e) =>
            new(
                e.EventId,
                e.OrganiserId,
                e.Name,
                e.Description,
                e.EventDate,
                e.Location,
                e.Distance,
                e.BannerImageUrl,
                e.CreatedAt);
    }
}
