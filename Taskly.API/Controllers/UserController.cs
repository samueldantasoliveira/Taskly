using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
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
            await _userService.AddUserAsync(user);
            return Ok(user);
        }
    }
}
