using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class UpdateOrderDto
{
    /// <summary>Processing | Shipped | Delivered | Cancelled</summary>
    [Required] public string Status { get; set; } = "";
}
