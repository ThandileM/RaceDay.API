using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class EnrolmentStatusesController(RaceDayDbContext db) : ControllerBase
    {
        [HttpGet] public async Task<IActionResult> GetAll() => Ok(await db.EnrolmentStatuses.AsNoTracking().OrderBy(s => s.StatusId).Select(s => new { s.StatusId, s.StatusName, s.Description }).ToListAsync());
        [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id) { var s = await db.EnrolmentStatuses.FindAsync(id); return s is null ? NotFound() : Ok(new { s.StatusId, s.StatusName, s.Description }); }
    }
}
