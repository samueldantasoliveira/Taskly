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

        public async Task<OperationResult<AddUserFailureReason, User>> AddUserAsync(CreateUserDto userDto)
        {

            if (String.IsNullOrWhiteSpace(userDto.Name))
                return OperationResult<AddUserFailureReason, User>.Fail(AddUserFailureReason.InvalidName);

            var user = new User(userDto.Name);
            await _userRepository.AddAsync(user);
            return OperationResult<AddUserFailureReason, User>.Ok(user);
        }
        // outras coisas
    }
}
