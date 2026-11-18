using Npgsql;
using portfolio.Models;

namespace portfolio.Repositories
{
    public class CourseRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public CourseRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<List<Course>> GetAllAsync()
        {
            var courses = new List<Course>();

            const string sql = """
                SELECT course_id, course_code, course_title, description
                FROM portfolio.course
                ORDER BY course_code;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                courses.Add(new Course
                {
                    CourseId = reader.GetInt64(0),
                    CourseCode = reader.GetString(1),
                    CourseTitle = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return courses;
        }

        public async Task<Course?> GetByIdAsync(long id)
        {
            const string sql = """
                SELECT course_id, course_code, course_title, description
                FROM portfolio.course
                WHERE course_id = @id;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Course
            {
                CourseId = reader.GetInt64(0),
                CourseCode = reader.GetString(1),
                CourseTitle = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3)
            };
        }

        public async Task AddAsync(Course course)
        {
            const string sql = """
                INSERT INTO portfolio.course
                    (course_code, course_title, description)
                VALUES
                    (@code, @title, @description);
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("code", course.CourseCode);
            command.Parameters.AddWithValue("title", course.CourseTitle);
            command.Parameters.AddWithValue("description",
                (object?)course.Description ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Course course)
        {
            const string sql = """
                UPDATE portfolio.course
                SET course_code = @code,
                    course_title = @title,
                    description = @description
                WHERE course_id = @id;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", course.CourseId);
            command.Parameters.AddWithValue("code", course.CourseCode);
            command.Parameters.AddWithValue("title", course.CourseTitle);
            command.Parameters.AddWithValue("description",
                (object?)course.Description ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(long id)
        {
            const string sql = """
                DELETE FROM portfolio.course
                WHERE course_id = @id;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", id);

            await command.ExecuteNonQueryAsync();
        }
    }
}