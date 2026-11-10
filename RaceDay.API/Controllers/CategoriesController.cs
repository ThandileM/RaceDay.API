
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController(RaceDayDbContext db)
        : ControllerBase
    {
        private int? CurrentUserId =>
            HttpContext.Session.GetInt32("UserId");

        private bool IsOrganiser =>
            string.Equals(
                HttpContext.Session.GetString("UserRole"),
                "Organiser",
                StringComparison.OrdinalIgnoreCase);

        private IActionResult? CheckOrganiserAccess()
        {
            if (CurrentUserId == null ||
                string.IsNullOrEmpty(
                    HttpContext.Session.GetString("UserRole")))
            {
                return Unauthorized(new
                {
                    message = "Please log in before managing categories."
                });
            }

            if (!IsOrganiser)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "Only organisers can manage categories."
                    });
            }

            return null;
        }

        // GET: api/Categories?eventId=1
        // Public endpoint for participants to retrieve categories.
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? eventId)
        {
            var query = db.Categories
                .AsNoTracking()
                .AsQueryable();

            if (eventId.HasValue)
            {
                query = query.Where(
                    c => c.EventId == eventId.Value);
            }

            var categories = await query
                .OrderBy(c => c.EventId)
                .ThenBy(c => c.Name)
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.Name,
                    c.Description
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/Categories/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var category = await db.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            return Ok(new
            {
                category.CategoryId,
                category.EventId,
                category.Name,
                category.Description
            });
        }

        // POST: api/Categories
        // Only the organiser who owns the event can create categories.
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateCategoryDto dto)
        {
            var accessError = CheckOrganiserAccess();

            if (accessError != null)
                return accessError;

            var eventItem = await db.Events
                .FirstOrDefaultAsync(e => e.EventId == dto.EventId);

            if (eventItem == null)
            {
                return BadRequest(new
                {
                    message = "Event does not exist."
                });
            }

            if (eventItem.OrganiserId != CurrentUserId!.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "You can only add categories to your own events."
                    });
            }

            if (await db.Categories.AnyAsync(
                c => c.EventId == dto.EventId &&
                     c.Name == dto.Name))
            {
                return Conflict(new
                {
                    message = "Category name already exists for this event."
                });
            }

            var category = new Category
            {
                EventId = dto.EventId,
                Name = dto.Name,
                Description = dto.Description
            };

            db.Categories.Add(category);

            await db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = category.CategoryId },
                new
                {
                    category.CategoryId,
                    category.EventId,
                    category.Name,
                    category.Description
                });
        }

        // DELETE: api/Categories/1
        // Only the organiser who owns the event can delete its categories.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var accessError = CheckOrganiserAccess();

            if (accessError != null)
                return accessError;

            var category = await db.Categories
                .FindAsync(id);

            if (category == null)
                return NotFound();

            var eventItem = await db.Events
                .FirstOrDefaultAsync(
                    e => e.EventId == category.EventId);

            if (eventItem == null)
                return NotFound();

            if (eventItem.OrganiserId != CurrentUserId!.Value)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "You can only delete categories from your own events."
                    });
            }

            db.Categories.Remove(category);

            try
            {
                await db.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "Category is used by an enrolment and cannot be deleted."
                });
            }
        }
    }
}
