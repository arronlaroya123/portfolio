using Npgsql;

namespace portfolio.Data
{
    public class DatabaseTest
    {
        private readonly NpgsqlDataSource _dataSource;

        public DatabaseTest(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<bool> TestConnectionAsync()
        {
            await using var connection =
                await _dataSource.OpenConnectionAsync();

            await using var command =
                new NpgsqlCommand("SELECT 1", connection);

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result) == 1;
        }
    }
}