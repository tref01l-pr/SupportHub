using SupportHub.Domain.Dto.EmailConversationDtos;

namespace SupportHub.Domain.Dtos.EmailMeesageDtos;

public class EmailMessageWithConversationDto : EmailMessageDto
{
    public EmailConversationDto EmailConversation { get; set; }
}