using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
using Taskly.Application.DTOs;
using Taskly.Application.Results;

namespace Taskly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [Authorize]
        [HttpGet("team/{teamId}")]
        public async Task<IActionResult> GetTeamProjects(Guid teamId)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _projectService.GetTeamProjectsAsync(
                teamId,
                authenticatedUserId);

            if (!result.Success)
                return MapErrorToResponse(result.Error!);

            return Ok(result.Value);
        }
        
        [Authorize]              
        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectDto project)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _projectService.AddProjectAsync(project, authenticatedUserId);
            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);
            }
            return Ok(result.Value);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateProjectDto projectDto)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _projectService.UpdateProjectAsync(id, projectDto, authenticatedUserId);
            if(!result.Success)
                return MapErrorToResponse(result.Error!);
            
            return Ok(result.Value);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _projectService.DeleteProjectAsync(id, authenticatedUserId);
            if (!result.Success)
                return MapErrorToResponse(result.Error!);
            
            return NoContent();
        }

        private bool TryGetAuthenticatedUserId(out Guid userId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
            return Guid.TryParse(claim, out userId);
        }
        private IActionResult MapErrorToResponse(Error error)
        {
            if (error == ProjectErrors.InvalidName)
                return BadRequest(error.Message);
            if (error == ProjectErrors.TeamInactive)
                return BadRequest(error.Message);
            if (error == ProjectErrors.NotFound)
                return NotFound(error.Message);
            if (error == ProjectErrors.TeamNotFound)
                return NotFound(error.Message);
            if (error == ProjectErrors.OwnerNotFound)
                return NotFound(error.Message);
            if (error == ProjectErrors.UserNotTeamMember)
                return StatusCode(StatusCodes.Status403Forbidden, error.Message);
            if (error == ProjectErrors.NotAuthorized)
                return StatusCode(StatusCodes.Status403Forbidden, error.Message);

            return StatusCode(500, error.Message);
        }
    }
}
