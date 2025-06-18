using API.Data;
using API.model;
using Microsoft.EntityFrameworkCore;

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

var app = builder.Build();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "Hello World!");

app.MapPost("/upload", async (IFormFile file, AppDbContext db) => {
    using var memoryStream = new MemoryStream();
    await file.CopyToAsync(memoryStream);

    var fileEntity = new FileEntity
    {
        FileName = file.FileName,
        ContentType = file.ContentType,
        Data = memoryStream.ToArray()
    };

    db.Files.Add(fileEntity);
    await db.SaveChangesAsync();

    return Results.Ok(new { fileEntity.Id, fileEntity.FileName });
}).DisableAntiforgery();


app.MapGet("/files", async (AppDbContext db) =>
{
    var files = await db.Files
    .Select(f => new { f.Id, f.FileName, f.UploadDate })
    .ToListAsync();

    return Results.Ok(files);
});

app.MapGet("/download/{id}", async (Guid id, AppDbContext db) =>
{
    var fileEntity = await db.Files.FindAsync(id);
    if (fileEntity == null) return Results.NotFound();

    return Results.File(fileEntity.Data, fileEntity.ContentType, fileEntity.FileName);
});

app.MapDelete("/files/{id}", async (Guid id, AppDbContext db) =>
{
    var file = await db.Files.FindAsync(id);
    if (file == null) return Results.NotFound();

    db.Files.Remove(file);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
