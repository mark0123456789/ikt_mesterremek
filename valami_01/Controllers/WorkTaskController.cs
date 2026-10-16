using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WarehouseApi.Models;
using WarehouseApi.Models.DTOs;

namespace WarehouseApi.Controllers
{
    [Route("task")]
    [ApiController]
    public class WorkTaskController : ControllerBase
    {
        private readonly string ConnectionString = "server=localhost;database=furniq;uid=root;password=";

        // A tábla tetején látható: hány megrendelés vár még elvégzésre
        // Szűrések: ?name=  (név), ?orderNumber=  (feladat szám)
        // Rendezés: ?sort=added (legutóbb felvett), ?sort=completed (legutóbb elkészült)
        [HttpGet]
        public object GetAllTask([FromQuery] string? name, [FromQuery] string? orderNumber, [FromQuery] string? sort)
        {
            List<WorkTask> tasks = new();

            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            string sql = @"SELECT * FROM worktask
                WHERE (@name IS NULL OR name LIKE @name)
                AND (@orderNumber IS NULL OR orderNumber LIKE @orderNumber)";

            if (sort == "completed")
                sql += " AND completedAt IS NOT NULL ORDER BY completedAt DESC;";
            else if (sort == "added")
                sql += " ORDER BY createdAt DESC;";
            else
                sql += " ORDER BY id;";

            var cmd = new MySqlCommand(sql, connector);

            cmd.Parameters.AddWithValue("@name", string.IsNullOrWhiteSpace(name) ? null : $"%{name}%");
            cmd.Parameters.AddWithValue("@orderNumber", string.IsNullOrWhiteSpace(orderNumber) ? null : $"%{orderNumber}%");

            var dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                var task = new WorkTask
                {
                    Id = dataReader.GetInt32(0),
                    OrderNumber = dataReader.GetString(1),
                    Name = dataReader.GetString(2),
                    PickerId = dataReader.IsDBNull(3) ? null : dataReader.GetInt32(3),
                    PackerId = dataReader.IsDBNull(4) ? null : dataReader.GetInt32(4),
                    Status = dataReader.GetString(5),
                    CreatedAt = dataReader.GetDateTime(6),
                    CompletedAt = dataReader.IsDBNull(7) ? null : dataReader.GetDateTime(7)
                };

                tasks.Add(task);
            }

            dataReader.Close();

            var remainingCmd = new MySqlCommand("SELECT COUNT(*) FROM worktask WHERE status != 'Done';", connector);
            int remaining = Convert.ToInt32(remainingCmd.ExecuteScalar());

            connector.Close();

            return new { remainingOrders = remaining, tasks = tasks };
        }

        [HttpPost]
        public WorkTask AddNewTask(AddTaskDto addTaskDto)
        {
            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            var task = new WorkTask
            {
                OrderNumber = addTaskDto.OrderNumber,
                Name = addTaskDto.Name,
                Status = "New",
                CreatedAt = DateTime.Now
            };

            var sql = "INSERT INTO `worktask`(`orderNumber`, `name`, `status`, `createdAt`) VALUES (@orderNumber,@name,@status,@createdAt)";

            var cmd = new MySqlCommand(sql, connector);

            cmd.Parameters.AddWithValue("@orderNumber", task.OrderNumber);
            cmd.Parameters.AddWithValue("@name", task.Name);
            cmd.Parameters.AddWithValue("@status", task.Status);
            cmd.Parameters.AddWithValue("@createdAt", task.CreatedAt);

            cmd.ExecuteNonQuery();

            task.Id = (int)cmd.LastInsertedId;

            connector.Close();

            return task;
        }

        [HttpPut]
        public object UpdateTask([FromQuery] int id, [FromBody] UpdateTaskDto updateTaskDto)
        {
            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            string sql = @"UPDATE `worktask` SET `orderNumber`=@orderNumber,`name`=@name
                WHERE `id`= @id;";

            var cmd = new MySqlCommand(sql, connector);

            cmd.Parameters.AddWithValue("@orderNumber", updateTaskDto.OrderNumber);
            cmd.Parameters.AddWithValue("@name", updateTaskDto.Name);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            connector.Close();

            return new { message = "Sikeres frissítés.", result = updateTaskDto };
        }

        [HttpDelete]
        public object DeleteTask(int id)
        {
            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            var sql = "DELETE FROM worktask WHERE id = @id";

            var cmd = new MySqlCommand(sql, connector);

            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            connector.Close();

            return new { message = "Sikeres törlés" };
        }

        [HttpPost("pick/start")]
        public object PickStart([FromQuery] int id, [FromQuery] int userId)
        {
            return Step(id, userId, "Pick", true);
        }

        [HttpPost("pick/finish")]
        public object PickFinish([FromQuery] int id, [FromQuery] int userId)
        {
            return Step(id, userId, "Pick", false);
        }

        [HttpPost("pack/start")]
        public object PackStart([FromQuery] int id, [FromQuery] int userId)
        {
            return Step(id, userId, "Pack", true);
        }

        [HttpPost("pack/finish")]
        public object PackFinish([FromQuery] int id, [FromQuery] int userId)
        {
            return Step(id, userId, "Pack", false);
        }

        private object Step(int id, int userId, string action, bool start)
        {
            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            // van-e ilyen feladat?
            var taskCmd = new MySqlCommand("SELECT COUNT(*) FROM worktask WHERE id = @id", connector);
            taskCmd.Parameters.AddWithValue("@id", id);
            if (Convert.ToInt32(taskCmd.ExecuteScalar()) == 0)
            {
                connector.Close();
                return new { message = "Nincs ilyen feladat." };
            }

            // dolgozó-e a felhasználó (a vásárló nem végezhet munkát)?
            var userCmd = new MySqlCommand("SELECT COUNT(*) FROM appuser WHERE id = @userId AND role != 'Customer'", connector);
            userCmd.Parameters.AddWithValue("@userId", userId);
            if (Convert.ToInt32(userCmd.ExecuteScalar()) == 0)
            {
                connector.Close();
                return new { message = "Érvénytelen dolgozó." };
            }

            if (start)
            {
                string column = action == "Pick" ? "pickerId" : "packerId";
                string status = action == "Pick" ? "Picking" : "Packing";

                var updateCmd = new MySqlCommand($"UPDATE `worktask` SET `{column}`=@userId,`status`=@status WHERE `id`=@id", connector);
                updateCmd.Parameters.AddWithValue("@userId", userId);
                updateCmd.Parameters.AddWithValue("@status", status);
                updateCmd.Parameters.AddWithValue("@id", id);
                updateCmd.ExecuteNonQuery();

                var logCmd = new MySqlCommand("INSERT INTO `backlog`(`taskId`, `userId`, `action`, `startedAt`) VALUES (@taskId,@userId,@action,@startedAt)", connector);
                logCmd.Parameters.AddWithValue("@taskId", id);
                logCmd.Parameters.AddWithValue("@userId", userId);
                logCmd.Parameters.AddWithValue("@action", action);
                logCmd.Parameters.AddWithValue("@startedAt", DateTime.Now);
                logCmd.ExecuteNonQuery();

                connector.Close();
                return new { message = "Munka elindítva." };
            }

            // befejezés: megkeressük a nyitott backlog bejegyzést
            var findCmd = new MySqlCommand(@"SELECT id FROM backlog
                WHERE taskId = @taskId AND userId = @userId AND action = @action AND finishedAt IS NULL
                ORDER BY startedAt DESC LIMIT 1", connector);
            findCmd.Parameters.AddWithValue("@taskId", id);
            findCmd.Parameters.AddWithValue("@userId", userId);
            findCmd.Parameters.AddWithValue("@action", action);
            var backlogId = findCmd.ExecuteScalar();

            if (backlogId == null)
            {
                connector.Close();
                return new { message = "Nincs elindított munka." };
            }

            var finishCmd = new MySqlCommand("UPDATE `backlog` SET `finishedAt`=@finishedAt WHERE `id`=@backlogId", connector);
            finishCmd.Parameters.AddWithValue("@finishedAt", DateTime.Now);
            finishCmd.Parameters.AddWithValue("@backlogId", Convert.ToInt32(backlogId));
            finishCmd.ExecuteNonQuery();

            // csomagolás befejezésekor a feladat elkészült, ekkor rögzítjük az idejét is
            var statusCmd = new MySqlCommand("UPDATE `worktask` SET `status`=@status,`completedAt`=@completedAt WHERE `id`=@id", connector);
            statusCmd.Parameters.AddWithValue("@status", action == "Pick" ? "Picked" : "Done");
            statusCmd.Parameters.AddWithValue("@completedAt", action == "Pick" ? DBNull.Value : (object)DateTime.Now);
            statusCmd.Parameters.AddWithValue("@id", id);
            statusCmd.ExecuteNonQuery();

            connector.Close();
            return new { message = "Munka befejezve." };
        }
    }
}
