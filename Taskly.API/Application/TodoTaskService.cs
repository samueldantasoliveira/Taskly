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
                AssignedUserId = todoTask.AssignedUserId
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
                AssignedUserId = todoTask.AssignedUserId
            };

            return todoTaskResponseDto;
        }


        public async Task<StructuredOperationResult<TodoTaskResponseDto>> UpdateAsync(Guid id, UpdateTodoTaskDto dto, Guid authenticatedUserId)
        {
            var existingTask = await _todoTaskRepository.GetByIdAsync(id);

            if (existingTask is null)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.NotFound);

            var project = await _projectRepository.GetByIdAsync(existingTask.ProjectId);
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

            if (dto.AssignedUserId.HasValue && dto.AssignedUserId != Guid.Empty)
            {
                var user = await _userRepository.GetByIdAsync(dto.AssignedUserId.Value);
                if (user == null)
                    return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.UserNotFound);
                if (!team.UserIds.Contains(user.Id))
                    return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.AssignedUserNotTeamMember);
            }

            existingTask.Title = dto.Title;
            existingTask.Description = dto.Description;
            existingTask.Status = dto.Status;
            existingTask.AssignedUserId = dto.AssignedUserId;
            

            var modified = await _todoTaskRepository.UpdateAsync(existingTask);

            if (!modified)
                return StructuredOperationResult<TodoTaskResponseDto>.Fail(TodoTaskErrors.NoChangesDetected);
            
            var todoTaskResponseDto = new TodoTaskResponseDto
            {
                Id = existingTask.Id,
                Title = existingTask.Title,
                Description = existingTask.Description,
                ProjectId = existingTask.ProjectId,
                AssignedUserId = existingTask.AssignedUserId
            };

            return StructuredOperationResult<TodoTaskResponseDto>.Ok(todoTaskResponseDto);
        }
    }
}
