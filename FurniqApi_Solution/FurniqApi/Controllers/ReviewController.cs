using FurniqApi.Models.DTOs;
using FurniqApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurniqApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController(FurniqContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ReviewDto>>> GetAll([FromQuery] int? productId, [FromQuery] int? userId, [FromQuery] int? minRating) =>
        await db.Reviews.AsNoTracking()
            .Where(r => !productId.HasValue || r.ProductId == productId)
            .Where(r => !userId.HasValue || r.UserId == userId)
            .Where(r => !minRating.HasValue || r.Rating >= minRating)
            .OrderByDescending(r => r.CreatedAt).Select(Projections.Review).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReviewDto>> GetById(int id)
    {
        var r = await db.Reviews.AsNoTracking().Where(x => x.Id == id).Select(Projections.Review).FirstOrDefaultAsync();
        return r is null ? NotFound() : r;
    }

    /// <summary>Egy termék átlaga, darabszáma és a csillagok eloszlása.</summary>
    [HttpGet("product/{productId:int}/summary")]
    public async Task<ActionResult<ReviewSummaryDto>> Summary(int productId)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
        if (product is null) return NotFound();

        var dist = await db.Reviews.AsNoTracking().Where(r => r.ProductId == productId)
            .GroupBy(r => r.Rating).OrderByDescending(g => g.Key).Select(g => new RatingBucketDto(g.Key, g.Count())).ToListAsync();

        var count = dist.Sum(d => d.Count);
        double? avg = count == 0 ? null : dist.Sum(d => d.Rating * d.Count) / (double)count;
        return new ReviewSummaryDto(productId, product.Name, avg, count, dist);
    }

    /// <summary>Vélemény írása. Termékenként felhasználónként egy vélemény engedélyezett.</summary>
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create(AddReviewDto dto)
    {
        if (!await db.Users.AnyAsync(u => u.Id == dto.UserId)) return NotFound(new { message = "Nincs ilyen felhasználó." });
        if (!await db.Products.AnyAsync(p => p.Id == dto.ProductId)) return NotFound(new { message = "Nincs ilyen termék." });
        if (await db.Reviews.AnyAsync(r => r.UserId == dto.UserId && r.ProductId == dto.ProductId))
            return Conflict(new { message = "Ehhez a termékhez már írtál véleményt." });

        var review = new Review { UserId = dto.UserId, ProductId = dto.ProductId, Rating = dto.Rating, Comment = dto.Comment, CreatedAt = DateTime.Now };
        db.Reviews.Add(review);
        await db.SaveChangesAsync();

        var result = await db.Reviews.AsNoTracking().Where(r => r.Id == review.Id).Select(Projections.Review).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = review.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateReviewDto dto)
    {
        var r = await db.Reviews.FindAsync(id);
        if (r is null) return NotFound();
        r.Rating = dto.Rating; r.Comment = dto.Comment;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await db.Reviews.FindAsync(id);
        if (r is null) return NotFound();
        db.Reviews.Remove(r);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
