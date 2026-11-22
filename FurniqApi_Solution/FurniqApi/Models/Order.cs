using System.ComponentModel.DataAnnotations.Schema;

namespace FurniqApi.Models;

[Table("orders")]
public class Order
{
    [Column("id")] public int Id { get; set; }
    [Column("user_id")] public int UserId { get; set; }
    [Column("product_id")] public int ProductId { get; set; }
    [Column("quantity")] public int Quantity { get; set; }
    [Column("unit_price", TypeName = "decimal(10,2)")] public decimal UnitPrice { get; set; }
    [Column("order_date")] public DateTime? OrderDate { get; set; }
    [Column("status")] public string? Status { get; set; }
    [Column("payment_method")] public string? PaymentMethod { get; set; }
    [Column("shipping_method")] public string? ShippingMethod { get; set; }
    [Column("total_price", TypeName = "decimal(10,2)")] public decimal TotalPrice { get; set; }

    public User? User { get; set; }
    public Product? Product { get; set; }
}
