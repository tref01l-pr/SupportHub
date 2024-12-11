using AutoMapper;
using SupportHub.DataAccess.SqlServer.Entities;
using SupportHub.DataAccess.SqlServer.Entities.Email;
using SupportHub.Domain.Dto.EmailConversationDtos;
using SupportHub.Domain.Dto.EmailRequesterDtos;
using SupportHub.Domain.Dtos.EmailBotDtos;
using SupportHub.Domain.Dtos.EmailConversationDtos;
using SupportHub.Domain.Dtos.EmailMeesageDtos;
using SupportHub.Domain.Models;

namespace SupportHub.DataAccess.SqlServer;

public class DataAccessMappingProfile : Profile
{
    public DataAccessMappingProfile()
    {
        CreateMap<UserEntity, User>().ReverseMap();
        CreateMap<SessionEntity, Session>().ReverseMap();
        
        CreateMap<CompanyEntity, Company>().ReverseMap();
        
        CreateMap<EmailMessageEntity, EmailMessage>().ReverseMap();
        CreateMap<EmailMessageEntity, EmailMessageDto>();
        CreateMap<EmailMessageEntity, EmailMessageWithConversationDto>();
        

        CreateMap<EmailBotEntity, EmailBot>().ReverseMap();
        CreateMap<EmailBotEntity, EmailBotDto>();

        CreateMap<EmailConversationEntity, EmailConversation>().ReverseMap();
        CreateMap<EmailConversationEntity, EmailConversationDto>();
        CreateMap<EmailConversationEntity, EmailConversationWithRequesterWithBotDto>();
        CreateMap<EmailConversationEntity, EmailConversationWithMessagesDto>();
        
        CreateMap<EmailRequesterEntity, EmailRequester>().ReverseMap();
        CreateMap<EmailRequesterEntity, EmailRequesterDto>();
    }
}