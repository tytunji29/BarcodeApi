namespace BarcodeApi.Entities;

public class User
{
    public Guid Id { get; set; }

    public string UserCode { get; set; } = default!;

    public string FirstName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Age { get; set; }
    public string PhoneNumber { get;  set; }
}