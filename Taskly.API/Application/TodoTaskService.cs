using Taskly.Application.DTOs;
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

        public async Task<StructuredOperationResult<TodoTaskResponseDto>> AddTodoTaskAsync(CreateTodoTaskDto todoTaskDto, Guid authenticatedUserId)
        {
            if (String.IsNullOrEmpty(todoTaskDto.Title))
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.InvalidTitle);

            var project = await _projectRepository.GetByIdAsync(todoTaskDto.ProjectId);
            if (project == null)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.ProjectNotFound);
            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.ProjectInactive);

            var team = await _teamRepository.GetByIdAsync(project.TeamId);
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

                var user = await _userRepository.GetByIdAsync(todoTaskDto.AssignedUserId.Value);
                if (user == null)
                    return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.UserNotFound);
            }
            

            var todoTask = new TodoTask(
                title: todoTaskDto.Title,
                description: todoTaskDto.Description,
                projectId: todoTaskDto.ProjectId,
                assignedUserId: todoTaskDto.AssignedUserId
            );
            await _todoTaskRepository.AddAsync(todoTask);

            var todoTaskResponseDto = new TodoTaskResponseDto
            {
                Id = todoTask.Id,
                Title = todoTask.Title,
                Description = todoTask.Description,
                ProjectId = todoTask.ProjectId,
                AssignedUserId = todoTask.AssignedUserId,
                Status = todoTask.Status,
                CreatedAt = todoTask.CreatedAt,
                UpdatedAt = todoTask.UpdatedAt
            };

            return StructuredOperationResult<TodoTaskResponseDto>.Ok(todoTaskResponseDto);
        }

        public async Task<TodoTaskResponseDto?> GetByIdAsync(Guid todoTaskId)
        {
            var todoTask = await _todoTaskRepository.GetByIdAsync(todoTaskId);
            if(todoTask == null)
                return null;

            var todoTaskResponseDto = new TodoTaskResponseDto
            {
                Id = todoTask.Id,
                Title = todoTask.Title,
                Description = todoTask.Description,
                ProjectId = todoTask.ProjectId,
                AssignedUserId = todoTask.AssignedUserId,
                Status = todoTask.Status,
                CreatedAt = todoTask.CreatedAt,
                UpdatedAt = todoTask.UpdatedAt
            };

            return todoTaskResponseDto;
        }


        public async Task<StructuredOperationResult<TodoTaskResponseDto>> UpdateAsync(Guid id, UpdateTodoTaskDto dto, Guid authenticatedUserId)
        {
            var todoTask = await _todoTaskRepository.GetByIdAsync(id);

            if (todoTask is null)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.NotFound);

            var project = await _projectRepository.GetByIdAsync(todoTask.ProjectId);
            if (project == null)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.ProjectNotFound);

            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.ProjectInactive);
            
            var team = await _teamRepository.GetByIdAsync(project.TeamId);
            if (team == null)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.TeamInactive);
            
            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.UserNotTeamMember);

            todoTask.Update(dto.Title, dto.Description);
            

            var modified = await _todoTaskRepository.UpdateAsync(todoTask);

            if (!modified)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.NoChangesDetected);
            
            var todoTaskResponseDto = new TodoTaskResponseDto
            {
                Id = todoTask.Id,
                Title = todoTask.Title,
                Description = todoTask.Description,
                ProjectId = todoTask.ProjectId,
                AssignedUserId = todoTask.AssignedUserId,
                Status = todoTask.Status,
                CreatedAt = todoTask.CreatedAt,
                UpdatedAt = todoTask.UpdatedAt
            };

            return StructuredOperationResult<TodoTaskResponseDto>.Ok(todoTaskResponseDto);
        }

        public async Task<StructuredOperationResult> DeleteTaskAsync(Guid taskId, Guid authenticatedUserId)
        {
            var task = await _todoTaskRepository.GetByIdAsync(taskId);
            if(task == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotFound);
            
            var project = await _projectRepository.GetByIdAsync(task.ProjectId);
            if (project == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.ProjectNotFound);
            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult.Fail(TodoTaskErrors.ProjectInactive);

            var team = await _teamRepository.GetByIdAsync(project.TeamId);
            if (team == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult.Fail(TodoTaskErrors.TeamInactive);
            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult.Fail(TodoTaskErrors.UserNotTeamMember);
            
            task.Delete();
            var modified = await _todoTaskRepository.UpdateAsync(task);

            if (!modified)
                return StructuredOperationResult.Fail(TodoTaskErrors.NoChangesDetected);

            return StructuredOperationResult.Ok();
        }

        public async Task<StructuredOperationResult> StartTaskAsync(Guid taskId, Guid authenticatedUserId)
        {
            var task = await _todoTaskRepository.GetByIdAsync(taskId);
            if(task == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotFound);
            if(task.AssignedUserId == null || task.AssignedUserId != authenticatedUserId)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotAssignedUser);
            task.Start();

            var result = await _todoTaskRepository.UpdateAsync(task);
            if(!result)
                return StructuredOperationResult.Fail(TodoTaskErrors.NoChangesDetected);

            return StructuredOperationResult.Ok();
        }
        public async Task<StructuredOperationResult> CompleteTaskAsync(Guid taskId, Guid authenticatedUserId)
        {
            var task = await _todoTaskRepository.GetByIdAsync(taskId);
            if(task == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotFound);
            if(task.AssignedUserId == null || task.AssignedUserId != authenticatedUserId)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotAssignedUser);
            task.Complete();

            var result = await _todoTaskRepository.UpdateAsync(task);
            if(!result)
                return StructuredOperationResult.Fail(TodoTaskErrors.NoChangesDetected);

            return StructuredOperationResult.Ok();
        }
        public async Task<StructuredOperationResult> CancelTaskAsync(Guid taskId, Guid authenticatedUserId)
        {
            var task = await _todoTaskRepository.GetByIdAsync(taskId);
            if(task == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotFound);
            if(task.AssignedUserId == null || task.AssignedUserId != authenticatedUserId)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotAssignedUser);
            task.Cancel();

            var result = await _todoTaskRepository.UpdateAsync(task);
            if(!result)
                return StructuredOperationResult.Fail(TodoTaskErrors.NoChangesDetected);

            return StructuredOperationResult.Ok();
        }

        public async Task<StructuredOperationResult> AssignUserAsync(Guid taskId, Guid? userId, Guid authenticatedUserId)
        {
            var todoTask = await _todoTaskRepository.GetByIdAsync(taskId);
            if (todoTask == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.NotFound);
            
            var project = await _projectRepository.GetByIdAsync(todoTask.ProjectId);
            if (project == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.ProjectNotFound);
            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult.Fail(TodoTaskErrors.ProjectInactive);

            var team = await _teamRepository.GetByIdAsync(project.TeamId);
            if (team == null)
                return StructuredOperationResult.Fail(TodoTaskErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult.Fail(TodoTaskErrors.TeamInactive);
            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult.Fail(TodoTaskErrors.UserNotTeamMember);

            if(userId != null)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if(user == null)
                    return StructuredOperationResult.Fail(TodoTaskErrors.UserNotFound);
                if(!team.UserIds.Contains(user.Id))
                    return StructuredOperationResult.Fail(TodoTaskErrors.AssignedUserNotTeamMember);
            }

            todoTask.AssignUser(userId);
            var result = await _todoTaskRepository.UpdateAsync(todoTask);

            if(!result)
                return StructuredOperationResult.Fail(TodoTaskErrors.NoChangesDetected);

            return StructuredOperationResult.Ok();
        }
    }
}
