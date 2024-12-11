using CSharpFunctionalExtensions;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.Application;

public interface ISystemAdminsService
{
    Task<Result> Delete(Guid id);

    Task<User[]> GetAsync();

    Task<Result<User>> GetByIdAsync(Guid id);
}