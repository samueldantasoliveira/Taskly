using Taskly.Application.DTOs;
using Taskly.Domain.Entities;
using Taskly.Infrastructure;

namespace Taskly.Application
{
    public interface IUserService
    {
        public Task<StructuredOperationResult<User>> AddUserAsync(CreateUserDto userDto);

        public Task<bool> DeleteUserAsync(Guid id);

        public Task<User?> GetByEmailAsync(string email);

        public Task<User?> GetByIdAsync(Guid id);

        public Task<StructuredOperationResult<User>> UpdateUserAsync(Guid id, UpdateUserDto userDto);
    }
}
