using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateTodoTaskDto todoTask)
        {
            var result = await _todoTaskService.AddTodoTaskAsync(todoTask);

            if (!result.Success)
                return MapErrorToResponse(result.Error!);

            return Ok(result.Value);

        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var todoTask = await _todoTaskService.GetByIdAsync(id);
            return todoTask is null ? NotFound() : Ok(todoTask);
        }

        [Authorize]
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetAllByProjectAsync(Guid projectId)
        {
            var todoTasks = await _todoTaskService.GetAllByProjectIdAsync(projectId);
            return todoTasks is null ? NotFound() : Ok(todoTasks);
        }

        [Authorize]
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
            if (error == TodoTaskErrors.ProjectNotFound)
                return NotFound(error.Message);
            if (error == TodoTaskErrors.ProjectInactive)
                return BadRequest(error.Message);
            if (error == TodoTaskErrors.UserNotFound)
                return NotFound(error.Message);
            if (error == TodoTaskErrors.InvalidTitle)
                return BadRequest(error.Message);
            if (error == TodoTaskErrors.NoChangesDetected)
                return Ok(error.Message);

            return StatusCode(500, error.Message);
        }
    }
}
