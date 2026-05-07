namespace BarcodeApi.Entities.Request;

public class CreateUser
{

    public string PhoneNumber { get;  set; }
        public string FirstName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Age { get; set; }
}