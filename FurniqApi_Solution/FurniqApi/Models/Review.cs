using System.ComponentModel.DataAnnotations.Schema;

namespace FurniqApi.Models;

[Table("reviews")]
public class Review
{
    [Column("id")] public int Id { get; set; }
    [Column("user_id")] public int UserId { get; set; }
    [Column("product_id")] public int ProductId { get; set; }
    [Column("rating")] public int Rating { get; set; }
    [Column("comment")] public string? Comment { get; set; }
    [Column("created_at")] public DateTime? CreatedAt { get; set; }

    public User? User { get; set; }
    public Product? Product { get; set; }
}
