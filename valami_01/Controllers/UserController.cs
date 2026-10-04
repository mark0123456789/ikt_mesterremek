using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WarehouseApi.Models;
using WarehouseApi.Models.DTOs;

namespace WarehouseApi.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly string ConnectionString = "server=localhost;database=warehouse;uid=root;password=";

        // ?staff=true -> csak dolgozók, ?staff=false -> csak vásárlók
        [HttpGet]
        public List<AppUser> GetAllUser([FromQuery] bool? staff)
        {
            List<AppUser> users = new();

            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            string sql = "SELECT * FROM appuser";

            if (staff == true) sql += " WHERE role != 'Customer'";
            if (staff == false) sql += " WHERE role = 'Customer'";

            var cmd = new MySqlCommand(sql, connector);

            var dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                var user = new AppUser
                {
                    Id = dataReader.GetInt32(0),
                    Name = dataReader.GetString(1),
                    Email = dataReader.IsDBNull(2) ? null : dataReader.GetString(2),
                    Phone = dataReader.IsDBNull(3) ? null : dataReader.GetString(3),
                    Role = dataReader.GetString(4)
                };

                users.Add(user);
            }

            connector.Close();
            return users;
        }

        [HttpPost]
        public AppUser AddNewUser(AddUserDto addUserDto)
        {
            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            var user = new AppUser
            {
                Name = addUserDto.Name,
                Email = addUserDto.Email,
                Phone = addUserDto.Phone,
                Role = addUserDto.Role
            };

            var sql = "INSERT INTO `appuser`(`name`, `email`, `phone`, `role`) VALUES (@name,@email,@phone,@role)";

            var cmd = new MySqlCommand(sql, connector);

            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@phone", user.Phone);
            cmd.Parameters.AddWithValue("@role", user.Role);

            cmd.ExecuteNonQuery();

            user.Id = (int)cmd.LastInsertedId;

            connector.Close();

            return user;
        }

        [HttpPut]
        public object UpdateUser([FromQuery] int id, [FromBody] UpdateUserDto updateUserDto)
        {
            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            string sql = @"UPDATE `appuser` SET `name`=@name,`email`=@email,`phone`=@phone,`role`=@role
                WHERE `id`= @id;";

            var cmd = new MySqlCommand(sql, connector);

            cmd.Parameters.AddWithValue("@name", updateUserDto.Name);
            cmd.Parameters.AddWithValue("@email", updateUserDto.Email);
            cmd.Parameters.AddWithValue("@phone", updateUserDto.Phone);
            cmd.Parameters.AddWithValue("@role", updateUserDto.Role);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            connector.Close();

            return new { message = "Sikeres frissítés.", result = updateUserDto };
        }

        [HttpDelete]
        public object DeleteUser(int id)
        {
            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            var sql = "DELETE FROM appuser WHERE id = @id";

            var cmd = new MySqlCommand(sql, connector);

            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            connector.Close();

            return new { message = "Sikeres törlés" };
        }
    }
}
