using CSharpFunctionalExtensions;
using SupportHub.Domain.Interfaces.Application;
using SupportHub.Domain.Interfaces.DataAccess;

namespace SupportHub.Application.Services;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;

    public UsersService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }
    
    public async Task<Result<TProjectTo?>> GetByEmailAndCompanyIdAsync<TProjectTo>(string email, int companyId) where TProjectTo : class
    {
        try
        {
            var user = await _usersRepository.GetByEmailAsync<TProjectTo>(email, companyId);

            return user;
        }
        catch (Exception e)
        {
            return Result.Failure<TProjectTo?>(e.Message);
        }
    }

    public async Task<Result<TProjectTo?>> GetById<TProjectTo>(Guid id) where TProjectTo : class
    {
        try
        {
            var user = await _usersRepository.GetByIdAsync<TProjectTo>(id);

            return user;
        }
        catch (Exception e)
        {
            return Result.Failure<TProjectTo?>(e.Message);
        }
    }
}