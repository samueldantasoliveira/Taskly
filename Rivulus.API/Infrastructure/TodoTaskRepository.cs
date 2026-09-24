using MongoDB.Driver;
using Rivulus.Application;
using Rivulus.Domain.Entities;
using Rivulus.Domain;
using System.Linq.Expressions;
using Rivulus.Application.Queries;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace Rivulus.Infrastructure
{
    public class TodoTaskRepository : ITodoTaskRepository
    {
        private readonly MongoDbContext _context;

        public TodoTaskRepository(MongoDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(TodoTask todoTask, CancellationToken cancellationToken = default)
        {
            await _context.TodoTasks.InsertOneAsync(todoTask, cancellationToken: cancellationToken);
        }

        public async Task<TodoTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.TodoTasks.Find(BaseFilter(t => t.Id == id)).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Guid>> GetAssignedUserIdsByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.TodoTasks.Find(BaseFilter(t => t.ProjectId == projectId && t.AssignedUserId != null))
                .Project(task => task.AssignedUserId!.Value).ToListAsync(cancellationToken);
        }

        public async Task<List<TodoTask>> GetActiveAssignedToUserAsync(IEnumerable<Guid> projectIds, Guid userId, CancellationToken cancellationToken = default)
        {
            var ids = projectIds.Distinct().ToList();
            if (ids.Count == 0)
                return [];

            var filter = Builders<TodoTask>.Filter.And(
                BaseFilter(task => ids.Contains(task.ProjectId) && task.AssignedUserId == userId),
                Builders<TodoTask>.Filter.In(task => task.Status, [TodoStatus.Todo, TodoStatus.InProgress]));

            return await _context.TodoTasks.Find(filter)
                .Sort(Builders<TodoTask>.Sort.Combine(
                    Builders<TodoTask>.Sort.Descending(task => task.Priority),
                    Builders<TodoTask>.Sort.Ascending(task => task.DueDate),
                    Builders<TodoTask>.Sort.Descending(task => task.CreatedAt)))
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<TodoTask>> GetByProjectIdAsync(
            Guid projectId,
            TodoTaskQuery query,
            CancellationToken cancellationToken = default)
        {
            var filters = new List<FilterDefinition<TodoTask>>
            {
                BaseFilter(t => t.ProjectId == projectId)
            };

            if (query.Status.HasValue)
            {
                filters.Add(
                    Builders<TodoTask>.Filter.Eq(
                        t => t.Status,
                        query.Status.Value
                    )
                );
            }

            if (query.AssigneeId.HasValue)
            {
                filters.Add(
                    Builders<TodoTask>.Filter.Eq(
                        t => t.AssignedUserId,
                        query.AssigneeId.Value
                    )
                );
            }

            if (!string.IsNullOrWhiteSpace(query.Title))
            {
                filters.Add(
                    Builders<TodoTask>.Filter.Regex(
                        task => task.Title,
                        new BsonRegularExpression(
                            Regex.Escape(query.Title.Trim()),
                            "i")));
            }

            
            var sortBuilder = Builders<TodoTask>.Sort;

            var primarySort = query.SortBy switch
            {
                TodoTaskSortBy.Title =>
                    query.SortDirection == TodoTaskSortDirection.Ascending
                        ? sortBuilder.Ascending(task => task.Title)
                        : sortBuilder.Descending(task => task.Title),

                TodoTaskSortBy.Status =>
                    query.SortDirection == TodoTaskSortDirection.Ascending
                        ? sortBuilder.Ascending(task => task.Status)
                        : sortBuilder.Descending(task => task.Status),

                TodoTaskSortBy.Priority =>
                    query.SortDirection == TodoTaskSortDirection.Ascending
                        ? sortBuilder.Ascending(task => task.Priority)
                        : sortBuilder.Descending(task => task.Priority),

                TodoTaskSortBy.DueDate =>
                    query.SortDirection == TodoTaskSortDirection.Ascending
                        ? sortBuilder.Ascending(task => task.DueDate)
                        : sortBuilder.Descending(task => task.DueDate),

                _ =>
                    query.SortDirection == TodoTaskSortDirection.Ascending
                        ? sortBuilder.Ascending(task => task.CreatedAt)
                        : sortBuilder.Descending(task => task.CreatedAt)
            };

            var sort = sortBuilder.Combine(
                primarySort,
                sortBuilder.Ascending(task => task.Id)
            );

            var filter = Builders<TodoTask>.Filter.And(filters);
            var skip = checked((query.Page - 1) * query.PageSize);
            List<TodoTask> tasks = await _context.TodoTasks
                .Find(filter)
                .Sort(sort)
                .Skip(skip)
                .Limit(query.PageSize)
                .ToListAsync(cancellationToken);

            var totalCount = await _context.TodoTasks
                .CountDocumentsAsync(
                    filter,
                    cancellationToken: cancellationToken);

            return new PagedResult<TodoTask>
            {
                Items = tasks, TotalCount = totalCount
            };
        }

        public async Task<bool> UpdateAsync(TodoTask task, CancellationToken cancellationToken = default)
        {
            var filter = BaseFilter(t => t.Id == task.Id);
            var versionedFilter = ConcurrencyGuard.WithVersion(filter, task.Version);
            task.AdvanceVersion();
            var result = await _context.TodoTasks.ReplaceOneAsync(
                versionedFilter,
                task,
                cancellationToken: cancellationToken);
            return await ConcurrencyGuard.CheckWrite(_context.TodoTasks, filter, result.MatchedCount, cancellationToken);
        }

        private FilterDefinition<TodoTask> BaseFilter(Expression<Func<TodoTask, bool>> filter)
{
            return Builders<TodoTask>.Filter.And(
                filter,
                Builders<TodoTask>.Filter.Eq(t => t.DeletedAt, null)
            );
        }
    }
}
