using Rivulus.Application.DTOs;
using Rivulus.Application.Results;
using Rivulus.Domain.Entities;


namespace Rivulus.Application;

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
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password) || password.Length > 128)
            return StructuredOperationResult<LoginResponseDto>.Fail(UserErrors.InvalidCredentials);
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRespository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user == null)
            return StructuredOperationResult<LoginResponseDto>.Fail(UserErrors.InvalidCredentials);
        cancellationToken.ThrowIfCancellationRequested();
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
                    Version = user.Version,
                    Name = user.Name,
                    Email = user.Email,
                    AvatarKey = user.AvatarKey
                }
            }
        );
    }
}
