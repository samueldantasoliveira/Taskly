using Microsoft.AspNetCore.Mvc;
using Taskly.Application;
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
    public async Task<IActionResult> Login(string email, string password)
    {
        var result = await _loginService.LoginAsync(email, password);
        if (!result.Success)
        {
            return MapErrorToResponse(result.Error!);
        }
            return Ok(new
        {
            message = "Login successful.",
            token = result.Value.token,
            expiresAt = result.Value.expiresAt,
            user = new
            {
                result.Value.user.Id,
                result.Value.user.Name,
                result.Value.user.Email
            }
        });
    }   
    private IActionResult MapErrorToResponse(Error error)
    {
        return error.Code switch
        {
            "User.NotFound" => NotFound(error.Message),
            "User.InvalidPassword" => Unauthorized(error.Message),
            _ => StatusCode(500, "Unexpected error")

        };

    }

}