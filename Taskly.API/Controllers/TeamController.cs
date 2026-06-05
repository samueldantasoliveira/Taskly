using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain.Entities;
using Taskly.Application.DTOs;
using Taskly.Application.Results;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Taskly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [Authorize]
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

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateTeamDto dto){
            var result = await _teamService.UpdateTeamAsync(id, dto);
            if(!result.Success)
                return MapErrorToResponse(result.Error!);
            
            return Ok(result.Value);
        }

        [Authorize]
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

        [Authorize]
        [HttpDelete("{teamId}/remove-member")]
        public async Task<IActionResult> RemoveMember(Guid teamId, Guid userId)
        {
            var result = await _teamService.RemoveMemberAsync(teamId, userId);
            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);

            }
            return Ok(result.Value);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _teamService.DeleteTeam(id);
            if (!deleted)
                return NotFound();
            return NoContent();
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
