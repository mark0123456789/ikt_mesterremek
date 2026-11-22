using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class UpdateReviewDto
{
    [Range(1, 5)] public int Rating { get; set; }
    [StringLength(2000)] public string? Comment { get; set; }
}
