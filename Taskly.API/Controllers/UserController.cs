using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
using Taskly.Application.Results;
using Taskly.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Taskly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserDto userDto, CancellationToken cancellationToken)
        {
            var result = await _userService.AddUserAsync(userDto, cancellationToken);
            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);
            }
            var userResponseDto = result.Value;
            return Ok(userResponseDto);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
                return Unauthorized();

            var user = await _userService.GetByIdAsync(
                authenticatedUserId,
                cancellationToken);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchByEmail(
            [FromQuery] string? email,
            CancellationToken cancellationToken)
        {
            var result = await _userService.SearchByEmailAsync(
                email,
                cancellationToken);

            if (!result.Success)
                return MapErrorToResponse(result.Error!);

            return Ok(result.Value);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
            return Unauthorized();

            if (authenticatedUserId != id)
                return Forbid();

            var deleted = await _userService.DeleteUserAsync(id, cancellationToken);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateUserDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
            return Unauthorized();

            if (authenticatedUserId != id)
                return Forbid();

            var result = await _userService.UpdateUserAsync(id, dto, cancellationToken);
            if(!result.Success)
                return MapErrorToResponse(result.Error!);
            
            return Ok(result.Value);
        }

        private bool TryGetAuthenticatedUserId(out Guid userId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
            return Guid.TryParse(claim, out userId);
        }

        private IActionResult MapErrorToResponse(Error error)
        {
            if (error == UserErrors.EmailAlreadyExists)
                return Conflict(error.Message);
            if (error == UserErrors.InvalidName)
                return BadRequest(error.Message);
            if (error == UserErrors.InvalidPassword)
                return BadRequest(error.Message);
            if (error == UserErrors.InvalidEmail)
                return BadRequest(error.Message);
            if (error == UserErrors.NotFound)
                return NotFound(error.Message);
                
            return StatusCode(500, error.Message);
        }
    }
}
