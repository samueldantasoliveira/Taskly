using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
using Taskly.Application.Results;
using Taskly.Domain.Entities;

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
        public async Task<IActionResult> Create(User user)
        {
            var result = await _userService.AddUserAsync(user);
            if (!result.Success)
            {
                return result.FailureReason switch
                {
                AddUserFailureReason.InvalidName => BadRequest("User name is invalid."),
                _ => StatusCode(500, "An unexpected error occurred.")
                };
            }
            return Ok(user);
        }
    }
}
