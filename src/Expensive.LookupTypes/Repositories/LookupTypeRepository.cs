using Dapper;
using Expensive.Common.Enums;
using Expensive.Infrastructure.Abstractions.Services;
using Expensive.Infrastructure.Exceptions;
using Expensive.LookupTypes.Abstractions.Repositories;
using Expensive.LookupTypes.Entities;
using Expensive.LookupTypes.Extensions;
using Expensive.LookupTypes.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Expensive.LookupTypes.Repositories;

public partial class LookupTypeRepository(
    IDbConnection connection,
    IDbTransaction transaction,
    ITimeProvider timeProvider,
    ILogger<LookupTypeRepository> logger) : ILookupTypeRepository
{
    public async Task<IEnumerable<LookupType>> GetAllLookupTypesAsync(LookupTypeCategory category, int page,
        int pageSize,
        string? searchString = null)
    {
        try
        {
            var offset = (page - 1) * pageSize;
            const string sqlWithoutSearch = """
                                                SELECT * FROM [dbo].[LookupTypes]
                                                WHERE Category = @Category
                                                ORDER BY Name
                                                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                                            """;
            const string sqlWithSearch = """
                                             SELECT * FROM [dbo].[LookupTypes]
                                             WHERE (Name LIKE '%' + @SearchString + '%' OR Code LIKE '%' + @SearchString + '%') AND Category = @Category
                                             ORDER BY Name
                                             OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                                         """;
            var sql = string.IsNullOrWhiteSpace(searchString) ? sqlWithoutSearch : sqlWithSearch;
            return await connection.QueryAsync<LookupType>(sql,
                new { SearchString = searchString, Category = category, Offset = offset, PageSize = pageSize },
                transaction);
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting lookup types", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting lookup types",
                innerException: ex);
        }
    }

    public async Task<int> GetLookupTypesCountAsync(LookupTypeCategory category, string? searchString = null)
    {
        try
        {
            const string sqlWithoutSearch = "SELECT COUNT(*) FROM [dbo].[LookupTypes] WHERE Category = @Category";
            const string sqlWithSearch =
                "SELECT COUNT(*) FROM [dbo].[LookupTypes] WHERE (Name LIKE '%' + @SearchString + '%' OR Code LIKE '%' + @SearchString + '%') AND Category = @Category";
            var sql = string.IsNullOrWhiteSpace(searchString) ? sqlWithoutSearch : sqlWithSearch;
            return await connection.ExecuteScalarAsync<int>(sql,
                new { SearchString = searchString, Category = category }, transaction);
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting lookup types count", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting lookup types count",
                innerException: ex);
        }
    }

    public async Task<LookupType> GetLookupTypeByIdAsync(Guid id)
    {
        try
        {
            const string sql = "SELECT * FROM [dbo].[LookupTypes] WHERE Id = @Id";
            var lookupType = await connection.QueryFirstOrDefaultAsync<LookupType>(sql, new { Id = id }, transaction);
            return lookupType ?? throw new EntityNotFoundException("Lookup Type not found");
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting lookup type by id", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting lookup type by id",
                innerException: ex);
        }
    }

    public async Task<Guid> CreateLookupTypeAsync(CreateLookupTypeModel createLookupTypeModel)
    {
        try
        {
            var lookupType = createLookupTypeModel.ToLookupType(timeProvider.Now);
            const string sql = """
                                   INSERT INTO [dbo].[LookupTypes] (Id, Category, Code, Name, CreatedByUserName, CreatedByUserId, CreatedOnDate)
                                   VALUES (@Id, @Category, @Code, @Name, @CreatedByUserName, @CreatedByUserId, @CreatedOnDate);
                               """;
            await connection.ExecuteAsync(sql, lookupType, transaction);
            return lookupType.Id;
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while creating lookup type", ex);
            transaction.Rollback();
            throw new DatabaseOperationFailedException("An error occurred while creating lookup type",
                innerException: ex);
        }
    }

    public async Task UpdateLookupTypeAsync(Guid id, UpdateLookupTypeModel updateLookupTypeModel)
    {
        try
        {
            var existingLookupType = await GetLookupTypeByIdAsync(id);
            if (existingLookupType.Category != updateLookupTypeModel.Category)
            {
                throw new InvalidOperationException("Category cannot be changed");
            }

            var lookupType = updateLookupTypeModel.ToLookupType(timeProvider.Now);
            lookupType.Id = id;
            const string updateSql = """
                                         UPDATE [dbo].[LookupTypes]
                                         SET Category = @Category,
                                             Code = @Code,
                                             Name = @Name,
                                             LastModifiedByUserName = @LastModifiedByUserName,
                                             LastModifiedByUserId = @LastModifiedByUserId,
                                             LastModifiedOnDate = @LastModifiedOnDate
                                         WHERE Id = @Id
                                     """;
            await connection.ExecuteAsync(updateSql, lookupType, transaction);
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while updating lookup type", ex);
            transaction.Rollback();
            throw new DatabaseOperationFailedException("An error occurred while updating lookup type",
                innerException: ex);
        }
    }

    public async Task DeleteLookupTypeAsync(Guid id)
    {
        try
        {
            const string sql = "DELETE FROM [dbo].[LookupTypes] WHERE Id = @Id";
            var recordsDeleted = await connection.ExecuteAsync(sql, new { Id = id }, transaction);
            if (recordsDeleted == 0)
            {
                throw new EntityNotFoundException("Designation not found");
            }
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while deleting lookup type", ex);
            transaction.Rollback();
            throw new DatabaseOperationFailedException("An error occurred while deleting lookup type",
                innerException: ex);
        }
    }

    [LoggerMessage(LogLevel.Error, Message = "Database operation failed: {Message}",
        EventName = "DatabaseOperationFailed")]
    private static partial void LogDatabaseOperationFailed(ILogger logger, string message, Exception ex);
}