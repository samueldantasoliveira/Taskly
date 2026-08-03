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
            if (!TryGetAuthenticatedUserId(out var userId))
                return Unauthorized();

            var result = await _teamService.AddTeamAsync(dto, userId);

            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);

            }
            return Ok(result.Value);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateTeamDto dto)
        {
            if (!TryGetAuthenticatedUserId(out var userId))
                return Unauthorized();

            var result = await _teamService.UpdateTeamAsync(id, dto, userId);

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
            if (!TryGetAuthenticatedUserId(out var userId))
                return Unauthorized();

            var result = await _teamService.DeleteTeamAsync(id, userId);
            
            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);

            }
            return NoContent();
        }

        private bool TryGetAuthenticatedUserId(out Guid userId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
            return Guid.TryParse(claim, out userId);
        }

        private IActionResult MapErrorToResponse(Error error)
        {
            if (error == TeamErrors.NotFound)
                return NotFound(error.Message);

            if (error == TeamErrors.InvalidName)
                return BadRequest(error.Message);

            if (error == TeamErrors.Inactive)
                return BadRequest(error.Message);

            if (error == TeamErrors.UserAlreadyMember)
                return Conflict(error.Message);

            if (error == TeamErrors.UserNotMember)
                return Conflict(error.Message);

            if (error == TeamErrors.NotOwner)
                return StatusCode(StatusCodes.Status403Forbidden, error.Message);

            return StatusCode(500, error.Message);
        }

    }
}
