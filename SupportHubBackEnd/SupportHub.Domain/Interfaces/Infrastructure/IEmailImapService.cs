using CSharpFunctionalExtensions;
using SupportHub.Domain.Helpers;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces;

public interface IEmailImapService
{
    Task<Result<List<ReceivedMessage>>> GetRecentMessages(string user, string password, int port, string host, int count);
    Task<Result<List<ReceivedMessage>>> GetUnreadMessages(string user, string password, int port, string host);

    Task<Result<Dictionary<string, List<ReceivedMessage>>>> GetConversationsByIds(string emailBotEmail,
        string emailBotPassword, int emailBotImapPort, string emailBotImapHost, List<SimpleMessageInfo> ids);
    
    Task<Result> TestImapConnectionAsync(string emailBot, string password, string imapHost, int imapPort);
}