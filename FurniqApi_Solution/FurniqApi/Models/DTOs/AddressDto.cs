using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public record AddressDto(int Id, int UserId, string Name, string PostalCode, string City, string Street, string HouseNumber, bool IsDefault);
