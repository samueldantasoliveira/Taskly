using Rivulus.Application.DTOs;
using Rivulus.Domain.Entities;
using Rivulus.Infrastructure;

namespace Rivulus.Application
{
    public interface IUserService
    {
        public Task<StructuredOperationResult<UserResponseDto>> AddUserAsync(CreateUserDto userDto, CancellationToken cancellationToken = default);

        public Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);

        public Task<UserResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        public Task<StructuredOperationResult<UserResponseDto>> SearchByEmailAsync(string? email, CancellationToken cancellationToken = default);

        public Task<UserResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        public Task<StructuredOperationResult<UserResponseDto>> UpdateUserAsync(Guid id, UpdateUserDto userDto, CancellationToken cancellationToken = default);
    }
}
