using MongoDB.Driver;
using EduAIAPI.Models;

namespace EduAIAPI.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            _database = client.GetDatabase("EduAI");
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Course> Courses => _database.GetCollection<Course>("Courses");
        public IMongoCollection<Topic> Topics => _database.GetCollection<Topic>("Topics");
        public IMongoCollection<Lecture> Lectures => _database.GetCollection<Lecture>("Lectures");
    }
}