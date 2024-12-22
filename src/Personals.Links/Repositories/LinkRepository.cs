using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Exceptions;
using Personals.Links.Abstractions.Repositories;
using Personals.Links.Entities;
using Personals.Links.Extensions;
using Personals.Links.Models;
using System.Data;

namespace Personals.Links.Repositories;

public partial class LinkRepository(IDbConnection connection, IDbTransaction transaction, ICurrentUserService currentUserService, ITimeProvider provider, ILogger<LinkRepository> logger) : ILinkRepository
{
    public async Task<IEnumerable<LinkModel>> GetAllLinksAsync(int page, int pageSize, string? search = null)
    {
        try
        {
            var offset = (page - 1) * pageSize;
            const string sqlWithoutSearch = """
                SELECT * FROM [dbo].[Links]
                WHERE UserId = @UserId
                ORDER BY CreatedOnDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;
            const string sqlWithSearch = """
                SELECT * FROM [dbo].[Links]
                WHERE (Url LIKE '%' + @SearchString + '%' OR Title LIKE '%' + @SearchString + '%' OR Description LIKE '%' + @SearchString + '%' OR Tags LIKE '%' + @SearchString + '%') AND UserId = @UserId
                ORDER BY CreatedOnDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;
            var sql = string.IsNullOrWhiteSpace(search) ? sqlWithoutSearch : sqlWithSearch;
            var links = await connection.QueryAsync<Link>(sql, new { SearchString = search, currentUserService.UserId, Offset = offset, PageSize = pageSize }, transaction);
            return links.Select(link => link.ToModel());
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting links", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting links", innerException: ex);
        }
    }

    public async Task<long> GetLinksCountAsync(string? search = null)
    {
        try
        {
            const string sqlWithoutSearch = "SELECT COUNT(*) FROM [dbo].[Links] WHERE UserId = @UserId";
            const string sqlWithSearch = """
                SELECT COUNT(*) FROM [dbo].[Links] 
                WHERE (Url LIKE '%' + @SearchString + '%' OR Title LIKE '%' + @SearchString + '%' OR Description LIKE '%' + @SearchString + '%' OR Tags LIKE '%' + @SearchString + '%') 
                AND UserId = @UserId
            """;
            var sql = string.IsNullOrWhiteSpace(search) ? sqlWithoutSearch : sqlWithSearch;
            return await connection.ExecuteScalarAsync<long>(sql, new { SearchString = search, currentUserService.UserId }, transaction);
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting links count", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting links count", innerException: ex);
        }
    }
    
    public async Task<LinkModel> GetLinkByIdAsync(Guid id)
    {
        try
        {
            const string sql = "SELECT * FROM [dbo].[Links] WHERE Id = @Id AND UserId = @UserId";
            var link = await connection.QuerySingleOrDefaultAsync<Link>(sql, new { Id = id, currentUserService.UserId }, transaction) 
                       ?? throw new EntityNotFoundException("Link not found");
            return link.ToModel();
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while getting a link", ex);
            throw new DatabaseOperationFailedException("An error occurred while getting a link", innerException: ex);
        }
    }

    public async Task<Guid> CreateLinkAsync(CreateLinkModel model)
    {
        try
        {
            var link = model.ToLink(currentUserService.UserId, provider.Now);
            const string sql = """
                INSERT INTO [dbo].[Links] (Id, Url, Title, Description, Tags, UserId, CreatedOnDate)
                VALUES (@Id, @Url, @Title, @Description, @Tags, @UserId, @CreatedOnDate)
            """;
            await connection.ExecuteAsync(sql, link, transaction);
            return link.Id;
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while creating a link", ex);
            throw new DatabaseOperationFailedException("An error occurred while creating a link", innerException: ex);
        }
    }

    public async Task UpdateLinkAsync(Guid id, UpdateLinkModel model)
    {
        try
        {
            var link = model.ToLink(provider.Now);
            link.Id = id;
            link.UserId = currentUserService.UserId;
            const string sql = """
                UPDATE [dbo].[Links]
                SET Url = @Url, Title = @Title, Description = @Description, Tags = @Tags, LastModifiedOnDate = @LastModifiedOnDate
                WHERE Id = @Id AND UserId = @UserId
            """;
            var recordsUpdated = await connection.ExecuteAsync(sql, link, transaction);
            if (recordsUpdated == 0)
            {
                throw new EntityNotFoundException("Link not found");
            }
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while updating a link", ex);
            throw new DatabaseOperationFailedException("An error occurred while updating a link", innerException: ex);
        }
    }

    public async Task DeleteLinkAsync(Guid id)
    {
        try
        {
            const string sql = "DELETE FROM [dbo].[Links] WHERE Id = @Id AND UserId = @UserId";
            var recordsDeleted = await connection.ExecuteAsync(sql, new { Id = id, currentUserService.UserId }, transaction);
            if (recordsDeleted == 0)
            {
                throw new EntityNotFoundException("Link not found");
            }
        }
        catch (SqlException ex)
        {
            LogDatabaseOperationFailed(logger, "An error occurred while deleting a link", ex);
            throw new DatabaseOperationFailedException("An error occurred while deleting a link", innerException: ex);
        }
    }

    [LoggerMessage(LogLevel.Error, Message = "Database operation failed: {Message}",
        EventName = "DatabaseOperationFailed")]
    private static partial void LogDatabaseOperationFailed(ILogger logger, string message, Exception ex);
}