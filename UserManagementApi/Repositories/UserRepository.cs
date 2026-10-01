using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using UserManagementApi.Models;

namespace UserManagementApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no fue encontrada.");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<User?> CreateAsync(string userName, string passwordHash, string email)
    {
        using var connection = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UserName", userName, DbType.String, ParameterDirection.Input, 50);
        parameters.Add("@PasswordHash", passwordHash, DbType.String, ParameterDirection.Input, 500);
        parameters.Add("@Email", email, DbType.String, ParameterDirection.Input, 150);

        return await connection.QuerySingleOrDefaultAsync<User>(
            "dbo.sp_User_Create",
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var connection = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id, DbType.Guid);

        return await connection.QuerySingleOrDefaultAsync<User>(
            "dbo.sp_User_GetById",
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<User>> GetActiveAsync()
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<User>(
            "dbo.sp_User_GetActive",
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<User?> UpdateAsync(Guid id, string email, byte status)
    {
        using var connection = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id, DbType.Guid);
        parameters.Add("@Email", email, DbType.String, ParameterDirection.Input, 150);
        parameters.Add("@Status", status, DbType.Byte);

        return await connection.QuerySingleOrDefaultAsync<User>(
            "dbo.sp_User_Update",
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<User?> DeactivateAsync(Guid id)
    {
        using var connection = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id, DbType.Guid);

        return await connection.QuerySingleOrDefaultAsync<User>(
            "dbo.sp_User_Deactivate",
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }
}