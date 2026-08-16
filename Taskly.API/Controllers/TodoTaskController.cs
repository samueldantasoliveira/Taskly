using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
using Taskly.Application.DTOs;
using Taskly.Application.Results;
using System.Security.Claims;

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
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _todoTaskService.AddTodoTaskAsync(todoTask, authenticatedUserId);

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
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTodoTaskDto todoTaskDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _todoTaskService.UpdateAsync(id, todoTaskDto, authenticatedUserId);

            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);
            }

            return Ok(result.Value);

        }
        
        private bool TryGetAuthenticatedUserId(out Guid userId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
            return Guid.TryParse(claim, out userId);
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
            if (error == TodoTaskErrors.TeamNotFound)
                return NotFound(error.Message);
            if (error == TodoTaskErrors.TeamInactive)
                return BadRequest(error.Message);
            if (error == TodoTaskErrors.UserNotTeamMember)
                return StatusCode(StatusCodes.Status403Forbidden, error.Message);
            if (error == TodoTaskErrors.AssignedUserNotTeamMember)
                return StatusCode(StatusCodes.Status403Forbidden, error.Message);

            return StatusCode(500, error.Message);
        }
    }
}
