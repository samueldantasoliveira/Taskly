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
        public TodoTaskService(ITodoTaskRepository todoTaskrepository, IProjectRepository projectService, IUserRepository userService, ITeamRepository teamService)
        {
            _todoTaskRepository = todoTaskrepository;
            _projectRepository = projectService;
            _userRepository = userService;
            _teamRepository = teamService;
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

            return StructuredOperationResult.Ok();
        }
    }
}
