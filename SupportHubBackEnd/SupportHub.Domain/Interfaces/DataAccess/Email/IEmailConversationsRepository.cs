using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.DataAccess;

public interface IEmailConversationsRepository
{
    Task<TProjectTo?> GetByIdAsync<TProjectTo>(int id);
    Task<List<TProjectTo>> GetAllAsync<TProjectTo>();
    Task<TProjectTo> CreateAsync<TProjectTo>(EmailConversation newConversationValue);
}