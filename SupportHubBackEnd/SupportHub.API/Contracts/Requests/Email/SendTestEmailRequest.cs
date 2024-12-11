namespace SupportHub.API.Contracts;

public class SendTestEmailRequest
{
    public string Email { get; set; }
    public int SmtpId { get; set; }
    public string Message { get; set; }
}