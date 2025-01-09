using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.DataAccess;

public interface IEmailConversationsRepository
{
    Task<TProjectTo?> GetByIdAsync<TProjectTo>(int id);
    Task<List<TProjectTo>> GetByCompanyIdAsync<TProjectTo>(int companyId);
    Task<List<TProjectTo>> GetLastByCompanyIdAsync<TProjectTo>(int companyId);
    Task<List<TProjectTo>> GetAllAsync<TProjectTo>();
    Task<TProjectTo> CreateAsync<TProjectTo>(EmailConversation newConversationValue);
}