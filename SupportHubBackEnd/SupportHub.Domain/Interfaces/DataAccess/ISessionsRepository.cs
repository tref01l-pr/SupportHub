using CSharpFunctionalExtensions;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.DataAccess;

public interface ISessionsRepository
{
    Task<Result<Session>> GetById(Guid userId);

    Task<Result<bool>> Create(Session session);

    Task<Result<bool>> Delete(Guid userId);
}