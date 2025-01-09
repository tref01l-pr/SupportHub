using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.DataAccess.Email;

public interface IEmailBotsRepository
{
    Task<TProjectTo?> GetByIdAsync<TProjectTo>(int id);
    Task<TProjectTo?> GetByEmailAsync<TProjectTo>(string email);
    Task<List<TProjectTo>> GetByEmailsAsync<TProjectTo>(List<string> botEmails);
    Task<List<TProjectTo>> GetByCompanyIdAsync<TProjectTo>(int companyId);
    Task<TProjectTo> CreateAsync<TProjectTo>(EmailBot emailBot);
    Task<TProjectTo> UpdateAsync<TProjectTo>(EmailBot emailBot);
    Task<TProjectTo> DeleteAsync<TProjectTo>(int id);
}