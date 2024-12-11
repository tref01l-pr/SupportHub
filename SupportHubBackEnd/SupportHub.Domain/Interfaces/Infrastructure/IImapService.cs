using CSharpFunctionalExtensions;
using SupportHub.Domain.Options;
using MailKit;
using MimeKit;
using SupportHub.API;
using SupportHub.Domain.Dtos.EmailBotDtos;
using SupportHub.Domain.Helpers;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces;

public interface IImapService
{
    public Task<Result<List<MimeMessage>>> GetMessagesAsync(EmailBotDto emailBotDto);
    Task<Result<List<ImapMessage>>> GetLastMessage(ImapOptions imapOptions);

    Task<Result<List<ImapMessage>>> GetMessagesFromRequester(ImapOptions imapOptions, string email);

    Task<Result<List<string>>>
        GetMessagesBody(ImapOptions imapOptions, Dictionary<string, IMessageSummary> allMessages);

    Task<Result<List<EmailMessage>>> GetAllSentMessages(ImapOptions imapOptions);
    Task<Result<List<ReceivedMessage>>> GetUnreadMessages(string user, string password, int port, string host);

    Task<Result<Dictionary<string, List<ReceivedMessage>>>> GetConversationsByIds(string emailBotEmail,
        string emailBotPassword, int emailBotImapPort, string emailBotImapHost, List<SimpleMessageInfo> ids);
}