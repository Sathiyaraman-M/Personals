using System.Data;

namespace Expensive.Infrastructure.Abstractions.Services;

public interface IDbContext
{
    IDbConnection GetConnection();
}