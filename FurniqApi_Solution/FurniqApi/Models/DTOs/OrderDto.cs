using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public record OrderDto(int Id, int UserId, string UserName, int ProductId, string ProductName, int Quantity,
    decimal UnitPrice, DateTime? OrderDate, string? Status, string? PaymentMethod, string? ShippingMethod, decimal TotalPrice);
