namespace BarcodeApi.Entities;



public class User
{
    public Guid Id { get; set; }

    public string ECerpacNumber { get; set; }

    public DateTime DateOfExpiry { get; set; }

    public string PassportNumber { get; set; }

    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    public string LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string Nationality { get; set; }

    public DateTime ValidityStartDate { get; set; }

    public DateTime ValidityEndDate { get; set; }


    public string Designation { get; set; }
    public string Status { get; set; }
    public string PolicyNumber { get; set; }




    public DateTime? IssueDate { get; set; }
    public DateTime? BiometricCaptureDate { get; set; }
    public DateTime? LastRenewalDate { get; set; }

public string Gender { get; set; }
    // FK
    public Guid CompanyId { get; set; }

    public Company Company { get; set; }
}