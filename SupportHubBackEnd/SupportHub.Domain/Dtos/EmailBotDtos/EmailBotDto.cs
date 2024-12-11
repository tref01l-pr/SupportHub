namespace SupportHub.Domain.Dtos.EmailBotDtos;

public class EmailBotDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpHost { get; set; }
    public int ImapPort { get; set; }
    public string ImapHost { get; set; }
}