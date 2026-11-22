using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class UpdateUserDto
{
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = "";
    [StringLength(20)] public string? Phone { get; set; }
}
