using CSharpFunctionalExtensions;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.Application;

public interface IClientMessagesService
{
    Task<Result<User>> GetUserInfoAsync(Guid userId);
}