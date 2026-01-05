using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
using Taskly.Application.DTOs;
using Taskly.Application.Results;
using Taskly.Domain.Entities;

namespace Taskly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService _projectService;

        public ProjectController(ProjectService projectService)
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

        private IActionResult MapErrorToResponse(Error error)
        {
            return error.Code switch
            {
                "TeamNotFound" => NotFound(error.Message),
                "TeamInactive" => BadRequest(error.Message),
                "InvalidName" => BadRequest(error.Message),
                _ => StatusCode(500, "Unexpected error")
            };
        }
    }
}
