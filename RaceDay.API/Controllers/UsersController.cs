
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(
        RaceDayDbContext db,
        PasswordService passwords) : ControllerBase
    {
        // Logged-in user information
        private int? CurrentUserId =>
            HttpContext.Session.GetInt32("UserId");

        private string? CurrentUserRole =>
            HttpContext.Session.GetString("UserRole");

        private IActionResult? CheckParticipantAccess()
        {
            if (CurrentUserId == null)
                return Unauthorized(new { message = "Please log in first." });

            if (CurrentUserRole != "Participant")
                return Forbid();

            return null;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
        {
            return (await db.Users.AsNoTracking().ToListAsync())
                .Select(ToDto)
                .ToList();
        }

        // GET: api/Users/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> Get(int id)
        {
            var u = await db.Users.FindAsync(id);

            return u is null ? NotFound() : Ok(ToDto(u));
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> Create(CreateUserDto dto)
        {
            if (await db.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict(new { message = "Email is already registered." });

            var u = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role,
                ProfilePictureUrl = dto.ProfilePictureUrl
            };

            u.PasswordHash = passwords.Hash(u, dto.Password);

            db.Users.Add(u);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get),
                new { id = u.UserId }, ToDto(u));
        }

        // PUT: api/Users/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateUserDto dto)
        {
            var u = await db.Users.FindAsync(id);

            if (u is null)
                return NotFound();

            if (await db.Users.AnyAsync(x =>
                x.Email == dto.Email && x.UserId != id))
            {
                return Conflict(new { message = "Email is already in use." });
            }

            u.FirstName = dto.FirstName;
            u.LastName = dto.LastName;
            u.Email = dto.Email;
            u.PhoneNumber = dto.PhoneNumber;
            u.Role = dto.Role;
            u.ProfilePictureUrl = dto.ProfilePictureUrl;

            await db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await db.Users.FindAsync(id);

            if (u is null)
                return NotFound();

            db.Users.Remove(u);

            try
            {
                await db.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "User is linked to events, enrolments or results and cannot be deleted."
                });
            }
        }

        // GET: api/Users/profile
        [HttpGet("profile")]
        public async Task<ActionResult<ProfileResponseDto>> GetMyProfile()
        {
            if (CurrentUserId == null)
                return Unauthorized(new { message = "Please log in first." });

            if (CurrentUserRole != "Participant")
                return Forbid();

            var user = await db.Users.FindAsync(CurrentUserId.Value);

            if (user == null)
                return NotFound(new { message = "Profile not found." });

            return Ok(new ProfileResponseDto(
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.ProfilePictureUrl
            ));
        }

        // PUT: api/Users/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile(UpdateProfileDto dto)
        {
            var accessError = CheckParticipantAccess();

            if (accessError != null)
                return accessError;

            var user = await db.Users.FindAsync(CurrentUserId!.Value);

            if (user == null)
                return NotFound(new { message = "Profile not found." });

            var email = dto.Email.Trim().ToLowerInvariant();

            if (await db.Users.AnyAsync(u =>
                u.Email.ToLower() == email &&
                u.UserId != user.UserId))
            {
                return Conflict(new
                {
                    message = "This email address is already in use."
                });
            }

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.Email = email;
            user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber)
                ? null
                : dto.PhoneNumber.Trim();

            await db.SaveChangesAsync();

            return Ok(new
            {
                message = "Your profile has been updated successfully."
            });
        }

        private static UserResponseDto ToDto(User u) =>
            new(
                u.UserId,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                u.Role,
                u.ProfilePictureUrl,
                u.CreatedAt
            );
    }
}
