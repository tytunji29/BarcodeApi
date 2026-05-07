namespace BarcodeApi.Entities;
public class Company
{
    public Guid Id { get; set; }

    public string CompanyName { get; set; }

    public string CompanyRCNumber { get; set; }

    public string CompanyRegisteredAddress { get; set; }

    public string CompanyTelephone { get; set; }

    public string CompanyEmail { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}