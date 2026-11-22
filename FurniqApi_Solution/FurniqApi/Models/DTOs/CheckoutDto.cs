using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class CheckoutDto
{
    [Required] public string PaymentMethod { get; set; } = "Card";          // Card | Cash
    [Required] public string ShippingMethod { get; set; } = "Home delivery"; // Home delivery | Parcel locker
}
