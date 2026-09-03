using Taskly.Application.DTOs;
using Taskly.Application.Results;
using Taskly.Domain.Entities;
using Taskly.Infrastructure;

namespace Taskly.Application
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository repository)
        {
            _userRepository = repository;
        }

        public async Task<StructuredOperationResult<UserResponseDto>> AddUserAsync(CreateUserDto userDto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userDto.Password))
                return StructuredOperationResult<UserResponseDto>.Fail(UserErrors.InvalidPassword);
            cancellationToken.ThrowIfCancellationRequested();
            var hash = PasswordHasher.HashPassword(userDto.Password);
            var user = new User(userDto.Name, userDto.Email, hash);

            if (await _userRepository.ExistsByEmailAsync(user.Email, cancellationToken))
                return StructuredOperationResult<UserResponseDto>.Fail(UserErrors.EmailAlreadyExists);


            await _userRepository.AddAsync(user, cancellationToken);

            var userResponse = new UserResponseDto{
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
            return StructuredOperationResult<UserResponseDto>.Ok(userResponse);
        }

        public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _userRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<UserResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = email.ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            if( user == null )
                return null;

            var userResponseDto = new UserResponseDto{
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };

            return userResponseDto;
        }

        public async Task<StructuredOperationResult<UserResponseDto>> SearchByEmailAsync(
            string? email,
            CancellationToken cancellationToken = default)
        {
            var normalizedEmail = email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalizedEmail) ||
                !User.IsValidEmail(normalizedEmail))
            {
                return StructuredOperationResult<UserResponseDto>.Fail(
                    UserErrors.InvalidEmail);
            }

            var user = await _userRepository.GetByEmailAsync(
                normalizedEmail,
                cancellationToken);

            if (user == null)
                return StructuredOperationResult<UserResponseDto>.Fail(UserErrors.NotFound);

            return StructuredOperationResult<UserResponseDto>.Ok(new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            });
        }

        public async Task<UserResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if( user == null )
                return null;

            var userResponseDto = new UserResponseDto{
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };

            return userResponseDto;
        }

        public async Task<StructuredOperationResult<UserResponseDto>> UpdateUserAsync(Guid id, UpdateUserDto userDto, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = userDto.Email?.ToLowerInvariant();
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
                return StructuredOperationResult<UserResponseDto>.Fail(UserErrors.NotFound);

            if (normalizedEmail != null && user.Email != normalizedEmail)
            {
                if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
                    return StructuredOperationResult<UserResponseDto>.Fail(UserErrors.EmailAlreadyExists);
            }

            string? passwordHash = null;
            if (userDto.Password != null)
            {
                if (string.IsNullOrWhiteSpace(userDto.Password))
                    return StructuredOperationResult<UserResponseDto>.Fail(UserErrors.InvalidPassword);
                cancellationToken.ThrowIfCancellationRequested();
                passwordHash = PasswordHasher.HashPassword(userDto.Password);
            }

            user.Update(userDto.Name, normalizedEmail, passwordHash);

            var updated = await _userRepository.UpdateAsync(user, cancellationToken);

            if (!updated)
                return StructuredOperationResult<UserResponseDto>.Fail(UserErrors.NotFound);

            var userResponse = new UserResponseDto{
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
            return StructuredOperationResult<UserResponseDto>.Ok(userResponse);
        }
    }
}
