using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public record ReviewDto(int Id, int UserId, string UserName, int ProductId, string ProductName,
    int Rating, string? Comment, DateTime? CreatedAt);

public record RatingBucketDto(int Rating, int Count);

public record ReviewSummaryDto(int ProductId, string ProductName, double? AvgRating, int ReviewCount, List<RatingBucketDto> Distribution);
