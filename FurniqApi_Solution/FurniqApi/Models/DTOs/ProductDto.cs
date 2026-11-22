using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public record ProductListDto(int Id, string Name, string? Description, decimal Price, int Stock,
    string? Category, string? ImageUrl, bool? Active, double? AvgRating, int ReviewCount);

public record ProductDetailDto(int Id, string Name, string? Description, decimal Price, int Stock,
    string? Category, string? ImageUrl, bool? Active, DateTime? CreatedAt,
    double? AvgRating, int ReviewCount, int TotalSold, List<ReviewDto> LatestReviews);

public record CategoryStatDto(string? Category, int ProductCount, int TotalStock, decimal MinPrice, decimal MaxPrice, decimal AvgPrice);

public record BestSellerDto(int ProductId, string Name, int QuantitySold, decimal Revenue);
