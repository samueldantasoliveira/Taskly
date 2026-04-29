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

        public async Task<StructuredOperationResult<User>> AddUserAsync(CreateUserDto userDto)
        {
            if (await _userRepository.ExistsByEmailAsync(userDto.Email))
                return StructuredOperationResult<User>.Fail(UserErrors.EmailAlreadyExists);

            var hash = PasswordHasher.HashPassword(userDto.Password);
            
            var user = new User(userDto.Name, userDto.Email, hash);
            await _userRepository.AddAsync(user);
            
            return StructuredOperationResult<User>.Ok(user);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<StructuredOperationResult<User>> UpdateUserAsync(Guid id, UpdateUserDto userDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return StructuredOperationResult<User>.Fail(UserErrors.NotFound);

            if (userDto.Email != null && user.Email != userDto.Email)
                if (await _userRepository.ExistsByEmailAsync(userDto.Email))
                    return StructuredOperationResult<User>.Fail(UserErrors.EmailAlreadyExists);
            
            string? passwordHash = null;
            if (userDto.Password != null)
                passwordHash = PasswordHasher.HashPassword(userDto.Password);
            user.Update(userDto.Name, userDto.Email, passwordHash);

            var updated = await _userRepository.UpdateAsync(user);

            if (!updated)
                return StructuredOperationResult<User>.Fail(UserErrors.NotFound);
                
            return StructuredOperationResult<User>.Ok(user);
        }
    }
}
