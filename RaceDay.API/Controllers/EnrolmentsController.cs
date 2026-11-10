
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController(
        RaceDayDbContext db,
        EnrolmentService enrolmentService) : ControllerBase
    {
        private int? CurrentUserId =>
            HttpContext.Session.GetInt32("UserId");

        private string? CurrentUserRole =>
            HttpContext.Session.GetString("UserRole");

        private IActionResult? CheckAccess(string requiredRole)
        {
            if (CurrentUserId == null ||
                string.IsNullOrWhiteSpace(CurrentUserRole))
            {
                return Unauthorized(new
                {
                    message = "Please log in to continue."
                });
            }

            if (!string.Equals(
                CurrentUserRole,
                requiredRole,
                StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            return null;
        }

        // GET: Retrieve enrolments belonging to the logged-in user.
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? participantId,
            [FromQuery] int? eventId)
        {
            if (CurrentUserId == null ||
                string.IsNullOrWhiteSpace(CurrentUserRole))
            {
                return Unauthorized(new
                {
                    message = "Please log in to view enrolments."
                });
            }

            var q = db.Enrolments
                .AsNoTracking()
                .AsQueryable();

            if (string.Equals(
                CurrentUserRole, "Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                if (participantId.HasValue &&
                    participantId.Value != CurrentUserId.Value)
                {
                    return Forbid();
                }

                q = q.Where(e =>
                    e.ParticipantId == CurrentUserId.Value);
            }
            else if (string.Equals(
                CurrentUserRole, "Organiser",
                StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(e =>
                    e.Event != null &&
                    e.Event.OrganiserId == CurrentUserId.Value);

                if (participantId.HasValue)
                {
                    q = q.Where(e =>
                        e.ParticipantId == participantId.Value);
                }
            }
            else
            {
                return Forbid();
            }

            if (eventId.HasValue)
            {
                q = q.Where(e => e.EventId == eventId.Value);
            }

            var enrolments = await q
                .OrderByDescending(e => e.EnrolmentDate)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    e.EventId,
                    e.CategoryId,
                    e.StatusId,
                    e.EnrolmentDate,
                    ParticipantName =
                        e.Participant!.FirstName + " " +
                        e.Participant.LastName,
                    EventName = e.Event!.Name,
                    CategoryName = e.Category!.Name,
                    StatusName = e.Status!.StatusName
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: Retrieve one enrolment if the user has permission.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            if (CurrentUserId == null ||
                string.IsNullOrWhiteSpace(CurrentUserRole))
            {
                return Unauthorized();
            }

            var q = db.Enrolments
                .AsNoTracking()
                .Where(e => e.EnrolmentId == id);

            if (string.Equals(
                CurrentUserRole, "Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(e =>
                    e.ParticipantId == CurrentUserId.Value);
            }
            else if (string.Equals(
                CurrentUserRole, "Organiser",
                StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(e =>
                    e.Event != null &&
                    e.Event.OrganiserId == CurrentUserId.Value);
            }
            else
            {
                return Forbid();
            }

            var enrolment = await q.Select(e => new
            {
                e.EnrolmentId,
                e.ParticipantId,
                e.EventId,
                e.CategoryId,
                e.StatusId,
                e.EnrolmentDate,
                ParticipantName =
                    e.Participant!.FirstName + " " +
                    e.Participant.LastName,
                EventName = e.Event!.Name,
                CategoryName = e.Category!.Name,
                StatusName = e.Status!.StatusName
            }).FirstOrDefaultAsync();

            return enrolment == null
                ? NotFound()
                : Ok(enrolment);
        }

        // POST: Only participants can create their own enrolments.
        [HttpPost]
        public async Task<IActionResult> Create(CreateEnrolmentDto dto)
        {
            var accessError = CheckAccess("Participant");

            if (accessError != null)
            {
                return accessError;
            }

            if (dto.ParticipantId != CurrentUserId!.Value)
            {
                return Forbid();
            }

            var (enrolment, error) =
                await enrolmentService.CreateAsync(dto);

            if (error != null)
            {
                return BadRequest(new { message = error });
            }

            return CreatedAtAction(
                nameof(Get),
                new { id = enrolment!.EnrolmentId },
                enrolment);
        }

        // PATCH: Only the organiser who owns the event can update its enrolment.
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            UpdateEnrolmentStatusDto dto)
        {
            var accessError = CheckAccess("Organiser");

            if (accessError != null)
            {
                return accessError;
            }

            var enrolment = await db.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
            {
                return NotFound();
            }

            if (enrolment.Event == null ||
                enrolment.Event.OrganiserId != CurrentUserId!.Value)
            {
                return Forbid();
            }

            if (!await db.EnrolmentStatuses.AnyAsync(
                s => s.StatusId == dto.StatusId))
            {
                return BadRequest(new
                {
                    message = "Status does not exist."
                });
            }

            enrolment.StatusId = dto.StatusId;

            await db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: Only the organiser who owns the event can delete its enrolment.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var accessError = CheckAccess("Organiser");

            if (accessError != null)
            {
                return accessError;
            }

            var enrolment = await db.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
            {
                return NotFound();
            }

            if (enrolment.Event == null ||
                enrolment.Event.OrganiserId != CurrentUserId!.Value)
            {
                return Forbid();
            }

            db.Enrolments.Remove(enrolment);

            try
            {
                await db.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message =
                        "Enrolment has a result recorded and cannot be deleted."
                });
            }
        }
    }
}
