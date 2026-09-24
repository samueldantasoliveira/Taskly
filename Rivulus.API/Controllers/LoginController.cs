using Microsoft.AspNetCore.Mvc;
using Rivulus.Application;
using Rivulus.Application.DTOs;
using Rivulus.Application.Results;

namespace Rivulus.Controllers;

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
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginDto loginDto, CancellationToken cancellationToken)
    {
        var result = await _loginService.LoginAsync(
            loginDto.Email,
            loginDto.Password,
            cancellationToken);
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
