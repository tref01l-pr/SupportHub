using AutoMapper;
using SupportHub.API.Contracts;
using SupportHub.Domain.Models;

namespace SupportHub.API;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        CreateMap<User, GetUserResponse>();
        CreateMap<ImapMessage, GetMessageResponse>();
    }
}