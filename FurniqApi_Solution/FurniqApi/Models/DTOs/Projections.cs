using System.Linq.Expressions;

namespace FurniqApi.Models.DTOs;

/// <summary>Újrahasznosítható EF-lekérdezési leképezések (entitás -> DTO).</summary>
public static class Projections
{
    public static readonly Expression<Func<Product, ProductListDto>> ProductList = p => new ProductListDto(
        p.Id, p.Name, p.Description, p.Price, p.Stock, p.Category, p.ImageUrl, p.Active,
        p.Reviews.Average(r => (double?)r.Rating), p.Reviews.Count());

    public static readonly Expression<Func<Review, ReviewDto>> Review = r => new ReviewDto(
        r.Id, r.UserId, r.User!.Name, r.ProductId, r.Product!.Name, r.Rating, r.Comment, r.CreatedAt);

    public static readonly Expression<Func<Order, OrderDto>> Order = o => new OrderDto(
        o.Id, o.UserId, o.User!.Name, o.ProductId, o.Product!.Name, o.Quantity, o.UnitPrice,
        o.OrderDate, o.Status, o.PaymentMethod, o.ShippingMethod, o.TotalPrice);

    public static readonly Expression<Func<User, UserDto>> User = u => new UserDto(
        u.Id, u.Name, u.Email, u.Phone, u.Role, u.RegistrationDate);

    public static readonly Expression<Func<Address, AddressDto>> Address = a => new AddressDto(
        a.Id, a.UserId, a.Name, a.PostalCode, a.City, a.Street, a.HouseNumber, a.IsDefault ?? false);

    public static Expression<Func<T, RoomItemDto>> RoomItem<T>(string room) where T : RoomItem =>
        x => new RoomItemDto(room, x.Id, x.Name, x.Description, x.Price, x.Stock, x.ImageUrl, x.Active);
}
