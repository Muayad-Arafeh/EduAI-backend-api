using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace EduAIAPI.Models
{
    public class Course
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("teacherName")]
        public string TeacherName { get; set; } = string.Empty; // Teacher's name

        [BsonElement("universityNumber")]
        public string UniversityNumber { get; set; } = string.Empty; // Teacher's university number

        [BsonElement("topics")]
        public List<string> Topics { get; set; } = new List<string>(); // List of topic IDs
    }
}