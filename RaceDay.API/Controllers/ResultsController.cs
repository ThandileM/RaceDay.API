
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController(RaceDayDbContext db) : ControllerBase
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

        // GET: Participants see their own results.
        // Organisers see results for their own events.
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? eventId)
        {
            if (CurrentUserId == null ||
                string.IsNullOrWhiteSpace(CurrentUserRole))
            {
                return Unauthorized(new
                {
                    message = "Please log in to view results."
                });
            }

            var q = db.Results
                .AsNoTracking()
                .AsQueryable();

            if (string.Equals(
                CurrentUserRole, "Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r =>
                    r.Enrolment != null &&
                    r.Enrolment.ParticipantId == CurrentUserId.Value);
            }
            else if (string.Equals(
                CurrentUserRole, "Organiser",
                StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r =>
                    r.Enrolment != null &&
                    r.Enrolment.Event != null &&
                    r.Enrolment.Event.OrganiserId == CurrentUserId.Value);
            }
            else
            {
                return Forbid();
            }

            if (eventId.HasValue)
            {
                q = q.Where(r =>
                    r.Enrolment!.EventId == eventId.Value);
            }

            var results = await q
                .OrderBy(r => r.FinishingPosition)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    r.RecordedById,
                    r.FinishTime,
                    r.FinishingPosition,
                    r.TotalFinishers,
                    r.RecordedAt,
                    ParticipantName =
                        r.Enrolment!.Participant!.FirstName + " " +
                        r.Enrolment.Participant.LastName,
                    EventName = r.Enrolment.Event!.Name
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: Retrieve one result if the logged-in user has access.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            if (CurrentUserId == null ||
                string.IsNullOrWhiteSpace(CurrentUserRole))
            {
                return Unauthorized();
            }

            var q = db.Results
                .AsNoTracking()
                .Where(r => r.ResultId == id);

            if (string.Equals(
                CurrentUserRole, "Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r =>
                    r.Enrolment != null &&
                    r.Enrolment.ParticipantId == CurrentUserId.Value);
            }
            else if (string.Equals(
                CurrentUserRole, "Organiser",
                StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r =>
                    r.Enrolment != null &&
                    r.Enrolment.Event != null &&
                    r.Enrolment.Event.OrganiserId == CurrentUserId.Value);
            }
            else
            {
                return Forbid();
            }

            var result = await q
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    r.RecordedById,
                    r.FinishTime,
                    r.FinishingPosition,
                    r.TotalFinishers,
                    r.RecordedAt
                })
                .FirstOrDefaultAsync();

            return result == null
                ? NotFound()
                : Ok(result);
        }

        // POST: Only the Organiser who owns the event can record a result.
        [HttpPost]
        public async Task<IActionResult> Create(CreateResultDto dto)
        {
            var accessError = CheckAccess("Organiser");

            if (accessError != null)
                return accessError;

            if (dto.FinishingPosition > dto.TotalFinishers)
            {
                return BadRequest(new
                {
                    message = "FinishingPosition cannot exceed TotalFinishers."
                });
            }

            var enrolment = await db.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(
                    e => e.EnrolmentId == dto.EnrolmentId);

            if (enrolment == null)
            {
                return BadRequest(new
                {
                    message = "Enrolment does not exist."
                });
            }

            if (enrolment.Event == null ||
                enrolment.Event.OrganiserId != CurrentUserId!.Value)
            {
                return Forbid();
            }

            if (enrolment.StatusId != 2)
            {
                return BadRequest(new
                {
                    message = "A result can only be recorded for a confirmed enrolment."
                });
            }

            if (await db.Results.AnyAsync(
                r => r.EnrolmentId == dto.EnrolmentId))
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrolment."
                });
            }

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                RecordedById = CurrentUserId.Value,
                FinishTime = dto.FinishTime,
                FinishingPosition = dto.FinishingPosition,
                TotalFinishers = dto.TotalFinishers
            };

            db.Results.Add(result);
            await db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = result.ResultId },
                result);
        }

        // DELETE: Only the Organiser who owns the event can delete a result.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var accessError = CheckAccess("Organiser");

            if (accessError != null)
                return accessError;

            var result = await db.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound();

            if (result.Enrolment?.Event == null ||
                result.Enrolment.Event.OrganiserId != CurrentUserId!.Value)
            {
                return Forbid();
            }

            db.Results.Remove(result);
            await db.SaveChangesAsync();

            return NoContent();
        }
    }
}
