using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WarehouseApi.Models;
using WarehouseApi.Models.DTOs;

namespace WarehouseApi.Controllers
{
    [Route("queries")]
    [ApiController]
    public class QueriesController : ControllerBase
    {
        private readonly string ConnectionString = "server=localhost;database=furniq;uid=root;password=";

        // Returns all users (LINQ over in-memory list populated from DB)
        [HttpGet("users")]
        public ActionResult<List<UserDto>> GetAllUsers()
        {
            var users = LoadUsersFromDb();

            var dto = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role
            }).ToList();

            return Ok(dto);
        }

        // Returns employees (role != Customer)
        [HttpGet("employees")]
        public ActionResult<List<UserDto>> GetEmployees()
        {
            var users = LoadUsersFromDb();

            var employees = users
                .Where(u => !string.Equals(u.Role, "Customer", StringComparison.OrdinalIgnoreCase))
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role
                }).ToList();

            return Ok(employees);
        }

        // Returns all products with location and quantity
        [HttpGet("products")]
        public ActionResult<List<ProductDetailDto>> GetProducts()
        {
            var products = LoadProductsFromDb();

            var dto = products.Select(p => new ProductDetailDto
            {
                Id = p.Id,
                ItemNumber = p.ItemNumber,
                Name = p.Name,
                Quantity = p.Quantity,
                Department = p.Department,
                Shelf = p.Shelf,
                SubShelf = p.SubShelf
            }).ToList();

            return Ok(dto);
        }

        // Get single product details by id (LINQ)
        [HttpGet("product/{id}")]
        public ActionResult<object> GetProduct(int id)
        {
            var products = LoadProductsFromDb();

            var product = products.FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound(new { message = "Product not found." });

            var dto = new ProductDetailDto
            {
                Id = product.Id,
                ItemNumber = product.ItemNumber,
                Name = product.Name,
                Quantity = product.Quantity,
                Department = product.Department,
                Shelf = product.Shelf,
                SubShelf = product.SubShelf
            };

            // NOTE: original DB model has no "who added" relation for products, so we cannot return that.
            return Ok(new { product = dto });
        }

        // Group products by location and show totals
        [HttpGet("products/by-location")]
        public ActionResult<object> GetProductsByLocation()
        {
            var products = LoadProductsFromDb();

            var grouped = products
                .GroupBy(p => new { p.Department, p.Shelf, p.SubShelf })
                .Select(g => new
                {
                    Department = g.Key.Department,
                    Shelf = g.Key.Shelf,
                    SubShelf = g.Key.SubShelf,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    Items = g.Select(x => new { x.Id, x.Name, x.Quantity }).ToList()
                }).ToList();

            return Ok(grouped);
        }

        // -- Helpers to load data from DB and perform LINQ to Objects
        private List<AppUser> LoadUsersFromDb()
        {
            var users = new List<AppUser>();

            using var connector = new MySqlConnection(ConnectionString);
            connector.Open();

            var cmd = new MySqlCommand("SELECT id, name, email, phone, role FROM appuser", connector);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                users.Add(new AppUser
                {
                    Id = rdr.GetInt32(0),
                    Name = rdr.GetString(1),
                    Email = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                    Phone = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                    Role = rdr.IsDBNull(4) ? "Customer" : rdr.GetString(4)
                });
            }

            return users;
        }

        private List<Product> LoadProductsFromDb()
        {
            var products = new List<Product>();

            using var connector = new MySqlConnection(ConnectionString);
            connector.Open();

            var cmd = new MySqlCommand("SELECT id, itemnumber, name, quantity, department, shelf, subshelf, created_by_user_id FROM product", connector);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                products.Add(new Product
                {
                    Id = rdr.GetInt32(0),
                    ItemNumber = rdr.IsDBNull(1) ? "" : rdr.GetString(1),
                    Name = rdr.IsDBNull(2) ? "" : rdr.GetString(2),
                    Quantity = rdr.IsDBNull(3) ? 0 : rdr.GetInt32(3),
                    Department = rdr.IsDBNull(4) ? "" : rdr.GetString(4),
                    Shelf = rdr.IsDBNull(5) ? "" : rdr.GetString(5),
                    SubShelf = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                    CreatedByUserId = rdr.IsDBNull(7) ? null : (int?)rdr.GetInt32(7)
                });
            }

            return products;
        }
    }
}
