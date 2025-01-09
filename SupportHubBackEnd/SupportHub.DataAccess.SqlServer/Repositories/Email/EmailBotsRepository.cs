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
            .Where(eb => eb.Email == email && eb.IsDeleted == false)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    public async Task<List<TProjectTo>> GetByCompanyIdAsync<TProjectTo>(int companyId) =>
        await _context.EmailBots
            .AsNoTracking()
            .Where(eb => eb.CompanyId == companyId && eb.IsDeleted == false)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .ToListAsync();

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

    public async Task<TProjectTo> UpdateAsync<TProjectTo>(EmailBot emailBot)
    {
        var emailBotEntity = _mapper.Map<EmailBot, EmailBotEntity>(emailBot);
        _context.EmailBots.Update(emailBotEntity);
        var result = await SaveAsync();
        if (result.IsFailure)
        {
            throw new Exception("Something went wrong during update message");
        }

        return _mapper.Map<EmailBotEntity, TProjectTo>(emailBotEntity);
    }

    public async Task<TProjectTo> DeleteAsync<TProjectTo>(int id)
    {
        var emailBotEntity = await _context.EmailBots.Where(eb => eb.Id == id).FirstOrDefaultAsync();
        if (emailBotEntity == null)
        {
            throw new Exception("Email bot not found");
        }

        emailBotEntity.IsDeleted = true;
        emailBotEntity.DeletedOn = DateOnly.FromDateTime(DateTime.Now);
        _context.EmailBots.Update(emailBotEntity);
        var result = await SaveAsync();
        if (result.IsFailure)
        {
            throw new Exception("Something went wrong during delete message");
        }

        return _mapper.Map<EmailBotEntity, TProjectTo>(emailBotEntity);
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