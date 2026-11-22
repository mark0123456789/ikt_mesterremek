using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class AddRoomItemDto
{
    [Required, StringLength(100)] public string Name { get; set; } = "";
    public string? Description { get; set; }
    [Range(0, 100_000_000)] public decimal Price { get; set; }
    [Range(0, int.MaxValue)] public int Stock { get; set; }
    [StringLength(255)] public string? ImageUrl { get; set; }
    public bool Active { get; set; } = true;
}
