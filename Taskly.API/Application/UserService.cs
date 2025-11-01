using Taskly.Application.DTOs;
using Taskly.Application.Results;
using Taskly.Domain.Entities;
using Taskly.Infrastructure;

namespace Taskly.Application
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository repository) 
        {
            _userRepository = repository;
        }

        public async Task<StructuredOperationResult<User>> AddUserAsync(CreateUserDto userDto)
        {
            if (String.IsNullOrWhiteSpace(userDto.Name))
                return StructuredOperationResult<User>.Fail(UserErrors.InvalidName);
            if (String.IsNullOrWhiteSpace(userDto.Password))
                return StructuredOperationResult<User>.Fail(UserErrors.InvalidPassword);

            var hash = PasswordHasher.HashPassword(userDto.Password);
            
            var user = new User(userDto.Name, hash);
            await _userRepository.AddAsync(user);
            
            return StructuredOperationResult<User>.Ok(user);
        }
        // outras coisas
    }
}
