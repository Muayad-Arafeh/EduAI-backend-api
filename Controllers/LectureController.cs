using Microsoft.AspNetCore.Mvc;
using EduAIAPI.Data;
using EduAIAPI.Models;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

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

        [HttpGet]
        public async Task<IActionResult> GetLectures()
        {
            var lectures = await _context.Lectures.Find(_ => true).ToListAsync();
            return Ok(lectures);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddLecture(Lecture lecture)
        {
            await _context.Lectures.InsertOneAsync(lecture);
            return Created($"/api/lectures/{lecture.Id}", lecture);
        }

        [HttpGet("secure")]
        [Authorize]
        public async Task<IActionResult> GetSecureLectures()
        {
            var lectures = await _context.Lectures.Find(_ => true).ToListAsync();
            return Ok(lectures);
        }
    }
}