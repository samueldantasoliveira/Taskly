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
            
            var result = await _teamService.AddTeamAsync(dto);

            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);

            }
            return Ok(result.Value);
        }

        [HttpPost("{teamId}/add-member")]
        public async Task<IActionResult> AddMember(Guid teamId, Guid userId)
        {
            var result = await _teamService.AddMemberAsync(teamId, userId);
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
                "UserNotFound" => NotFound(error.Message),
                "UserInactive" => BadRequest(error.Message),
                "UserAlreadyMember" => Conflict(error.Message),
                "InvalidName" => BadRequest(error.Message),
                _ => StatusCode(500, error.Message)
            };
        }

    }
}
