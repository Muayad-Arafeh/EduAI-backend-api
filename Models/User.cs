using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EduAIAPI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("universityNumber")]
        public string UniversityNumber { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("salt")]
        public string Salt { get; set; } = string.Empty;

        [BsonElement("role")]
        public string Role { get; set; } = "Student"; // Default role is "Student"
    }
}