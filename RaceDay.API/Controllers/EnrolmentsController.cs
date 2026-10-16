using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class EnrolmentsController(RaceDayDbContext db, EnrolmentService enrolmentService) : ControllerBase
    {
        [HttpGet] 
        public async Task<IActionResult> GetAll([FromQuery]
        int? participantId, [FromQuery] int? eventId) 
        { 
            var q = db.Enrolments.AsNoTracking().AsQueryable(); 
            if (participantId.HasValue) q = q.Where(e => e.ParticipantId == participantId.Value); 
            if (eventId.HasValue) q = q.Where(e => e.EventId == eventId.Value); 
            return Ok(await q.OrderByDescending(e => e.EnrolmentDate).Select(e =>
            new { e.EnrolmentId, e.ParticipantId, e.EventId, e.CategoryId, e.StatusId, e.EnrolmentDate, ParticipantName = e.Participant!.FirstName + " " + e.Participant.LastName, EventName = e.Event!.Name, CategoryName = e.Category!.Name, StatusName = e.Status!.StatusName }).ToListAsync());
        }
        [HttpGet("{id:int}")] 
        public async Task<IActionResult> Get(int id) 
        {
            var e = await db.Enrolments.AsNoTracking().Where(e => e.EnrolmentId == id).Select(e => new
            { 
                e.EnrolmentId, e.ParticipantId, e.EventId, e.CategoryId, e.StatusId, e.EnrolmentDate, ParticipantName = e.Participant!.FirstName + " " + e.Participant.LastName, EventName = e.Event!.Name, CategoryName = e.Category!.Name, StatusName = e.Status!.StatusName }).FirstOrDefaultAsync(); return e is null ? NotFound() : Ok(e); 
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateEnrolmentDto dto) 
        {
            var (enrolment, error) = await enrolmentService.CreateAsync(dto);
            if (error is not null) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(Get), new { id = enrolment!.EnrolmentId }, enrolment);
        }
        [HttpPatch("{id:int}/status")] 
        public async Task<IActionResult> UpdateStatus(int id, UpdateEnrolmentStatusDto dto)
        {
            var e = await db.Enrolments.FindAsync(id); if (e is null) return NotFound();
            if (!await db.EnrolmentStatuses.AnyAsync(s => s.StatusId == dto.StatusId)) 
                return BadRequest(new 
                {
                    message = "Status does not exist." });
            e.StatusId = dto.StatusId; await db.SaveChangesAsync(); 
            return NoContent(); 
        }

        [HttpDelete("{id:int}")] 
        public async Task<IActionResult> Delete(int id) 
        {
            var e = await db.Enrolments.FindAsync(id); if (e is null) 
                return NotFound(); db.Enrolments.Remove(e); 
            try { await db.SaveChangesAsync(); 
                return NoContent(); } catch (DbUpdateException) { 
                return Conflict(new 
                {
                    message = "Enrolment has a result recorded and cannot be deleted." }); 
            }
        }
    }
}
