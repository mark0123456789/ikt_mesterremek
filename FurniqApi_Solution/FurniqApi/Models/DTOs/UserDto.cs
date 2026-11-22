using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public record UserDto(int Id, string Name, string Email, string? Phone, string? Role, DateTime? RegistrationDate);

public record UserDetailDto(int Id, string Name, string Email, string? Phone, string? Role, DateTime? RegistrationDate,
    int OrderCount, decimal TotalSpent, int ReviewCount, int CartItemCount, List<AddressDto> Addresses);
