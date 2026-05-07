


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
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

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
    var user = await _db.Users
        .Include(x => x.Company)
        .FirstOrDefaultAsync(x => x.Id == userId);

    if (user == null)
        return null;

    var userimage = await _db.UserImages
        .FirstOrDefaultAsync(x => x.UserId == userId);

    var qrCodeBytes = await GenerateUserQrCode(user.Id);

    using var stream = new MemoryStream();

    var writer = new PdfWriter(stream);
    var pdf = new PdfDocument(writer);
    var document = new Document(pdf);

    document.SetMargins(10, 10, 10, 10);

    // =====================================================
    // MAIN TABLE
    // =====================================================

    var mainTable = new Table(UnitValue.CreatePercentArray(new float[] { 70, 30 }))
        .UseAllAvailableWidth();

    // =====================================================
    // LEFT SECTION
    // =====================================================

    var leftCell = new Cell();

    // ================= HEADER =================

    var headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 }))
        .UseAllAvailableWidth();

    // LOGO
    var logo = new Paragraph("🇳🇬")
        .SetFontSize(40);

    headerTable.AddCell(
        new Cell()
            .Add(logo)
            .SetBorder(Border.NO_BORDER)
    );

    // HEADER TEXT
    headerTable.AddCell(
        new Cell()
            .Add(new Paragraph("FEDERAL REPUBLIC OF NIGERIA")
    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(20))
            .Add(new Paragraph("Electronic Comprehensive Expatriate Residence Permit"))
            .SetBorder(Border.NO_BORDER)
    );

    leftCell.Add(headerTable);

    leftCell.Add(new Paragraph("\n"));

    // ================= DETAILS TABLE =================

    var detailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 40, 60 }))
        .UseAllAvailableWidth();

    detailsTable.AddCell(CreateLabelCell("e-CERPAC:"));
    detailsTable.AddCell(CreateValueCell(user.ECerpacNumber));

    detailsTable.AddCell(CreateLabelCell("Insurance Policy Number:"));
    detailsTable.AddCell(CreateValueCell(user.PolicyNumber));
    detailsTable.AddCell(CreateLabelCell("First Name:"));
    detailsTable.AddCell(CreateValueCell(user.FirstName));

    detailsTable.AddCell(CreateLabelCell("Middle Name:"));
    detailsTable.AddCell(CreateValueCell(user.MiddleName));
    detailsTable.AddCell(CreateLabelCell("Last Name:"));
    detailsTable.AddCell(CreateValueCell(user.LastName));

    detailsTable.AddCell(CreateLabelCell("Date of Birth:"));
    detailsTable.AddCell(CreateValueCell(user.DateOfBirth.ToString("dd MMM yyyy")));

    detailsTable.AddCell(CreateLabelCell("Status:"));
    detailsTable.AddCell(CreateValueCell(user.Status));

    detailsTable.AddCell(CreateLabelCell("Validity:"));
    detailsTable.AddCell(CreateValueCell($"{user.ValidityStartDate:dd MMM yyyy} - {user.ValidityEndDate:dd MMM yyyy}"));

    leftCell.Add(detailsTable);

    mainTable.AddCell(leftCell);

    // =====================================================
    // RIGHT SECTION
    // =====================================================

    var rightCell = new Cell();

    rightCell.Add(
        new Paragraph("e-CERPAC")
    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
            .SetFontSize(22)
            .SetTextAlignment(TextAlignment.CENTER)
    );

    rightCell.Add(new Paragraph("\n"));

    // ================= USER IMAGE =================

    if (userimage != null && !string.IsNullOrWhiteSpace(userimage.ImageData))
    {
        var base64 = userimage.ImageData;

        if (base64.Contains(","))
        {
            base64 = base64.Split(',')[1];
        }

        byte[] imageBytes = Convert.FromBase64String(base64);

        var profileImage =  new iText.Layout.Element.Image(ImageDataFactory.Create(imageBytes))
            .SetWidth(150)
            .SetHeight(150)
            .SetHorizontalAlignment(HorizontalAlignment.CENTER);

        rightCell.Add(profileImage);
    }

    rightCell.Add(new Paragraph("\n"));

    // ================= QR CODE =================

    var qrImage =  new iText.Layout.Element.Image(ImageDataFactory.Create(qrCodeBytes))
        .SetWidth(150)
        .SetHeight(150)
        .SetHorizontalAlignment(HorizontalAlignment.CENTER);

    rightCell.Add(qrImage);

    mainTable.AddCell(rightCell);

    // =====================================================
    // ADD MAIN TABLE
    // =====================================================

    document.Add(mainTable);

    // ================= FOOTER =================

    document.Add(
        new Paragraph($"Generated on {DateTime.UtcNow:dd MMM yyyy}")
            .SetFontSize(10)
            .SetTextAlignment(TextAlignment.CENTER)
    );

    document.Close();

    return stream.ToArray();
}


// =====================================================
// HELPER METHODS
// =====================================================

private Cell CreateLabelCell(string text)
{
    return new Cell()
        .Add(new Paragraph(text).SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
        .SetBorder(Border.NO_BORDER);
}

private Cell CreateValueCell(string? text)
{
    return new Cell()
        .Add(new Paragraph(text ?? "-"))
        .SetBorder(Border.NO_BORDER);
}
 private async Task<byte[]> GenerateUserQrCode(Guid userId)
    {
        var qrContent = $"https://barcodeapi-cwc7.onrender.com/verify/index?applicationId={userId}";
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
