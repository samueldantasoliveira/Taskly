using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Taskly.Domain.Entities;

namespace Taskly.Infrastructure
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(
            MongoClient client,
            IOptions<MongoDbSettings> mongoDbOptions
        )
        {
            _database = client.GetDatabase(
                mongoDbOptions.Value.DatabaseName
            );
        }

        // Coleção de tarefas
        public IMongoCollection<TodoTask> TodoTasks => _database.GetCollection<TodoTask>("TodoTasks");
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Team> Teams => _database.GetCollection<Team>("Teams");
        public IMongoCollection<Project> Projects => _database.GetCollection<Project>("Projects");
        public IMongoCollection<ProjectActivity> ProjectActivities => _database.GetCollection<ProjectActivity>("ProjectActivities");
        public IMongoCollection<TaskComment> TaskComments => _database.GetCollection<TaskComment>("TaskComments");
        public IMongoCollection<TeamInvitation> TeamInvitations => _database.GetCollection<TeamInvitation>("TeamInvitations");
        public IMongoCollection<UserNotification> UserNotifications => _database.GetCollection<UserNotification>("UserNotifications");
    
        public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
        {
            await EnsureUserIndexes(cancellationToken);
            await EnsureTeamIndexes(cancellationToken);
            await EnsureProjectIndexes(cancellationToken);
            await EnsureTodoTaskIndexes(cancellationToken);
            await EnsureProjectActivityIndexes(cancellationToken);
            await EnsureTeamInvitationIndexes(cancellationToken);
            var commentIndex = new CreateIndexModel<TaskComment>(Builders<TaskComment>.IndexKeys.Ascending(comment => comment.TaskId).Ascending(comment => comment.CreatedAt), new CreateIndexOptions { Name = "ix_task_comments_task_created_at" });
            await TaskComments.Indexes.CreateOneAsync(commentIndex, cancellationToken: cancellationToken);
            var notificationIndex = new CreateIndexModel<UserNotification>(Builders<UserNotification>.IndexKeys.Ascending(x => x.UserId).Descending(x => x.CreatedAt), new CreateIndexOptions { Name = "ix_notifications_user_created_at" });
            await UserNotifications.Indexes.CreateOneAsync(notificationIndex, cancellationToken: cancellationToken);
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
            var boardIndex = new CreateIndexModel<TodoTask>(
                Builders<TodoTask>.IndexKeys.Ascending(task => task.ProjectId)
                    .Ascending(task => task.Status).Ascending(task => task.DueDate),
                new CreateIndexOptions { Name = "ix_todo_tasks_project_status_due_date" });
            await TodoTasks.Indexes.CreateOneAsync(boardIndex, cancellationToken: cancellationToken);
            var myWorkIndex = new CreateIndexModel<TodoTask>(
                Builders<TodoTask>.IndexKeys.Ascending(task => task.AssignedUserId)
                    .Ascending(task => task.Status).Descending(task => task.Priority)
                    .Ascending(task => task.DueDate),
                new CreateIndexOptions { Name = "ix_todo_tasks_assignee_status_priority_due_date" });
            await TodoTasks.Indexes.CreateOneAsync(myWorkIndex, cancellationToken: cancellationToken);
        }

        private async Task EnsureProjectActivityIndexes(CancellationToken cancellationToken)
        {
            var index = new CreateIndexModel<ProjectActivity>(
                Builders<ProjectActivity>.IndexKeys.Ascending(activity => activity.ProjectId).Descending(activity => activity.CreatedAt),
                new CreateIndexOptions { Name = "ix_project_activities_project_created_at" });
            await ProjectActivities.Indexes.CreateOneAsync(index, cancellationToken: cancellationToken);
        }

        private async Task EnsureTeamInvitationIndexes(CancellationToken cancellationToken)
        {
            await TeamInvitations.Indexes.CreateManyAsync([
                new CreateIndexModel<TeamInvitation>(Builders<TeamInvitation>.IndexKeys.Ascending(x => x.TokenHash), new CreateIndexOptions { Name = "ux_team_invitations_token", Unique = true }),
                new CreateIndexModel<TeamInvitation>(Builders<TeamInvitation>.IndexKeys.Ascending(x => x.TeamId).Ascending(x => x.Email), new CreateIndexOptions { Name = "ix_team_invitations_team_email" })
            ], cancellationToken: cancellationToken);
        }
    }
}
