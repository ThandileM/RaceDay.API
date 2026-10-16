using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;


namespace RaceDay.Api.Services;

public class EnrolmentService(RaceDayDbContext db)
{
    public async Task<(Enrolment? Enrolment, string? Error)> CreateAsync(CreateEnrolmentDto dto)
    {
        var participant = await db.Users.FindAsync(dto.ParticipantId);
        if (participant is null || participant.Role != "Participant") return (null, "ParticipantId must refer to an existing Participant user.");
        var ev = await db.Events.FindAsync(dto.EventId);
        if (ev is null) return (null, "Event does not exist.");
        var category = await db.Categories.FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId && c.EventId == dto.EventId);
        if (category is null) return (null, "Category must belong to the selected event.");
        if (!await db.EnrolmentStatuses.AnyAsync(s => s.StatusId == dto.StatusId)) return (null, "Enrolment status does not exist.");
        if (await db.Enrolments.AnyAsync(e => e.ParticipantId == dto.ParticipantId && e.EventId == dto.EventId)) return (null, "This participant is already enrolled in this event.");
        var enrolment = new Enrolment { ParticipantId = dto.ParticipantId, EventId = dto.EventId, CategoryId = dto.CategoryId, StatusId = dto.StatusId };
        db.Enrolments.Add(enrolment);
        await db.SaveChangesAsync();
        return (enrolment, null);
    }
}
