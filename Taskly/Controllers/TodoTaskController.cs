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
            try
            {
                var newTodoTask = await _todoTaskService.AddTodoTaskAsync(todoTask);
                return CreatedAtAction(nameof(GetById), new { id = newTodoTask.Id }, newTodoTask);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
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
            try
            {
                var todoTasks = await _todoTaskService.GetAllByProjectIdAsync(projectId);
                return Ok(todoTasks);
            }
            catch(Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTodoTaskDto todoTaskDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _todoTaskService.UpdateAsync(id, todoTaskDto);

            if (!result.Sucess)
            {
                return result.FailureReason switch
                {
                    UpdateFailureReason.TaskNotFound => NotFound("Task not found."),
                    UpdateFailureReason.AssignedUserInvalid => BadRequest("Invalid assigned user."),
                    _ => StatusCode(500, "Unexpected error.")
                };
            }

            return result.Modified ? NoContent() : Ok("No modifications made"); // HTTP 204
            


            
        }
    }
}
