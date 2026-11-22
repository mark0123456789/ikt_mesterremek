using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class AddOrderDto
{
    [Range(1, int.MaxValue)] public int UserId { get; set; }
    [Range(1, int.MaxValue)] public int ProductId { get; set; }
    [Range(1, 1000)] public int Quantity { get; set; } = 1;
    [Required] public string PaymentMethod { get; set; } = "Card";
    [Required] public string ShippingMethod { get; set; } = "Home delivery";
}
