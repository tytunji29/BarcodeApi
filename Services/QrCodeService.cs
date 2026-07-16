


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
using PuppeteerSharp;
using PuppeteerSharp.Media;

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
    var model = await _db.Users
        .Include(x => x.Company)
        .FirstOrDefaultAsync(x => x.Id == userId);

    if (model == null)
        return null;

    var userimage = await _db.UserImages
        .FirstOrDefaultAsync(x => x.UserId == userId);

    var qrCodeBytes = await GenerateUserQrCode(model.Id);
var pic =$"data:image/png;base64,{userimage.ImageData}";
var qrSrc = $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
    // Read HTML template
    var templatePath = Path.Combine(Directory.GetCurrentDirectory(),
        "Helpers",
        "htmlhelper.html");

    var html = await File.ReadAllTextAsync(templatePath);

    // Replace variables
    html = html.Replace("{{CerpacNumber}}", model.ECerpacNumber ?? "");
    html = html.Replace("{{InsurancePolicyNumber}}", model.PolicyNumber ?? "");
    html = html.Replace("{{FirstName}}", model.FirstName ?? "");
    html = html.Replace("{{MiddleName}}", model.MiddleName ?? "");
    html = html.Replace("{{LastName}}", model.LastName ?? "");
    html = html.Replace("{{DateOfBirth}}", model.DateOfBirth.ToString("dd MMM yyyy") ?? "");
    html = html.Replace("{{Status}}", model.Status ?? "");
    html = html.Replace("{{ValidFrom}}", model.ValidityStartDate.ToString("dd MMM yyyy") ?? "");
    html = html.Replace("{{ValidTo}}", model.ValidityEndDate.ToString("dd MMM yyyy") ?? "");
    html = html.Replace("{{PhotoSrc}}", pic);
    html = html.Replace("{{QrCodeSrc}}", qrSrc );
  await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = true,
    ExecutablePath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH"),
    Args = new[]
    {
        "--no-sandbox",
        "--disable-setuid-sandbox"
    }
});
// await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
// {
//     Headless = true,
//     ExecutablePath = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
//     Args = new[]
//     {
//         "--no-sandbox",
//         "--disable-dev-shm-usage"
//     }
// });

await using var page = await browser.NewPageAsync();

await page.SetContentAsync(html, new NavigationOptions
{
    WaitUntil = new[]
    {
        WaitUntilNavigation.DOMContentLoaded
    },
    Timeout = 0
});

var pdfBytes = await page.PdfDataAsync(new PdfOptions
{
    Format = PaperFormat.A4,
    PrintBackground = true,
    PreferCSSPageSize = true
});

return pdfBytes;

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
        var qrContent = $"https://cerpac-immgration.gof.ng/verify/index?applicationId={userId}";
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
