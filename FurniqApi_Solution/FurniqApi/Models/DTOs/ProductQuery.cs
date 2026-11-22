using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class ProductQuery
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public bool? OnlyActive { get; set; } = true;
    public double? MinRating { get; set; }
    /// <summary>name | price | stock | rating | newest</summary>
    public string? SortBy { get; set; }
    public bool Desc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
