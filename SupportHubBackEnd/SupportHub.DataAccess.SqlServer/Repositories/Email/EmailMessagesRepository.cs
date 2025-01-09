using AutoMapper;
using AutoMapper.QueryableExtensions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SupportHub.DataAccess.SqlServer.Entities.Email;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Models;

namespace SupportHub.DataAccess.SqlServer.Repositories;

public class EmailMessagesRepository : IEmailMessagesRepository
{
    private readonly SupportHubDbContext _context;
    private readonly IMapper _mapper;

    private IConfigurationProvider _mapperConfig => _mapper.ConfigurationProvider;

    public EmailMessagesRepository(SupportHubDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<int>> GetNumberOfMessagesAsync()
    {
        int count = await _context.EmailMessages.CountAsync();
        return Result.Success(count);
    }

    public async Task<EmailMessage[]> GetByRequesterIdAsync(int id) =>
        await _context.EmailMessages
            .TagWith("Get By Requester Async messages by question email")
            .AsNoTracking()
            .Where(x => x.EmailRequesterId == id)
            .ProjectTo<EmailMessage>(_mapperConfig)
            .ToArrayAsync();

    public async Task<EmailMessage[]> GetLastMessagesAsync() =>
        await _context.EmailMessages
            .TagWith("Get last sent messages by question email")
            .AsNoTracking()
            .GroupBy(m => m.EmailConversationId)
            .Select(g =>
                g.OrderByDescending(m => m.Date).FirstOrDefault())
            .ProjectTo<EmailMessage>(_mapperConfig)
            .ToArrayAsync();

    public async Task<Result<TProjectTo>> CreateAsync<TProjectTo>(EmailMessage emailMessage)
    {
        var receivedMessageEntity = _mapper.Map<EmailMessage, EmailMessageEntity>(emailMessage);
        await _context.EmailMessages.AddAsync(receivedMessageEntity);

        //Update conversation last message
        var conversation = await _context.EmailConversations
            .TagWith("Get conversation by id")
            .FirstOrDefaultAsync(x => x.Id == emailMessage.EmailConversationId);
        if (conversation == null)
        {
            return Result.Failure<TProjectTo>("Conversation not found");
        }

        if (conversation.LastUpdateDate < emailMessage.Date)
        {
            conversation.LastUpdateDate = emailMessage.Date;
            _context.EmailConversations.Update(conversation);
        }

        var result = await SaveAsync();
        if (!result.Value)
        {
            return Result.Failure<TProjectTo>("Something went wrong during save message");
        }

        return _mapper.Map<EmailMessageEntity, TProjectTo>(receivedMessageEntity);
    }

    public async Task<TProjectTo?> GetByMessageIdAsync<TProjectTo>(string messageReplyToMsgId) =>
        await _context.EmailMessages
            .TagWith("Get By Message Id Async")
            .AsNoTracking()
            .Where(x => x.MessageId == messageReplyToMsgId)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    private async Task<Result<bool>> SaveAsync() => await _context.SaveChangesAsync() > 0;
}