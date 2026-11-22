using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public record CartItemDto(int CartId, int ProductId, string ProductName, string? ImageUrl, decimal UnitPrice, int Quantity, decimal LineTotal, int StockAvailable);

public record CartDto(int UserId, List<CartItemDto> Items, int TotalQuantity, decimal TotalPrice);
