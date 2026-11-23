
using Npgsql;
using portfolio.Models;
using Microsoft.AspNetCore.Identity;

namespace portfolio.Data
{
    public class AccountRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public AccountRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
            _passwordHasher = new PasswordHasher<AppUser>();
        }

        public async Task<bool> RegisterAsync(RegisterViewModel model)
        {
            await using var connection =
                await _dataSource.OpenConnectionAsync();

            var user = new AppUser
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = "Viewer"
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user, model.Password);

            const string sql = @"
                INSERT INTO portfolio.app_user
                (full_name, email, password_hash, role)
                VALUES (@name, @email, @password, 'Viewer')
                ON CONFLICT (email) DO NOTHING;
            ";

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("name", user.FullName);
            command.Parameters.AddWithValue("email", user.Email);
            command.Parameters.AddWithValue("password", user.PasswordHash);

            return await command.ExecuteNonQueryAsync() == 1;
        }

        public async Task<AppUser?> LoginAsync(LoginViewModel model)
        {
            await using var connection =
                await _dataSource.OpenConnectionAsync();

            const string sql = @"
                SELECT user_id, full_name, email, password_hash, role
                FROM portfolio.app_user
                WHERE email = @email;
            ";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("email", model.Email);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            var user = new AppUser
            {
                UserId = reader.GetInt64(0),
                FullName = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                Role = reader.GetString(4)
            };

            var result = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, model.Password);

            return result == PasswordVerificationResult.Failed
                ? null
                : user;
        }
    }
}
