using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Personals.CodeSnippets.Abstractions.Repositories;
using Personals.CodeSnippets.Entities;
using Personals.CodeSnippets.Extensions;
using Personals.CodeSnippets.Models;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Exceptions;
using System.Data;

namespace Personals.CodeSnippets.Repositories;

public partial class CodeSnippetRepository(IDbConnection connection, IDbTransaction transaction, ICurrentUserService currentUserService, ITimeProvider timeProvider, ILogger<CodeSnippetRepository> logger) : ICodeSnippetRepository
{
    public async Task<IEnumerable<CodeSnippetModel>> GetAllCodeSnippetsAsync(int page, int pageSize, string? search = null)
    {
        try
        {
            var offset = (page - 1) * pageSize;
            const string sqlWithoutSearch = """
                SELECT * FROM [dbo].[CodeSnippets]
                WHERE UserId = @UserId
                ORDER BY CreatedOnDate DESC, Snippet ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;
            const string sqlWithSearch = """
                SELECT * FROM [dbo].[CodeSnippets]
                WHERE (Title LIKE '%' + @SearchString + '%' OR Snippet LIKE '%' + @SearchString + '%' OR Remarks LIKE '%' + @SearchString + '%' OR Tags LIKE '%' + @SearchString + '%') AND UserId = @UserId
                ORDER BY CreatedOnDate DESC, Snippet ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;
            var sql = string.IsNullOrWhiteSpace(search) ? sqlWithoutSearch : sqlWithSearch;
            var codeSnippets = await connection.QueryAsync<CodeSnippet>(sql, new { SearchString = search, currentUserService.UserId, Offset = offset, PageSize = pageSize }, transaction);
            return codeSnippets.Select(codeSnippet => codeSnippet.ToModel());
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting code snippets", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting code snippets", innerException: ex);
        }
    }

    public async Task<long> GetCodeSnippetsCountAsync(string? search = null)
    {
        try
        {
            const string sqlWithoutSearch = "SELECT COUNT(*) FROM [dbo].[CodeSnippets] WHERE UserId = @UserId";
            const string sqlWithSearch = """
                SELECT COUNT(*) FROM [dbo].[CodeSnippets] 
                WHERE (Title LIKE '%' + @SearchString + '%' OR Snippet LIKE '%' + @SearchString + '%' OR Remarks LIKE '%' + @SearchString + '%' OR Tags LIKE '%' + @SearchString + '%') 
                AND UserId = @UserId
            """;
            var sql = string.IsNullOrWhiteSpace(search) ? sqlWithoutSearch : sqlWithSearch;
            return await connection.ExecuteScalarAsync<long>(sql, new { SearchString = search, currentUserService.UserId }, transaction);
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting code snippets count", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting code snippets count", innerException: ex);
        }
    }

    public async Task<CodeSnippetModel> GetCodeSnippetByIdAsync(Guid id)
    {
        try
        {
            const string sql = "SELECT * FROM [dbo].[CodeSnippets] WHERE Id = @Id AND UserId = @UserId";
            var codeSnippet = await connection.QuerySingleOrDefaultAsync<CodeSnippet>(sql, new { Id = id, currentUserService.UserId }, transaction) 
                              ?? throw new EntityNotFoundException("Code Snippet not found");
            return codeSnippet.ToModel();
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting code snippet by id", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting code snippet by id", innerException: ex);
        }
    }

    public async Task<Guid> CreateCodeSnippetAsync(CreateCodeSnippetModel model)
    {
        try
        {
            var codeSnippet = model.ToCodeSnippet(currentUserService.UserId, timeProvider.Now);
            const string sql = """
                INSERT INTO [dbo].[CodeSnippets] (Id, Title, Snippet, Language, Remarks, Tags, UserId, CreatedOnDate)
                VALUES (@Id, @Title, @Snippet, @Language, @Remarks, @Tags, @UserId, @CreatedOnDate)
            """;
            await connection.ExecuteAsync(sql, codeSnippet, transaction);
            return codeSnippet.Id;
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while creating code snippet", ex);
            throw new DatabaseOperationFailedException("An error occurred while creating code snippet", innerException: ex);
        }
    }

    public async Task UpdateCodeSnippetAsync(Guid id, UpdateCodeSnippetModel model)
    {
        try
        {
            var codeSnippet = model.ToCodeSnippet(timeProvider.Now);
            codeSnippet.Id = id;
            codeSnippet.UserId = currentUserService.UserId;
            const string sql = """
                UPDATE [dbo].[CodeSnippets] 
                SET Title = @Title, Snippet = @Snippet, Language = @Language, Remarks = @Remarks, Tags = @Tags, LastModifiedOnDate = @LastModifiedOnDate
                WHERE Id = @Id AND UserId = @UserId
            """;
            var recordsUpdated = await connection.ExecuteAsync(sql, codeSnippet, transaction);
            if (recordsUpdated == 0)
            {
                throw new EntityNotFoundException("Code Snippet not found");
            }
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while updating code snippet", ex);
            throw new DatabaseOperationFailedException("An error occurred while updating code snippet", innerException: ex);
        }
    }

    public async Task DeleteCodeSnippetAsync(Guid id)
    {
        try
        {
            const string sql = "DELETE FROM [dbo].[CodeSnippets] WHERE Id = @Id";
            var recordsDeleted = await connection.ExecuteAsync(sql, new { Id = id, currentUserService.UserId }, transaction);
            if (recordsDeleted == 0)
            {
                throw new EntityNotFoundException("Code Snippet not found");
            }
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while deleting code snippet", ex);
            throw new DatabaseOperationFailedException("An error occurred while deleting code snippet", innerException: ex);
        }
    }

    [LoggerMessage(LogLevel.Error, Message = "Database operation failed: {Message}",
        EventName = "DatabaseOperationFailed")]
    private static partial void LogDatabaseOperationFailed(ILogger logger, string message, Exception ex);
}