using Microsoft.Data.SqlClient;
using Personals.Infrastructure.Abstractions.Services;
using System.Data;

namespace Personals.Infrastructure.Services;

public sealed class SqlServerDbContext(string connectionString) : IDbContext
{
    public IDbConnection GetConnection() => new SqlConnection(connectionString);
}