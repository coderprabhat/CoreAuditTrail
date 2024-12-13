using CoreAuditTrail.Data;
using CoreAuditTrail.DTOs;
using CoreAuditTrail.Models;
using CoreAuditTrail.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace CoreAuditTrail.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
                return BadRequest("Username already exists.");

            var salt = AuthService.GenerateSalt();
            var hashedPassword = AuthService.HashPassword(user.PasswordHash, salt);

            user.PasswordHash = hashedPassword;
            user.Salt = salt;

            // Assign Role based on input
            if (!Enum.IsDefined(typeof(User.UserRole), user.Role))
                return BadRequest("Invalid role specified.");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDto user)
        {

            var existingUser = _context.Users
                .FirstOrDefault(u => u.Username == user.Username || u.Email == user.Email);

            if (existingUser == null)
                return Unauthorized("Invalid username or password.");

            var isPasswordValid = AuthService.VerifyPassword(user.Password, existingUser.Salt, existingUser.PasswordHash);
            if (!isPasswordValid)
                return Unauthorized("Invalid username or password.");

            // Pass the RoleType (enum or string) instead of the entire Role object
            var token = TokenService.GenerateToken(existingUser.Username, existingUser.Role.ToString(), existingUser.Email, existingUser.Id);
           
            return Ok(new { Token = token });

        }
    }
}
