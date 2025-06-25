using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
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

        [HttpPost]
        public async Task<IActionResult> Create(Project project)
        {
            var result = await _projectService.AddProjectAsync(project);
            if (!result.Success)
            {
                return result.FailureReason switch
                {
                    AddProjectFailureReason.TeamNotFound => NotFound("Team not found."),
                    AddProjectFailureReason.TeamInactive => BadRequest("Team is inactive."),
                    AddProjectFailureReason.InvalidName => BadRequest("Project name is invalid."),
                    _ => StatusCode(500, "Unexpected error")
                };
            }
                return Ok(project);
        }
    }
}
