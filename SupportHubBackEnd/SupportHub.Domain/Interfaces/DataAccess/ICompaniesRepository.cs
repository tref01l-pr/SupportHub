using CSharpFunctionalExtensions;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.DataAccess;

public interface ICompaniesRepository
{
    Task<TProjectTo?> GetByIdAsync<TProjectTo>(int companyId);
    Task<TProjectTo?> GetByUrlAsync<TProjectTo>(string url);

    Task<Result<Company>> CreateAsync(Company company);

    Task<Result> DeleteAsync(int companyId);
}