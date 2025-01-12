using CSharpFunctionalExtensions;
using SupportHub.Domain.Interfaces.Application;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Services;

public class ClientsService : IClientMessagesService
{
    private readonly IUsersRepository _usersRepository;

    public ClientsService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Result<User>> GetUserInfoAsync(Guid userId)
    {
        var user = await _usersRepository.GetByIdAsync<User>(userId);

        if (user is null)
        {
            return Result.Failure<User>($"User with {nameof(userId)} not found");
        }

        return user;
    }
}