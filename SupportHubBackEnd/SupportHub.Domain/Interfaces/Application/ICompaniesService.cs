using CSharpFunctionalExtensions;

namespace SupportHub.Domain.Interfaces.Application;

public interface ICompaniesService
{
    Task<Result<TProjectTo?>> GetByIdAsync<TProjectTo>(int companyId);
    Task<Result<TProjectTo?>> GetByNameAsync<TProjectTo>(string name);
}