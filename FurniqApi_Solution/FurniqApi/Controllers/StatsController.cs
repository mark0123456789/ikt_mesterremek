using FurniqApi.Models;
using FurniqApi.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurniqApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController(FurniqContext db) : ControllerBase
{
    /// <summary>Admin főoldal: egy hívással a legfontosabb számok.</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard()
    {
        var validOrders = db.Orders.AsNoTracking().Where(o => o.Status != "Cancelled");

        return new DashboardDto(
            Users: await db.Users.CountAsync(),
            Products: await db.Products.CountAsync(),
            ActiveProducts: await db.Products.CountAsync(p => p.Active == true),
            Orders: await db.Orders.CountAsync(),
            TotalRevenue: await validOrders.SumAsync(o => (decimal?)o.TotalPrice) ?? 0,
            AvgOrderValue: await validOrders.AverageAsync(o => (decimal?)o.TotalPrice) ?? 0,
            PendingOrders: await db.Orders.CountAsync(o => o.Status == "Processing"),
            LowStockProducts: await db.Products.CountAsync(p => p.Active == true && p.Stock <= 5),
            AvgRating: await db.Reviews.AverageAsync(r => (double?)r.Rating),
            ReviewCount: await db.Reviews.CountAsync(),
            OpenCarts: await db.Carts.Select(c => c.UserId).Distinct().CountAsync());
    }

    [HttpGet("orders-by-status")]
    public async Task<ActionResult<List<StatusStatDto>>> OrdersByStatus()
    {
        var rows = await db.Orders.AsNoTracking().GroupBy(o => o.Status)
            .Select(g => new { g.Key, Count = g.Count(), Revenue = g.Sum(o => o.TotalPrice) }).ToListAsync();
        return rows.Select(r => new StatusStatDto(r.Key, r.Count, r.Revenue)).OrderByDescending(r => r.OrderCount).ToList();
    }

    [HttpGet("orders-by-payment")]
    public async Task<ActionResult<List<MethodStatDto>>> OrdersByPayment()
    {
        var rows = await db.Orders.AsNoTracking().GroupBy(o => o.PaymentMethod)
            .Select(g => new { g.Key, Count = g.Count(), Revenue = g.Sum(o => o.TotalPrice) }).ToListAsync();
        return rows.Select(r => new MethodStatDto(r.Key, r.Count, r.Revenue)).ToList();
    }

    [HttpGet("orders-by-shipping")]
    public async Task<ActionResult<List<MethodStatDto>>> OrdersByShipping()
    {
        var rows = await db.Orders.AsNoTracking().GroupBy(o => o.ShippingMethod)
            .Select(g => new { g.Key, Count = g.Count(), Revenue = g.Sum(o => o.TotalPrice) }).ToListAsync();
        return rows.Select(r => new MethodStatDto(r.Key, r.Count, r.Revenue)).ToList();
    }

    /// <summary>Havi bevétel (lemondott rendelések nélkül).</summary>
    [HttpGet("monthly-revenue")]
    public async Task<ActionResult<List<MonthlyRevenueDto>>> MonthlyRevenue()
    {
        var rows = await db.Orders.AsNoTracking()
            .Where(o => o.OrderDate != null && o.Status != "Cancelled")
            .GroupBy(o => new { o.OrderDate!.Value.Year, o.OrderDate!.Value.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count(), Revenue = g.Sum(o => o.TotalPrice) })
            .ToListAsync();
        return rows.Select(r => new MonthlyRevenueDto(r.Year, r.Month, r.Count, r.Revenue)).ToList();
    }

    [HttpGet("top-customers")]
    public async Task<ActionResult<List<TopCustomerDto>>> TopCustomers([FromQuery] int count = 5)
    {
        var rows = await db.Users.AsNoTracking()
            .Select(u => new
            {
                u.Id, u.Name, u.Email,
                Orders = u.Orders.Count(o => o.Status != "Cancelled"),
                Spent = u.Orders.Where(o => o.Status != "Cancelled").Sum(o => (decimal?)o.TotalPrice) ?? 0
            })
            .Where(x => x.Orders > 0)
            .OrderByDescending(x => x.Spent).Take(Math.Clamp(count, 1, 50)).ToListAsync();
        return rows.Select(r => new TopCustomerDto(r.Id, r.Name, r.Email, r.Orders, r.Spent)).ToList();
    }

    /// <summary>Kosárban hagyott (még meg nem rendelt) termékek értéke felhasználónként.</summary>
    [HttpGet("abandoned-carts")]
    public async Task<ActionResult<object>> AbandonedCarts()
    {
        var rows = await db.Carts.AsNoTracking()
            .GroupBy(c => new { c.UserId, c.User!.Name })
            .Select(g => new
            {
                g.Key.UserId, g.Key.Name,
                Items = g.Sum(c => c.Quantity),
                Value = g.Sum(c => c.Quantity * c.Product!.Price),
                OldestItem = g.Min(c => c.AddedAt)
            }).ToListAsync();
        return rows;
    }

    /// <summary>Készletérték kategóriánként (ár × darabszám).</summary>
    [HttpGet("stock-value")]
    public async Task<ActionResult<object>> StockValue()
    {
        var rows = await db.Products.AsNoTracking().Where(p => p.Active == true)
            .GroupBy(p => p.Category)
            .Select(g => new { Category = g.Key, Units = g.Sum(p => p.Stock), Value = g.Sum(p => p.Stock * p.Price) })
            .ToListAsync();
        return new { Categories = rows, Total = rows.Sum(r => r.Value) };
    }
}
