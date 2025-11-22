using Taskly.Application.DTOs;
using Taskly.Application.Results;
using Taskly.Domain.Entities;

namespace Taskly.Application;

public class LoginService
{
    private readonly IUserRepository _userRepository;
    public LoginService(IUserRepository repository)
    {
        _userRepository = repository;
    }
    public async Task<StructuredOperationResult<User>> LoginAsync(string email, string password)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (user == null)
            return StructuredOperationResult<User>.Fail(UserErrors.NotFound);
        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            return StructuredOperationResult<User>.Fail(UserErrors.InvalidPassword);

        return StructuredOperationResult<User>.Ok(user);
    }
}