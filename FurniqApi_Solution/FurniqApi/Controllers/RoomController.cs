using FurniqApi.Models.DTOs;
using FurniqApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurniqApi.Controllers;

/// <summary>A szobánkénti terméktáblák (bathroom, bedroom, hallway, kitchen, livingroom) egységes kezelése.</summary>
[ApiController]
[Route("api/rooms")]
public class RoomController(FurniqContext db) : ControllerBase
{
    private static readonly string[] RoomNames = ["bathroom", "bedroom", "hallway", "kitchen", "livingroom"];

    private IQueryable<RoomItemDto>? Query(string room) => room.ToLowerInvariant() switch
    {
        "bathroom" => db.Bathroom.AsNoTracking().Select(Projections.RoomItem<Bathroom>("bathroom")),
        "bedroom" => db.Bedroom.AsNoTracking().Select(Projections.RoomItem<Bedroom>("bedroom")),
        "hallway" => db.Hallway.AsNoTracking().Select(Projections.RoomItem<Hallway>("hallway")),
        "kitchen" => db.Kitchen.AsNoTracking().Select(Projections.RoomItem<Kitchen>("kitchen")),
        "livingroom" => db.Livingroom.AsNoTracking().Select(Projections.RoomItem<Livingroom>("livingroom")),
        _ => null
    };

    private static Type? EntityType(string room) => room.ToLowerInvariant() switch
    {
        "bathroom" => typeof(Bathroom),
        "bedroom" => typeof(Bedroom),
        "hallway" => typeof(Hallway),
        "kitchen" => typeof(Kitchen),
        "livingroom" => typeof(Livingroom),
        _ => null
    };

    [HttpGet]
    public ActionResult<string[]> GetRooms() => RoomNames;

    /// <summary>Az összes szoba termékei egy listában (opcionális keresés / ársáv).</summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<RoomItemDto>>> GetAllRooms([FromQuery] string? search, [FromQuery] decimal? maxPrice)
    {
        var result = new List<RoomItemDto>();
        foreach (var name in RoomNames)
        {
            var q = Query(name)!;
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.Name.Contains(search));
            if (maxPrice.HasValue) q = q.Where(x => x.Price <= maxPrice);
            result.AddRange(await q.ToListAsync());
        }
        return result.OrderBy(x => x.Price).ToList();
    }

    /// <summary>Szobánkénti összesítés: darabszám, készlet, készletérték, átlagár.</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<object>> Summary()
    {
        var list = new List<object>();
        foreach (var name in RoomNames)
        {
            var items = await Query(name)!.ToListAsync();
            list.Add(new
            {
                Room = name,
                ItemCount = items.Count,
                TotalStock = items.Sum(i => i.Stock),
                StockValue = items.Sum(i => i.Price * i.Stock),
                AvgPrice = items.Count == 0 ? 0 : items.Average(i => i.Price)
            });
        }
        return list;
    }

    [HttpGet("{room}")]
    public async Task<ActionResult<List<RoomItemDto>>> GetByRoom(string room, [FromQuery] string? search,
        [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] bool? inStock)
    {
        var q = Query(room);
        if (q is null) return NotFound(new { message = $"Ismeretlen szoba. Elérhető: {string.Join(", ", RoomNames)}." });
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.Name.Contains(search) || (x.Description != null && x.Description.Contains(search)));
        if (minPrice.HasValue) q = q.Where(x => x.Price >= minPrice);
        if (maxPrice.HasValue) q = q.Where(x => x.Price <= maxPrice);
        if (inStock == true) q = q.Where(x => x.Stock > 0);
        return await q.OrderBy(x => x.Name).ToListAsync();
    }

    [HttpGet("{room}/{id:int}")]
    public async Task<ActionResult<RoomItemDto>> GetOne(string room, int id)
    {
        var q = Query(room);
        if (q is null) return NotFound(new { message = "Ismeretlen szoba." });
        var item = await q.FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : item;
    }

    [HttpPost("{room}")]
    public async Task<ActionResult<RoomItemDto>> Create(string room, AddRoomItemDto dto)
    {
        var type = EntityType(room);
        if (type is null) return NotFound(new { message = "Ismeretlen szoba." });

        var item = (RoomItem)Activator.CreateInstance(type)!;
        Fill(item, dto);
        db.Add(item);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOne), new { room = room.ToLowerInvariant(), id = item.Id },
            new RoomItemDto(room.ToLowerInvariant(), item.Id, item.Name, item.Description, item.Price, item.Stock, item.ImageUrl, item.Active));
    }

    [HttpPut("{room}/{id:int}")]
    public async Task<IActionResult> Update(string room, int id, UpdateRoomItemDto dto)
    {
        var type = EntityType(room);
        if (type is null) return NotFound(new { message = "Ismeretlen szoba." });
        if (await db.FindAsync(type, id) is not RoomItem item) return NotFound();
        Fill(item, dto);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{room}/{id:int}/stock")]
    public async Task<ActionResult<object>> ChangeStock(string room, int id, StockChangeDto dto)
    {
        var type = EntityType(room);
        if (type is null) return NotFound(new { message = "Ismeretlen szoba." });
        if (await db.FindAsync(type, id) is not RoomItem item) return NotFound();
        if (item.Stock + dto.Delta < 0) return BadRequest(new { message = "A készlet nem mehet nulla alá." });
        item.Stock += dto.Delta;
        await db.SaveChangesAsync();
        return new { item.Id, item.Name, item.Stock };
    }

    [HttpDelete("{room}/{id:int}")]
    public async Task<IActionResult> Delete(string room, int id)
    {
        var type = EntityType(room);
        if (type is null) return NotFound(new { message = "Ismeretlen szoba." });
        if (await db.FindAsync(type, id) is not RoomItem item) return NotFound();
        db.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static void Fill(RoomItem item, AddRoomItemDto dto)
    {
        item.Name = dto.Name; item.Description = dto.Description; item.Price = dto.Price;
        item.Stock = dto.Stock; item.ImageUrl = dto.ImageUrl; item.Active = dto.Active;
    }
}
