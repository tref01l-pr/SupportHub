using CSharpFunctionalExtensions;
using SupportHub.API;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.DataAccess;

public interface IEmailMessagesRepository
{
    Task<Result<int>> GetNumberOfMessagesAsync();
    Task<EmailMessage[]> GetByRequesterIdAsync(int id);
    Task<EmailMessage[]> GetLastMessagesAsync();
    Task<Result<TProjectTo>> CreateAsync<TProjectTo>(EmailMessage emailMessage);
    Task<TProjectTo?> GetByMessageIdAsync<TProjectTo>(string messageReplyToMsgId);
}