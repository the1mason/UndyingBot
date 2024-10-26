using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace UndyingBot.Core.Db;

public class SqliteConnectionFactory(string connectionString)
{
    public DbConnection CreateConnection()
    {
        return new SqliteConnection(connectionString);
    }
}