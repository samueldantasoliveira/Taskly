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

        public MongoDbContext(MongoClient client, IConfiguration configuration)
        {
            var databaseName = configuration["MongoDb:DatabaseName"];

            _database = client.GetDatabase(databaseName);
        }

        // Coleção de tarefas
        public IMongoCollection<TodoTask> TodoTasks => _database.GetCollection<TodoTask>("TodoTasks");
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Team> Teams => _database.GetCollection<Team>("Teams");
        public IMongoCollection<Project> Projects => _database.GetCollection<Project>("Projects");
    
        public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
        {
            await EnsureUserIndexes(cancellationToken);
            await EnsureTeamIndexes(cancellationToken);
            await EnsureProjectIndexes(cancellationToken);
            await EnsureTodoTaskIndexes(cancellationToken);
        }

        private async Task EnsureUserIndexes(CancellationToken cancellationToken)
        {
            var emailIndex = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions
                {
                    Unique = true
                });

            await Users.Indexes.CreateOneAsync(emailIndex, cancellationToken: cancellationToken);
        }

        private async Task EnsureTeamIndexes(CancellationToken cancellationToken)
        {
            var userIdsIndex = new CreateIndexModel<Team>(
                Builders<Team>.IndexKeys.Ascending("UserIds"),
                new CreateIndexOptions
                {
                    Name = "ix_teams_user_ids"
                });

            await Teams.Indexes.CreateOneAsync(userIdsIndex, cancellationToken: cancellationToken);
        }

        private async Task EnsureProjectIndexes(CancellationToken cancellationToken)
        {
            var teamIdIndex = new CreateIndexModel<Project>(
                Builders<Project>.IndexKeys.Ascending(p => p.TeamId),
                new CreateIndexOptions
                {
                    Name = "ix_projects_team_id"
                });

            await Projects.Indexes.CreateOneAsync(teamIdIndex, cancellationToken: cancellationToken);
        }

        private async Task EnsureTodoTaskIndexes(CancellationToken cancellationToken)
        {
            var projectIdIndex = new CreateIndexModel<TodoTask>(
                Builders<TodoTask>.IndexKeys.Ascending(task => task.ProjectId),
                new CreateIndexOptions
                {
                    Name = "ix_todo_tasks_project_id"
                });

            await TodoTasks.Indexes.CreateOneAsync(projectIdIndex, cancellationToken: cancellationToken);
        }
    }
}
