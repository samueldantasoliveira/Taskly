using Taskly.Domain.Entities;
using Taskly.Infrastructure;

namespace Taskly.Application
{
    public class UserService
    {
        private readonly IUserRepository _UserRepository;
        public UserService(IUserRepository repository) 
        {
            _UserRepository = repository;
        }

        public async Task AddUserAsync(User user)
        {
            if (String.IsNullOrEmpty(user.Name))
                throw new ArgumentException("Name must not be empty");

            await _UserRepository.AddAsync(user);
        }
        // outras coisas
    }
}
