using Microsoft.Data.SqlClient;
using UserManagementApi.DTOs;
using UserManagementApi.Exceptions;
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
        // Hasheo de contraseña
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        try
        {
            var user = await _userRepository.CreateAsync(dto.UserName, passwordHash, dto.Email);
            return MapToResponseDto(user!);
        }
        catch (SqlException ex) when (ex.Number == 50001 || ex.Number == 50002)
        {
            throw new ConflictException(ex.Message); // Provoca HTTP 409
        }
    }

    public async Task<UserResponseDto> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            throw new NotFoundException($"El usuario con ID {id} no fue encontrado."); // Provoca HTTP 404
        }
        return MapToResponseDto(user);
    }

    public async Task<IEnumerable<UserResponseDto>> GetActiveUsersAsync()
    {
        var users = await _userRepository.GetActiveAsync();
        return users.Select(MapToResponseDto);
    }

    public async Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        try
        {
            var user = await _userRepository.UpdateAsync(id, dto.Email, dto.Status);
            if (user is null)
            {
                throw new NotFoundException($"El usuario con ID {id} no fue encontrado."); // Provoca HTTP 404
            }
            return MapToResponseDto(user);
        }
        catch (SqlException ex) when (ex.Number == 50002)
        {
            throw new ConflictException(ex.Message); // Provoca HTTP 409
        }
        catch (SqlException ex) when (ex.Number == 50003)
        {
            throw new NotFoundException(ex.Message); // Provoca HTTP 404
        }
        catch (SqlException ex) when (ex.Number == 50004)
        {
            throw new ArgumentException(ex.Message); // Provoca HTTP 400
        }
    }

    public async Task<UserResponseDto> DeactivateUserAsync(Guid id)
    {
        try
        {
            var user = await _userRepository.DeactivateAsync(id);
            if (user is null)
            {
                throw new NotFoundException($"El usuario con ID {id} no fue encontrado."); // Provoca HTTP 404
            }
            return MapToResponseDto(user);
        }
        catch (SqlException ex) when (ex.Number == 50003)
        {
            throw new NotFoundException(ex.Message); // Provoca HTTP 404
        }
    }

    private static UserResponseDto MapToResponseDto(User user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        Status = user.Status,
        CreatedDate = user.CreatedDate,
        UpdatedDate = user.UpdatedDate
    };
}