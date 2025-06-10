using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
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
            var status = await _projectService.AddProjectAsync(project);
            if(status)
                return Ok(project);
            return BadRequest("Could not add member to the team.");
        }
    }
}
