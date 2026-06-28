using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace CF.IntegrationTest.Factories;

internal static class SqlServerContainer
{
    private static readonly MsSqlContainer Container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    private static readonly SemaphoreSlim StartLock = new(1, 1);
    private static bool _started;

    public static async Task<string> GetConnectionStringAsync(string database)
    {
        await EnsureStartedAsync();

        return new SqlConnectionStringBuilder(Container.GetConnectionString())
        {
            InitialCatalog = database
        }.ConnectionString;
    }

    private static async Task EnsureStartedAsync()
    {
        if (_started) return;

        await StartLock.WaitAsync();
        try
        {
            if (!_started)
            {
                await Container.StartAsync();
                _started = true;
            }
        }
        finally
        {
            StartLock.Release();
        }
    }
}
