using System.Net.Http.Headers;
using BarcodeApi.Entities;

namespace BarcodeApi.Services;

public interface IWhatsAppService
{
    Task SendWhatsAppMessage(User user);
}public class WhatsAppService : IWhatsAppService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    public WhatsAppService(IConfiguration config, IHttpClientFactory httpFactory)
    {
        _config = config;
        _http = httpFactory.CreateClient();
    }

    public async Task SendWhatsAppMessage(User user)
    {
        var accessToken = _config["WhatsApp:AccessToken"];
        var phoneNumberId = _config["WhatsApp:PhoneNumberId"];

        var url = $"https://graph.facebook.com/v20.0/{phoneNumberId}/messages";

        var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        request.Content = JsonContent.Create(new
        {
            messaging_product = "whatsapp",
            //to = user.PhoneNumber,
            type = "text",
            text = new
            {
                body = $"Welcome {user.FirstName}, your account has been created successfully 🎉"
            }
        });

        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"WhatsApp Error: {error}");
        }
    }
}