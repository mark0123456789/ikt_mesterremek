using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class UpdateProductDto : AddProductDto
{
    public bool Active { get; set; } = true;
}
