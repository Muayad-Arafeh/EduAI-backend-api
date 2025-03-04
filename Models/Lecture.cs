using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EduAIAPI.Models
{
    public class Lecture
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("videoUrl")]
        public string VideoUrl { get; set; } = string.Empty;

        [BsonElement("documentUrl")]
        public string DocumentUrl { get; set; } = string.Empty;

        [BsonElement("topic")]
        public string Topic { get; set; } = string.Empty; // Reference to the topic (topic ID)
    }
}