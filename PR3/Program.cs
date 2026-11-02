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

app.Run();