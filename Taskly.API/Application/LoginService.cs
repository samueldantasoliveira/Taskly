using Taskly.Application.DTOs;
using Taskly.Application.Results;
using Taskly.Domain.Entities;


namespace Taskly.Application;

public class LoginService
{
    private readonly IUserRepository _userRespository;
    private readonly ITokenService _tokenService;

    public LoginService(IUserRepository userService, ITokenService tokenService)
    {
        _userRespository = userService;
        _tokenService = tokenService;
    }
   
    public async Task<StructuredOperationResult<LoginResponseDto>> LoginAsync(
        string email, 
        string password)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var user = await _userRespository.GetByEmailAsync(normalizedEmail);
        if (user == null)
            return StructuredOperationResult<LoginResponseDto>.Fail(UserErrors.InvalidCredentials);
        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            return StructuredOperationResult<LoginResponseDto>.Fail(UserErrors.InvalidCredentials);

        var token = _tokenService.GenerateToken(user, out var expiresAt);
        return StructuredOperationResult<LoginResponseDto>.Ok(
            new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email
                }
            }
        );
    }
}