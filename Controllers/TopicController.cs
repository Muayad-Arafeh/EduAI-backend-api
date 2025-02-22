using Microsoft.AspNetCore.Mvc;
using EduAIAPI.Data;
using EduAIAPI.Models;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EduAIAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopicController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public TopicController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("by-course/{courseId}")]
        [Authorize]
        public async Task<IActionResult> GetTopicsByCourse(string courseId)
        {
            // Fetch topics for the specified course
            var topics = await _context.Topics
                .Find(t => t.Course == courseId)
                .ToListAsync();

            return Ok(topics);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddTopic([FromBody] TopicDto topicDto)
        {
            // Get the current user's university number from the JWT token
            var universityNumber = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(universityNumber))
            {
                return Unauthorized("User not authenticated.");
            }

            // Find the teacher's course (assuming a teacher can only manage one course)
            var course = await _context.Courses
                .Find(c => c.UniversityNumber == universityNumber) // Use UniversityNumber instead of TeacherName
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound("No course found for the current teacher.");
            }

            // Map the DTO to the Topic model
            var topic = new Topic
            {
                Title = topicDto.Title,
                Description = topicDto.Description,
                Course = course.Id, // Set the course ID from the teacher's context
                Lectures = new List<string>() // Initialize an empty list of lectures
            };

            // Save the topic to the database (MongoDB will auto-generate the Id)
            await _context.Topics.InsertOneAsync(topic);

            // Add the topic to the course's list of topics
            course.Topics.Add(topic.Id);
            await _context.Courses.ReplaceOneAsync(c => c.Id == course.Id, course);

            return Ok("Topic added successfully.");
        }
    }
}