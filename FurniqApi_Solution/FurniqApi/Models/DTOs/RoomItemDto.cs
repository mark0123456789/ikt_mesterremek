using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public record RoomItemDto(string Room, int Id, string Name, string? Description, decimal Price, int Stock, string? ImageUrl, bool? Active);
