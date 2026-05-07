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
            // In a real application, you would fetch from database using applicationId
            // For demo, we create sample data based on the applicationId parameter
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
            return det;
            }
        //     return new CerpacViewModel
        //     {
        //         FullName = "JOHN OLUCHI OKORO",
        //         Gender = "Male",
        //         DateOfBirth = "12 March 1985",
        //         Nationality = "Nigeria",
        //         PlaceOfBirth = "Lagos, Nigeria",
        //         PassportNumber = "A12345678",
        //         NinNumber = "19876543210",
        //         PhoneNumber = "+234 802 345 6789",
        //         Email = "john.okoro@email.com",
        //         ResidentialAddress = "12, Adeola Odeku Street, Victoria Island, Lagos.",
        //         Occupation = "IT Consultant",
        //         CerpacNumber = "NIS/CERPAC/2240-9082",
        //         ResidencePermitType = "Subject to regularization (CERPAC)",
        //         IssueDate = "15 April 2024",
        //         ExpiryDate = "14 April 2027",
        //         IssuingAuthority = "Nigeria Immigration Service, Alausa",
        //         Status = "ACTIVE",
        //         BiometricCaptureDate = "10 March 2024",
        //         LastRenewalDate = "April 2024 (initial issuance)",
        //         ApplicationId = applicationId ?? "NIS/CERPAC/2025/04289",
        //         ProfileInitials = "JO",
        //         // Conditional logic can be applied based on applicationId
        //     };
        // }
    }

