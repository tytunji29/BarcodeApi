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

    public string Sex { get; set; }

    public string Nationality { get; set; }

    public DateTime ValidityStartDate { get; set; }

    public DateTime ValidityEndDate { get; set; }

    public string ResidentClass { get; set; }

    public string Designation { get; set; }

    // FK
    public Guid CompanyId { get; set; }

    public Company Company { get; set; }
}