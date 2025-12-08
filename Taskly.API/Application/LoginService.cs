using Taskly.Application.DTOs;
using Taskly.Application.Results;
using Taskly.Domain.Entities;


namespace Taskly.Application;

public class LoginService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public LoginService(IUserRepository repository, ITokenService tokenService)
    {
        _userRepository = repository;
        _tokenService = tokenService;
    }
    public async Task<StructuredOperationResult<(User user, string token, DateTime expiresAt)>> LoginAsync(
        string email, 
        string password)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (user == null)
            return StructuredOperationResult<(User, string, DateTime)>.Fail(UserErrors.NotFound);
        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            return StructuredOperationResult<(User, string, DateTime)>.Fail(UserErrors.InvalidPassword);

        var token = _tokenService.GenerateToken(user, out var expiresAt);

        return StructuredOperationResult<(User, string, DateTime)>.Ok((user, token, expiresAt));
    }
}