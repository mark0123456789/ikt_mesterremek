using System.ComponentModel.DataAnnotations.Schema;

namespace FurniqApi.Models;

[Table("carts")]
public class Cart
{
    [Column("id")] public int Id { get; set; }
    [Column("user_id")] public int UserId { get; set; }
    [Column("product_id")] public int ProductId { get; set; }
    [Column("quantity")] public int Quantity { get; set; } = 1;
    [Column("added_at")] public DateTime? AddedAt { get; set; }

    public User? User { get; set; }
    public Product? Product { get; set; }
}
