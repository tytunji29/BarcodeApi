using BarcodeApi.Data;
using BarcodeApi.Entities;
using BarcodeApi.Entities.Request;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using ZXing;
using ZXing.Common;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

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
async (CreateUser createUser, AppDbContext db, IConfiguration config, IHttpClientFactory httpFactory) =>
{
    var user = new User
    {
        Id = Guid.NewGuid(),
        UserCode = $"USER{DateTime.UtcNow.Ticks}",
        FirstName = createUser.FirstName,
        Email = createUser.Email,
        PhoneNumber = CleanForWhatsApp(createUser.PhoneNumber),
        Age = createUser.Age
    };

    await db.Users.AddAsync(user);
    await db.SaveChangesAsync();

    //var http = httpFactory.CreateClient();
   // await SendWhatsAppMessage(user, config, http);

    return Results.Ok(user);
});
// ===========================================
// 2. Upload Excel Users
// ===========================================
app.MapPost("/api/users/upload",
async (IFormFile file, AppDbContext db, IConfiguration config, IHttpClientFactory httpFactory) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest("File is required");

    using var stream = new MemoryStream();
    await file.CopyToAsync(stream);
    stream.Position = 0;

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
            Age = row.Cell(3).GetString(),
            PhoneNumber = CleanForWhatsApp(row.Cell(4).GetString())
        });

        count++;
    }

    await db.Users.AddRangeAsync(users);
    await db.SaveChangesAsync();

    // // 🔥 background processing (safe version)
    // _ = Task.Run(async () =>
    // {
    //     var http = httpFactory.CreateClient();
    //     var semaphore = new SemaphoreSlim(5);

    //     var tasks = users.Select(async user =>
    //     {
    //         await semaphore.WaitAsync();
    //         try
    //         {
    //             await SendWhatsAppMessage(user, config, http);
    //         }
    //         catch (Exception ex)
    //         {
    //             Console.WriteLine($"WhatsApp failed for {user.PhoneNumber}: {ex.Message}");
    //         }
    //         finally
    //         {
    //             semaphore.Release();
    //         }
    //     });

    //     await Task.WhenAll(tasks);
    // });

    return Results.Ok(new
    {
        message = "Users uploaded successfully. WhatsApp messages are being processed."
    });
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

    ws.Cell(1, 1).Value = "FirstName";
    ws.Cell(1, 2).Value = "Email";
    ws.Cell(1, 3).Value = "Age";
    ws.Cell(1, 4).Value = "PhoneNumber";

    var stream = new MemoryStream();
    workbook.SaveAs(stream);
    stream.Position = 0;

    return Results.File(
        stream,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "user-sample-template.xlsx"
    );
});
string CleanForWhatsApp(string phoneNumber, string defaultCountryCode = "234")
{
    if (string.IsNullOrWhiteSpace(phoneNumber))
        return string.Empty;

    var digits = Regex.Replace(phoneNumber, @"\D", "");

    if (digits.StartsWith(defaultCountryCode))
        return digits;

    if (digits.StartsWith("0"))
        digits = digits.Substring(1);

    return defaultCountryCode + digits;
}
async Task SendWhatsAppMessage(User user, IConfiguration config, HttpClient http)
{
    var accessToken = config["WhatsApp:AccessToken"];
    var phoneNumberId = config["WhatsApp:PhoneNumberId"];

    var url = $"https://graph.facebook.com/v20.0/{phoneNumberId}/messages";

    var request = new HttpRequestMessage(HttpMethod.Post, url);
    request.Headers.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

    request.Content = JsonContent.Create(new
    {
        messaging_product = "whatsapp",
        to = user.PhoneNumber,
        type = "text",
        text = new
        {
            body = $"Welcome {user.FirstName}, your account has been created successfully 🎉"
        }
    });

    await http.SendAsync(request);
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");
app.Run();