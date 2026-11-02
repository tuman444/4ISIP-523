using Microsoft.EntityFrameworkCore;
using PR3.Models;   
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddDbContext<PR3Context>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        IssuerSigningKey = jwtKey
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

Expression<Func<Clothe, ClothDto>> toDto = c => new ClothDto(
    c.Id, c.Name, c.Price, c.Description, c.PurchaseCount, c.IsAvailable,
    c.Type.Name,
    c.Sizes.Select(s => new SizeDto(s.Id, s.Name, s.Number, s.Quantity)).ToList());

// показ товаров по запросу
app.MapGet("/clothes", async (PR3Context db) =>
    await db.Clothes.Select(toDto).ToListAsync());

// поиск товаров
app.MapGet("/clothes/search", async (string name, PR3Context db) =>
    await db.Clothes
        .Where(c => c.Name.Contains(name))
        .Select(toDto)
        .ToListAsync());

// регистрация
app.MapPost("/auth/register", async (RegisterDto dto, PR3Context db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Login) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.BadRequest("Логин и пароль обязательны");
    if (await db.Users.AnyAsync(u => u.Login == dto.Login)) return Results.Conflict("Логин уже занят");

    var role = await db.Roles.FirstAsync(r => r.Name == "User");

    db.Users.Add(new User
    {
        Login = dto.Login,
        Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        PhoneNumber = dto.PhoneNumber,
        Address = dto.Address,
        RoleId = role.Id
    });
    await db.SaveChangesAsync();
    return Results.Ok("Пользователь зарегистрирован");
});

// авторизация
app.MapPost("/auth/authorization", async (LoginDto dto, PR3Context db) =>
{
    var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Login == dto.Login);

    if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        return Results.Unauthorized();

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Login),
        new Claim(ClaimTypes.Role, user.Role.Name)
    };
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256));
    return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
});

app.MapGet("/me", (ClaimsPrincipal me) => new
{
    Id = me.FindFirstValue(ClaimTypes.NameIdentifier),
    Login = me.FindFirstValue(ClaimTypes.Name),
    Role = me.FindFirstValue(ClaimTypes.Role)
}).RequireAuthorization();


// заказы, доступны только менеджеру
int GetUserId(ClaimsPrincipal p) => int.Parse(p.FindFirstValue(ClaimTypes.NameIdentifier)!);

Expression<Func<Order, OrderDto>> toOrderDto = o => new OrderDto(
    o.Id, o.Date, o.Status.Name, o.UserId, o.User.Login,
    o.ClothesOrders.Sum(i => i.Quantity * i.UnitPrice),
    o.ClothesOrders.Select(i => new OrderItemInfo(
        i.Size.Clothes.Name, i.Size.Name, i.Quantity, i.UnitPrice)).ToList());

app.MapPost("/orders", async (CreateOrderDto dto, ClaimsPrincipal me, PR3Context db) =>
{
    if (dto.Items is null || dto.Items.Count == 0 || dto.Items.Any(i => i.Quantity <= 0))
        return Results.BadRequest("Укажите позиции с количеством больше 0");

    var status = await db.Statuses.FirstAsync(s => s.Name == "Новый");
    var order = new Order { Date = DateTime.Now, StatusId = status.Id, UserId = GetUserId(me) };

    foreach (var item in dto.Items)
    {
        var size = await db.Sizes.Include(s => s.Clothes)
            .FirstOrDefaultAsync(s => s.Id == item.SizeId);

        if (size is null)
            return Results.BadRequest($"Размер {item.SizeId} не найден");
        if (!size.Clothes.IsAvailable)
            return Results.BadRequest($"Товар «{size.Clothes.Name}» снят с продажи");
        if (size.Quantity < item.Quantity)
            return Results.BadRequest($"Недостаточно товара «{size.Clothes.Name}» ({size.Name}): осталось {size.Quantity}");

        size.Quantity -= item.Quantity;
        size.Clothes.PurchaseCount += item.Quantity;
        order.ClothesOrders.Add(new ClothesOrder
        {
            SizeId = size.Id,
            Quantity = item.Quantity,
            UnitPrice = size.Clothes.Price
        });
    }

    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Results.Ok(new { order.Id });
}).RequireAuthorization();

app.MapGet("/orders", async (int statusId, ClaimsPrincipal me, PR3Context db) =>
{
    var query = db.Orders.Where(o => o.StatusId == statusId);
    if (!me.IsInRole("Manager"))
    {
        var userId = GetUserId(me);
        query = query.Where(o => o.UserId == userId);
    }
    return await query.Select(toOrderDto).ToListAsync();
}).RequireAuthorization();

app.MapGet("/users/{id}/orders", async (int id, ClaimsPrincipal me, PR3Context db) =>
{
    if (!me.IsInRole("Manager") && GetUserId(me) != id)
        return Results.Forbid();

    var orders = await db.Orders.Where(o => o.UserId == id).Select(toOrderDto).ToListAsync();
    return Results.Ok(orders);
}).RequireAuthorization();

app.Run();