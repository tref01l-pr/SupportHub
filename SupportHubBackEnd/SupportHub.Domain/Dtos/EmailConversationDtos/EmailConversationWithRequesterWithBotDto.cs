using SupportHub.Domain.Dto.EmailConversationDtos;
using SupportHub.Domain.Dto.EmailRequesterDtos;
using SupportHub.Domain.Dtos.EmailBotDtos;

namespace SupportHub.Domain.Dtos.EmailConversationDtos;

public class EmailConversationWithRequesterWithBotDto : EmailConversationDto
{
    public EmailRequesterDto EmailRequester { get; set; }
    public EmailBotDto EmailBot { get; set; }
}