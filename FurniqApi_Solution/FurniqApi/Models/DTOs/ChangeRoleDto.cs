using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class ChangeRoleDto
{
    [Required, RegularExpression("^(user|admin)$", ErrorMessage = "A role csak 'user' vagy 'admin' lehet.")]
    public string Role { get; set; } = "user";
}
