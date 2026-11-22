using FurniqApi.Models.DTOs;
using FurniqApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurniqApi.Controllers;

[ApiController]
[Route("api/users/{userId:int}/cart")]
public class CartController(FurniqContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(int userId)
    {
        if (!await db.Users.AnyAsync(u => u.Id == userId)) return NotFound(new { message = "Nincs ilyen felhasználó." });
        return await BuildCart(userId);
    }

    /// <summary>Termék kosárba tétele. Ha már benne van, a mennyiség összeadódik.</summary>
    [HttpPost]
    public async Task<ActionResult<CartDto>> Add(int userId, AddCartDto dto)
    {
        if (!await db.Users.AnyAsync(u => u.Id == userId)) return NotFound(new { message = "Nincs ilyen felhasználó." });
        var product = await db.Products.FindAsync(dto.ProductId);
        if (product is null || product.Active != true) return NotFound(new { message = "Nincs ilyen termék." });

        var item = await db.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == dto.ProductId);
        var newQty = (item?.Quantity ?? 0) + dto.Quantity;
        if (newQty > product.Stock) return BadRequest(new { message = $"Nincs elég készlet (elérhető: {product.Stock})." });

        if (item is null)
            db.Carts.Add(new Cart { UserId = userId, ProductId = dto.ProductId, Quantity = dto.Quantity, AddedAt = DateTime.Now });
        else
            item.Quantity = newQty;

        await db.SaveChangesAsync();
        return await BuildCart(userId);
    }

    [HttpPut("{cartId:int}")]
    public async Task<ActionResult<CartDto>> UpdateQuantity(int userId, int cartId, UpdateCartDto dto)
    {
        var item = await db.Carts.Include(c => c.Product).FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);
        if (item is null) return NotFound();
        if (dto.Quantity > item.Product!.Stock) return BadRequest(new { message = $"Nincs elég készlet (elérhető: {item.Product.Stock})." });
        item.Quantity = dto.Quantity;
        await db.SaveChangesAsync();
        return await BuildCart(userId);
    }

    [HttpDelete("{cartId:int}")]
    public async Task<ActionResult<CartDto>> Remove(int userId, int cartId)
    {
        var item = await db.Carts.FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);
        if (item is null) return NotFound();
        db.Carts.Remove(item);
        await db.SaveChangesAsync();
        return await BuildCart(userId);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(int userId)
    {
        await db.Carts.Where(c => c.UserId == userId).ExecuteDeleteAsync();
        return NoContent();
    }

    /// <summary>Fizetés: a kosár tételeiből rendelések lesznek, a készlet csökken, a kosár kiürül (egy tranzakcióban).</summary>
    [HttpPost("checkout")]
    public async Task<ActionResult<List<OrderDto>>> Checkout(int userId, CheckoutDto dto)
    {
        var items = await db.Carts.Include(c => c.Product).Where(c => c.UserId == userId).ToListAsync();
        if (items.Count == 0) return BadRequest(new { message = "A kosár üres." });

        foreach (var c in items)
            if (c.Product is null || c.Product.Active != true || c.Product.Stock < c.Quantity)
                return BadRequest(new { message = $"Nincs elég készlet ehhez: {c.Product?.Name ?? "ismeretlen termék"}." });

        await using var tx = await db.Database.BeginTransactionAsync();
        var now = DateTime.Now;
        var orders = new List<Order>();
        foreach (var c in items)
        {
            c.Product!.Stock -= c.Quantity;
            orders.Add(new Order
            {
                UserId = userId, ProductId = c.ProductId, Quantity = c.Quantity, UnitPrice = c.Product.Price,
                TotalPrice = c.Product.Price * c.Quantity, OrderDate = now, Status = "Processing",
                PaymentMethod = dto.PaymentMethod, ShippingMethod = dto.ShippingMethod
            });
        }
        db.Orders.AddRange(orders);
        db.Carts.RemoveRange(items);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        var ids = orders.Select(o => o.Id).ToList();
        return await db.Orders.AsNoTracking().Where(o => ids.Contains(o.Id)).Select(Projections.Order).ToListAsync();
    }

    private async Task<CartDto> BuildCart(int userId)
    {
        var items = await db.Carts.AsNoTracking().Where(c => c.UserId == userId)
            .Select(c => new CartItemDto(c.Id, c.ProductId, c.Product!.Name, c.Product.ImageUrl, c.Product.Price,
                c.Quantity, c.Product.Price * c.Quantity, c.Product.Stock))
            .ToListAsync();
        return new CartDto(userId, items, items.Sum(i => i.Quantity), items.Sum(i => i.LineTotal));
    }
}
