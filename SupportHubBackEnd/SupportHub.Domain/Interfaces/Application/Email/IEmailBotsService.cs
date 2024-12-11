using CSharpFunctionalExtensions;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.Application.Email;

public interface IEmailBotsService
{
    Task<Result<TProjectTo>> GetByIdAsync<TProjectTo>(int id);
    Task<Result<TProjectTo>> GetByEmailAsync<TProjectTo>(string email);
    Task<Result<TProjectTo>> CreateAsync<TProjectTo>(EmailBot emailBot);
    Task<Result<TProjectTo>> UpdateAsync<TProjectTo>(EmailBot emailBot);
    Task<Result<TProjectTo>> DeleteAsync<TProjectTo>(int id);
}