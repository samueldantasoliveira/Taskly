using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Rivulus.Application;
using Rivulus.Domain.Entities;
using Rivulus.Application.DTOs;
using Rivulus.Application.Results;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Rivulus.Controllers
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
        [HttpGet("{teamId}/members")]
        public async Task<IActionResult> GetMembers(
            Guid teamId,
            CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var result = await _teamService.GetMembersAsync(
                teamId,
                authenticatedUserId,
                cancellationToken);

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
        [HttpPost("{teamId}/invitations")]
        public async Task<IActionResult> CreateInvitation(Guid teamId, CreateTeamInvitationDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();
            var result = await _teamService.CreateInvitationAsync(teamId, dto, userId, cancellationToken);
            return result.Success ? Ok(result.Value) : MapErrorToResponse(result.Error!);
        }

        [Authorize]
        [HttpGet("{teamId}/invitations")]
        public async Task<IActionResult> GetInvitations(Guid teamId, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();
            var result = await _teamService.GetInvitationsAsync(teamId, userId, cancellationToken);
            return result.Success ? Ok(result.Value) : MapErrorToResponse(result.Error!);
        }

        [Authorize]
        [HttpDelete("{teamId}/invitations/{invitationId}")]
        public async Task<IActionResult> RevokeInvitation(Guid teamId, Guid invitationId, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();
            var result = await _teamService.RevokeInvitationAsync(teamId, invitationId, userId, cancellationToken);
            return result.Success ? NoContent() : MapErrorToResponse(result.Error!);
        }

        [Authorize]
        [HttpPost("invitations/{token}/accept")]
        public async Task<IActionResult> AcceptInvitation(string token, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();
            var result = await _teamService.AcceptInvitationAsync(token, userId, cancellationToken);
            return result.Success ? Ok(result.Value) : MapErrorToResponse(result.Error!);
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

            if (error == TeamErrors.InvalidInvitationEmail)
                return BadRequest(error.Message);
            if (error == TeamErrors.InvitationAlreadyPending || error == TeamErrors.InvitationEmailMismatch)
                return Conflict(error.Message);
            if (error == TeamErrors.InvitationNotFound)
                return NotFound(error.Message);
            if (error == TeamErrors.InvitationExpired)
                return StatusCode(StatusCodes.Status410Gone, error.Message);

            return StatusCode(500, error.Message);
        }

    }
}
