using BarcodeApi.Data;
using BarcodeApi.Entities;
using BarcodeApi.Entities.Request;
using BarcodeApi.MVC;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace BarcodeApi.Services;

public interface IUserService
{
   Task<byte[]> CreateUser(CreateUser request);

   // Task<List<object>> UploadUsers(IFormFile file);
}


public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IQrCodeService _qrCode;

    public UserService(
        AppDbContext db,
        IWhatsAppService whatsAppService,IQrCodeService qrCode)
    {
        _db = db;
        _whatsAppService = whatsAppService;
         _qrCode=qrCode;
    }

    public async Task<byte[]> CreateUser(CreateUser request)
    {
        var company = await _db.Companies
            .FirstOrDefaultAsync(x =>
                x.CompanyRCNumber == request.CompanyRCNumber);

        if (company == null)
        {
            company = new Company
            {
                Id = Guid.NewGuid(),
                CompanyName = request.CompanyName,
                CompanyRCNumber = request.CompanyRCNumber,
                CompanyRegisteredAddress = request.CompanyRegisteredAddress,
                CompanyTelephone = request.CompanyTelephone,
                CompanyEmail = request.CompanyEmail
            };

            await _db.Companies.AddAsync(company);
        }

        var user = new User
        {

ApplicationId=Guid.NewGuid(),
BiometricCaptureDate=request.BiometricCaptureDate,
LastRenewalDate=request.LastRenewalDate,
Email = request.Email,
Nin = request.Nin,
Gender = request.Gender,
PhoneNumber = request.PhoneNumber,
Address = request.Address,
Occupation = request.Occupation,
ResidencePermitType = request.ResidencePermitType,
PlaceOfBirth = request.PlaceOfBirth,
    ECerpacNumber = request.ECerpacNumber,

    DateOfExpiry = request.DateOfExpiry.ToUniversalTime(),

    PassportNumber = request.PassportNumber,

    FirstName = request.FirstName,

    MiddleName = request.MiddleName,

    LastName = request.LastName,

    DateOfBirth = request.DateOfBirth.ToUniversalTime(),

    Sex = request.Sex,
    PolicyNumber=$"GEN/{DateTime.UtcNow:yyyyMMdd}/{Random.Shared.Next(10000, 99999)}",
    Status="Expatriate",

    Nationality = request.Nationality,

    ValidityStartDate = request.ValidityStartDate.ToUniversalTime(),

    ValidityEndDate = request.ValidityEndDate.ToUniversalTime(),

    ResidentClass = request.ResidentClass,

    Designation = request.Designation,

    CompanyId = company.Id
};

        await _db.Users.AddAsync(user);

var userImage = new UserImage
        {
            Id = Guid.NewGuid(),
            UserId = user.Id
        };

            userImage.ImageData = await ConvertToBase64(request.Picture);
        await _db.UserImages.AddAsync(userImage);
        await _db.SaveChangesAsync();

        // await _whatsAppService.SendWhatsAppMessage(user);

      var res= await _qrCode.GeneratePermitPdf(user.Id);
return res;
    }


public async Task<CerpacViewModel> GetDetial(Guid userId)
{
    var user = await _db.Users
        .Include(x => x.Company)
        .FirstOrDefaultAsync(x => x.Id == userId);

    if (user == null)
        return null;

    var userimage = await _db.UserImages
        .FirstOrDefaultAsync(x => x.UserId == userId);

    return new CerpacViewModel
    {
        FullName = $"{user.FirstName} {user.LastName}",
        DateOfBirth = user.DateOfBirth.ToString("yyyy-MM-dd"),
        Nationality = user.Nationality ?? string.Empty,
        PassportNumber = user.PassportNumber ?? string.Empty,

        CerpacNumber = user.ECerpacNumber ?? string.Empty,
          IssueDate = user.ValidityStartDate.ToString("yyyy-MM-dd") ?? string.Empty,
        ExpiryDate = user.ValidityEndDate.ToString("yyyy-MM-dd") ?? string.Empty,

        IssuingAuthority = user.Company?.CompanyName ?? string.Empty,
        Status = user.Status ?? string.Empty,

        BiometricCaptureDate = user.BiometricCaptureDate?.ToString("yyyy-MM-dd") ?? string.Empty,
        LastRenewalDate = user.LastRenewalDate?.ToString("yyyy-MM-dd") ?? string.Empty,

        ApplicationId = user.ApplicationId?.ToString() ?? string.Empty,
        Email = user.Email ?? string.Empty,
 ResidencePermitType = user.ResidencePermitType ?? string.Empty,


        PlaceOfBirth = user.PlaceOfBirth ?? string.Empty,
        Gender = user.Gender ?? string.Empty,
        NinNumber = user.Nin ?? string.Empty,
        PhoneNumber = user.PhoneNumber ?? string.Empty,
        ResidentialAddress = user.Address ?? string.Empty,
        Occupation = user.Occupation ?? string.Empty,     
        ProfileInitials = GenerateInitials(user.FirstName, user.LastName)
    };
}

private string GenerateInitials(string firstName, string lastName)
{
    var f = string.IsNullOrWhiteSpace(firstName) ? "" : firstName[0].ToString();
    var l = string.IsNullOrWhiteSpace(lastName) ? "" : lastName[0].ToString();

    return (f + l).ToUpper();
}
public async Task<string> ConvertToBase64(IFormFile file)
{
    using var memoryStream = new MemoryStream();

    await file.CopyToAsync(memoryStream);

    byte[] fileBytes = memoryStream.ToArray();

    return Convert.ToBase64String(fileBytes);
}
    // public async Task<List<object>> UploadUsers(IFormFile file)
    // {
    //     using var stream = new MemoryStream();

    //     await file.CopyToAsync(stream);

    //     stream.Position = 0;

    //     using var workbook = new XLWorkbook(stream);

    //     var worksheet = workbook.Worksheet(1);

    //     var rows = worksheet.RowsUsed().Skip(1);

    //     var response = new List<object>();

    //     foreach (var row in rows)
    //     {
    //         var companyRc = row.Cell(8).GetString();

    //         var company = await _db.Companies
    //             .FirstOrDefaultAsync(x => x.CompanyRCNumber == companyRc);

    //         if (company == null)
    //         {
    //             company = new Company
    //             {
    //                 Id = Guid.NewGuid(),
    //                 CompanyName = row.Cell(7).GetString(),
    //                 CompanyRCNumber = companyRc
    //             };

    //             await _db.Companies.AddAsync(company);
    //         }

    //         var user = new User
    //         {
    //             Id = Guid.NewGuid(),
    //             UserCode = $"USER{DateTime.UtcNow.Ticks}",
    //             FirstName = row.Cell(1).GetString(),
    //             LastName = row.Cell(2).GetString(),
    //             Email = row.Cell(3).GetString(),
    //             PhoneNumber = PhoneHelper.CleanForWhatsApp(row.Cell(4).GetString()),
    //             CompanyId = company.Id
    //         };

    //         await _db.Users.AddAsync(user);

    //         response.Add(new
    //         {
    //             user.FirstName,
    //             Company = company.CompanyName
    //         });
    //     }

    //     await _db.SaveChangesAsync();

    //     return response;
    // }


}