using Taskly.Application.DTOs;
using Taskly.Application.Queries;
using Taskly.Application.Results;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public class TodoTaskService
    {
        private readonly ITodoTaskRepository _todoTaskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProjectActivityRepository? _activityRepository;
        private readonly ITaskCommentRepository? _commentRepository;
        private readonly IUserNotificationRepository? _notificationRepository;
        public TodoTaskService(ITodoTaskRepository todoTaskrepository, IProjectRepository projectService, IUserRepository userService, ITeamRepository teamService, IProjectActivityRepository? activityRepository = null, ITaskCommentRepository? commentRepository = null, IUserNotificationRepository? notificationRepository = null)
        {
            _todoTaskRepository = todoTaskrepository;
            _projectRepository = projectService;
            _userRepository = userService;
            _teamRepository = teamService;
            _activityRepository = activityRepository;
            _commentRepository = commentRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<StructuredOperationResult<TodoTaskResponseDto>> AddTodoTaskAsync(CreateTodoTaskDto todoTaskDto, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrEmpty(todoTaskDto.Title))
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.InvalidTitle);
            if (!Enum.IsDefined(todoTaskDto.Priority))
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.InvalidPriority);

            var project = await _projectRepository.GetByIdAsync(todoTaskDto.ProjectId, cancellationToken);
            if (project == null)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.ProjectNotFound);
            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.ProjectInactive);

            var team = await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);
            if (team == null)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.TeamInactive);
            
            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.UserNotTeamMember);

            if (todoTaskDto.AssignedUserId.HasValue && todoTaskDto.AssignedUserId != Guid.Empty)
            {
                if(!team.UserIds.Contains(todoTaskDto.AssignedUserId.Value))
                    return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.AssignedUserNotTeamMember);

                var user = await _userRepository.GetByIdAsync(todoTaskDto.AssignedUserId.Value, cancellationToken);
                if (user == null)
                    return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.UserNotFound);
            }
            

            var todoTask = new TodoTask(
                title: todoTaskDto.Title,
                description: todoTaskDto.Description,
                projectId: todoTaskDto.ProjectId,
                assignedUserId: todoTaskDto.AssignedUserId,
                priority: todoTaskDto.Priority,
                dueDate: todoTaskDto.DueDate
            );
            await _todoTaskRepository.AddAsync(todoTask, cancellationToken);
            await RecordActivityAsync(todoTask, authenticatedUserId, "criou a tarefa", cancellationToken);
            if (todoTask.AssignedUserId is Guid createdAssignee && createdAssignee != authenticatedUserId && _notificationRepository != null)
                await _notificationRepository.AddAsync(new UserNotification(createdAssignee, $"Você foi atribuído à tarefa '{todoTask.Title}'.", $"/projects/{todoTask.ProjectId}"), cancellationToken);

            var todoTaskResponseDto = new TodoTaskResponseDto
            {
                Id = todoTask.Id,
                Version = todoTask.Version,
                Title = todoTask.Title,
                Description = todoTask.Description,
                ProjectId = todoTask.ProjectId,
                AssignedUserId = todoTask.AssignedUserId,
                Status = todoTask.Status,
                Priority = todoTask.Priority,
                DueDate = todoTask.DueDate,
                CreatedAt = todoTask.CreatedAt,
                UpdatedAt = todoTask.UpdatedAt
            };

            return StructuredOperationResult<TodoTaskResponseDto>.Ok(todoTaskResponseDto);
        }

        public async Task<StructuredOperationResult<TodoTaskResponseDto>> GetByIdAsync(
            Guid todoTaskId,
            Guid authenticatedUserId,
            CancellationToken cancellationToken = default)
        {
            var todoTask = await _todoTaskRepository.GetByIdAsync(todoTaskId, cancellationToken);
            if (todoTask == null)
                return StructuredOperationResult<TodoTaskResponseDto>
                    .Fail(TodoTaskErrors.NotFound);

            var project = await _projectRepository.GetByIdAsync(todoTask.ProjectId, cancellationToken);
            if (project == null)
                return StructuredOperationResult<TodoTaskResponseDto>
                    .Fail(TodoTaskErrors.ProjectNotFound);

            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult<TodoTaskResponseDto>
                    .Fail(TodoTaskErrors.ProjectInactive);

            var team = await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);
            if (team == null)
                return StructuredOperationResult<TodoTaskResponseDto>
                    .Fail(TodoTaskErrors.TeamNotFound);

            if (!team.IsActive)
                return StructuredOperationResult<TodoTaskResponseDto>
                    .Fail(TodoTaskErrors.TeamInactive);

            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<TodoTaskResponseDto>
                    .Fail(TodoTaskErrors.UserNotTeamMember);

            var todoTaskResponseDto = new TodoTaskResponseDto
            {
                Id = todoTask.Id,
                Version = todoTask.Version,
                Title = todoTask.Title,
                Description = todoTask.Description,
                ProjectId = todoTask.ProjectId,
                AssignedUserId = todoTask.AssignedUserId,
                Status = todoTask.Status,
                Priority = todoTask.Priority,
                DueDate = todoTask.DueDate,
                CreatedAt = todoTask.CreatedAt,
                UpdatedAt = todoTask.UpdatedAt
            };

            return StructuredOperationResult<TodoTaskResponseDto>
                .Ok(todoTaskResponseDto);
        }

        public async Task<StructuredOperationResult<PagedResult<TodoTaskResponseDto>>> GetByProjectIdAsync(
            Guid projectId,
            Guid authenticatedUserId,
            TodoTaskQuery query,
            CancellationToken cancellationToken = default)
        {
            if(query.Page<1)
                return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                    .Fail(TodoTaskErrors.InvalidPage);
            if(query.PageSize<1 || query.PageSize>100)
                return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                    .Fail(TodoTaskErrors.InvalidPageSize);

            var offset = ((long)query.Page - 1) * query.PageSize;
            if (offset > int.MaxValue)
                return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                    .Fail(TodoTaskErrors.PaginationLimitExceeded);

            var user = await _userRepository.GetByIdAsync(authenticatedUserId, cancellationToken);
            if (user == null)
                return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                    .Fail(TodoTaskErrors.UserNotFound);

            var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
            if (project == null)
                return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                    .Fail(TodoTaskErrors.ProjectNotFound);

            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                    .Fail(TodoTaskErrors.ProjectInactive);

            var team = await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);
            if (team == null)
                return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                    .Fail(TodoTaskErrors.TeamNotFound);

            if (!team.IsActive)
                return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                    .Fail(TodoTaskErrors.TeamInactive);

            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                    .Fail(TodoTaskErrors.UserNotTeamMember);
            
            if (query.Status.HasValue &&
                !Enum.IsDefined(query.Status.Value))
            {
                return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                    .Fail(TodoTaskErrors.InvalidStatusFilter);
            }

            if (!Enum.IsDefined(query.SortBy))
            {
                return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                    .Fail(TodoTaskErrors.InvalidSortBy);
            }

            if (!Enum.IsDefined(query.SortDirection))
            {
                return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                    .Fail(TodoTaskErrors.InvalidSortDirection);
            }


            var pagedResult = await _todoTaskRepository.GetByProjectIdAsync(
                projectId,
                query,
                cancellationToken);

            var todoTasksDto = pagedResult.Items
                .Select(todoTask => new TodoTaskResponseDto
                {
                    Id = todoTask.Id,
                    Version = todoTask.Version,
                    Title = todoTask.Title,
                    Description = todoTask.Description,
                    Status = todoTask.Status,
                    Priority = todoTask.Priority,
                    DueDate = todoTask.DueDate,
                    ProjectId = todoTask.ProjectId,
                    AssignedUserId = todoTask.AssignedUserId,
                    CreatedAt = todoTask.CreatedAt,
                    UpdatedAt = todoTask.UpdatedAt
                })
                .ToList();
            
            var response = new PagedResult<TodoTaskResponseDto>
            {
                Items = todoTasksDto, 
                TotalCount = pagedResult.TotalCount
            };

            return StructuredOperationResult<PagedResult<TodoTaskResponseDto>>
                .Ok(response);
        }


        public async Task<StructuredOperationResult<TodoTaskResponseDto>> UpdateAsync(Guid id, UpdateTodoTaskDto dto, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var todoTask = await _todoTaskRepository.GetByIdAsync(id, cancellationToken);

            if (todoTask is null)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.NotFound);

            var project = await _projectRepository.GetByIdAsync(todoTask.ProjectId, cancellationToken);
            if (project == null)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.ProjectNotFound);

            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.ProjectInactive);
            
            var team = await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);
            if (team == null)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.TeamInactive);
            
            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.UserNotTeamMember);

            ConcurrencyConflictException.Check(dto.Version, todoTask.Version);
            if (!Enum.IsDefined(dto.Priority))
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.InvalidPriority);
            todoTask.Update(dto.Title, dto.Description, dto.Priority, dto.DueDate);
            

            var modified = await _todoTaskRepository.UpdateAsync(todoTask, cancellationToken);

            if (!modified)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.NoChangesDetected);
            await RecordActivityAsync(todoTask, authenticatedUserId, "atualizou a tarefa", cancellationToken);
            
            var todoTaskResponseDto = new TodoTaskResponseDto
            {
                Id = todoTask.Id,
                Version = todoTask.Version,
                Title = todoTask.Title,
                Description = todoTask.Description,
                ProjectId = todoTask.ProjectId,
                AssignedUserId = todoTask.AssignedUserId,
                Status = todoTask.Status,
                Priority = todoTask.Priority,
                DueDate = todoTask.DueDate,
                CreatedAt = todoTask.CreatedAt,
                UpdatedAt = todoTask.UpdatedAt
            };

            return StructuredOperationResult<TodoTaskResponseDto>.Ok(todoTaskResponseDto);
        }

        public async Task<StructuredOperationResult> DeleteTaskAsync(Guid taskId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var task = await _todoTaskRepository.GetByIdAsync(taskId, cancellationToken);
            if(task == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotFound);
            
            var project = await _projectRepository.GetByIdAsync(task.ProjectId, cancellationToken);
            if (project == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.ProjectNotFound);
            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult.Fail(TodoTaskErrors.ProjectInactive);

            var team = await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);
            if (team == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult.Fail(TodoTaskErrors.TeamInactive);
            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult.Fail(TodoTaskErrors.UserNotTeamMember);
            
            task.Delete();
            var modified = await _todoTaskRepository.UpdateAsync(task, cancellationToken);

            if (!modified)
                return StructuredOperationResult.Fail(TodoTaskErrors.NoChangesDetected);
            await RecordActivityAsync(task, authenticatedUserId, "excluiu a tarefa", cancellationToken);

            return StructuredOperationResult.Ok();
        }

        public async Task<StructuredOperationResult> StartTaskAsync(Guid taskId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var task = await _todoTaskRepository.GetByIdAsync(taskId, cancellationToken);
            if(task == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotFound);
            var permissionError = await CanChangeStatusAsync(task, authenticatedUserId, cancellationToken);
            if (permissionError != null)
                return StructuredOperationResult.Fail(permissionError);
            task.Start();

            var result = await _todoTaskRepository.UpdateAsync(task, cancellationToken);
            if(!result)
                return StructuredOperationResult.Fail(TodoTaskErrors.NoChangesDetected);
            await RecordActivityAsync(task, authenticatedUserId, "iniciou a tarefa", cancellationToken);

            return StructuredOperationResult.Ok();
        }
        public async Task<StructuredOperationResult> CompleteTaskAsync(Guid taskId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var task = await _todoTaskRepository.GetByIdAsync(taskId, cancellationToken);
            if(task == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotFound);
            var permissionError = await CanChangeStatusAsync(task, authenticatedUserId, cancellationToken);
            if (permissionError != null)
                return StructuredOperationResult.Fail(permissionError);
            task.Complete();

            var result = await _todoTaskRepository.UpdateAsync(task, cancellationToken);
            if(!result)
                return StructuredOperationResult.Fail(TodoTaskErrors.NoChangesDetected);
            await RecordActivityAsync(task, authenticatedUserId, "concluiu a tarefa", cancellationToken);

            return StructuredOperationResult.Ok();
        }
        public async Task<StructuredOperationResult> CancelTaskAsync(Guid taskId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var task = await _todoTaskRepository.GetByIdAsync(taskId, cancellationToken);
            if(task == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotFound);
            var permissionError = await CanChangeStatusAsync(task, authenticatedUserId, cancellationToken);
            if (permissionError != null)
                return StructuredOperationResult.Fail(permissionError);
            task.Cancel();

            var result = await _todoTaskRepository.UpdateAsync(task, cancellationToken);
            if(!result)
                return StructuredOperationResult.Fail(TodoTaskErrors.NoChangesDetected);
            await RecordActivityAsync(task, authenticatedUserId, "cancelou a tarefa", cancellationToken);

            return StructuredOperationResult.Ok();
        }

        private async Task<Error?> CanChangeStatusAsync(
            TodoTask task, Guid authenticatedUserId, CancellationToken cancellationToken)
        {
            if (task.AssignedUserId != authenticatedUserId)
                return TodoTaskErrors.NotAssignedUser;

            var project = await _projectRepository.GetByIdAsync(task.ProjectId, cancellationToken);
            if (project == null)
                return TodoTaskErrors.ProjectNotFound;
            if (project.Status == ProjectStatus.Inactive)
                return TodoTaskErrors.ProjectInactive;

            var team = await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);
            if (team == null)
                return TodoTaskErrors.TeamNotFound;
            if (!team.UserIds.Contains(authenticatedUserId))
                return TodoTaskErrors.UserNotTeamMember;
            if (!team.IsActive)
                return TodoTaskErrors.TeamInactive;

            return null;
        }

        public async Task<StructuredOperationResult<MyWorkDashboardResponseDto>> GetMyWorkAsync(
            Guid authenticatedUserId,
            CancellationToken cancellationToken = default)
        {
            if (await _userRepository.GetByIdAsync(authenticatedUserId, cancellationToken) == null)
                return StructuredOperationResult<MyWorkDashboardResponseDto>.Fail(TodoTaskErrors.UserNotFound);

            var teams = (await _teamRepository.GetUserTeamsAsync(authenticatedUserId, cancellationToken))
                .Where(team => team.IsActive)
                .ToList();
            var projectsByTeam = await Task.WhenAll(teams.Select(team => _projectRepository.GetByTeamIdAsync(team.Id, cancellationToken)));
            var projects = projectsByTeam.SelectMany(projects => projects)
                .Where(project => project.Status == ProjectStatus.Active)
                .ToList();
            var projectNames = projects.ToDictionary(project => project.Id, project => project.Name);
            var teamNames = teams.ToDictionary(team => team.Id, team => team.Name);
            var teamNamesByProjectId = projects.ToDictionary(project => project.Id, project => teamNames[project.TeamId]);
            var tasks = await _todoTaskRepository.GetActiveAssignedToUserAsync(projectNames.Keys, authenticatedUserId, cancellationToken);
            var today = DateTime.UtcNow.Date;

            return StructuredOperationResult<MyWorkDashboardResponseDto>.Ok(new MyWorkDashboardResponseDto
            {
                TodoCount = tasks.Count(task => task.Status == TodoStatus.Todo),
                InProgressCount = tasks.Count(task => task.Status == TodoStatus.InProgress),
                OverdueCount = tasks.Count(task => task.DueDate.HasValue && task.DueDate.Value.Date < today),
                Items = tasks.Select(task => new MyWorkItemResponseDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Status = task.Status,
                    Priority = task.Priority,
                    DueDate = task.DueDate,
                    ProjectId = task.ProjectId,
                    ProjectName = projectNames[task.ProjectId],
                    TeamName = teamNamesByProjectId[task.ProjectId]
                }).ToList()
            });
        }

        public async Task<StructuredOperationResult> AssignUserAsync(Guid taskId, Guid? userId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var todoTask = await _todoTaskRepository.GetByIdAsync(taskId, cancellationToken);
            if (todoTask == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotFound);
            
            var project = await _projectRepository.GetByIdAsync(todoTask.ProjectId, cancellationToken);
            if (project == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.ProjectNotFound);
            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult.Fail(TodoTaskErrors.ProjectInactive);

            var team = await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);
            if (team == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult.Fail(TodoTaskErrors.TeamInactive);
            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult.Fail(TodoTaskErrors.UserNotTeamMember);

            if(userId != null)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
                if(user == null)
                    return StructuredOperationResult.Fail(TodoTaskErrors.UserNotFound);
                if(!team.UserIds.Contains(user.Id))
                    return StructuredOperationResult.Fail(TodoTaskErrors.AssignedUserNotTeamMember);
            }

            todoTask.AssignUser(userId);
            var result = await _todoTaskRepository.UpdateAsync(todoTask, cancellationToken);

            if(!result)
                return StructuredOperationResult.Fail(TodoTaskErrors.NoChangesDetected);

            await RecordActivityAsync(todoTask, authenticatedUserId, userId.HasValue ? "atribuiu um responsável à tarefa" : "removeu o responsável da tarefa", cancellationToken);
            if (userId is Guid assignedId && assignedId != authenticatedUserId && _notificationRepository != null)
                await _notificationRepository.AddAsync(new UserNotification(assignedId, $"Você foi atribuído à tarefa '{todoTask.Title}'.", $"/projects/{todoTask.ProjectId}"), cancellationToken);

            return StructuredOperationResult.Ok();
        }

        public async Task<StructuredOperationResult<List<ProjectActivityResponseDto>>> GetProjectActivitiesAsync(
            Guid projectId,
            Guid authenticatedUserId,
            CancellationToken cancellationToken = default)
        {
            var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
            if (project == null)
                return StructuredOperationResult<List<ProjectActivityResponseDto>>.Fail(TodoTaskErrors.ProjectNotFound);
            var team = await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);
            if (team == null)
                return StructuredOperationResult<List<ProjectActivityResponseDto>>.Fail(TodoTaskErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult<List<ProjectActivityResponseDto>>.Fail(TodoTaskErrors.TeamInactive);
            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<List<ProjectActivityResponseDto>>.Fail(TodoTaskErrors.UserNotTeamMember);

            var activities = _activityRepository == null
                ? []
                : await _activityRepository.GetByProjectIdAsync(projectId, 50, cancellationToken);
            return StructuredOperationResult<List<ProjectActivityResponseDto>>.Ok(activities.Select(activity => new ProjectActivityResponseDto
            {
                Id = activity.Id,
                ActorId = activity.ActorId,
                ActorName = activity.ActorName,
                TaskId = activity.TaskId,
                TaskTitle = activity.TaskTitle,
                Description = activity.Description,
                CreatedAt = activity.CreatedAt
            }).ToList());
        }

        private async Task RecordActivityAsync(TodoTask task, Guid actorId, string description, CancellationToken cancellationToken)
        {
            if (_activityRepository == null)
                return;
            var actor = await _userRepository.GetByIdAsync(actorId, cancellationToken);
            if (actor == null)
                return;
            await _activityRepository.AddAsync(new ProjectActivity(task.ProjectId, actorId, actor.Name, task.Id, task.Title, description), cancellationToken);
        }

        public async Task<StructuredOperationResult<List<TaskCommentResponseDto>>> GetCommentsAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
        {
            var task = await _todoTaskRepository.GetByIdAsync(taskId, cancellationToken);
            if (task == null) return StructuredOperationResult<List<TaskCommentResponseDto>>.Fail(TodoTaskErrors.NotFound);
            var project = await _projectRepository.GetByIdAsync(task.ProjectId, cancellationToken);
            var team = project == null ? null : await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);
            if (project == null) return StructuredOperationResult<List<TaskCommentResponseDto>>.Fail(TodoTaskErrors.ProjectNotFound);
            if (team == null) return StructuredOperationResult<List<TaskCommentResponseDto>>.Fail(TodoTaskErrors.TeamNotFound);
            if (!team.IsActive || !team.UserIds.Contains(userId)) return StructuredOperationResult<List<TaskCommentResponseDto>>.Fail(TodoTaskErrors.UserNotTeamMember);
            var comments = _commentRepository == null ? [] : await _commentRepository.GetByTaskIdAsync(taskId, cancellationToken);
            return StructuredOperationResult<List<TaskCommentResponseDto>>.Ok(comments.Select(ToCommentDto).ToList());
        }

        public async Task<StructuredOperationResult<TaskCommentResponseDto>> AddCommentAsync(Guid taskId, CreateTaskCommentDto dto, Guid userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Trim().Length > 1000) return StructuredOperationResult<TaskCommentResponseDto>.Fail(TodoTaskErrors.InvalidComment);
            var access = await GetCommentsAsync(taskId, userId, cancellationToken);
            if (!access.Success) return StructuredOperationResult<TaskCommentResponseDto>.Fail(access.Error!);
            var author = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (author == null || _commentRepository == null) return StructuredOperationResult<TaskCommentResponseDto>.Fail(TodoTaskErrors.UserNotFound);
            var comment = new TaskComment(taskId, userId, author.Name, dto.Content);
            await _commentRepository.AddAsync(comment, cancellationToken);
            var task = await _todoTaskRepository.GetByIdAsync(taskId, cancellationToken);
            if (task?.AssignedUserId is Guid assignedId && assignedId != userId && _notificationRepository != null)
                await _notificationRepository.AddAsync(new UserNotification(assignedId, $"{author.Name} comentou em '{task.Title}'.", $"/projects/{task.ProjectId}"), cancellationToken);
            return StructuredOperationResult<TaskCommentResponseDto>.Ok(new TaskCommentResponseDto { Id = comment.Id, AuthorId = comment.AuthorId, AuthorName = comment.AuthorName, Content = comment.Content, CreatedAt = comment.CreatedAt });
            if (task != null) await RecordActivityAsync(task, userId, "comentou na tarefa", cancellationToken);
            return StructuredOperationResult<TaskCommentResponseDto>.Ok(ToCommentDto(comment));
        }

        public async Task<StructuredOperationResult<TaskCommentResponseDto>> UpdateCommentAsync(Guid taskId, Guid commentId, CreateTaskCommentDto dto, Guid userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Trim().Length > 1000) return StructuredOperationResult<TaskCommentResponseDto>.Fail(TodoTaskErrors.InvalidComment);
            var access = await GetCommentsAsync(taskId, userId, cancellationToken);
            if (!access.Success) return StructuredOperationResult<TaskCommentResponseDto>.Fail(access.Error!);
            var comment = _commentRepository == null ? null : await _commentRepository.GetByIdAsync(commentId, cancellationToken);
            if (comment == null || comment.TaskId != taskId) return StructuredOperationResult<TaskCommentResponseDto>.Fail(TodoTaskErrors.CommentNotFound);
            if (comment.AuthorId != userId) return StructuredOperationResult<TaskCommentResponseDto>.Fail(TodoTaskErrors.NotCommentAuthor);
            comment.Update(dto.Content); await _commentRepository!.UpdateAsync(comment, cancellationToken);
            return StructuredOperationResult<TaskCommentResponseDto>.Ok(ToCommentDto(comment));
        }

        public async Task<StructuredOperationResult> DeleteCommentAsync(Guid taskId, Guid commentId, Guid userId, CancellationToken cancellationToken = default)
        {
            var access = await GetCommentsAsync(taskId, userId, cancellationToken);
            if (!access.Success) return StructuredOperationResult.Fail(access.Error!);
            var comment = _commentRepository == null ? null : await _commentRepository.GetByIdAsync(commentId, cancellationToken);
            if (comment == null || comment.TaskId != taskId) return StructuredOperationResult.Fail(TodoTaskErrors.CommentNotFound);
            if (comment.AuthorId != userId) return StructuredOperationResult.Fail(TodoTaskErrors.NotCommentAuthor);
            comment.Delete(); await _commentRepository!.UpdateAsync(comment, cancellationToken);
            return StructuredOperationResult.Ok();
        }

        private static TaskCommentResponseDto ToCommentDto(TaskComment comment) => new() { Id = comment.Id, AuthorId = comment.AuthorId, AuthorName = comment.AuthorName, Content = comment.Content, CreatedAt = comment.CreatedAt, UpdatedAt = comment.UpdatedAt };
    }
}
