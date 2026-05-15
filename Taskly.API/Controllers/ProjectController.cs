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
        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectDto project)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString is null)
            return Unauthorized("Invalid token.");
            var userId = Guid.Parse(userIdString);


            var result = await _projectService.AddProjectAsync(project, userId);
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
            var result = await _projectService.UpdateProjectAsync(id, projectDto);
            if(!result.Success)
                return MapErrorToResponse(result.Error!);
            
            return Ok(result.Value);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _projectService.DeleteProjectAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
        private IActionResult MapErrorToResponse(Error error)
        {
            return error.Code switch
            {
                "Project.InvalidName" => BadRequest(error.Message),
                "Project.TeamInactive" => BadRequest(error.Message),
                "Project.NotFound" => NotFound(error.Message),
                "Project.TeamNotFound" => NotFound(error.Message),
                "Project.OwnerNotFound" => NotFound(error.Message),
                _ => StatusCode(500, "Unexpected error")
            };
        }
    }
}
