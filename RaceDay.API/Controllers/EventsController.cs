using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController
        (RaceDayDbContext db,
        BlobStorageService blobStorage) : ControllerBase
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
                BannerImageUrl = null
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
            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id:int}/banner")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadBanner(
            int id,
            [FromForm] IFormFile file)
        {
            var accessError = CheckOrganiserAccess();
            if (accessError is not null)
                return accessError;

            var e = await db.Events.FindAsync(id);

            if (e is null)
                return NotFound(new { message = "Event not found." });

            if (e.OrganiserId != CurrentUserId!.Value)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only upload banners for your own events."
                });
            }


            try
            {
                var oldBlobName = e.BannerImageUrl;
                var newBlobName = await blobStorage.UploadAsync(file);

                e.BannerImageUrl = newBlobName;
                await db.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(oldBlobName))
                {
                    await blobStorage.DeleteAsync(oldBlobName);
                }

                return Ok(new
                {
                    message = "Event banner uploaded successfully.",
                    blobName = newBlobName,
                    bannerImageUrl = $"/api/Events/{id}/banner"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpGet("{id:int}/banner")]
        public async Task<IActionResult> GetBanner(int id)
        {
            var e = await db.Events.FindAsync(id);

            if (e is null || string.IsNullOrWhiteSpace(e.BannerImageUrl))
                return NotFound();

            var image = await blobStorage.DownloadAsync(e.BannerImageUrl);

            if (image is null)
                return NotFound();

            return File(
                image.Content,
                image.Details.ContentType ?? "application/octet-stream");
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
                String.IsNullOrWhiteSpace(e.BannerImageUrl)
                ? null
                : $"/api/Events/{e.EventId}/banner",
                e.CreatedAt);
    }
}
