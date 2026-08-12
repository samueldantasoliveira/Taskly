using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
using Taskly.Application.DTOs;
using Taskly.Application.Results;

namespace Taskly.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly LoginService _loginService;
    public LoginController(LoginService loginService)
    {
        _loginService = loginService;
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var result = await _loginService.LoginAsync(loginDto.Email, loginDto.Password);
        if (!result.Success)
        {
            return MapErrorToResponse(result.Error!);
        }
        return Ok(result.Value);
    }   
    private IActionResult MapErrorToResponse(Error error)
    {
        if (error == UserErrors.InvalidCredentials)
            return Unauthorized(error.Message);
        
        return StatusCode(500, error.Message);
    }

}