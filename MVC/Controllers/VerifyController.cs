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
         var det =await  _userService.GetDetial(applicationId);
        
            return det;
        }
        
        }