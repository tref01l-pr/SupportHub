namespace SupportHub.API.Contracts;

public class CreateEmailBotRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpHost { get; set; }
    public int ImapPort { get; set; }
    public string ImapHost { get; set; }
}