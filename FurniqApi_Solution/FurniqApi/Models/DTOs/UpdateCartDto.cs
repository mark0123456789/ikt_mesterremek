using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class UpdateCartDto
{
    [Range(1, 1000)] public int Quantity { get; set; }
}
