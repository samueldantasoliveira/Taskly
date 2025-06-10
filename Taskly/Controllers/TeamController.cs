using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain.Entities;
using Taskly.Application.DTOs;

namespace Taskly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly TeamService _teamService;

        public TeamController(TeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeamDto dto)
        {
            var team = new Team(dto.Name);
            await _teamService.AddTeamAsync(team);
            return Ok(team);
        }

        [HttpPost("{teamId}/add-member")]
        public async Task<IActionResult> AddMember(Guid teamId,[FromBody] Guid userId)
        {
            
            return Ok(await _teamService.AddMemberAsync(teamId, userId));
        }

    }
}
