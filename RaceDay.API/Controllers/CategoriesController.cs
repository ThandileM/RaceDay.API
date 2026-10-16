using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CategoriesController(RaceDayDbContext db) : ControllerBase
    {
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] int? eventId) {
            var q = db.Categories.AsNoTracking().AsQueryable();
            if (eventId.HasValue) q = q.Where(c => c.EventId == eventId.Value); 
            return Ok(await q.OrderBy
                (c => c.EventId).ThenBy(c => c.Name).Select(c =>
                new { c.CategoryId, c.EventId, c.Name, c.Description }).ToListAsync()); }
        [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id) { 
            var c = await db.Categories.FindAsync(id); 
            return c is null ? NotFound() : Ok(new {
                c.CategoryId, c.EventId, c.Name, c.Description }); }
        [HttpPost] public async Task<IActionResult> Create(CreateCategoryDto dto) {
            if (!await db.Events.AnyAsync(e => e.EventId == dto.EventId)) 
                return BadRequest(new { message = "Event does not exist." }); 
            if (await db.Categories.AnyAsync(c => c.EventId == dto.EventId && c.Name == dto.Name))
                return Conflict(new { message = "Category name already exists for this event." }); 
            var c = new Category { EventId = dto.EventId, Name = dto.Name, Description = dto.Description }; db.Categories.Add(c); 
            await db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = c.CategoryId }, 
                new { c.CategoryId, c.EventId, c.Name, c.Description }); }
        [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id) {
            var c = await db.Categories.FindAsync(id);
            if (c is null) return NotFound(); db.Categories.Remove(c); 
            try { await db.SaveChangesAsync(); return NoContent(); } 
            catch (DbUpdateException) { return Conflict(new { 
                message = "Category is used by an enrolment and cannot be deleted." }); } }
    }
}
