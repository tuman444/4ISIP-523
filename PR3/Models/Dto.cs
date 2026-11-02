namespace PR3.Models
{
    public record SizeDto(int Id, string Name, int Number, int Quantity);

    public record ClothDto(
        int Id, string Name, decimal Price, string Description,
        int PurchaseCount, bool IsAvailable, string Type, List<SizeDto> Sizes);
    public record RegisterDto(string Login, string Password, string PhoneNumber, string Address);

    public record LoginDto(string Login, string Password);
}
