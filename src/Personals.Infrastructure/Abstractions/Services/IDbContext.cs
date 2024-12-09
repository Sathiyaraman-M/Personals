using System.Data;

namespace Personals.Infrastructure.Abstractions.Services;

public interface IDbContext
{
    IDbConnection GetConnection();
}