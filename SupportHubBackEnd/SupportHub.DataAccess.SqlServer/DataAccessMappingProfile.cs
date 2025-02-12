﻿using AutoMapper;
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
        CreateMap<UserEntity, UserEntity>();
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
        CreateMap<EmailConversationEntity, EmailConversationWithLastUpdateMessagesDto>()
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src =>
                src.EmailMessages
                    .OrderByDescending(message => message.Date)
                    .FirstOrDefault()));
        CreateMap<EmailConversationEntity, EmailConversationWithMessagesDto>()
            .IncludeBase<EmailConversationEntity, EmailConversationDto>()
            .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => src.EmailMessages));

        CreateMap<EmailRequesterEntity, EmailRequester>().ReverseMap();
        CreateMap<EmailRequesterEntity, EmailRequesterDto>();
    }
}