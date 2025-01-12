using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SupportHub.DataAccess.SqlServer.Entities;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Models;

namespace SupportHub.DataAccess.SqlServer.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly SupportHubDbContext _context;
    private readonly IMapper _mapper;

    private IConfigurationProvider _mapperConfig => _mapper.ConfigurationProvider;

    public UsersRepository(
        SupportHubDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TProjectTo?> GetByIdAsync<TProjectTo>(Guid id) where TProjectTo : class
    {
        var userEntity = await _context.Users
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (typeof(TProjectTo) == typeof(UserEntity))
        {
            return userEntity as TProjectTo;
        }

        return _mapper.Map<TProjectTo>(userEntity);
    }

    public async Task<User[]> GetByIdsAsync(IEnumerable<Guid> userIds)
    {
        var usersEntity = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToArrayAsync();

        var users = _mapper.Map<UserEntity[], User[]>(usersEntity);
        return users;
    }

    public async Task<TProjectTo?> GetByEmailAsync<TProjectTo>(string email, int companyId) where TProjectTo : class
    {
        var userEntity = await _context.Users
            .Where(x => x.Email == email && x.CompanyId == companyId)
            .FirstOrDefaultAsync();

        if (typeof(TProjectTo) == typeof(UserEntity))
        {
            return userEntity as TProjectTo;
        }

        return _mapper.Map<TProjectTo>(userEntity);
    }

    public async Task<User[]> GetAsync()
    {
        var usersEntities = await _context.Users
            .AsNoTracking()
            .ToArrayAsync();

        var users = _mapper.Map<UserEntity[], User[]>(usersEntities);
        return users;
    }

    public async Task<bool> Delete(Guid id)
    {
        var userToDelete = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (userToDelete is null)
        {
            return false;
        }

        _context.Remove(userToDelete);
        await _context.SaveChangesAsync();

        return true;
    }
}