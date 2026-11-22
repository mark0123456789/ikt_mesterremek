using System.ComponentModel.DataAnnotations.Schema;

namespace FurniqApi.Models;

[Table("products")]
public class Product
{
    [Column("id")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; } = "";
    [Column("description")] public string? Description { get; set; }
    [Column("price", TypeName = "decimal(10,2)")] public decimal Price { get; set; }
    [Column("stock")] public int Stock { get; set; }
    [Column("category")] public string? Category { get; set; }
    [Column("image_url")] public string? ImageUrl { get; set; }
    [Column("active")] public bool? Active { get; set; }
    [Column("created_at")] public DateTime? CreatedAt { get; set; }

    public List<Cart> Carts { get; set; } = [];
    public List<Order> Orders { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
}
