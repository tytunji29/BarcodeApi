using BarcodeApi.Data;
using BarcodeApi.Entities;
using BarcodeApi.Entities.Request;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using System.Drawing;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Common;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Swagger
app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();


// ===========================================
// 1. Create A User
// ===========================================

app.MapPost("/api/users",
async (CreateUser createUser, AppDbContext db) =>
{
    var user = new User
    {
        Id = Guid.NewGuid(),
        UserCode = $"USER{DateTime.UtcNow.Ticks:D4}",
        FirstName = createUser.FirstName,
        Email = createUser.Email,
        Age = createUser.Age
    };

    await db.Users.AddAsync(user);
    await db.SaveChangesAsync();

    return Results.Ok(user);
});

// ===========================================
// 2. Upload Excel Users
// ===========================================

app.MapPost("/api/users/upload", async (IFormFile file, AppDbContext db) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest("File is required");

    using var stream = new MemoryStream();
    stream.Position = 0;
    await file.CopyToAsync(stream);
    

    using var workbook = new XLWorkbook(stream);
    var worksheet = workbook.Worksheet(1);
    var rows = worksheet.RowsUsed().Skip(1);

    var users = new List<User>();
    int count = 1;

    foreach (var row in rows)
    {
        users.Add(new User
        {
            Id = Guid.NewGuid(),
            UserCode = $"USER{count:D4}",
            FirstName = row.Cell(1).GetString(),
            Email = row.Cell(2).GetString(),
            Age = row.Cell(3).GetString()
        });

        count++;
    }

    await db.Users.AddRangeAsync(users);
    await db.SaveChangesAsync();

    return Results.Ok(users);
})
.DisableAntiforgery();

// ===========================================
// 3. Get User By Barcode
// ===========================================

app.MapGet("/api/barcodes/{userId:guid}",
async (Guid userId, AppDbContext db) =>
{
    var user = await db.Users
        .FirstOrDefaultAsync(x => x.Id == userId);

    if (user == null)
        return Results.NotFound();

    var writer = new BarcodeWriterPixelData
    {
        Format = BarcodeFormat.CODE_128,
        Options = new EncodingOptions
        {
            Width = 400,
            Height = 120,
            Margin = 2
        }
    };

    var pixelData = writer.Write(user.UserCode);

    using var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(
        pixelData.Width,
        pixelData.Height);

    for (int y = 0; y < pixelData.Height; y++)
    {
        for (int x = 0; x < pixelData.Width; x++)
        {
            var index = (y * pixelData.Width + x) * 4;

            image[x, y] = new SixLabors.ImageSharp.PixelFormats.Rgba32(
                pixelData.Pixels[index],
                pixelData.Pixels[index + 1],
                pixelData.Pixels[index + 2],
                pixelData.Pixels[index + 3]);
        }
    }

    using var stream = new MemoryStream();

    await image.SaveAsPngAsync(stream);

    return Results.File(stream.ToArray(), "image/png");
});

// ===========================================
// 4. Get User QR Code
// ===========================================

app.MapGet("/api/qrcodes/{userId:guid}",
async (Guid userId, AppDbContext db) =>
{
    var user = await db.Users
        .FirstOrDefaultAsync(x => x.Id == userId);

    if (user == null)
        return Results.NotFound();

    var writer = new ZXing.BarcodeWriterPixelData
    {
        Format = ZXing.BarcodeFormat.QR_CODE,
        Options = new ZXing.Common.EncodingOptions
        {
            Width = 300,
            Height = 300,
            Margin = 1
        }
    };

    var pixelData = writer.Write(user.UserCode + "|" + user.FirstName + "|" + user.Email);

    using var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(
        pixelData.Width,
        pixelData.Height);

    for (int y = 0; y < pixelData.Height; y++)
    {
        for (int x = 0; x < pixelData.Width; x++)
        {
            var index = (y * pixelData.Width + x) * 4;

            image[x, y] = new SixLabors.ImageSharp.PixelFormats.Rgba32(
                pixelData.Pixels[index],
                pixelData.Pixels[index + 1],
                pixelData.Pixels[index + 2],
                pixelData.Pixels[index + 3]);
        }
    }

    using var stream = new MemoryStream();

    await image.SaveAsPngAsync(stream);

    return Results.File(stream.ToArray(), "image/png");
});



// ===========================================
// 5. Get User QR Code
// ===========================================
app.MapGet("/api/users/sample-excel", () =>
{
    var workbook = new XLWorkbook();
    var ws = workbook.Worksheets.Add("Users");

    ws.Cell(1, 1).Value = "UserCode";
    ws.Cell(1, 2).Value = "FirstName";
    ws.Cell(1, 3).Value = "Email";
    ws.Cell(1, 4).Value = "Age";

    var stream = new MemoryStream();
    workbook.SaveAs(stream);
    stream.Position = 0;

    return Results.File(
        stream,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "user-sample-template.xlsx"
    );
});
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");
app.Run();