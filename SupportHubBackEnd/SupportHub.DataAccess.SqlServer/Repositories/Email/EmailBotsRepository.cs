using AutoMapper;
using AutoMapper.QueryableExtensions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SupportHub.DataAccess.SqlServer.Entities.Email;
using SupportHub.Domain.Interfaces.DataAccess.Email;
using SupportHub.Domain.Models;

namespace SupportHub.DataAccess.SqlServer.Repositories;

public class EmailBotsRepository : IEmailBotsRepository
{
    private readonly SupportHubDbContext _context;
    private readonly IMapper _mapper;
    
    private IConfigurationProvider _mapperConfig => _mapper.ConfigurationProvider;

    public EmailBotsRepository(SupportHubDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<TProjectTo?> GetByIdAsync<TProjectTo>(int id) =>
        await _context.EmailBots
            .AsNoTracking()
            .Where(eb => eb.Id == id)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    public async Task<TProjectTo?> GetByEmailAsync<TProjectTo>(string email) =>
        await _context.EmailBots
            .AsNoTracking()
            .Where(eb => eb.Email == email)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    public async Task<TProjectTo> CreateAsync<TProjectTo>(EmailBot emailBot)
    {
        var emailBotEntity = _mapper.Map<EmailBot, EmailBotEntity>(emailBot);
        await _context.EmailBots.AddAsync(emailBotEntity);
        var result = await SaveAsync();
        if (result.IsFailure)
        {
            throw new Exception("Something went wrong during save message");
        }

        return _mapper.Map<EmailBotEntity, TProjectTo>(emailBotEntity);
    }

    public Task<TProjectTo> UpdateAsync<TProjectTo>(EmailBot emailBot)
    {
        throw new NotImplementedException();
    }

    public Task<TProjectTo> DeleteAsync<TProjectTo>(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<TProjectTo>> GetByEmailsAsync<TProjectTo>(List<string> botEmails) =>
        await _context.EmailBots
            .AsNoTracking()
            .Where(eb => botEmails.Contains(eb.Email))
            .ProjectTo<TProjectTo>(_mapperConfig)
            .ToListAsync();

    private async Task<Result> SaveAsync() =>
        await _context.SaveChangesAsync() > 0
            ? Result.Success()
            : Result.Failure<bool>("Something went wrong during save message");
}