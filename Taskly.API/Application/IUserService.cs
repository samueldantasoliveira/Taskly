using Taskly.Application.DTOs;
using Taskly.Domain.Entities;
using Taskly.Infrastructure;

namespace Taskly.Application
{
    public interface IUserService
    {
        public Task<StructuredOperationResult<UserResponseDto>> AddUserAsync(CreateUserDto userDto);

        public Task<bool> DeleteUserAsync(Guid id);

        public Task<UserResponseDto?> GetByEmailAsync(string email);

        public Task<UserResponseDto?> GetByIdAsync(Guid id);

        public Task<StructuredOperationResult<UserResponseDto>> UpdateUserAsync(Guid id, UpdateUserDto userDto);
    }
}
