using CSharpFunctionalExtensions;
using SupportHub.Domain.Interfaces.Application;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Services;

public class SystemAdminsService : ISystemAdminsService
{
    private readonly IUsersRepository _usersRepository;

    public SystemAdminsService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<User[]> GetAsync()
    {
        return await _usersRepository.GetAsync();
    }

    public async Task<Result<User>> GetByIdAsync(Guid id)
    {
        var user = await _usersRepository.GetByIdAsync<User>(id);

        if (user is null)
        {
            return Result.Failure<User>("No user with this id");
        }

        return user;
    }

    public async Task<Result> Delete(Guid id)
    {
        var result = await _usersRepository.Delete(id);

        if (result == false)
        {
            return Result.Failure("Не удалось удалить пользователя с таким идентификатором");
        }

        return Result.Success();
    }
}