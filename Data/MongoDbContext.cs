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

        public IMongoCollection<Lecture> Lectures => _database.GetCollection<Lecture>("Lectures");
        
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    }
}