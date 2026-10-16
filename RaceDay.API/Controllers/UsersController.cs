using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;


namespace RaceDay.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class UsersController(RaceDayDbContext db, PasswordService passwords) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll() => (await db.Users.AsNoTracking().ToListAsync()).Select(ToDto).ToList();
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> Get(int id) { var u = await db.Users.FindAsync(id); return u is null ? NotFound() : Ok(ToDto(u)); }
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> Create(CreateUserDto dto)
        {
            if (await db.Users.AnyAsync(u => u.Email == dto.Email)) return Conflict(new { message = "Email is already registered." });
            var u = new User { FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email, PhoneNumber = dto.PhoneNumber, Role = dto.Role, ProfilePictureUrl = dto.ProfilePictureUrl };
            u.PasswordHash = passwords.Hash(u, dto.Password);
            db.Users.Add(u); await db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = u.UserId }, ToDto(u));
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateUserDto dto)
        {
            var u = await db.Users.FindAsync(id); if (u is null) return NotFound();
            if (await db.Users.AnyAsync(x => x.Email == dto.Email && x.UserId != id)) return Conflict(new { message = "Email is already in use." });
            u.FirstName = dto.FirstName; u.LastName = dto.LastName; u.Email = dto.Email; u.PhoneNumber = dto.PhoneNumber; u.Role = dto.Role; u.ProfilePictureUrl = dto.ProfilePictureUrl;
            await db.SaveChangesAsync(); return NoContent();
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id) { var u = await db.Users.FindAsync(id); if (u is null) return NotFound(); db.Users.Remove(u); try { await db.SaveChangesAsync(); return NoContent(); } catch (DbUpdateException) { return Conflict(new { message = "User is linked to events, enrolments or results and cannot be deleted." }); } }
        private static UserResponseDto ToDto(User u) => new(u.UserId, u.FirstName, u.LastName, u.Email, u.PhoneNumber, u.Role, u.ProfilePictureUrl, u.CreatedAt);
    }

}
