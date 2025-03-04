using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace EduAIAPI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("universityNumber")]
        public string UniversityNumber { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("salt")]
        public string Salt { get; set; } = string.Empty;

        [BsonElement("role")]
        public string Role { get; set; } = "Student"; // Default role is "Student"

        [BsonElement("enrolledCourses")]
        public List<string> EnrolledCourses { get; set; } = new List<string>(); // List of course IDs

        [BsonElement("lectureProgress")]
        public Dictionary<string, double> LectureProgress { get; set; } = new Dictionary<string, double>(); // Lecture ID -> Timestamp (in seconds)
    }
}