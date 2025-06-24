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

        public async Task<OperationResult<AddUserFailureReason>> AddUserAsync(User user)
        {
            if (String.IsNullOrWhiteSpace(user.Name))
                return OperationResult<AddUserFailureReason>.Fail(AddUserFailureReason.InvalidName);

            await _userRepository.AddAsync(user);
            return OperationResult<AddUserFailureReason>.Ok();
        }
        // outras coisas
    }
}
