using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class AddProductDto
{
    [Required, StringLength(100)] public string Name { get; set; } = "";
    public string? Description { get; set; }
    [Range(0, 100_000_000)] public decimal Price { get; set; }
    [Range(0, int.MaxValue)] public int Stock { get; set; }
    [StringLength(50)] public string? Category { get; set; }
    [StringLength(255)] public string? ImageUrl { get; set; }
}
