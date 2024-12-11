using AutoMapper;
using AutoMapper.QueryableExtensions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SupportHub.DataAccess.SqlServer.Entities.Email;
using SupportHub.Domain.Interfaces.Application;
using SupportHub.Domain.Models;

namespace SupportHub.DataAccess.SqlServer.Repositories;

public class EmailRequestersRepository : IEmailRequestersRepository
{
    private readonly SupportHubDbContext _context;
    private readonly IMapper _mapper;
    
    private IConfigurationProvider _mapperConfig => _mapper.ConfigurationProvider;

    public EmailRequestersRepository(SupportHubDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<TProjectTo?> GetByEmailAsync<TProjectTo>(string email) =>
        await _context.EmailRequesters
            .AsNoTracking()
            .Where(er => er.Email == email)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    public async Task<TProjectTo?> GetByIdAsync<TProjectTo>(int id) =>
        await _context.EmailRequesters
            .AsNoTracking()
            .Where(er => er.Id == id)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    public async Task<TProjectTo> CreateAsync<TProjectTo>(EmailRequester emailRequester)
    {
        var emailRequesterEntity = _mapper.Map<EmailRequester, EmailRequesterEntity>(emailRequester);
        await _context.EmailRequesters.AddAsync(emailRequesterEntity);
        var result = await SaveAsync();
        if (result.IsFailure)
        {
            throw new Exception("Something went wrong during save message");
        }

        return _mapper.Map<EmailRequesterEntity, TProjectTo>(emailRequesterEntity);
    }

    public Task<TProjectTo> UpdateAsync<TProjectTo>(EmailRequester emailRequester)
    {
        throw new NotImplementedException();
    }

    public Task<TProjectTo> DeleteAsync<TProjectTo>(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<TProjectTo>> GetByEmailsAsync<TProjectTo>(List<string> emails) =>
        _context.EmailRequesters
            .AsNoTracking()
            .Where(er => emails.Contains(er.Email))
            .ProjectTo<TProjectTo>(_mapperConfig)
            .ToListAsync();
    
    private async Task<Result> SaveAsync() =>
        await _context.SaveChangesAsync() > 0
            ? Result.Success()
            : Result.Failure<bool>("Something went wrong during save message");
}