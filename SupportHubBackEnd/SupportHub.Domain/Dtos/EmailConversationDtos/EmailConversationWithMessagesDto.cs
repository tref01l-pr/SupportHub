using SupportHub.Domain.Dto.EmailConversationDtos;
using SupportHub.Domain.Dtos.EmailMeesageDtos;

namespace SupportHub.Domain.Dtos.EmailConversationDtos;

public class EmailConversationWithMessagesDto : EmailConversationDto
{
    public List<EmailMessageDto> Messages { get; set; }
}