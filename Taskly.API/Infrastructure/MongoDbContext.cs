using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using Taskly.Domain.Entities;

namespace Taskly.Infrastructure
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(MongoClient client)
        {
            _database = client.GetDatabase("Taskly");

        }

        // Coleção de tarefas
        public IMongoCollection<TodoTask> TodoTasks => _database.GetCollection<TodoTask>("TodoTasks");
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Team> Teams => _database.GetCollection<Team>("Teams");
        public IMongoCollection<Project> Projects => _database.GetCollection<Project>("Projects");
    }
}
