using FurniqApi.Models.DTOs;
using FurniqApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurniqApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController(FurniqContext db) : ControllerBase
{
    /// <summary>Szűrés, keresés, rendezés, lapozás egyben.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListDto>>> GetAll([FromQuery] ProductQuery q)
    {
        var page = Math.Max(q.Page, 1);
        var size = Math.Clamp(q.PageSize, 1, 100);

        var query = db.Products.AsNoTracking().AsQueryable();

        if (q.OnlyActive == true) query = query.Where(p => p.Active == true);
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(p => p.Name.Contains(q.Search) || (p.Description != null && p.Description.Contains(q.Search)));
        if (!string.IsNullOrWhiteSpace(q.Category)) query = query.Where(p => p.Category == q.Category);
        if (q.MinPrice.HasValue) query = query.Where(p => p.Price >= q.MinPrice);
        if (q.MaxPrice.HasValue) query = query.Where(p => p.Price <= q.MaxPrice);
        if (q.InStock == true) query = query.Where(p => p.Stock > 0);
        if (q.MinRating.HasValue) query = query.Where(p => p.Reviews.Average(r => (double?)r.Rating) >= q.MinRating);

        query = (q.SortBy?.ToLowerInvariant(), q.Desc) switch
        {
            ("price", false) => query.OrderBy(p => p.Price),
            ("price", true) => query.OrderByDescending(p => p.Price),
            ("stock", false) => query.OrderBy(p => p.Stock),
            ("stock", true) => query.OrderByDescending(p => p.Stock),
            ("rating", false) => query.OrderBy(p => p.Reviews.Average(r => (double?)r.Rating)),
            ("rating", true) => query.OrderByDescending(p => p.Reviews.Average(r => (double?)r.Rating)),
            ("newest", _) => query.OrderByDescending(p => p.CreatedAt),
            (_, true) => query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).Select(Projections.ProductList).ToListAsync();
        return new PagedResult<ProductListDto>(items, page, size, total);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id)
    {
        var p = await db.Products.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id, x.Name, x.Description, x.Price, x.Stock, x.Category, x.ImageUrl, x.Active, x.CreatedAt,
                Avg = x.Reviews.Average(r => (double?)r.Rating),
                Cnt = x.Reviews.Count(),
                Sold = x.Orders.Where(o => o.Status != "Cancelled").Sum(o => (int?)o.Quantity) ?? 0
            }).FirstOrDefaultAsync();
        if (p is null) return NotFound();

        var latest = await db.Reviews.AsNoTracking().Where(r => r.ProductId == id)
            .OrderByDescending(r => r.CreatedAt).Take(5).Select(Projections.Review).ToListAsync();

        return new ProductDetailDto(p.Id, p.Name, p.Description, p.Price, p.Stock, p.Category, p.ImageUrl, p.Active,
            p.CreatedAt, p.Avg, p.Cnt, p.Sold, latest);
    }

    [HttpGet("{id:int}/reviews")]
    public async Task<ActionResult<List<ReviewDto>>> GetReviews(int id) =>
        await db.Reviews.AsNoTracking().Where(r => r.ProductId == id)
            .OrderByDescending(r => r.CreatedAt).Select(Projections.Review).ToListAsync();

    /// <summary>Kategóriánkénti összesítés.</summary>
    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryStatDto>>> Categories()
    {
        var rows = await db.Products.AsNoTracking().Where(p => p.Active == true)
            .GroupBy(p => p.Category)
            .OrderBy(g => g.Key)
            .Select(g => new { g.Key, Count = g.Count(), Stock = g.Sum(p => p.Stock), Min = g.Min(p => p.Price), Max = g.Max(p => p.Price), Avg = g.Average(p => p.Price) })
            .ToListAsync();
        return rows.Select(r => new CategoryStatDto(r.Key, r.Count, r.Stock, r.Min, r.Max, r.Avg)).ToList();
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<List<ProductListDto>>> LowStock([FromQuery] int threshold = 5) =>
        await db.Products.AsNoTracking().Where(p => p.Active == true && p.Stock <= threshold)
            .OrderBy(p => p.Stock).Select(Projections.ProductList).ToListAsync();

    [HttpGet("top-rated")]
    public async Task<ActionResult<List<ProductListDto>>> TopRated([FromQuery] int count = 5, [FromQuery] int minReviews = 1) =>
        await db.Products.AsNoTracking().Where(p => p.Reviews.Count() >= minReviews)
            .OrderByDescending(p => p.Reviews.Average(r => (double?)r.Rating))
            .Take(Math.Clamp(count, 1, 50)).Select(Projections.ProductList).ToListAsync();

    [HttpGet("best-sellers")]
    public async Task<ActionResult<List<BestSellerDto>>> BestSellers([FromQuery] int count = 5)
    {
        var rows = await db.Products.AsNoTracking()
            .Select(p => new
            {
                p.Id, p.Name,
                Qty = p.Orders.Where(o => o.Status != "Cancelled").Sum(o => (int?)o.Quantity) ?? 0,
                Revenue = p.Orders.Where(o => o.Status != "Cancelled").Sum(o => (decimal?)o.TotalPrice) ?? 0
            })
            .OrderByDescending(x => x.Qty).Take(Math.Clamp(count, 1, 50)).ToListAsync();
        return rows.Select(r => new BestSellerDto(r.Id, r.Name, r.Qty, r.Revenue)).ToList();
    }

    [HttpGet("never-ordered")]
    public async Task<ActionResult<List<ProductListDto>>> NeverOrdered() =>
        await db.Products.AsNoTracking().Where(p => !p.Orders.Any()).Select(Projections.ProductList).ToListAsync();

    [HttpGet("newest")]
    public async Task<ActionResult<List<ProductListDto>>> Newest([FromQuery] int count = 5) =>
        await db.Products.AsNoTracking().OrderByDescending(p => p.CreatedAt)
            .Take(Math.Clamp(count, 1, 50)).Select(Projections.ProductList).ToListAsync();

    [HttpPost]
    public async Task<ActionResult<ProductListDto>> Create(AddProductDto dto)
    {
        var p = new Product
        {
            Name = dto.Name, Description = dto.Description, Price = dto.Price, Stock = dto.Stock,
            Category = dto.Category, ImageUrl = dto.ImageUrl, Active = true, CreatedAt = DateTime.Now
        };
        db.Products.Add(p);
        await db.SaveChangesAsync();
        var result = await db.Products.AsNoTracking().Where(x => x.Id == p.Id).Select(Projections.ProductList).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = p.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();
        p.Name = dto.Name; p.Description = dto.Description; p.Price = dto.Price; p.Stock = dto.Stock;
        p.Category = dto.Category; p.ImageUrl = dto.ImageUrl; p.Active = dto.Active;
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Készletmódosítás (+ beszállítás / - kivét).</summary>
    [HttpPatch("{id:int}/stock")]
    public async Task<ActionResult<object>> ChangeStock(int id, StockChangeDto dto)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();
        if (p.Stock + dto.Delta < 0) return BadRequest(new { message = "A készlet nem mehet nulla alá." });
        p.Stock += dto.Delta;
        await db.SaveChangesAsync();
        return new { p.Id, p.Name, p.Stock };
    }

    /// <summary>Lágy törlés: a termék inaktív lesz (a rendelések megmaradnak, a CASCADE miatt a végleges törlés elvinné őket).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();
        p.Active = false;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
