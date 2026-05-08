


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
using iText.Kernel.Colors;

namespace BarcodeApi.Services;

public interface IQrCodeService
{
    Task<byte[]> GeneratePermitPdf(Guid userId);
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

        document.SetMargins(20, 20, 20, 20); // Increased margins for better spacing

        // Define fonts
        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        // Nigeria green color
        var nigeriaGreen = new DeviceRgb(0, 135, 81); // #008751

        // =====================================================
        // MAIN TABLE - GREEN HEADER + CONTENT
        // =====================================================

        var mainContainer = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
            .UseAllAvailableWidth();

        // ================= GREEN HEADER BAR =================
        var headerBar = new Cell()
            .SetBackgroundColor(nigeriaGreen)
            .SetPadding(10);

        var headerText = new Paragraph("FEDERAL REPUBLIC OF NIGERIA")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetFontColor(ColorConstants.WHITE)
            .SetTextAlignment(TextAlignment.CENTER);

        var subHeaderText = new Paragraph("Electronic Comprehensive Expatriate Residence Permits and Automated Card (e-CERPAC)")
            .SetFont(regularFont)
            .SetFontSize(9)
            .SetFontColor(ColorConstants.WHITE)
            .SetTextAlignment(TextAlignment.CENTER);

        headerBar.Add(headerText);
        headerBar.Add(subHeaderText);
        mainContainer.AddCell(headerBar);

        // ================= CONTENT AREA =================
        var contentCell = new Cell()
            .SetPadding(15)
            .SetBorder(new SolidBorder(1)); // Add border around content

        // ================= TWO-COLUMN LAYOUT =================
        var contentTable = new Table(UnitValue.CreatePercentArray(new float[] { 65, 35 }))
            .UseAllAvailableWidth();

        // ================= LEFT SIDE - DETAILS =================
        var leftDetails = new Cell()
            .SetBorder(Border.NO_BORDER)
            .SetPadding(5);

        // e-CERPAC Number
        leftDetails.Add(new Paragraph("e-CERPAC: " + user.ECerpacNumber)
            .SetFont(boldFont)
            .SetFontSize(11));

        // Insurance Policy
        leftDetails.Add(new Paragraph("Insurance Policy Number: " + user.PolicyNumber)
            .SetFont(regularFont)
            .SetFontSize(10));

        leftDetails.Add(new Paragraph("")); // Spacer

        // Personal Details - using bold labels with regular values
        leftDetails.Add(CreateDetailLine(boldFont, regularFont, "First Name:", user.FirstName));
        leftDetails.Add(CreateDetailLine(boldFont, regularFont, "Middle Name:", user.MiddleName ?? "-"));
        leftDetails.Add(CreateDetailLine(boldFont, regularFont, "Last Name:", user.LastName));
        leftDetails.Add(CreateDetailLine(boldFont, regularFont, "Date of birth:", user.DateOfBirth.ToString("dd MMM yyyy")));
        leftDetails.Add(CreateDetailLine(boldFont, regularFont, "Status:", user.Status));
        leftDetails.Add(CreateDetailLine(boldFont, regularFont, "Validity:",
            $"{user.ValidityStartDate:dd MMM yyyy} to {user.ValidityEndDate:dd MMM yyyy}"));

        contentTable.AddCell(leftDetails);

        // ================= RIGHT SIDE - PHOTO & QR =================
        var rightDetails = new Cell()
            .SetBorder(Border.NO_BORDER)
            .SetPadding(5)
            .SetTextAlignment(TextAlignment.CENTER);

        // e-CERPAC label above photo
        rightDetails.Add(new Paragraph("e-CERPAC")
            .SetFont(boldFont)
            .SetFontSize(16)
            .SetFontColor(nigeriaGreen)
            .SetTextAlignment(TextAlignment.CENTER));

        rightDetails.Add(new Paragraph("")); // Spacer

        // User Photo
        if (userimage != null && !string.IsNullOrWhiteSpace(userimage.ImageData))
        {
            var base64 = userimage.ImageData;
            if (base64.Contains(","))
                base64 = base64.Split(',')[1];

            byte[] imageBytes = Convert.FromBase64String(base64);
            var profileImage = new iText.Layout.Element.Image(ImageDataFactory.Create(imageBytes))
                .SetWidth(120)
                .SetHeight(140)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            rightDetails.Add(profileImage);
        }
        else
        {
            // Placeholder if no image
            rightDetails.Add(new Paragraph("[Photo]")
                .SetFont(regularFont)
                .SetFontSize(10));
        }

        rightDetails.Add(new Paragraph("")); // Spacer

        // QR Code
        var qrImage = new iText.Layout.Element.Image(ImageDataFactory.Create(qrCodeBytes))
            .SetWidth(100)
            .SetHeight(100)
            .SetHorizontalAlignment(HorizontalAlignment.CENTER);

        rightDetails.Add(qrImage);

        contentTable.AddCell(rightDetails);

        contentCell.Add(contentTable);
        mainContainer.AddCell(contentCell);

        // Add the main container to document
        document.Add(mainContainer);

        document.Close();
        return stream.ToArray();
    }

    // Helper method for creating consistent detail lines
    private Paragraph CreateDetailLine(PdfFont boldFont, PdfFont regularFont, string label, string value)
    {
        var paragraph = new Paragraph()
            .SetMarginBottom(3);

        paragraph.Add(new Text(label + " ")
            .SetFont(boldFont)
            .SetFontSize(10));

        paragraph.Add(new Text(value)
            .SetFont(regularFont)
            .SetFontSize(10));

        return paragraph;
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
