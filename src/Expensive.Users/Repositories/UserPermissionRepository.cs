using Dapper;
using Expensive.Infrastructure.Exceptions;
using Expensive.Users.Abstractions.Repositories;
using Expensive.Users.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Expensive.Users.Repositories;

public partial class UserPermissionRepository(
    IDbConnection connection,
    IDbTransaction transaction,
    ILogger<UserPermissionRepository> logger) : IUserPermissionRepository
{
    public async Task<List<AppUserPermission>> GetAllPermissionsAsync(Guid appUserId)
    {
        try
        {
            const string sql = "SELECT * FROM [dbo].[AppUserPermissions] WHERE [AppUserId] = @AppUserId";
            return (await connection.QueryAsync<AppUserPermission>(sql, new { AppUserId = appUserId }, transaction))
                .ToList();
        }
        catch (SqlException ex)
        {
            LogGetPermissionsError(logger, ex);
            throw new DatabaseOperationFailedException("An error occurred while getting user permissions", ex);
        }
    }

    public async Task UpdatePermissionsAsync(Guid appUserId, IEnumerable<AppUserPermission> permissions)
    {
        try
        {
            const string deleteSql = "DELETE FROM [dbo].[AppUserPermissions] WHERE [AppUserId] = @AppUserId";
            await connection.ExecuteAsync(deleteSql, new { AppUserId = appUserId }, transaction);

            const string insertSql =
                "INSERT INTO [dbo].[AppUserPermissions] ([AppUserId], [Permission]) VALUES (@AppUserId, @Permission)";
            foreach (var permission in permissions)
            {
                await connection.ExecuteAsync(insertSql, new { AppUserId = appUserId, permission.Permission },
                    transaction);
            }
        }
        catch (SqlException ex)
        {
            LogUpdatePermissionsError(logger, ex);
            transaction.Rollback();
            throw new DatabaseOperationFailedException("An error occurred while updating user permissions", ex);
        }
    }

    [LoggerMessage(LogLevel.Error, Message = "An error occurred while getting user permissions")]
    private static partial void LogGetPermissionsError(ILogger logger, Exception ex);

    [LoggerMessage(LogLevel.Error, Message = "An error occurred while updating user permissions")]
    private static partial void LogUpdatePermissionsError(ILogger logger, Exception ex);
}