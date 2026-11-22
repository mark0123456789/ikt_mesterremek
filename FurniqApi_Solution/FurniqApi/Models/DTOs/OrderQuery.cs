using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class OrderQuery
{
    public int? UserId { get; set; }
    public int? ProductId { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ShippingMethod { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
    public bool Desc { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
