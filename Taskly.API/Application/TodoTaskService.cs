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

        public async Task<TodoTask?> GetByIdAsync(Guid todoTaskId)
        {
            return await _todoTaskReposiory.GetByIdAsync(todoTaskId);
        }

        public async Task<List<TodoTask>> GetAllByProjectIdAsync(Guid projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);

            if(project==null)
            {
                throw new KeyNotFoundException("Project not found");
            }

            return await _todoTaskReposiory.GetAllByProjectAsync(projectId);
        }

        public async Task<TodoTask> AddTodoTaskAsync(CreateTodoTaskDto todoTaskDto)
        {
            var project = await _projectRepository.GetByIdAsync(todoTaskDto.ProjectId);
            if (project == null)
                throw new KeyNotFoundException("Project not found");

            if (project.Status == ProjectStatus.Inactive)
                throw new InvalidOperationException("Project is inactive");

            if(todoTaskDto.AssignedUserId.HasValue && todoTaskDto.AssignedUserId != Guid.Empty)
            {
                var user = await _userRepository.GetByIdAsync(todoTaskDto.AssignedUserId.Value);
                if (user == null)
                    throw new KeyNotFoundException("AssignedUser not found");
                if (!user.IsActive)
                    throw new InvalidOperationException("AssignedUser is inactive");
            }
            

            if (String.IsNullOrEmpty(todoTaskDto.Title))
                throw new ArgumentException("Title must not be empty");

            var todoTask = new TodoTask(
                title: todoTaskDto.Title,
                description: todoTaskDto.Description,
                projectId: todoTaskDto.ProjectId,
                assignedUserId: todoTaskDto.AssignedUserId.Value
            );
            await _todoTaskReposiory.AddAsync(todoTask);
            return todoTask;
        }

        public async Task<UpdateResult> UpdateAsync(Guid id, UpdateTodoTaskDto dto)
        {
            var existingTask = await _todoTaskReposiory.GetByIdAsync(id);

            if (existingTask is null)
                return new UpdateResult { FailureReason = UpdateFailureReason.TaskNotFound };
            if (dto.AssignedUserId.HasValue && dto.AssignedUserId != Guid.Empty)
            {
                var user = await _userRepository.GetByIdAsync(dto.AssignedUserId.Value);
                if (!user.IsActive || user == null)
                {
                    return new UpdateResult{ FailureReason = UpdateFailureReason.AssignedUserInvalid};
                }
            }

            existingTask.Title = dto.Title;
            existingTask.Description = dto.Description;
            existingTask.Status = dto.Status;
            existingTask.ProjectId = dto.ProjectId;
            existingTask.AssignedUserId = dto.AssignedUserId;
            

            var modified = await _todoTaskReposiory.UpdateAsync(existingTask);
            return new UpdateResult
            {
                Modified = modified
            };
        }
    }
}
