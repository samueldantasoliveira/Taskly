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
        private readonly ITodoTaskRepository _todoTaskReposiory;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        public TodoTaskService(ITodoTaskRepository todoTaskrepository, IProjectRepository projectRepository, IUserRepository userRepository)
        {
            _todoTaskReposiory = todoTaskrepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }

        public async Task<StructuredOperationResult<TodoTask>> AddTodoTaskAsync(CreateTodoTaskDto todoTaskDto)
        {
            var project = await _projectRepository.GetByIdAsync(todoTaskDto.ProjectId);
            if (project == null)
                return StructuredOperationResult<TodoTask>.Fail(ProjectErrors.NotFound);

            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult<TodoTask>.Fail(ProjectErrors.Inactive);

            if (todoTaskDto.AssignedUserId.HasValue && todoTaskDto.AssignedUserId != Guid.Empty)
            {
                var user = await _userRepository.GetByIdAsync(todoTaskDto.AssignedUserId.Value);
                if (user == null)
                    return StructuredOperationResult<TodoTask>.Fail(UserErrors.NotFound);
                if (!user.IsActive)
                    return StructuredOperationResult<TodoTask>.Fail(UserErrors.Inactive);
            }
            

            if (String.IsNullOrEmpty(todoTaskDto.Title))
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.InvalidTitle);

            var todoTask = new TodoTask(
                title: todoTaskDto.Title,
                description: todoTaskDto.Description,
                projectId: todoTaskDto.ProjectId,
                assignedUserId: todoTaskDto.AssignedUserId
            );
            await _todoTaskReposiory.AddAsync(todoTask);
            return StructuredOperationResult<TodoTask>.Ok(todoTask);
        }

        public async Task<TodoTask?> GetByIdAsync(Guid todoTaskId)
        {
            return await _todoTaskReposiory.GetByIdAsync(todoTaskId);
        }

        public async Task<List<TodoTask>> GetAllByProjectIdAsync(Guid projectId)
        {
            return await _todoTaskReposiory.GetAllByProjectAsync(projectId);
        }

        public async Task<StructuredOperationResult<TodoTask>> UpdateAsync(Guid id, UpdateTodoTaskDto dto)
        {
            var existingTask = await _todoTaskReposiory.GetByIdAsync(id);

            if (existingTask is null)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.NotFound);

            var project = await _projectRepository.GetByIdAsync(dto.ProjectId);

            if (project == null)
                return StructuredOperationResult<TodoTask>.Fail(ProjectErrors.NotFound);

            if (project.Status == ProjectStatus.Inactive)
                return StructuredOperationResult<TodoTask>.Fail(ProjectErrors.Inactive);
                
            if (dto.AssignedUserId.HasValue && dto.AssignedUserId != Guid.Empty)
            {
                var user = await _userRepository.GetByIdAsync(dto.AssignedUserId.Value);
                if (user == null)
                    return StructuredOperationResult<TodoTask>.Fail(UserErrors.NotFound);
                if (!user.IsActive)
                    return StructuredOperationResult<TodoTask>.Fail(UserErrors.Inactive);
            }

            existingTask.Title = dto.Title;
            existingTask.Description = dto.Description;
            existingTask.Status = dto.Status;
            existingTask.ProjectId = dto.ProjectId;
            existingTask.AssignedUserId = dto.AssignedUserId;
            

            var modified = await _todoTaskReposiory.UpdateAsync(existingTask);

            if (!modified)
                return StructuredOperationResult<TodoTask>.Fail(TodoTaskErrors.NoChangesDetected);

            return StructuredOperationResult<TodoTask>.Ok(existingTask);
        }
    }
}
