using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WarehouseApi.Models;
using WarehouseApi.Models.DTOs;

namespace WarehouseApi.Controllers
{
    [Route("product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly string ConnectionString = "server=localhost;database=furniq;uid=root;password=";

        // Szűrések: ?itemNumber=  (cikkszám), ?name=  (név), ?shelf=  (polc / sub polc)
        // Több kritérium egyszerre is megadható, ilyenkor mind teljesül (ÉS kapcsolat).
        [HttpGet]
        public List<Product> GetAllProduct([FromQuery] string? itemNumber, [FromQuery] string? name, [FromQuery] string? shelf)
        {
            List<Product> products = new();

            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            string sql = @"SELECT id, itemNumber, name, quantity, department, shelf, subShelf, created_by_user_id FROM product
                WHERE (@itemNumber IS NULL OR itemNumber LIKE @itemNumber)
                AND (@name IS NULL OR name LIKE @name)
                AND (@shelf IS NULL OR shelf LIKE @shelf OR subShelf LIKE @shelf);";

            var cmd = new MySqlCommand(sql, connector);

            cmd.Parameters.AddWithValue("@itemNumber", string.IsNullOrWhiteSpace(itemNumber) ? null : $"%{itemNumber}%");
            cmd.Parameters.AddWithValue("@name", string.IsNullOrWhiteSpace(name) ? null : $"%{name}%");
            cmd.Parameters.AddWithValue("@shelf", string.IsNullOrWhiteSpace(shelf) ? null : $"%{shelf}%");

            var dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                var product = new Product
                {
                    Id = dataReader.GetInt32(0),
                    ItemNumber = dataReader.GetString(1),
                    Name = dataReader.GetString(2),
                    Quantity = dataReader.GetInt32(3),
                    Department = dataReader.GetString(4),
                    Shelf = dataReader.GetString(5),
                    SubShelf = dataReader.IsDBNull(6) ? null : dataReader.GetString(6),
                    CreatedByUserId = dataReader.IsDBNull(7) ? null : (int?)dataReader.GetInt32(7)
                };

                products.Add(product);
            }

            connector.Close();
            return products;
        }

        [HttpPost]
        public Product AddNewProduct(AddProductDto addProductDto)
        {
            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            var product = new Product
            {
                ItemNumber = addProductDto.ItemNumber,
                Name = addProductDto.Name,
                Quantity = addProductDto.Quantity,
                Department = addProductDto.Department,
                Shelf = addProductDto.Shelf,
                SubShelf = addProductDto.SubShelf
            };

            var sql = "INSERT INTO `product`(`itemNumber`, `name`, `quantity`, `department`, `shelf`, `subShelf`, `created_by_user_id`) VALUES (@itemNumber,@name,@quantity,@department,@shelf,@subShelf,@createdBy)";

            var cmd = new MySqlCommand(sql, connector);

            cmd.Parameters.AddWithValue("@itemNumber", product.ItemNumber);
            cmd.Parameters.AddWithValue("@name", product.Name);
            cmd.Parameters.AddWithValue("@quantity", product.Quantity);
            cmd.Parameters.AddWithValue("@department", product.Department);
            cmd.Parameters.AddWithValue("@shelf", product.Shelf);
            cmd.Parameters.AddWithValue("@subShelf", product.SubShelf);
            cmd.Parameters.AddWithValue("@createdBy", addProductDto.CreatedByUserId.HasValue ? (object)addProductDto.CreatedByUserId.Value : DBNull.Value);

            cmd.ExecuteNonQuery();

            product.Id = (int)cmd.LastInsertedId;

            connector.Close();

            return product;
        }

        [HttpPut]
        public object UpdateProduct([FromQuery] int id, [FromBody] UpdateProductDto updateProductDto)
        {
            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            string sql = @"UPDATE `product` SET `itemNumber`=@itemNumber,`name`=@name,`quantity`=@quantity,`department`=@department,`shelf`=@shelf,`subShelf`=@subShelf,`created_by_user_id`=@createdBy
                WHERE `id`= @id;";

            var cmd = new MySqlCommand(sql, connector);

            cmd.Parameters.AddWithValue("@itemNumber", updateProductDto.ItemNumber);
            cmd.Parameters.AddWithValue("@name", updateProductDto.Name);
            cmd.Parameters.AddWithValue("@quantity", updateProductDto.Quantity);
            cmd.Parameters.AddWithValue("@department", updateProductDto.Department);
            cmd.Parameters.AddWithValue("@shelf", updateProductDto.Shelf);
            cmd.Parameters.AddWithValue("@subShelf", updateProductDto.SubShelf);
            cmd.Parameters.AddWithValue("@createdBy", updateProductDto.CreatedByUserId.HasValue ? (object)updateProductDto.CreatedByUserId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            connector.Close();

            return new { message = "Sikeres frissítés.", result = updateProductDto };
        }

        [HttpDelete]
        public object DeleteProduct(int id)
        {
            var connector = new MySqlConnection(ConnectionString);

            connector.Open();

            var sql = "DELETE FROM product WHERE id = @id";

            var cmd = new MySqlCommand(sql, connector);

            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            connector.Close();

            return new { message = "Sikeres törlés" };
        }
    }
}
