namespace BarcodeApi.Entities.Request;

public class CreateUser
{

    public string UserCode { get; set; } = default!;

    public string FirstName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Age { get; set; }
}