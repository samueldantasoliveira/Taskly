using Taskly.Application.DTOs;
using Taskly.Application.Results;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public class TodoTaskService
    {
        private readonly ITodoTaskRepository _todoTaskRepository;
        private readonly IProjectService _projectService;
        private readonly ITeamService _teamService;
        private readonly IUserService _userService;
        public TodoTaskService(ITodoTaskRepository todoTaskrepository, IProjectService projectService, IUserService userService, ITeamService teamService)
        {
            _todoTaskRepository = todoTaskrepository;
            _projectService = projectService;
            _userService = userService;
            _teamService = teamService;
        }

        public async Task<StructuredOperationResult<TodoTask>> AddTodoTaskAsync(CreateTodoTaskDto todoTaskDto, Guid authenticatedUserId)
        {
            if (String.IsNullOrEmpty(todoTaskDto.Title))
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.InvalidTitle);

            var project = await _projectService.GetByIdAsync(todoTaskDto.ProjectId);
            if (project == null)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.ProjectNotFound);
            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.ProjectInactive);

            var team = await _teamService.GetByIdAsync(project.TeamId);
            if (team == null)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.TeamInactive);
            
            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.UserNotTeamMember);

            if (todoTaskDto.AssignedUserId.HasValue && todoTaskDto.AssignedUserId != Guid.Empty)
            {
                if(!team.UserIds.Contains(todoTaskDto.AssignedUserId.Value))
                    return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.AssignedUserNotTeamMember);

                var user = await _userService.GetByIdAsync(todoTaskDto.AssignedUserId.Value);
                if (user == null)
                    return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.UserNotFound);
            }
            

            var todoTask = new TodoTask(
                title: todoTaskDto.Title,
                description: todoTaskDto.Description,
                projectId: todoTaskDto.ProjectId,
                assignedUserId: todoTaskDto.AssignedUserId
            );
            await _todoTaskRepository.AddAsync(todoTask);
            return StructuredOperationResult<TodoTask>.Ok(todoTask);
        }

        public async Task<TodoTask?> GetByIdAsync(Guid todoTaskId)
        {
            return await _todoTaskRepository.GetByIdAsync(todoTaskId);
        }

        public async Task<List<TodoTask>> GetAllByProjectIdAsync(Guid projectId)
        {
            return await _todoTaskRepository.GetAllByProjectAsync(projectId);
        }

        public async Task<StructuredOperationResult<TodoTask>> UpdateAsync(Guid id, UpdateTodoTaskDto dto, Guid authenticatedUserId)
        {
            var existingTask = await _todoTaskRepository.GetByIdAsync(id);

            if (existingTask is null)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.NotFound);

            var project = await _projectService.GetByIdAsync(existingTask.ProjectId);
            if (project == null)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.ProjectNotFound);

            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.ProjectInactive);
            
            var team = await _teamService.GetByIdAsync(project.TeamId);
            if (team == null)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.TeamInactive);
            
            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.UserNotTeamMember);

            if (dto.AssignedUserId.HasValue && dto.AssignedUserId != Guid.Empty)
            {
                var user = await _userService.GetByIdAsync(dto.AssignedUserId.Value);
                if (user == null)
                    return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.UserNotFound);
                if (!team.UserIds.Contains(user.Id))
                    return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.AssignedUserNotTeamMember);
            }

            existingTask.Title = dto.Title;
            existingTask.Description = dto.Description;
            existingTask.Status = dto.Status;
            existingTask.AssignedUserId = dto.AssignedUserId;
            

            var modified = await _todoTaskRepository.UpdateAsync(existingTask);

            if (!modified)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.NoChangesDetected);

            return StructuredOperationResult<TodoTask>.Ok(existingTask);
        }
    }
}
