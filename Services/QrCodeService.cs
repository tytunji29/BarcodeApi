


  using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using System.IO;
using ZXing;
using ZXing.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using BarcodeApi.Data;
using Microsoft.EntityFrameworkCore;

namespace BarcodeApi.Services;
public interface IQrCodeService
{    Task<byte[]> GeneratePermitPdf(Guid userId);
}

public class QrCodeService : IQrCodeService
{
    private readonly AppDbContext _db;

    public QrCodeService(AppDbContext db)
    {
        _db = db;
    }

public async Task<byte[]> GeneratePermitPdf(Guid userId)
{
    var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);

    if (user == null)
        return null;

    var qrCodeBytes = await GenerateUserQrCode(user.Id);

    using var stream = new MemoryStream();

    var writer = new PdfWriter(stream);
    var pdf = new PdfDocument(writer);
    var document = new Document(pdf);

    // ================= HEADER =================
    document.Add(new Paragraph("NIGERIAN IMMIGRATION e-CERPAC")
       //.setbo()
        .SetFontSize(18)
        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

    document.Add(new Paragraph("\n"));

    // ================= BODY =================
    document.Add(new Paragraph($"Permit Number: {user.ECerpacNumber}"));
    document.Add(new Paragraph($"First Name: {user.FirstName}"));
    document.Add(new Paragraph($"Last Name: {user.LastName}"));
    document.Add(new Paragraph($"Date of Birth: {user.DateOfBirth:dd MMM yyyy}"));
    document.Add(new Paragraph($"Passport Number: {user.PassportNumber}"));

    if (user.Company != null)
    {
        document.Add(new Paragraph($"Company: {user.Company.CompanyName}"));
        //document.Add(new Paragraph($"Sponsor: {user.Company.SponsorName}"));
    }

    document.Add(new Paragraph("\n"));

    // ================= QR CODE =================
    var imageData = ImageDataFactory.Create(qrCodeBytes);
    var qrImage = new iText.Layout.Element.Image(imageData)
        .SetWidth(150)
        .SetHeight(150)
        .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

    document.Add(qrImage);

    // ================= FOOTER =================
    document.Add(new Paragraph($"\nGenerated on {DateTime.UtcNow:dd MMM yyyy}")
        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
        .SetFontSize(10));

    document.Close();

    return stream.ToArray();
}
    private async Task<byte[]> GenerateUserQrCode(Guid userId)
    {
        var qrContent = $"https://yourdomain.com/users/{userId}";

        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new EncodingOptions
            {
                Width = 300,
                Height = 300,
                Margin = 1
            }
        };

        var pixelData = writer.Write(qrContent);

        using var image = new Image<Rgba32>(
            pixelData.Width,
            pixelData.Height);

        for (int y = 0; y < pixelData.Height; y++)
        {
            for (int x = 0; x < pixelData.Width; x++)
            {
                var index = (y * pixelData.Width + x) * 4;

                image[x, y] = new Rgba32(
                    pixelData.Pixels[index],
                    pixelData.Pixels[index + 1],
                    pixelData.Pixels[index + 2],
                    pixelData.Pixels[index + 3]);
            }
        }

        using var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream);

        return stream.ToArray();
    }


}
