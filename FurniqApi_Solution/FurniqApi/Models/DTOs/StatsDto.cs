using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public record StatusStatDto(string? Status, int OrderCount, decimal Revenue);

public record MethodStatDto(string? Method, int OrderCount, decimal Revenue);

public record MonthlyRevenueDto(int Year, int Month, int OrderCount, decimal Revenue);

public record TopCustomerDto(int UserId, string Name, string Email, int OrderCount, decimal TotalSpent);

public record DashboardDto(int Users, int Products, int ActiveProducts, int Orders, decimal TotalRevenue,
    decimal AvgOrderValue, int PendingOrders, int LowStockProducts, double? AvgRating, int ReviewCount, int OpenCarts);
