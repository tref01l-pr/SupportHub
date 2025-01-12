using CSharpFunctionalExtensions;

namespace SupportHub.Domain.Interfaces.Application;

public interface IUsersService
{
    Task<Result<TProjectTo?>> GetByEmailAndCompanyIdAsync<TProjectTo>(string email, int companyId) where TProjectTo : class;
    Task<Result<TProjectTo?>> GetById<TProjectTo>(Guid id) where TProjectTo : class;
}