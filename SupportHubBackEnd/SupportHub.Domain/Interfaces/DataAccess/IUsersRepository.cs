using CSharpFunctionalExtensions;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.DataAccess;

public interface IUsersRepository
{
    Task<TProjectTo?> GetByIdAsync<TProjectTo>(Guid id) where TProjectTo : class;
    Task<User[]> GetByIdsAsync(IEnumerable<Guid> userIds);
    Task<TProjectTo?> GetByEmailAsync<TProjectTo>(string email, int companyId) where TProjectTo : class;
    Task<User[]> GetAsync();
    Task<bool> Delete(Guid id);
}
