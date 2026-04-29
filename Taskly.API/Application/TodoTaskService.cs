using System.ComponentModel;
using System.Data;
using Taskly.Application.DTOs;
using Taskly.Application.Results;
using Taskly.Domain;
using Taskly.Domain.Entities;
using Taskly.Infrastructure;

namespace Taskly.Application
{
    public class TodoTaskService
    {
        private readonly ITodoTaskRepository _todoTaskRepository;
        private readonly IProjectService _projectService;
        private readonly IUserService _userService;
        public TodoTaskService(ITodoTaskRepository todoTaskrepository, IProjectService projectService, IUserService userService)
        {
            _todoTaskRepository = todoTaskrepository;
            _projectService = projectService;
            _userService = userService;
        }

        public async Task<StructuredOperationResult<TodoTask>> AddTodoTaskAsync(CreateTodoTaskDto todoTaskDto)
        {
            var project = await _projectService.GetByIdAsync(todoTaskDto.ProjectId);
            if (project == null)
                return StructuredOperationResult<TodoTask>.Fail(ProjectErrors.NotFound);

            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult<TodoTask>.Fail(ProjectErrors.Inactive);

            if (todoTaskDto.AssignedUserId.HasValue && todoTaskDto.AssignedUserId != Guid.Empty)
            {
                var user = await _userService.GetByIdAsync(todoTaskDto.AssignedUserId.Value);
                if (user == null)
                    return StructuredOperationResult<TodoTask>.Fail(UserErrors.NotFound);
            }
            

            if (String.IsNullOrEmpty(todoTaskDto.Title))
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.InvalidTitle);

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

        public async Task<StructuredOperationResult<TodoTask>> UpdateAsync(Guid id, UpdateTodoTaskDto dto)
        {
            var existingTask = await _todoTaskRepository.GetByIdAsync(id);

            if (existingTask is null)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.NotFound);

            var project = await _projectService.GetByIdAsync(dto.ProjectId);

            if (project == null)
                return StructuredOperationResult<TodoTask>.Fail(ProjectErrors.NotFound);

            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult<TodoTask>.Fail(ProjectErrors.Inactive);
                
            if (dto.AssignedUserId.HasValue && dto.AssignedUserId != Guid.Empty)
            {
                var user = await _userService.GetByIdAsync(dto.AssignedUserId.Value);
                if (user == null)
                    return StructuredOperationResult<TodoTask>.Fail(UserErrors.NotFound);
            }

            existingTask.Title = dto.Title;
            existingTask.Description = dto.Description;
            existingTask.Status = dto.Status;
            existingTask.ProjectId = dto.ProjectId;
            existingTask.AssignedUserId = dto.AssignedUserId;
            

            var modified = await _todoTaskRepository.UpdateAsync(existingTask);

            if (!modified)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.NoChangesDetected);

            return StructuredOperationResult<TodoTask>.Ok(existingTask);
        }
    }
}
