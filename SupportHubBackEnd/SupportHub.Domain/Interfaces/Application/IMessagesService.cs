using CSharpFunctionalExtensions;
using SupportHub.API;
using SupportHub.Domain.Dtos.EmailMeesageDtos;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.Application;

public interface IMessagesService
{
    Task<Result<EmailMessageDto>> SendEmailMessageAsync(int emailConversationId, int companyId, string body, Guid userId);
    Task<Result> SendTestEmailMessageAsync(int emailSmtpId, string message, string to);

    Task<Result<ImapMessage[]>> GetLastMessagesAsync(ImapOptions imapOptions);

    Task<Result> EventOnMessageReceivedAsync(ImapOptions imapOptions);
    Task<Result<bool>> RemoveKeyAsync();
}