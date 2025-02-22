namespace EduAIAPI.Models
{
    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty; // User's name
        public string UniversityNumber { get; set; } = string.Empty; // University number
        public string Password { get; set; } = string.Empty; // Password
    }
}