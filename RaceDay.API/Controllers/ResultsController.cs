using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;



namespace RaceDay.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class ResultsController(RaceDayDbContext db) : ControllerBase
    {
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] int? eventId) { var q = db.Results.AsNoTracking().AsQueryable(); if (eventId.HasValue) q = q.Where(r => r.Enrolment!.EventId == eventId.Value); return Ok(await q.OrderBy(r => r.FinishingPosition).Select(r => new { r.ResultId, r.EnrolmentId, r.RecordedById, r.FinishTime, r.FinishingPosition, r.TotalFinishers, r.RecordedAt, ParticipantName = r.Enrolment!.Participant!.FirstName + " " + r.Enrolment.Participant.LastName, EventName = r.Enrolment.Event!.Name }).ToListAsync()); }
        [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id) { var r = await db.Results.AsNoTracking().Where(r => r.ResultId == id).Select(r => new { r.ResultId, r.EnrolmentId, r.RecordedById, r.FinishTime, r.FinishingPosition, r.TotalFinishers, r.RecordedAt }).FirstOrDefaultAsync(); return r is null ? NotFound() : Ok(r); }
        [HttpPost] public async Task<IActionResult> Create(CreateResultDto dto) { if (dto.FinishingPosition > dto.TotalFinishers) return BadRequest(new { message = "FinishingPosition cannot exceed TotalFinishers." }); var enrolment = await db.Enrolments.Include(e => e.Participant).FirstOrDefaultAsync(e => e.EnrolmentId == dto.EnrolmentId); if (enrolment is null) return BadRequest(new { message = "Enrolment does not exist." }); if (enrolment.StatusId != 2) return BadRequest(new { message = "A result can only be recorded for a confirmed enrolment." }); if (await db.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId)) return Conflict(new { message = "A result already exists for this enrolment." }); var recorder = await db.Users.FindAsync(dto.RecordedById); if (recorder is null || recorder.Role != "Organiser") return BadRequest(new { message = "RecordedById must refer to an existing Organiser." }); var r = new Result { EnrolmentId = dto.EnrolmentId, RecordedById = dto.RecordedById, FinishTime = dto.FinishTime, FinishingPosition = dto.FinishingPosition, TotalFinishers = dto.TotalFinishers }; db.Results.Add(r); await db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = r.ResultId }, r); }
        [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id) { var r = await db.Results.FindAsync(id); if (r is null) return NotFound(); db.Results.Remove(r); await db.SaveChangesAsync(); return NoContent(); }
    }

}
