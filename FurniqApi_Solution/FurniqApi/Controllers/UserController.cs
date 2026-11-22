using FurniqApi.Models.DTOs;
using FurniqApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurniqApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(FurniqContext db) : ControllerBase
{
    private static readonly PasswordHasher<User> Hasher = new();

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll([FromQuery] string? search, [FromQuery] string? role) =>
        await db.Users.AsNoTracking()
            .Where(u => string.IsNullOrWhiteSpace(search) || u.Name.Contains(search) || u.Email.Contains(search))
            .Where(u => string.IsNullOrWhiteSpace(role) || u.Role == role)
            .OrderBy(u => u.Name).Select(Projections.User).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDetailDto>> GetById(int id)
    {
        var u = await db.Users.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id, x.Name, x.Email, x.Phone, x.Role, x.RegistrationDate,
                Orders = x.Orders.Count(),
                Spent = x.Orders.Where(o => o.Status != "Cancelled").Sum(o => (decimal?)o.TotalPrice) ?? 0,
                Reviews = x.Reviews.Count(),
                Cart = x.Carts.Sum(c => (int?)c.Quantity) ?? 0
            }).FirstOrDefaultAsync();
        if (u is null) return NotFound();

        var addresses = await db.Addresses.AsNoTracking().Where(a => a.UserId == id)
            .OrderByDescending(a => a.IsDefault).Select(Projections.Address).ToListAsync();

        return new UserDetailDto(u.Id, u.Name, u.Email, u.Phone, u.Role, u.RegistrationDate,
            u.Orders, u.Spent, u.Reviews, u.Cart, addresses);
    }

    [HttpGet("{id:int}/orders")]
    public async Task<ActionResult<List<OrderDto>>> GetOrders(int id) =>
        await db.Orders.AsNoTracking().Where(o => o.UserId == id)
            .OrderByDescending(o => o.OrderDate).Select(Projections.Order).ToListAsync();

    [HttpGet("{id:int}/reviews")]
    public async Task<ActionResult<List<ReviewDto>>> GetReviews(int id) =>
        await db.Reviews.AsNoTracking().Where(r => r.UserId == id)
            .OrderByDescending(r => r.CreatedAt).Select(Projections.Review).ToListAsync();

    /// <summary>Regisztráció – a jelszó hash-elve kerül az adatbázisba.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(AddUserDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { message = "Ezzel az e-mail címmel már van regisztráció." });

        var user = new User { Name = dto.Name, Email = dto.Email, Phone = dto.Phone, Role = "user", RegistrationDate = DateTime.Now };
        user.PasswordHash = Hasher.HashPassword(user, dto.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await db.Users.AsNoTracking().Where(u => u.Id == user.Id).Select(Projections.User).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, result);
    }

    /// <summary>Bejelentkezés. A mintaadatok sima szöveges jelszavait is elfogadja, és első belépéskor átalakítja hash-sé.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !VerifyPassword(user, dto.Password))
            return Unauthorized(new { message = "Hibás e-mail vagy jelszó." });

        if (user.PasswordHash == dto.Password) // régi, hash nélküli jelszó -> frissítés
        {
            user.PasswordHash = Hasher.HashPassword(user, dto.Password);
            await db.SaveChangesAsync();
        }
        return new UserDto(user.Id, user.Name, user.Email, user.Phone, user.Role, user.RegistrationDate);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserDto dto)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (await db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
            return Conflict(new { message = "Ez az e-mail cím már foglalt." });
        user.Name = dto.Name; user.Email = dto.Email; user.Phone = dto.Phone;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/role")]
    public async Task<IActionResult> ChangeRole(int id, ChangeRoleDto dto)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        user.Role = dto.Role;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/password")]
    public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto dto)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (!VerifyPassword(user, dto.OldPassword)) return BadRequest(new { message = "A régi jelszó hibás." });
        user.PasswordHash = Hasher.HashPassword(user, dto.NewPassword);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        db.Users.Remove(user); // a címek, kosár, rendelések, vélemények is törlődnek (CASCADE)
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static bool VerifyPassword(User user, string password)
    {
        if (user.PasswordHash == password) return true; // seed adatok (pl. 'test123')
        try
        {
            return Hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
        }
        catch (FormatException) { return false; }
    }
}
