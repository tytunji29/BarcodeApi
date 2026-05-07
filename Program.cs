using BarcodeApi.Data;
using BarcodeApi.Entities;
using BarcodeApi.Entities.Request;
using BarcodeApi.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
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
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
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
async ([FromForm]CreateUser request, IUserService userService) =>
{
    var result = await userService.CreateUser(request);
  return Results.File(result, "application/pdf",
            $"{request.MiddleName}.pdf");
});
// ===========================================
// 2. Upload Excel Users
// ===========================================
// app.MapPost("/api/users/upload",
// async (IFormFile file, IUserService userService) =>
// {
//     var result = await userService.UploadUsers(file);

//     return Results.Ok(result);
// })
// .DisableAntiforgery();

// ===========================================
// 4. Get User QR Code
// ===========================================

app.MapGet("/api/qrcodes/{userId:guid}",
async (Guid userId, IQrCodeService qrCodeService) =>
{
    var qrCode = await qrCodeService.GeneratePermitPdf(userId);

    if (qrCode == null)
        return Results.NotFound();

    return Results.File(qrCode, "application/pdf",
            $"{userId}.pdf");
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

// var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
// app.Urls.Add($"http://0.0.0.0:{port}");
app.Run();