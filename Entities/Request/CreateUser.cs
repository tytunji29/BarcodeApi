namespace BarcodeApi.Entities.Request;

public class CreateUser{
    public string ECerpacNumber { get; set; }

    public DateTime DateOfExpiry { get; set; }

    public string PassportNumber { get; set; }

    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    public string LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string Sex { get; set; }

    public string Nationality { get; set; }

    public DateTime ValidityStartDate { get; set; }

    public DateTime ValidityEndDate { get; set; }

    public string ResidentClass { get; set; }

    // Employment Information
    public string CompanyName { get; set; }

    public string CompanyRCNumber { get; set; }

    public string CompanyRegisteredAddress { get; set; }

    public string CompanyTelephone { get; set; }

    public string CompanyEmail { get; set; }

    public string Designation { get; set; }

    public IFormFile Picture { get; set; }




    public DateTime? BiometricCaptureDate { get; set; }
    public DateTime? LastRenewalDate { get; set; }


public string Email { get; set; }
public string Nin { get; set; }
public string Gender { get; set; }
public string PhoneNumber { get; set; }
public string Address { get; set; }
public string Occupation { get; set; }
public string ResidencePermitType { get; set; }
public string PlaceOfBirth { get; set; }
}