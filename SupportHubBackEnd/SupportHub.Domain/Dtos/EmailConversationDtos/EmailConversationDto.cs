namespace SupportHub.Domain.Dto.EmailConversationDtos;

public class EmailConversationDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int EmailBotId { get; set; }
    public int EmailRequesterId { get; set; }
    public string MsgId { get; set; }
    public string Subject { get; set; }
    public DateTimeOffset LastUpdateDate { get; set; }
}