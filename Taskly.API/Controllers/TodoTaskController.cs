using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
using Taskly.Application.DTOs;
using Taskly.Application.Results;
using System.Security.Claims;
using Taskly.Application.Queries;

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
        public async Task<IActionResult> Create(CreateTodoTaskDto todoTask, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _todoTaskService.AddTodoTaskAsync(todoTask, authenticatedUserId, cancellationToken);

            if (!result.Success)
                return MapErrorToResponse(result.Error!);

            return Ok(result.Value);

        }

        [Authorize]
        [HttpGet("{taskId}/comments")]
        public async Task<IActionResult> GetComments(Guid taskId, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();
            var result = await _todoTaskService.GetCommentsAsync(taskId, userId, cancellationToken);
            return result.Success ? Ok(result.Value) : MapErrorToResponse(result.Error!);
        }

        [Authorize]
        [HttpPost("{taskId}/comments")]
        public async Task<IActionResult> AddComment(Guid taskId, CreateTaskCommentDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();
            var result = await _todoTaskService.AddCommentAsync(taskId, dto, userId, cancellationToken);
            return result.Success ? Ok(result.Value) : MapErrorToResponse(result.Error!);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _todoTaskService.GetByIdAsync(
                id,
                authenticatedUserId,
                cancellationToken);

            if (!result.Success)
                return MapErrorToResponse(result.Error!);

            return Ok(result.Value);
        }

        [Authorize]
        [HttpGet("my-work")]
        public async Task<IActionResult> GetMyWork(CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _todoTaskService.GetMyWorkAsync(authenticatedUserId, cancellationToken);
            return result.Success ? Ok(result.Value) : MapErrorToResponse(result.Error!);
        }

        [Authorize]
        [HttpGet("project/{projectId}/activity")]
        public async Task<IActionResult> GetProjectActivities(Guid projectId, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _todoTaskService.GetProjectActivitiesAsync(projectId, authenticatedUserId, cancellationToken);
            return result.Success ? Ok(result.Value) : MapErrorToResponse(result.Error!);
        }

        [Authorize]
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetByProjectId(
            Guid projectId,
            CancellationToken cancellationToken,
            [FromQuery] TodoTaskQuery query)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _todoTaskService.GetByProjectIdAsync(
                projectId,
                authenticatedUserId,
                query,
                cancellationToken);

            if (!result.Success)
                return MapErrorToResponse(result.Error!);

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTodoTaskDto todoTaskDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _todoTaskService.UpdateAsync(id, todoTaskDto, authenticatedUserId, cancellationToken);

            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);
            }

            return Ok(result.Value);

        }

        [Authorize]
        [HttpDelete("{taskId}")]
        public async Task<IActionResult> Delete(Guid taskId, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();
            
            var result = await _todoTaskService.DeleteTaskAsync(taskId, authenticatedUserId, cancellationToken);

            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);
            }

            return Ok();
        }


        [Authorize]
        [HttpPost("{taskId}/start")]
        public async Task<IActionResult> Start(Guid taskId, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _todoTaskService.StartTaskAsync(taskId, authenticatedUserId, cancellationToken);

            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);
            }

            return Ok();
        }

        [Authorize]
        [HttpPost("{taskId}/complete")]
        public async Task<IActionResult> Complete(Guid taskId, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _todoTaskService.CompleteTaskAsync(taskId, authenticatedUserId, cancellationToken);

            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);
            }

            return Ok();
        }

        [Authorize]
        [HttpPost("{taskId}/cancel")]
        public async Task<IActionResult> Cancel(Guid taskId, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _todoTaskService.CancelTaskAsync(taskId, authenticatedUserId, cancellationToken);

            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);
            }

            return Ok();
        }

        [Authorize]
        [HttpPost("{taskId}/assign")]
        public async Task<IActionResult> AssignUser(Guid taskId, [FromBody] AssignTodoTaskUserDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _todoTaskService.AssignUserAsync(
                taskId,
                dto.UserId,
                authenticatedUserId,
                cancellationToken);

            if (!result.Success)
                return MapErrorToResponse(result.Error!);

            return Ok();
        }

        private bool TryGetAuthenticatedUserId(out Guid userId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
            return Guid.TryParse(claim, out userId);
        }
        private IActionResult MapErrorToResponse(Error error)
        {
            if (error == TodoTaskErrors.NotFound)
                return NotFound(error.Message);
            if (error == TodoTaskErrors.ProjectNotFound)
                return NotFound(error.Message);
            if (error == TodoTaskErrors.ProjectInactive)
                return BadRequest(error.Message);
            if (error == TodoTaskErrors.UserNotFound)
                return NotFound(error.Message);
            if (error == TodoTaskErrors.InvalidTitle)
                return BadRequest(error.Message);
            if (error == TodoTaskErrors.InvalidPriority)
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
            if (error == TodoTaskErrors.NotAssignedUser)
                return StatusCode(StatusCodes.Status403Forbidden, error.Message);

            if (error == TodoTaskErrors.InvalidPage
                || error == TodoTaskErrors.InvalidPageSize
                || error == TodoTaskErrors.PaginationLimitExceeded
                || error == TodoTaskErrors.InvalidStatusFilter
                || error == TodoTaskErrors.InvalidSortBy
                || error == TodoTaskErrors.InvalidSortDirection)
            {
                return BadRequest(error.Message);
            }
            return StatusCode(500, error.Message);
        }
    }
}
