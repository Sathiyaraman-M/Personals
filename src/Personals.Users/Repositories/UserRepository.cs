using Dapper;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Exceptions;
using Personals.Users.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Personals.Users.Abstractions.Repositories;
using Personals.Users.Entities;
using Personals.Users.Models;
using System.Data;

namespace Personals.Users.Repositories;

public partial class UserRepository(
    IDbConnection connection,
    IDbTransaction transaction,
    ITimeProvider timeProvider,
    ILogger<UserRepository> logger) : IUserRepository
{
    public async Task<IEnumerable<AppUser>> GetAllAsync(int page, int pageSize, string? search = null)
    {
        try
        {
            var offset = (page - 1) * pageSize;
            const string sqlWithoutSearch = """
                                                SELECT * FROM [dbo].[AppUsers]
                                                ORDER BY FullName
                                                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                                            """;
            const string sqlWithSearch = """
                                             SELECT * FROM [dbo].[AppUsers]
                                             WHERE FullName LIKE '%' + @SearchString + '%' OR LoginName LIKE '%' + @SearchString + '%' OR EmailAddress LIKE '%' + @SearchString + '%' OR PhoneNumber LIKE '%' + @SearchString + '%' OR Address1 LIKE '%' + @SearchString + '%' OR Address2 LIKE '%' + @SearchString + '%' OR City LIKE '%' + @SearchString + '%' OR PostCode LIKE '%' + @SearchString + '%' OR Code LIKE '%' + @SearchString + '%'
                                             ORDER BY FullName
                                             OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                                         """;
            var sql = string.IsNullOrWhiteSpace(search) ? sqlWithoutSearch : sqlWithSearch;
            return await connection.QueryAsync<AppUser>(sql,
                new { SearchString = search, Offset = offset, PageSize = pageSize }, transaction);
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting users", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting users", innerException: ex);
        }
    }

    public async Task<int> GetCountAsync(string? search = null)
    {
        try
        {
            const string sqlWithoutSearch = "SELECT COUNT(*) FROM [dbo].[AppUsers]";
            const string sqlWithSearch = """
                                             SELECT COUNT(*) FROM [dbo].[AppUsers]
                                             WHERE FullName LIKE '%' + @SearchString + '%' OR LoginName LIKE '%' + @SearchString + '%' OR EmailAddress LIKE '%' + @SearchString + '%' OR PhoneNumber LIKE '%' + @SearchString + '%' OR Address1 LIKE '%' + @SearchString + '%' OR Address2 LIKE '%' + @SearchString + '%' OR City LIKE '%' + @SearchString + '%' OR PostCode LIKE '%' + @SearchString + '%' OR Code LIKE '%' + @SearchString + '%'
                                         """;
            var sql = string.IsNullOrWhiteSpace(search) ? sqlWithoutSearch : sqlWithSearch;
            return await connection.ExecuteScalarAsync<int>(sql, new { SearchString = search }, transaction);
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting users count", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting users count",
                innerException: ex);
        }
    }

    public async Task<bool> IsEmailExistsAsync(string emailAddress, List<Guid>? excludeIds = null)
    {
        try
        {
            var excludeIdsSql = excludeIds != null && excludeIds.Count != 0 ? " AND Id NOT IN @ExcludeIds" : "";
            var sql = "SELECT COUNT(*) FROM [dbo].[AppUsers] WHERE EmailAddress = @EmailAddress";
            sql += excludeIdsSql;
            var count = await connection.ExecuteScalarAsync<int>(sql,
                new { EmailAddress = emailAddress, ExcludeIds = excludeIds ?? [] }, transaction);
            return count > 0;
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while checking if email exists", ex);
            throw new DatabaseOperationFailedException("An error occurred while checking if email exists",
                innerException: ex);
        }
    }

    public async Task<bool> IsLoginNameExistsAsync(string loginName, List<Guid>? excludeIds = null)
    {
        try
        {
            var excludeIdsSql = excludeIds != null && excludeIds.Count != 0 ? " AND Id NOT IN @ExcludeIds" : "";
            var sql = "SELECT COUNT(*) FROM [dbo].[AppUsers] WHERE LoginName = @LoginName";
            sql += excludeIdsSql;
            var count = await connection.ExecuteScalarAsync<int>(sql,
                new { LoginName = loginName, ExcludeIds = excludeIds ?? [] }, transaction);
            return count > 0;
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while checking if login name exists", ex);
            throw new DatabaseOperationFailedException("An error occurred while checking if login name exists",
                innerException: ex);
        }
    }

    public async Task<bool> IsPhoneNumberExistsAsync(string phoneNumber, List<Guid>? excludeIds = null)
    {
        try
        {
            var excludeIdsSql = excludeIds != null && excludeIds.Count != 0 ? " AND Id NOT IN @ExcludeIds" : "";
            var sql = "SELECT COUNT(*) FROM [dbo].[AppUsers] WHERE PhoneNumber = @PhoneNumber";
            sql += excludeIdsSql;
            var count = await connection.ExecuteScalarAsync<int>(sql,
                new { PhoneNumber = phoneNumber, ExcludeIds = excludeIds ?? [] }, transaction);
            return count > 0;
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while checking if phone number exists", ex);
            throw new DatabaseOperationFailedException("An error occurred while checking if phone number exists",
                innerException: ex);
        }
    }

    public async Task<bool> IsCodeExistsAsync(string code, List<Guid>? excludeIds = null)
    {
        try
        {
            var excludeIdsSql = excludeIds != null && excludeIds.Count != 0 ? " AND Id NOT IN @ExcludeIds" : "";
            var sql = "SELECT COUNT(*) FROM [dbo].[AppUsers] WHERE Code = @Code";
            sql += excludeIdsSql;
            var count = await connection.ExecuteScalarAsync<int>(sql,
                new { Code = code, ExcludeIds = excludeIds ?? [] }, transaction);
            return count > 0;
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while checking if code exists", ex);
            throw new DatabaseOperationFailedException("An error occurred while checking if code exists",
                innerException: ex);
        }
    }

    public async Task<AppUser> GetByIdAsync(Guid id)
    {
        try
        {
            const string sql = "SELECT * FROM [dbo].[AppUsers] WHERE Id = @Id";
            var appUser = await connection.QueryFirstOrDefaultAsync<AppUser>(sql, new { Id = id }, transaction);
            return appUser ?? throw new EntityNotFoundException("User not found");
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting user by id", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting user by id",
                innerException: ex);
        }
    }

    public async Task<AppUser> GetByLoginNameAsync(string loginName)
    {
        try
        {
            const string sql = "SELECT * FROM [dbo].[AppUsers] WHERE LoginName = @LoginName";
            var appUser =
                await connection.QueryFirstOrDefaultAsync<AppUser>(sql, new { LoginName = loginName }, transaction);
            return appUser ?? throw new EntityNotFoundException("User not found");
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting user by login name", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting user by login name",
                innerException: ex);
        }
    }

    public async Task<Guid> CreateAsync(CreateAppUserModel model)
    {
        try
        {
            var appUser = model.ToAppUser(timeProvider.Now);
            const string sql = """
                                   INSERT INTO [dbo].[AppUsers] (Id, Code, LoginName, FullName, Address1, Address2, City, PostCode, StateCode, EmailAddress, PhoneNumber, PasswordHash, IsActive, CreatedOnDate, CreatedByUserName, CreatedByUserId)
                                   VALUES (@Id, @Code, @LoginName, @FullName, @Address1, @Address2, @City, @PostCode, @StateCode, @EmailAddress, @PhoneNumber, @PasswordHash, @IsActive, @CreatedOnDate, @CreatedByUserName, @CreatedByUserId)
                               """;
            await connection.ExecuteAsync(sql, appUser, transaction);
            return appUser.Id;
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while creating user", ex);
            transaction.Rollback();
            throw new DatabaseOperationFailedException("An error occurred while creating user", innerException: ex);
        }
    }

    public async Task UpdateAsync(Guid id, UpdateAppUserModel model)
    {
        try
        {
            var appUser = model.ToAppUser(timeProvider.Now);
            appUser.Id = id;
            const string sql = """
                                   UPDATE [dbo].[AppUsers]
                                   SET Code = @Code, LoginName = @LoginName, FullName = @FullName, Address1 = @Address1, Address2 = @Address2, City = @City, PostCode = @PostCode, StateCode = @StateCode, EmailAddress = @EmailAddress, PhoneNumber = @PhoneNumber, IsActive = @IsActive, LastModifiedOnDate = @LastModifiedOnDate, LastModifiedByUserName = @LastModifiedByUserName, LastModifiedByUserId = @LastModifiedByUserId
                                   WHERE Id = @Id
                               """;
            var rowsAffected = await connection.ExecuteAsync(sql, appUser, transaction);
            if (rowsAffected == 0)
            {
                throw new EntityNotFoundException("User not found");
            }
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while updating user", ex);
            transaction.Rollback();
            throw new DatabaseOperationFailedException("An error occurred while updating user", innerException: ex);
        }
    }

    public async Task UpdateRefreshTokenAsync(Guid id, string refreshToken, DateTime refreshTokenExpiryTime)
    {
        try
        {
            const string sql = """
                                   UPDATE [dbo].[AppUsers]
                                   SET RefreshToken = @RefreshToken, RefreshTokenExpiryTime = @RefreshTokenExpiryTime
                                   WHERE Id = @Id
                               """;
            var rowsUpdated = await connection.ExecuteAsync(sql,
                new { RefreshToken = refreshToken, RefreshTokenExpiryTime = refreshTokenExpiryTime, Id = id },
                transaction);
            if (rowsUpdated == 0)
            {
                throw new EntityNotFoundException("User not found");
            }
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while updating refresh token", ex);
            transaction.Rollback();
            throw new DatabaseOperationFailedException("An error occurred while updating refresh token",
                innerException: ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            const string sql = "DELETE FROM [dbo].[AppUsers] WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id }, transaction);
            if (rowsAffected == 0)
            {
                throw new EntityNotFoundException("User not found");
            }
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while deleting user", ex);
            transaction.Rollback();
            throw new DatabaseOperationFailedException("An error occurred while deleting user", innerException: ex);
        }
    }

    [LoggerMessage(LogLevel.Error, Message = "Database operation failed: {Message}",
        EventName = "DatabaseOperationFailed")]
    private static partial void LogDatabaseOperationFailed(ILogger logger, string message, Exception ex);
}