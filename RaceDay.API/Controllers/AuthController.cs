using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Services;
using RaceDay.Api.Data;
using RaceDay.API.DTOs;
using RaceDay.Api.Models;
using Microsoft.IdentityModel.Tokens;


namespace RaceDay.API.Models
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        

        private readonly PasswordService _passwordService ;

        public AuthController(RaceDayDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        //POST: api/Auth/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            bool emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == email);

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "An account with this email already exists."
                });
            }

            // validate DateOfBirth is present
            if (!request.DateOfBirth.HasValue)
            {
                return BadRequest(new
                {
                    message = "DateOfBirth is required."
                });
            }
            if (string.IsNullOrWhiteSpace(request.Role) ||
                !(request.Role.Equals("Participant", StringComparison.OrdinalIgnoreCase) ||
                request.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    message = "Role must be Participant or Organiser."
                });
            }
            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                Role = request.Role,
                PhoneNumber = request.PhoneNumber
            };

            user.PasswordHash = _passwordService.Hash(user, request.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "User registered successfully."
            });
        }

        // POST: api/Auth/Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "User not found. Check the database lookup."
                });
            }

            bool passwordValid = _passwordService.Verify(user, request.Password);

            if (!passwordValid)
            {
                return Unauthorized(new
                {
                    message = "User found, but password verification failed."
                });
            }
            // Check the selected role against the database role.
            if (!string.Equals(
                user.Role,
                request.Role,
                StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new
                {
                    message = "The selected role does not match your account."
                });
            }


            //Save user information in session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", user.Role);

            //Count successful logins for the user
            int loginCount = HttpContext.Session.GetInt32("LoginCount") ?? 0;
            loginCount++;
            HttpContext.Session.SetInt32("LoginCount", loginCount);

            return Ok(new
            {
                message = "Login successful.",
                userId = user.UserId,
                name = $"{user.FirstName} {user.LastName}",
                email = user.Email,
                role = user.Role
            });
        
        }
        //POST: api/Auth/Logout
        [HttpPost("Logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new
            {
                message = "You have been logged out."
            });
        }

        // GET: api/Auth/session
        // GET: api/Auth/session/count

        [HttpGet("session")]
        public IActionResult GetSession()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || string.IsNullOrEmpty(userRole))
            {
                return Unauthorized(new
                {
                    message = "User is not logged in."
                });
            }

            return Ok(new
            {
                userId,
                userRole,
                loginCount = HttpContext.Session.GetInt32("LoginCount") ?? 0
            });
        }

        [HttpGet("session/count")]
        public IActionResult GetLoginCount()
        {
            int loginCount = HttpContext.Session.GetInt32("LoginCount") ?? 0;
            

            return Ok(new
            {
               loginCount
            });
        }


    }
}

































