using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.Application;

public interface IEmailRequestersRepository
{
    Task<TProjectTo?> GetByEmailAsync<TProjectTo>(string email);
    Task<TProjectTo> GetByIdAsync<TProjectTo>(int id);
    Task<TProjectTo> CreateAsync<TProjectTo>(EmailRequester emailRequester);
    Task<TProjectTo> UpdateAsync<TProjectTo>(EmailRequester emailRequester);
    Task<TProjectTo> DeleteAsync<TProjectTo>(int id);
    Task<List<TProjectTo>> GetByEmailsAsync<TProjectTo>(List<string> emails);
}