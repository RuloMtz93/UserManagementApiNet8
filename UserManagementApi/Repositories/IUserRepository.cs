using UserManagementApi.Models;

namespace UserManagementApi.Repositories;

public interface IUserRepository
{
    Task<User?> CreateAsync(string userName, string passwordHash, string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetActiveAsync();
    Task<User?> UpdateAsync(Guid id, string email, byte status);
    Task<User?> DeactivateAsync(Guid id);
}