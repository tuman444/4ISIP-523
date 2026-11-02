namespace PR3.Models
{
    public record SizeDto(int Id, string Name, int Number, int Quantity);

    public record ClothDto(
        int Id, string Name, decimal Price, string Description,
        int PurchaseCount, bool IsAvailable, string Type, List<SizeDto> Sizes);
    public record RegisterDto(string Login, string Password, string PhoneNumber, string Address);

    public record LoginDto(string Login, string Password);

    public record OrderItemDto(int SizeId, int Quantity);
    public record CreateOrderDto(List<OrderItemDto> Items);

    public record OrderItemInfo(string Cloth, string Size, int Quantity, decimal UnitPrice);
    public record OrderDto(int Id, DateTime Date, string Status, int UserId, 
        string UserLogin, decimal Total, List<OrderItemInfo> Items);
}
