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
        [HttpGet]
        public async Task<IActionResult> GetUserTeams(CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _teamService.GetUserTeamsAsync(authenticatedUserId, cancellationToken);

            if (!result.Success)
                return MapErrorToResponse(result.Error!);

            return Ok(result.Value);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _teamService.GetByIdAsync(id, authenticatedUserId, cancellationToken);

            if (!result.Success)
                return MapErrorToResponse(result.Error!);

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeamDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _teamService.AddTeamAsync(dto, authenticatedUserId, cancellationToken);

            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);

            }
            return Ok(result.Value);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateTeamDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _teamService.UpdateTeamAsync(id, dto, authenticatedUserId, cancellationToken);

            if(!result.Success)
                return MapErrorToResponse(result.Error!);
            
            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("{teamId}/add-member")]
        public async Task<IActionResult> AddMember(Guid teamId, Guid userId, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _teamService.AddMemberAsync(teamId, userId, authenticatedUserId, cancellationToken);
            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);

            }
            return Ok(result.Value);
        }

        [Authorize]
        [HttpDelete("{teamId}/remove-member")]
        public async Task<IActionResult> RemoveMember(Guid teamId, Guid userId, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _teamService.RemoveMemberAsync(teamId, userId, authenticatedUserId, cancellationToken);
            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);

            }
            return Ok(result.Value);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _teamService.DeleteTeamAsync(id, authenticatedUserId, cancellationToken);
            
            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);

            }
            return NoContent();
        }
        [HttpDelete("{teamId}/leave")]
        [Authorize]
        public async Task<IActionResult> LeaveTeam(Guid teamId, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _teamService.LeaveTeamAsync(
                teamId,
                authenticatedUserId,
                cancellationToken
            );

            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);

            }
            return Ok();
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

            if (error == TeamErrors.UserNotFound)
                return NotFound(error.Message);

            if (error == TeamErrors.NotOwner)
                return StatusCode(StatusCodes.Status403Forbidden, error.Message);

            if (error == TeamErrors.NotAuthorized)
                return StatusCode(StatusCodes.Status403Forbidden, error.Message);
            
            if (error == TeamErrors.OwnerCannotBeRemoved)
                return Conflict(error.Message);

            return StatusCode(500, error.Message);
        }

    }
}
