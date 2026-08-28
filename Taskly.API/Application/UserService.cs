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

        public async Task<StructuredOperationResult<UserResponseDto>> AddUserAsync(CreateUserDto userDto)
        {
            var hash = PasswordHasher.HashPassword(userDto.Password);
            var user = new User(userDto.Name, userDto.Email, hash);

            if (await _userRepository.ExistsByEmailAsync(user.Email))
                return StructuredOperationResult<UserResponseDto>.Fail(UserErrors.EmailAlreadyExists);


            await _userRepository.AddAsync(user);

            var userResponse = new UserResponseDto{
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
            return StructuredOperationResult<UserResponseDto>.Ok(userResponse);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        public async Task<UserResponseDto?> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(normalizedEmail);
            if( user == null )
                return null;

            var userResponseDto = new UserResponseDto{
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };

            return userResponseDto;
        }

        public async Task<UserResponseDto?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if( user == null )
                return null;

            var userResponseDto = new UserResponseDto{
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };

            return userResponseDto;
        }

        public async Task<StructuredOperationResult<UserResponseDto>> UpdateUserAsync(Guid id, UpdateUserDto userDto)
        {
            var normalizedEmail = userDto.Email?.ToLowerInvariant();
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return StructuredOperationResult<UserResponseDto>.Fail(UserErrors.NotFound);

            if (normalizedEmail != null && user.Email != normalizedEmail)
            {
                if (await _userRepository.ExistsByEmailAsync(normalizedEmail))
                    return StructuredOperationResult<UserResponseDto>.Fail(UserErrors.EmailAlreadyExists);
            }

            string? passwordHash = null;
            if (userDto.Password != null)
                passwordHash = PasswordHasher.HashPassword(userDto.Password);
            user.Update(userDto.Name, normalizedEmail, passwordHash);

            var updated = await _userRepository.UpdateAsync(user);

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
