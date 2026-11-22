using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class AddReviewDto
{
    [Range(1, int.MaxValue)] public int UserId { get; set; }
    [Range(1, int.MaxValue)] public int ProductId { get; set; }
    [Range(1, 5)] public int Rating { get; set; }
    [StringLength(2000)] public string? Comment { get; set; }
}
