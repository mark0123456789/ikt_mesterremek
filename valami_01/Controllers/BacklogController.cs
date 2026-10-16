using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WarehouseApi.Models;

namespace WarehouseApi.Controllers
{
    [Route("backlog")]
    [ApiController]
    public class BacklogController : ControllerBase
    {
        private readonly string ConnectionString = "server=localhost;database=furniq;uid=root;password=";

        // ?userId=1 és/vagy ?action=Pick|Pack
        [HttpGet]
        public List<BacklogEntry> GetAllBacklog([FromQuery] int? userId, [FromQuery] string? action)
        {
            List<BacklogEntry> entries = new();

            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            string sql = "SELECT * FROM backlog WHERE (@userId IS NULL OR userId = @userId) AND (@action IS NULL OR action = @action) ORDER BY startedAt DESC;";

            var cmd = new MySqlCommand(sql, connector);

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@action", action);

            var dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                var entry = new BacklogEntry
                {
                    Id = dataReader.GetInt32(0),
                    TaskId = dataReader.GetInt32(1),
                    UserId = dataReader.GetInt32(2),
                    Action = dataReader.GetString(3),
                    StartedAt = dataReader.GetDateTime(4),
                    FinishedAt = dataReader.IsDBNull(5) ? null : dataReader.GetDateTime(5)
                };

                entries.Add(entry);
            }

            connector.Close();
            return entries;
        }
    }
}
