using FurniqApi.Models.DTOs;
using FurniqApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurniqApi.Controllers;

[ApiController]
[Route("api")]
public class AddressController(FurniqContext db) : ControllerBase
{
    [HttpGet("users/{userId:int}/addresses")]
    public async Task<ActionResult<List<AddressDto>>> GetForUser(int userId) =>
        await db.Addresses.AsNoTracking().Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault).ThenBy(a => a.Id).Select(Projections.Address).ToListAsync();

    [HttpGet("addresses/{id:int}")]
    public async Task<ActionResult<AddressDto>> GetById(int id)
    {
        var a = await db.Addresses.AsNoTracking().Where(x => x.Id == id).Select(Projections.Address).FirstOrDefaultAsync();
        return a is null ? NotFound() : a;
    }

    /// <summary>Város szerinti keresés (pl. hány címünk van Miskolcon).</summary>
    [HttpGet("addresses/by-city/{city}")]
    public async Task<ActionResult<List<AddressDto>>> ByCity(string city) =>
        await db.Addresses.AsNoTracking().Where(a => a.City == city).Select(Projections.Address).ToListAsync();

    [HttpPost("users/{userId:int}/addresses")]
    public async Task<ActionResult<AddressDto>> Create(int userId, AddAddressDto dto)
    {
        if (!await db.Users.AnyAsync(u => u.Id == userId)) return NotFound(new { message = "Nincs ilyen felhasználó." });

        var hasAny = await db.Addresses.AnyAsync(a => a.UserId == userId);
        var makeDefault = dto.IsDefault || !hasAny; // az első cím automatikusan alapértelmezett
        if (makeDefault) await ClearDefault(userId);

        var a = new Address
        {
            UserId = userId, Name = dto.Name, PostalCode = dto.PostalCode, City = dto.City,
            Street = dto.Street, HouseNumber = dto.HouseNumber, IsDefault = makeDefault
        };
        db.Addresses.Add(a);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = a.Id }, new AddressDto(a.Id, a.UserId, a.Name, a.PostalCode, a.City, a.Street, a.HouseNumber, makeDefault));
    }

    [HttpPut("addresses/{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateAddressDto dto)
    {
        var a = await db.Addresses.FindAsync(id);
        if (a is null) return NotFound();
        if (dto.IsDefault) await ClearDefault(a.UserId);
        a.Name = dto.Name; a.PostalCode = dto.PostalCode; a.City = dto.City;
        a.Street = dto.Street; a.HouseNumber = dto.HouseNumber; a.IsDefault = dto.IsDefault;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("addresses/{id:int}/default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var a = await db.Addresses.FindAsync(id);
        if (a is null) return NotFound();
        await ClearDefault(a.UserId);
        a.IsDefault = true;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("addresses/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await db.Addresses.FindAsync(id);
        if (a is null) return NotFound();
        db.Addresses.Remove(a);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task ClearDefault(int userId)
    {
        var defaults = await db.Addresses.Where(a => a.UserId == userId && a.IsDefault == true).ToListAsync();
        foreach (var d in defaults) d.IsDefault = false;
    }
}
