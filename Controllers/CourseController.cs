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
    public class CourseController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public CourseController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("enrolled")]
        [Authorize]
        public async Task<IActionResult> GetEnrolledCourses()
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

            // Fetch the courses the user is enrolled in
            var courses = await _context.Courses
                .Find(c => user.EnrolledCourses.Contains(c.Id))
                .ToListAsync();

            return Ok(courses);
        }

        [HttpGet("taught")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetTaughtCourses()
        {
            // Get the current user's university number from the JWT token
            var universityNumber = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(universityNumber))
            {
                return Unauthorized("User not authenticated.");
            }

            // Fetch the courses taught by the teacher
            var courses = await _context.Courses
                .Find(c => c.UniversityNumber == universityNumber)
                .ToListAsync();

            return Ok(courses);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddCourse([FromBody] CourseDto courseDto)
        {
            // Get the current user's university number from the JWT token
            var universityNumber = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(universityNumber))
            {
                return Unauthorized("User not authenticated.");
            }

            // Map the DTO to the Course model
            var course = new Course
            {
                Name = courseDto.Name,
                TeacherName = courseDto.TeacherName,
                UniversityNumber = universityNumber, // Set the teacher's university number from the JWT token
                Topics = new List<string>() // Initialize an empty list of topics
            };

            // Save the course to the database (MongoDB will auto-generate the Id)
            await _context.Courses.InsertOneAsync(course);

            return Ok("Course added successfully.");
        }
    }
}