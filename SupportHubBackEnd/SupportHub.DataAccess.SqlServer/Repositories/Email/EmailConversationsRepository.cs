using AutoMapper;
using AutoMapper.QueryableExtensions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SupportHub.DataAccess.SqlServer.Entities.Email;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Models;

namespace SupportHub.DataAccess.SqlServer.Repositories;

public class EmailConversationsRepository : IEmailConversationsRepository
{
    private readonly SupportHubDbContext _context;
    private readonly IMapper _mapper;
    
    private IConfigurationProvider _mapperConfig => _mapper.ConfigurationProvider;

    public EmailConversationsRepository(SupportHubDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TProjectTo?> GetByIdAsync<TProjectTo>(int id) =>
        await _context.EmailConversations
            .AsNoTracking()
            .Where(ec => ec.Id == id)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    public async Task<List<TProjectTo>> GetByCompanyIdAsync<TProjectTo>(int companyId) =>
        await _context.EmailConversations
            .AsNoTracking()
            .Where(ec => ec.CompanyId == companyId)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .ToListAsync();

    public async Task<List<TProjectTo>> GetAllAsync<TProjectTo>() =>
        await _context.EmailConversations
            .AsNoTracking()
            .ProjectTo<TProjectTo>(_mapperConfig)
            .ToListAsync();

    public async Task<TProjectTo> CreateAsync<TProjectTo>(EmailConversation newConversationValue)
    {
        var newConversationEntity = _mapper.Map<EmailConversation, EmailConversationEntity>(newConversationValue);
        await _context.EmailConversations.AddAsync(newConversationEntity);
        var result = await SaveAsync();
        if (result.IsFailure)
        {
            throw new Exception("Something went wrong during save message");
        }
        
        return _mapper.Map<EmailConversationEntity, TProjectTo>(newConversationEntity);
    }
    
    private async Task<Result> SaveAsync() =>
        await _context.SaveChangesAsync() > 0
            ? Result.Success()
            : Result.Failure<bool>("Something went wrong during save message");
}