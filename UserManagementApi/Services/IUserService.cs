using UserManagementApi.DTOs;

namespace UserManagementApi.Services;

public interface IUserService
{
    Task<UserResponseDto> CreateUserAsync(CreateUserDto dto);
    Task<UserResponseDto> GetUserByIdAsync(Guid id);
    Task<IEnumerable<UserResponseDto>> GetActiveUsersAsync();
    Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task<UserResponseDto> DeactivateUserAsync(Guid id);
}