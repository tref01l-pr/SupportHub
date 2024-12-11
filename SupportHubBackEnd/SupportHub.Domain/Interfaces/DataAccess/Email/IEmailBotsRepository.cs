using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.DataAccess.Email;

public interface IEmailBotsRepository
{
    Task<TProjectTo?> GetByIdAsync<TProjectTo>(int id);
    Task<TProjectTo?> GetByEmailAsync<TProjectTo>(string email);
    Task<TProjectTo> CreateAsync<TProjectTo>(EmailBot emailBot);
    Task<TProjectTo> UpdateAsync<TProjectTo>(EmailBot emailBot);
    Task<TProjectTo> DeleteAsync<TProjectTo>(int id);
    Task<List<TProjectTo>> GetByEmailsAsync<TProjectTo>(List<string> botEmails);
}