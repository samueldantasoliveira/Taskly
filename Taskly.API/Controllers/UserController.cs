using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
using Taskly.Application.Results;
using Taskly.Application.DTOs;

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
                return result.FailureReason switch
                {
                    AddUserFailureReason.InvalidName => BadRequest("User name is invalid."),
                    _ => StatusCode(500, "An unexpected error occurred.")
                };
            }
            var user = result.Value!;
            return Ok(user);
        }
    }
}
