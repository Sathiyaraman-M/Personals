using Expensive.Infrastructure.Abstractions.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Expensive.Infrastructure.Services;

public sealed class SqlServerDbContext(string connectionString) : IDbContext
{
    public IDbConnection GetConnection() => new SqlConnection(connectionString);
}