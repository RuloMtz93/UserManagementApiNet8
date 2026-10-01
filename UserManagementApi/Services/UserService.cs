using Microsoft.Data.SqlClient;
using UserManagementApi.DTOs;
using UserManagementApi.Models;
using UserManagementApi.Repositories;

namespace UserManagementApi.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
    {
        // 1. Regla de seguridad: Hashear la contraseña con salt automático usando BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        try
        {
            // 2. Invocar al repositorio
            var user = await _userRepository.CreateAsync(dto.UserName, passwordHash, dto.Email);
            
            if (user == null)
            {
                throw new InvalidOperationException("No se pudo completar el registro del usuario.");
            }

            return MapToResponseDto(user);
        }
        catch (SqlException ex) when (ex.Number == 50001 || ex.Number == 50002)
        {
            // Propagar el mensaje de negocio de duplicados
            throw new ArgumentException(ex.Message);
        }
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user is null ? null : MapToResponseDto(user);
    }

    public async Task<IEnumerable<UserResponseDto>> GetActiveUsersAsync()
    {
        var users = await _userRepository.GetActiveAsync();
        return users.Select(MapToResponseDto);
    }

    public async Task<UserResponseDto?> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        try
        {
            var user = await _userRepository.UpdateAsync(id, dto.Email, dto.Status);
            return user is null ? null : MapToResponseDto(user);
        }
        catch (SqlException ex) when (ex.Number == 50002 || ex.Number == 50004)
        {
            throw new ArgumentException(ex.Message);
        }
        catch (SqlException ex) when (ex.Number == 50003)
        {
            return null; // Usuario no encontrado
        }
    }

    public async Task<UserResponseDto?> DeactivateUserAsync(Guid id)
    {
        try
        {
            var user = await _userRepository.DeactivateAsync(id);
            return user is null ? null : MapToResponseDto(user);
        }
        catch (SqlException ex) when (ex.Number == 50003)
        {
            return null; // Usuario no encontrado
        }
    }

    private static UserResponseDto MapToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Status = user.Status,
            CreatedDate = user.CreatedDate,
            UpdatedDate = user.UpdatedDate
        };
    }
}