using Npgsql;
using portfolio.Models;

namespace portfolio.Repositories
{
    public class ProjectRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public ProjectRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<List<Project>> GetAllAsync()
        {
            var projects = new List<Project>();

            const string sql = """
                SELECT p.project_id, p.course_id, p.project_title,
                       p.description, p.video_url,
                       c.course_code, c.course_title
                FROM portfolio.project p
                INNER JOIN portfolio.course c
                    ON p.course_id = c.course_id
                ORDER BY p.project_id;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                projects.Add(new Project
                {
                    ProjectId = reader.GetInt64(0),
                    CourseId = reader.GetInt64(1),
                    ProjectTitle = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    VideoUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Course = new Course
                    {
                        CourseCode = reader.GetString(5),
                        CourseTitle = reader.GetString(6)
                    }
                });
            }

            return projects;
        }

        public async Task<Project?> GetByIdAsync(long id)
        {
            const string sql = """
                SELECT project_id, course_id, project_title,
                       description, video_url
                FROM portfolio.project
                WHERE project_id = @id;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Project
            {
                ProjectId = reader.GetInt64(0),
                CourseId = reader.GetInt64(1),
                ProjectTitle = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                VideoUrl = reader.IsDBNull(4) ? null : reader.GetString(4)
            };
        }

        public async Task AddWithImagesAsync(
            ProjectCreateViewModel model,
            List<string> imageUrls)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                const string projectSql = """
                    INSERT INTO portfolio.project
                        (course_id, project_title, description, video_url)
                    VALUES
                        (@courseId, @title, @description, @videoUrl)
                    RETURNING project_id;
                    """;

                await using var projectCommand =
                    new NpgsqlCommand(projectSql, connection, transaction);

                projectCommand.Parameters.AddWithValue("courseId", model.CourseId);
                projectCommand.Parameters.AddWithValue("title", model.ProjectTitle);
                projectCommand.Parameters.AddWithValue(
                    "description", (object?)model.Description ?? DBNull.Value);
                projectCommand.Parameters.AddWithValue(
                    "videoUrl", (object?)model.VideoUrl ?? DBNull.Value);

                long projectId = Convert.ToInt64(
                    await projectCommand.ExecuteScalarAsync());

                const string imageSql = """
                    INSERT INTO portfolio.project_image
                        (project_id, image_url, sort_order)
                    VALUES
                        (@projectId, @imageUrl, @sortOrder);
                    """;

                for (int i = 0; i < imageUrls.Count; i++)
                {
                    await using var imageCommand =
                        new NpgsqlCommand(imageSql, connection, transaction);

                    imageCommand.Parameters.AddWithValue("projectId", projectId);
                    imageCommand.Parameters.AddWithValue("imageUrl", imageUrls[i]);
                    imageCommand.Parameters.AddWithValue("sortOrder", i);

                    await imageCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }
        public async Task UpdateWithImagesAsync(
    ProjectEditViewModel model,
    List<string> imageUrls)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                const string updateSql = """
            UPDATE portfolio.project
            SET course_id = @courseId,
                project_title = @title,
                description = @description,
                video_url = @videoUrl
            WHERE project_id = @id;
            """;

                await using var updateCommand =
                    new NpgsqlCommand(updateSql, connection, transaction);

                updateCommand.Parameters.AddWithValue("id", model.ProjectId);
                updateCommand.Parameters.AddWithValue("courseId", model.CourseId);
                updateCommand.Parameters.AddWithValue("title", model.ProjectTitle);
                updateCommand.Parameters.AddWithValue(
                    "description", (object?)model.Description ?? DBNull.Value);
                updateCommand.Parameters.AddWithValue(
                    "videoUrl", (object?)model.VideoUrl ?? DBNull.Value);

                int affectedRows = await updateCommand.ExecuteNonQueryAsync();

                if (affectedRows == 0)
                    throw new InvalidOperationException("Project not found.");

                const string deleteSql = """
            DELETE FROM portfolio.project_image
            WHERE project_id = @id;
            """;

                await using var deleteCommand =
                    new NpgsqlCommand(deleteSql, connection, transaction);

                deleteCommand.Parameters.AddWithValue("id", model.ProjectId);
                await deleteCommand.ExecuteNonQueryAsync();

                const string imageSql = """
            INSERT INTO portfolio.project_image
                (project_id, image_url, sort_order)
            VALUES
                (@projectId, @imageUrl, @sortOrder);
            """;

                for (int i = 0; i < imageUrls.Count; i++)
                {
                    await using var imageCommand =
                        new NpgsqlCommand(imageSql, connection, transaction);

                    imageCommand.Parameters.AddWithValue("projectId", model.ProjectId);
                    imageCommand.Parameters.AddWithValue("imageUrl", imageUrls[i]);
                    imageCommand.Parameters.AddWithValue("sortOrder", i);

                    await imageCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<bool> DeleteAsync(long id)
        {
            const string sql = """
        DELETE FROM portfolio.project
        WHERE project_id = @id;
        """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", id);

            int affectedRows = await command.ExecuteNonQueryAsync();

            return affectedRows > 0;
        }
    }

}