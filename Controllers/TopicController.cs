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

            // Find the teacher's course
            var course = await _context.Courses
                .Find(c => c.UniversityNumber == universityNumber)
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
                Course = course.Id,
                Lectures = new List<string>()
            };

            // Save the topic to the database
            await _context.Topics.InsertOneAsync(topic);

            // Add the topic to the course's list of topics
            course.Topics.Add(topic.Id);
            await _context.Courses.ReplaceOneAsync(c => c.Id == course.Id, course);

            return Ok("Topic added successfully.");
        }

        [HttpDelete("{topicId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteTopic(string topicId)
        {
            // Get the current user's university number from the JWT token
            var universityNumber = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(universityNumber))
            {
                return Unauthorized("User not authenticated.");
            }

            // Find the topic
            var topic = await _context.Topics
                .Find(t => t.Id == topicId)
                .FirstOrDefaultAsync();

            if (topic == null)
            {
                return NotFound("Topic not found.");
            }

            // Find the course that owns the topic
            var course = await _context.Courses
                .Find(c => c.Id == topic.Course)
                .FirstOrDefaultAsync();

            if (course == null || course.UniversityNumber != universityNumber)
            {
                return Unauthorized("You do not have permission to delete this topic.");
            }

            // Remove the topic from the course's list of topics
            course.Topics.Remove(topicId);
            await _context.Courses.ReplaceOneAsync(c => c.Id == course.Id, course);

            // Delete the topic
            await _context.Topics.DeleteOneAsync(t => t.Id == topicId);

            return Ok("Topic deleted successfully.");
        }
    }
}