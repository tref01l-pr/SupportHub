using CSharpFunctionalExtensions;
using SupportHub.API;
using SupportHub.Domain.Dtos.EmailBotDtos;
using SupportHub.Domain.Dtos.EmailMeesageDtos;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.Application;

public interface IMessagesService
{
    Task<Result<EmailMessageDto>> SendEmailMessageAsync(int emailConversationId, int companyId, string body, Guid userId);
    Task<Result> AddMessageOnInitialize(EmailBotDto emailBot, int count);
    Task<Result<List<TProjectTo>>> GetLastConversationsByCompanyIdAsync<TProjectTo>(int companyId);
    Task<Result<TProjectTo?>> GetConversationById<TProjectTo>(int conversationId);
    Task<Result> EventOnMessageReceivedAsync(ImapOptions imapOptions);
    Task<Result<bool>> RemoveKeyAsync();
}