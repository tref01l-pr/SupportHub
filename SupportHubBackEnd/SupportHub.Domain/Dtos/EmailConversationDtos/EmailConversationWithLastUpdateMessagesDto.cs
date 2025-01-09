using SupportHub.Domain.Dto.EmailConversationDtos;
using SupportHub.Domain.Dtos.EmailMeesageDtos;

namespace SupportHub.Domain.Dtos.EmailConversationDtos;

public class EmailConversationWithLastUpdateMessagesDto : EmailConversationDto
{
    public EmailMessageDto Message { get; set; }
}