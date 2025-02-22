using Microsoft.AspNetCore.Mvc;

namespace EduAIAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        [HttpGet("recommendations")]
        public IActionResult GetRecommendations()
        {
            // Placeholder logic
            var recommendations = new[]
            {
                new { Id = "1", Title = "Advanced Algebra" },
                new { Id = "2", Title = "Advanced Calculus" }
            };

            return Ok(recommendations);
        }
    }
}