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
    public class LectureController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public LectureController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("by-topic/{topicId}")]
        [Authorize]
        public async Task<IActionResult> GetLecturesByTopic(string topicId)
        {
            // Fetch lectures for the specified topic
            var lectures = await _context.Lectures
                .Find(l => l.Topic == topicId)
                .ToListAsync();

            return Ok(lectures);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddLecture([FromBody] LectureDto lectureDto)
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

            // Find the first topic in the course
            var topic = await _context.Topics
                .Find(t => t.Course == course.Id)
                .FirstOrDefaultAsync();

            if (topic == null)
            {
                return NotFound("No topic found for the current course.");
            }

            // Map the DTO to the Lecture model
            var lecture = new Lecture
            {
                Title = lectureDto.Title,
                VideoUrl = lectureDto.VideoUrl,
                DocumentUrl = lectureDto.DocumentUrl,
                Topic = topic.Id
            };

            // Save the lecture to the database
            await _context.Lectures.InsertOneAsync(lecture);

            // Add the lecture to the topic's list of lectures
            topic.Lectures.Add(lecture.Id);
            await _context.Topics.ReplaceOneAsync(t => t.Id == topic.Id, topic);

            return Ok("Lecture added successfully.");
        }

        [HttpDelete("{lectureId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteLecture(string lectureId)
        {
            // Get the current user's university number from the JWT token
            var universityNumber = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(universityNumber))
            {
                return Unauthorized("User not authenticated.");
            }

            // Find the lecture
            var lecture = await _context.Lectures
                .Find(l => l.Id == lectureId)
                .FirstOrDefaultAsync();

            if (lecture == null)
            {
                return NotFound("Lecture not found.");
            }

            // Find the topic that owns the lecture
            var topic = await _context.Topics
                .Find(t => t.Id == lecture.Topic)
                .FirstOrDefaultAsync();

            if (topic == null)
            {
                return NotFound("Parent topic not found.");
            }

            // Find the course that owns the topic
            var course = await _context.Courses
                .Find(c => c.Id == topic.Course)
                .FirstOrDefaultAsync();

            if (course == null || course.UniversityNumber != universityNumber)
            {
                return Unauthorized("You do not have permission to delete this lecture.");
            }

            // Remove the lecture from the topic's list of lectures
            topic.Lectures.Remove(lectureId);
            await _context.Topics.ReplaceOneAsync(t => t.Id == topic.Id, topic);

            // Delete the lecture
            await _context.Lectures.DeleteOneAsync(l => l.Id == lectureId);

            return Ok("Lecture deleted successfully.");
        }
    }
}