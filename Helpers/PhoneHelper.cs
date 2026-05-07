using System.Text.RegularExpressions;

namespace BarcodeApi.Helpers;

public static class PhoneHelper
{
    public static string CleanForWhatsApp(string phoneNumber, string defaultCountryCode = "234")
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        var digits = Regex.Replace(phoneNumber, @"\D", "");

        if (digits.StartsWith(defaultCountryCode))
            return digits;

        if (digits.StartsWith("0"))
            digits = digits.Substring(1);

        return defaultCountryCode + digits;
    }
}