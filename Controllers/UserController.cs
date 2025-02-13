using Microsoft.AspNetCore.Mvc;
using EduAIAPI.Data;
using EduAIAPI.Models;
using EduAIAPI.Helpers;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;

namespace EduAIAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(MongoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(LoginRequest request)
        {
            // Check if the user already exists
            var existingUser = await _context.Users
                .Find(u => u.UniversityNumber == request.UniversityNumber)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return BadRequest("User already exists.");
            }

            // Hash the password and generate a salt
            var (hashedPassword, salt) = PasswordHelper.HashPassword(request.Password);

            // Create a new user
            var user = new User
            {
                UniversityNumber = request.UniversityNumber,
                PasswordHash = hashedPassword,
                Salt = salt,
                Role = "Student" // Default role
            };

            // Save the user to the database
            await _context.Users.InsertOneAsync(user);

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            // Find the user by university number
            var user = await _context.Users
                .Find(u => u.UniversityNumber == request.UniversityNumber)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return BadRequest("Invalid university number or password.");
            }

            // Verify the password
            bool isPasswordValid = PasswordHelper.VerifyPassword(request.Password, user.PasswordHash, user.Salt);

            if (!isPasswordValid)
            {
                return BadRequest("Invalid university number or password.");
            }

            // Generate a JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!); // Use null-forgiving operator
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.UniversityNumber),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"]!,
                Audience = _configuration["Jwt:Audience"]!
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }
    }
}