using System.Net.Mail;

namespace SupportHub.Domain.Helpers;

public static class EmailValidator
{
    public static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return address.Address == email;
        }
        catch
        {
            return false;
        }
    }
}