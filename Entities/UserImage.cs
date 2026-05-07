namespace BarcodeApi.Entities;
public class UserImage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ImageData { get; set; }

}