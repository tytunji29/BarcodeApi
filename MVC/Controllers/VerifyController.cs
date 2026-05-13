using BarcodeApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarcodeApi.MVC.Controllers;

public class VerifyController : Controller
{
    private readonly IUserService _userService;
    public VerifyController(IUserService userService)
    {
        _userService = userService;
    }
    public async Task<IActionResult> Index(Guid applicationId, string type = "cerpac")
        {
         
         if (applicationId == Guid.Empty)
    {
        return Redirect("https://cerpac.immigration.gov.ng/");
    }

            var model = await GetCerpacDataFromDatabase(applicationId);
            
            if (model == null)
            {
                return NotFound();
            }
            
            ViewBag.QrCodeData = $"https://verify.immigration.gov.ng/verify?ref={applicationId}";
            ViewBag.ApplicationId = applicationId;
            ViewBag.Type = type;
            
            return View(model);
        }
        
        // Simulated database fetch - replace with actual EF Core / Dapper query
        private async Task<CerpacViewModel> GetCerpacDataFromDatabase(Guid applicationId)
        {
            // In production: _context.CerpacApplications.Include(a => a.Holder).FirstOrDefault(a => a.ApplicationId == applicationId);
            
            // Demo data - different data based on ID for realism
         var det =await  _userService.GetDetial(applicationId);
        if (det == null)
        {
            //      return new CerpacViewModel
            // {
            //     FullName = "JOHN OLUCHI OKORO",
            //     FirstName="John",
            //     CountryInitial="ng",
            //     LastName="Okoro",
            //     MiddleName="Oluchi",
            //     Gender = "Male",
            //     DateOfBirth = "12 March 1985",
            //     Nationality = "Nigeria",
            //     PlaceOfBirth = "Lagos, Nigeria",
            //     PassportNumber = "A12345678",
            //     NinNumber = "19876543210",
            //     PhoneNumber = "+234 802 345 6789",
            //     Email = "john.okoro@email.com",
            //     ResidentialAddress = "12, Adeola Odeku Street, Victoria Island, Lagos.",
            //     Occupation = "IT Consultant",
            //     CerpacNumber = "NIS/CERPAC/2240-9082",
            //     ResidencePermitType = "Subject to regularization (CERPAC)",
            //     IssueDate = "15 April 2024",
            //     ExpiryDate = "14 April 2027",
            //     IssuingAuthority = "Nigeria Immigration Service, Alausa",
            //     Status = "ACTIVE",
            //     BiometricCaptureDate = "10 March 2024",
            //     LastRenewalDate = "April 2024 (initial issuance)",
            //     CompanyName="Tech Solutions Ltd",
            //     CompanyRCNumber="RC123456",
            //     CompanyRegisteredAddress="45 Industrial Avenue, Ikeja, Lagos.",
            //     CompanyTelephone="08034567890",
            //     CompanyEmail="info@techsolutions.com",
            //     Designation="Senior Software Engineer",
            //     //ApplicationId = applicationId ?? "019e0d9b-6a0c-7d23-9ae4-e48b56251824",
            //     ProfileInitials = "JO",
            //     // Conditional logic can be applied based on applicationId
            // };
        
        }
            return det;
        }
        
        }