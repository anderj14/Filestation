using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Data;
using API.model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static API.model.AuthModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAntiforgery(options =>
{
    options.SuppressXFrameOptionsHeader = true;
});

builder.Services.AddSwaggerGen();

builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
});

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(opt =>
{
    opt.AddFixedWindowLimiter("login", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
    });
});

builder.Services.AddCors();

var app = builder.Build();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors(x =>
    x.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin()
        .WithOrigins("http://localhost:4200", "https://localhost:4200")
    );

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();


app.MapGet("/api/", () => "Hello World!");

// Account

app.MapPost("/api/auth/register", [EnableRateLimiting("register")] async (RegisterRequest request, UserManager<AppUser> userManager) =>
{
    var user = new AppUser { UserName = request.Email, Email = request.Email };
    var result = await userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
        var errors = result.Errors.Select(e => e.Description);
        return Results.BadRequest(new { Error = errors });
    }

    return Results.Ok(new { Message = "User Registered" });
});


app.MapPost("/api/auth/login", [EnableRateLimiting("login")] async (
    LoginRequest request, 
    UserManager<AppUser> userManager, 
    IConfiguration config) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);

    if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        return Results.Unauthorized();

    // Si el usuario no tiene el email confirmado, lo confirmamos aquí
    if (!user.EmailConfirmed)
    {
        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user); // Guardamos el cambio
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email!)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: config["Jwt:Issuer"],
        audience: config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.Now.AddHours(2),
        signingCredentials: creds
    );

    return Results.Ok(new AuthResponse(
        Token: new JwtSecurityTokenHandler().WriteToken(token),
        Email: user.Email!,
        Username: user.UserName!
    ));
});

// File

app.MapPost("/api/upload", [Authorize] async (
    IFormFile file,
    AppDbContext db,
    UserManager<AppUser> userManager,
    ClaimsPrincipal userPrincipal
    ) =>
{
    if (file.Length == 0) return Results.BadRequest("Empty file.");
    if (file.Length > 10 * 1024 * 1024) return Results.BadRequest("Maximum size: 10MB");

    var user = await userManager.GetUserAsync(userPrincipal);
    if (user == null) return Results.Unauthorized();

    var usedBytes = await db.Files
    .Where(f => f.UserId == user.Id)
    .SumAsync(f => (long)f.Data.Length);

    const long MAX_STORAGE_BYTES = 100 * 1024 * 1024;

    if (usedBytes + file.Length > MAX_STORAGE_BYTES)
        return Results.BadRequest("Exceed your 100MB storage limit.");

    using var memoryStream = new MemoryStream();
    await file.CopyToAsync(memoryStream);

    var fileEntity = new FileEntity
    {
        FileName = $"{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}",
        ContentType = file.ContentType,
        Data = memoryStream.ToArray(),
        UserId = user.Id
    };

    db.Files.Add(fileEntity);
    await db.SaveChangesAsync();

    return Results.Ok(new { fileEntity.Id, fileEntity.FileName });
}).DisableAntiforgery();

app.MapGet("/api/auth/current-user", [Authorize] async (
    UserManager<AppUser> userManager,
    ClaimsPrincipal userPrincipal
) =>
{
    var user = await userManager.GetUserAsync(userPrincipal);
    if (user == null) return Results.Unauthorized();

    return Results.Ok(new
    {
        Id = user.Id,
        Email = user.Email,
        Username = user.UserName
    });
});



app.MapGet("/api/files", [Authorize] async (
    AppDbContext db,
    UserManager<AppUser> userManager,
    ClaimsPrincipal userPrincipal
    ) =>
{
    var user = await userManager.GetUserAsync(userPrincipal);
    if (user == null) return Results.Unauthorized();

    var files = await db.Files
    .Where(f => f.UserId == user.Id)
    .Select(f => new { f.Id, f.FileName, f.UploadDate, Size = f.Data.Length })
    .ToListAsync();

    var totalUsed = files.Sum(f => (long)f.Size);
    const long MAX_STORAGE_BYTES = 100 * 1024 * 1024;

    return Results.Ok(new
    {
        StorageUsedBytes = totalUsed,
        StorageUsedMB = Math.Round(totalUsed / 1024.0 / 1024.0, 2),
        StorageLimitBytes = MAX_STORAGE_BYTES,
        StorageLimitMB = 100,
        Files = files
    });
});

app.MapGet("/api/download/{id}", [Authorize] async (
    Guid id,
    AppDbContext db,
    UserManager<AppUser> userManager,
    ClaimsPrincipal userPrincipal
    ) =>
{
    var user = await userManager.GetUserAsync(userPrincipal);
    if (user == null) return Results.Unauthorized();

    var fileEntity = await db.Files.FirstOrDefaultAsync(f => f.Id == id && f.UserId == user.Id);
    if (fileEntity == null) return Results.NotFound("");

    return Results.File(fileEntity.Data, fileEntity.ContentType, fileEntity.FileName);
});

app.MapDelete("/api/files/{id}", [Authorize] async (
    Guid id,
    AppDbContext db,
    UserManager<AppUser> userManager,
    ClaimsPrincipal userPrincipal
    ) =>
{
    var user = await userManager.GetUserAsync(userPrincipal);
    if (user == null) return Results.Unauthorized();

    var file = await db.Files.FirstOrDefaultAsync(f => f.Id == id && f.UserId == user.Id); ;
    if (file == null) return Results.NotFound();

    db.Files.Remove(file);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
