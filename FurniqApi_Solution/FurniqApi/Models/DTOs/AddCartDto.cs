using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class AddCartDto
{
    [Range(1, int.MaxValue)] public int ProductId { get; set; }
    [Range(1, 1000)] public int Quantity { get; set; } = 1;
}
