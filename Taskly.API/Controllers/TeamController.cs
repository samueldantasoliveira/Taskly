using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain.Entities;
using Taskly.Application.DTOs;
using Taskly.Application.Results;

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
            var result = await _teamService.AddTeamAsync(team);

            if (!result.Success)
            {
                return result.FailureReason switch
                { 
                    AddTeamFailureReason.InvalidName => BadRequest("Team name is invalid."),
                    _ => StatusCode(500, "An unexpected error occurred.")
                };
                
            }
            return Ok(team);
        }

        [HttpPost("{teamId}/add-member")]
        public async Task<IActionResult> AddMember(Guid teamId, Guid userId)
        {
            var result = await _teamService.AddMemberAsync(teamId, userId);
            if (!result.Success)
            {
                return result.FailureReason switch
                {
                    AddMemberFailureReason.TeamNotFound => NotFound("Team not found."),
                    AddMemberFailureReason.TeamInactive => BadRequest("Team is inactive."),
                    AddMemberFailureReason.UserNotFound => NotFound("User not found."),
                    AddMemberFailureReason.UserInactive => BadRequest("User is inactive."),
                    AddMemberFailureReason.UserAlreadyMember => Conflict("User is already a member of the team."),
                    _ => StatusCode(500, "Unexpected error.")
                };
                    
            }
            return Ok();
        }

    }
}
