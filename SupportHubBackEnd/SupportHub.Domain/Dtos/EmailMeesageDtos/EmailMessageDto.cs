namespace SupportHub.Domain.Dtos.EmailMeesageDtos;

public class EmailMessageDto
{
    public int Id { get; set; }
    public int EmailConversationId { get; set; }
    public int? EmailRequesterId { get; set; }
    public Guid? UserId { get; set; }
    public string MessageId { get; set; }
    public string? Subject { get; set; }
    public string Body { get; set; }
}