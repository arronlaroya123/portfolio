
using Npgsql;
using portfolio.Models;

namespace portfolio.Repositories
{
    public class ProjectImageRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public ProjectImageRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<List<ProjectImage>> GetByProjectIdAsync(long projectId)
        {
            var images = new List<ProjectImage>();

            const string sql = """
                SELECT image_id, project_id, image_url, sort_order
                FROM portfolio.project_image
                WHERE project_id = @projectId
                ORDER BY sort_order, image_id;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("projectId", projectId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                images.Add(new ProjectImage
                {
                    ImageId = reader.GetInt64(0),
                    ProjectId = reader.GetInt64(1),
                    ImageUrl = reader.GetString(2),
                    SortOrder = reader.GetInt32(3)
                });
            }

            return images;
        }

        public async Task<ProjectImage?> GetByIdAsync(long id)
        {
            const string sql = """
                SELECT image_id, project_id, image_url, sort_order
                FROM portfolio.project_image
                WHERE image_id = @id;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new ProjectImage
            {
                ImageId = reader.GetInt64(0),
                ProjectId = reader.GetInt64(1),
                ImageUrl = reader.GetString(2),
                SortOrder = reader.GetInt32(3)
            };
        }

        public async Task AddAsync(ProjectImage image)
        {
            const string sql = """
                INSERT INTO portfolio.project_image
                    (project_id, image_url, sort_order)
                VALUES
                    (@projectId, @imageUrl, @sortOrder);
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("projectId", image.ProjectId);
            command.Parameters.AddWithValue("imageUrl", image.ImageUrl);
            command.Parameters.AddWithValue("sortOrder", image.SortOrder);

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(ProjectImage image)
        {
            const string sql = """
                UPDATE portfolio.project_image
                SET image_url = @imageUrl,
                    sort_order = @sortOrder
                WHERE image_id = @id;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", image.ImageId);
            command.Parameters.AddWithValue("imageUrl", image.ImageUrl);
            command.Parameters.AddWithValue("sortOrder", image.SortOrder);

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(long id)
        {
            const string sql = """
                DELETE FROM portfolio.project_image
                WHERE image_id = @id;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", id);

            await command.ExecuteNonQueryAsync();
        }
    }
}
