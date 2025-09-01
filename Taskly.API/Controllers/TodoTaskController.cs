using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Taskly.Application;
using Taskly.Application.DTOs;
using Taskly.Application.Results;
using Taskly.Domain.Entities;

namespace Taskly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoTaskController : ControllerBase
    {
        private readonly TodoTaskService _todoTaskService;

        public TodoTaskController(TodoTaskService todoTaskService)
        {
            _todoTaskService = todoTaskService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTodoTaskDto todoTask)
        {
            var result = await _todoTaskService.AddTodoTaskAsync(todoTask);

            if (!result.Success)
                return MapErrorToResponse(result.Error!);

            return Ok(result.Value);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var todoTask = await _todoTaskService.GetByIdAsync(id);
            return todoTask is null ? NotFound() : Ok(todoTask);
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetAllByProjectAsync(Guid projectId)
        {
            var todoTasks = await _todoTaskService.GetAllByProjectIdAsync(projectId);
            return todoTasks is null ? NotFound() : Ok(todoTasks);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTodoTaskDto todoTaskDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _todoTaskService.UpdateAsync(id, todoTaskDto);

            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);
            }

            return Ok(result.Value);

        }
        
        private IActionResult MapErrorToResponse(Error error)
        {
            return error.Code switch
                {
                    "Project.NotFound" => NotFound(error.Message),
                    "Project.Inactive" => BadRequest(error.Message),
                    "User.NotFound" => NotFound(error.Message),
                    "User.Inactive" => BadRequest(error.Message),
                    "TodoTask.InvalidTitle" => BadRequest(error.Message),
                    "TodoTask.NoChangesDetected" => Ok(error.Message),
                    _ => StatusCode(500, error.Message)
                };
        }
    }
}
