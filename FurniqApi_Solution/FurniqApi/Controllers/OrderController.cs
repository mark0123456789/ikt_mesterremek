using FurniqApi.Models.DTOs;
using FurniqApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurniqApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController(FurniqContext db) : ControllerBase
{
    private static readonly string[] Statuses = ["Processing", "Shipped", "Delivered", "Cancelled"];

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetAll([FromQuery] OrderQuery q)
    {
        var page = Math.Max(q.Page, 1);
        var size = Math.Clamp(q.PageSize, 1, 100);

        var query = db.Orders.AsNoTracking().AsQueryable();
        if (q.UserId.HasValue) query = query.Where(o => o.UserId == q.UserId);
        if (q.ProductId.HasValue) query = query.Where(o => o.ProductId == q.ProductId);
        if (!string.IsNullOrWhiteSpace(q.Status)) query = query.Where(o => o.Status == q.Status);
        if (!string.IsNullOrWhiteSpace(q.PaymentMethod)) query = query.Where(o => o.PaymentMethod == q.PaymentMethod);
        if (!string.IsNullOrWhiteSpace(q.ShippingMethod)) query = query.Where(o => o.ShippingMethod == q.ShippingMethod);
        if (q.From.HasValue) query = query.Where(o => o.OrderDate >= q.From);
        if (q.To.HasValue) query = query.Where(o => o.OrderDate <= q.To);
        if (q.MinTotal.HasValue) query = query.Where(o => o.TotalPrice >= q.MinTotal);
        if (q.MaxTotal.HasValue) query = query.Where(o => o.TotalPrice <= q.MaxTotal);

        query = q.Desc ? query.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id)
                       : query.OrderBy(o => o.OrderDate).ThenBy(o => o.Id);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).Select(Projections.Order).ToListAsync();
        return new PagedResult<OrderDto>(items, page, size, total);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var o = await db.Orders.AsNoTracking().Where(x => x.Id == id).Select(Projections.Order).FirstOrDefaultAsync();
        return o is null ? NotFound() : o;
    }

    /// <summary>Közvetlen rendelés (kosár nélkül): készletet ellenőriz és csökkent.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(AddOrderDto dto)
    {
        if (!await db.Users.AnyAsync(u => u.Id == dto.UserId)) return NotFound(new { message = "Nincs ilyen felhasználó." });
        var product = await db.Products.FindAsync(dto.ProductId);
        if (product is null || product.Active != true) return NotFound(new { message = "Nincs ilyen termék." });
        if (product.Stock < dto.Quantity) return BadRequest(new { message = $"Nincs elég készlet (elérhető: {product.Stock})." });

        product.Stock -= dto.Quantity;
        var order = new Order
        {
            UserId = dto.UserId, ProductId = dto.ProductId, Quantity = dto.Quantity, UnitPrice = product.Price,
            TotalPrice = product.Price * dto.Quantity, OrderDate = DateTime.Now, Status = "Processing",
            PaymentMethod = dto.PaymentMethod, ShippingMethod = dto.ShippingMethod
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var result = await db.Orders.AsNoTracking().Where(o => o.Id == order.Id).Select(Projections.Order).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, result);
    }

    /// <summary>Státuszváltás. Lemondáskor a készlet visszakerül.</summary>
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<OrderDto>> ChangeStatus(int id, UpdateOrderDto dto)
    {
        if (!Statuses.Contains(dto.Status)) return BadRequest(new { message = $"Érvényes státuszok: {string.Join(", ", Statuses)}." });

        var order = await db.Orders.Include(o => o.Product).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();
        if (order.Status == "Cancelled") return BadRequest(new { message = "Lemondott rendelés státusza nem módosítható." });

        if (dto.Status == "Cancelled" && order.Product is not null) order.Product.Stock += order.Quantity;
        order.Status = dto.Status;
        await db.SaveChangesAsync();

        return await db.Orders.AsNoTracking().Where(o => o.Id == id).Select(Projections.Order).FirstAsync();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null) return NotFound();
        db.Orders.Remove(order);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
