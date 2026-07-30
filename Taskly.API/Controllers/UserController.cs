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
        public async Task<IActionResult> Create(CreateUserDto userDto)
        {
            var result = await _userService.AddUserAsync(userDto);
            if (!result.Success)
            {
                return MapErrorToResponse(result.Error!);
            }
            var userResponseDto = result.Value;
            return Ok(userResponseDto);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
            return Unauthorized();

            if (authenticatedUserId != id)
                return Forbid();

            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateUserDto dto)
        {
            if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
            return Unauthorized();

            if (authenticatedUserId != id)
                return Forbid();

            var result = await _userService.UpdateUserAsync(id, dto);
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
            if (error == UserErrors.NotFound)
                return NotFound(error.Message);
                
            return StatusCode(500, error.Message);
        }
    }
}
