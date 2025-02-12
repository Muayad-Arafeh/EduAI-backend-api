using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EduAIAPI.Models
{
    public class Lecture
    {
        [BsonId] // Marks this property as the primary key
        [BsonRepresentation(BsonType.ObjectId)] // Ensures the Id is stored as an ObjectId in MongoDB
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // Generates a new ObjectId as a string

        [BsonElement("title")] // Maps the "title" field in MongoDB to the "Title" property
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")] // Maps the "description" field in MongoDB to the "Description" property
        public string Description { get; set; } = string.Empty;

        [BsonElement("videoUrl")] // Maps the "videoUrl" field in MongoDB to the "VideoUrl" property
        public string VideoUrl { get; set; } = string.Empty;
    }
}