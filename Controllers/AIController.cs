using Microsoft.AspNetCore.Mvc;
using EduAIAPI.Services;
using System.Threading.Tasks;
using System.Security.Claims;
using EduAIAPI.Data;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;

namespace EduAIAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly ICohereService _cohereService;
        private readonly MongoDbContext _context;

        public AIController(ICohereService cohereService, MongoDbContext context)
        {
            _cohereService = cohereService;
            _context = context;
        }

        [HttpPost("ask-question")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AskQuestion([FromBody] AskQuestionRequest request)
        {
            // Get the current user's university number from the JWT token
            var universityNumber = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(universityNumber))
            {
                return Unauthorized("User not authenticated.");
            }

            // Find the user
            var user = await _context.Users
                .Find(u => u.UniversityNumber == universityNumber)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update lecture progress (if provided)
            if (request.LectureId != null && request.Timestamp.HasValue)
            {
                user.LectureProgress[request.LectureId] = request.Timestamp.Value;
                await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
            }

            // Fetch the lecture details from the database
            var lecture = await _context.Lectures
                .Find(l => l.Id == request.LectureId)
                .FirstOrDefaultAsync();

            if (lecture == null)
            {
                return NotFound("Lecture not found.");
            }

            // Build the prompt with lecture context
            var prompt = $"The student is watching a lecture titled '{lecture.Title}' at timestamp {request.Timestamp} seconds. " +
                         $"The lecture is about: {lecture.Description}. " +
                         $"They asked: {request.Question}. " +
                         $"Please explain the concept in simpler terms and provide an example.";

            // Generate a response using Cohere
            var response = await _cohereService.GetResponseAsync(prompt);

            return Ok(new { Answer = response });
        }
    }

    public class AskQuestionRequest
    {
        public string? LectureId { get; set; } // ID of the lecture (nullable)
        public double? Timestamp { get; set; } // Timestamp where the student paused (in seconds)
        public string? Question { get; set; } // The student's question (nullable)
    }
}