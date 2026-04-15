using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
using Taskly.Application.Results;
using Taskly.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Taskly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
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
            var user = result.Value;
            return Ok(user);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
        private IActionResult MapErrorToResponse(Error error)
        {
            return error.Code switch
                {
                    "InvalidName" => BadRequest(error.Message),
                    _ => StatusCode(500, error.Message)
                };
        }
    }
}
