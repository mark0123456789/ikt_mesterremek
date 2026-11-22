using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class AddAddressDto
{
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, StringLength(10)] public string PostalCode { get; set; } = "";
    [Required, StringLength(50)] public string City { get; set; } = "";
    [Required, StringLength(100)] public string Street { get; set; } = "";
    [Required, StringLength(20)] public string HouseNumber { get; set; } = "";
    public bool IsDefault { get; set; }
}
