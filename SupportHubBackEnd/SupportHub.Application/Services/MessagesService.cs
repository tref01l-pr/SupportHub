using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SupportHub.API;
using SupportHub.Domain.Dto.EmailConversationDtos;
using SupportHub.Domain.Dto.EmailRequesterDtos;
using SupportHub.Domain.Dtos.EmailBotDtos;
using SupportHub.Domain.Dtos.EmailConversationDtos;
using SupportHub.Domain.Dtos.EmailMeesageDtos;
using SupportHub.Domain.Helpers;
using SupportHub.Domain.Interfaces;
using SupportHub.Domain.Interfaces.Application;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Interfaces.DataAccess.Email;
using SupportHub.Domain.Interfaces.Infrastructure;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Services;

public class MessagesService : IMessagesService
{
    private readonly IEmailImapService _emailImapService;
    private readonly IEmailSmtpService _emailSmtpService;
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IEmailMessagesRepository _emailMessagesRepository;
    private readonly ICacheRepository _cacheRepository;
    private readonly IEmailRequestersRepository _emailRequesterRepository;
    private readonly IEmailConversationsRepository _emailConversationsRepository;
    private readonly IEmailBotsRepository _emailBotsRepository;
    private readonly ILogger<MessagesService> _logger;

    public MessagesService(
        IEmailImapService emailImapService,
        IEmailSmtpService emailSmtpService,
        IUsersRepository usersRepository,
        ITransactionsRepository transactionsRepository,
        IEmailMessagesRepository emailMessagesRepository,
        ICacheRepository cacheRepository,
        IEmailRequestersRepository emailRequestersRepository,
        IEmailConversationsRepository emailConversationsRepository,
        IEmailBotsRepository emailBotsRepository,
        ILogger<MessagesService> logger)
    {
        _emailImapService = emailImapService;
        _emailSmtpService = emailSmtpService;
        _usersRepository = usersRepository;
        _transactionsRepository = transactionsRepository;
        _emailMessagesRepository = emailMessagesRepository;
        _cacheRepository = cacheRepository;
        _emailRequesterRepository = emailRequestersRepository;
        _emailConversationsRepository = emailConversationsRepository;
        _emailBotsRepository = emailBotsRepository;
        _logger = logger;
    }

    public async Task<Result<EmailMessageDto>> SendEmailMessageAsync(int emailConversationId, int companyId,
        string body, Guid userId)
    {
        try
        {
            await _transactionsRepository.BeginTransactionAsync();
            var emailConversation =
                await _emailConversationsRepository.GetByIdAsync<EmailConversationWithRequesterWithBotDto>(
                    emailConversationId);

            if (emailConversation == null)
            {
                throw new Exception("Email conversation not found");
            }

            if (companyId != emailConversation.CompanyId)
            {
                throw new Exception("Email Conversation not found");
            }

            var emailMessageBuilder = EmailMessage.Builder()
                .SetEmailConversationId(emailConversationId)
                .SetEmailRequesterId(emailConversation.EmailRequesterId)
                .SetUserId(userId)
                .SetSubject("Re: " + emailConversation.Subject)
                .SetBody(body)
                .SetDate(DateTimeOffset.Now)
                .SetMessageType(MessageTypes.Answer);

            var emailMessageWithoutMsgId = emailMessageBuilder.Build();
            if (emailMessageWithoutMsgId.IsFailure)
            {
                throw new Exception(emailMessageWithoutMsgId.Error);
            }

            var sendResult = await _emailSmtpService.SendReplyMessageAsync(emailConversation.EmailBot,
                emailMessageWithoutMsgId.Value,
                emailConversation.EmailRequester.Email, emailConversation.MsgId);
            if (sendResult.IsFailure)
            {
                throw new Exception(sendResult.Error);
            }

            var emailMessage = emailMessageBuilder
                .SetMessageId(sendResult.Value)
                .Build();

            if (emailMessage.IsFailure)
            {
                throw new Exception(emailMessage.Error);
            }

            var createResult = await _emailMessagesRepository.CreateAsync<EmailMessageDto>(emailMessage.Value);
            if (createResult.IsFailure)
            {
                throw new Exception(createResult.Error);
            }

            var transactionResult = await _transactionsRepository.CommitAsync();

            if (transactionResult.IsFailure)
            {
                return Result.Failure<EmailMessageDto>(transactionResult.Error);
            }

            return createResult.Value;
        }
        catch (Exception e)
        {
            var transactionRollbackResult = await _transactionsRepository.RollbackAsync();

            if (transactionRollbackResult.IsFailure)
            {
                return Result.Failure<EmailMessageDto>(transactionRollbackResult.Error);
            }

            return Result.Failure<EmailMessageDto>(e.Message);
        }
    }

    public async Task<Result> SendTestEmailMessageAsync(int emailSmtpId, string message, string to)
    {
        try
        {
            var emailBot = await _emailBotsRepository.GetByIdAsync<EmailBotDto>(emailSmtpId);
            if (emailBot == null)
            {
                throw new Exception("Email bot not found");
            }

            var sendResult = await _emailSmtpService.SendMessageAsync(emailBot, "Test message", message, to);
            if (sendResult.IsFailure)
            {
                throw new Exception(sendResult.Error);
            }

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }


    //TODO make here a request to redis
    public async Task<Result<List<TProjectTo>>> GetLastConversationsByCompanyIdAsync<TProjectTo>(int companyId)
    {
        /*var cacheLastMessages = await _cacheRepository.GetLastMessagesAsync();
        if (cacheLastMessages.IsFailure)
        {
            return Result.Failure<ImapMessage[]>(cacheLastMessages.Error);
        }

        if (cacheLastMessages.Value.Any())
        {
            return cacheLastMessages.Value.ToArray();
        }*/
        try
        {
            var receivedMessageFromQuestionEmails = await _emailConversationsRepository.GetLastByCompanyIdAsync<TProjectTo>(companyId);
            if (!receivedMessageFromQuestionEmails.Any())
            {
                throw new Exception("Received messages not found");
            }
        
            return receivedMessageFromQuestionEmails;
        }
        catch (Exception e)
        {
            return Result.Failure<List<TProjectTo>>(e.Message);
        }
    }

    public async Task<Result> EventOnMessageReceivedAsync(ImapOptions imapOptions)
    {
        try
        {
            var newMessages =
                await CheckUpdates(imapOptions.User, imapOptions.Password, imapOptions.Port, "imap.gmail.com");
            if (newMessages.IsFailure)
            {
                return Result.Failure(newMessages.Error);
            }

            if (newMessages.Value.Count == 0)
            {
                return Result.Failure("new messages not found");
            }

            List<string> botEmails = new List<string>();
            //Here we can choose which emails we want to get
            //if there aren't any bots with this email, we can add it to our romanListSender conversation
            foreach (var message in newMessages.Value)
            {
                if (!botEmails.Contains(message.EmailBot))
                {
                    botEmails.Add(message.EmailBot);
                }
            }

            //Get emailBots by emails from romanListSender
            var emailBots = await _emailBotsRepository.GetByEmailsAsync<EmailBotDto>(botEmails);
            if (emailBots == null)
            {
                return Result.Failure("Email bots not found");
            }

            foreach (var emailBot in emailBots)
            {
                try
                {
                    await _transactionsRepository.BeginTransactionAsync();

                    var addEmailMessagesResult = await AddEmailMessagesByEmailBot(emailBot);
                    if (addEmailMessagesResult.IsFailure)
                    {
                        throw new Exception(addEmailMessagesResult.Error);
                    }

                    await _transactionsRepository.CommitAsync();
                }
                catch (Exception e)
                {
                    await _transactionsRepository.RollbackAsync();
                    _logger.LogError(e, e.Message);
                }
                //Check updates for every emailBot
            }

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result> AddMessageOnInitialize(EmailBotDto emailBot, int count)
    {
        var newMessagesFromEmailBot = await CheckUpdates(emailBot.Email, emailBot.Password,
            emailBot.ImapPort, emailBot.ImapHost);
        if (newMessagesFromEmailBot.IsFailure)
        {
            return Result.Failure(newMessagesFromEmailBot.Error);
        }

        var messagesByCount = await _emailImapService.GetRecentMessages(emailBot.Email, emailBot.Password,
            emailBot.ImapPort, emailBot.ImapHost, count);
        if (messagesByCount.Value.Count == 0)
        {
            return Result.Failure(messagesByCount.Error);
        }
        
        return await ProcessEmailMessages(
            newMessagesFromEmailBot.Value
                .Concat(messagesByCount.Value)
                .GroupBy(msg => msg.MsgId)
                .Select(group => group.First())
                .ToList(),
            emailBot);
    }

    private async Task<Result> AddEmailMessagesByEmailBot(EmailBotDto emailBot)
    {
        var newMessagesFromEmailBot = await CheckUpdates(emailBot.Email, emailBot.Password,
            emailBot.ImapPort, emailBot.ImapHost);

        if (newMessagesFromEmailBot.IsFailure)
        {
            return Result.Failure(newMessagesFromEmailBot.Error);
        }

        if (newMessagesFromEmailBot.Value.Count == 0)
        {
            return Result.Failure("new messages not found");
        }

        //test it

        return await ProcessEmailMessages(newMessagesFromEmailBot.Value, emailBot);
    }

    private async Task<Result> ProcessEmailMessages(IEnumerable<ReceivedMessage> messages, EmailBotDto emailBot)
    {
        var messageDictionary = messages.ToDictionary(m => m.MsgId);
        var conversations = new Dictionary<string, EmailConversationDto>();
        var emailRequesters = new Dictionary<string, EmailRequesterDto>();

        try
        {
            foreach (var message in messages.Where(m => m.ReplyToMsgId == null).ToList())
            {
                var result = await ProcessNewMessageAsync(emailRequesters, conversations, message, emailBot);
                if (result.IsFailure)
                {
                    _logger.LogError(result.Error);
                }
            }

            var pendingReplies = new List<ReceivedMessage>();
            foreach (var message in messages.Where(m => m.ReplyToMsgId != null).ToList())
            {
                if (messageDictionary.TryGetValue(message.ReplyToMsgId, out var parentMessage))
                {
                    if (conversations.TryGetValue(parentMessage.MsgId, out var conversation))
                    {
                        await CreateEmailMessage(message, conversation.Id, conversation.EmailRequesterId);
                        continue;
                    }

                    pendingReplies.Add(message);
                    continue;
                }

                var parentMessageDb =
                    await _emailMessagesRepository.GetByMessageIdAsync<EmailMessageDto>(message.ReplyToMsgId);

                if (parentMessageDb == null)
                {
                    pendingReplies.Add(message);
                    continue;
                }

                await CreateEmailMessage(message, parentMessageDb.EmailConversationId,
                    parentMessageDb.EmailRequesterId.Value);
            }

            if (pendingReplies.Any())
            {
                var result = await GetConversationsByReplyIdsAsync(emailBot, pendingReplies.Select(
                    m => new SimpleMessageInfo
                    {
                        Id = m.MsgId,
                        RequesterEmail = m.RequsterEmail,
                    }).ToList());
                if (result.IsFailure)
                {
                    //throw new Exception(result.Error);
                }

                foreach (var conversation in result.Value)
                {
                    var messageFromDb =
                        await _emailMessagesRepository.GetByMessageIdAsync<EmailMessageDto>(conversation.Key);
                    if (messageFromDb == null)
                    {
                        var firstMessage = conversation.Value.Find(m => m.MsgId == conversation.Key);
                        if (firstMessage == null)
                        {
                            continue;
                        }

                        var resultOfCreationFirstMessage =
                            await ProcessNewMessageAsync(emailRequesters, conversations, firstMessage, emailBot);

                        if (resultOfCreationFirstMessage.IsFailure)
                        {
                            continue;
                        }

                        messageFromDb = resultOfCreationFirstMessage.Value;
                    }

                    foreach (var message in conversation.Value.Where(m => m.MsgId != conversation.Key))
                    {
                        await CreateEmailMessage(message, messageFromDb.EmailConversationId,
                            messageFromDb.EmailRequesterId.Value);
                    }
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result<EmailMessageDto>> ProcessNewMessageAsync(
        Dictionary<string, EmailRequesterDto> emailRequesters,
        Dictionary<string, EmailConversationDto> conversations,
        ReceivedMessage message,
        EmailBotDto emailBot)
    {
        var messageExist = await _emailMessagesRepository.GetByMessageIdAsync<EmailMessageDto>(message.MsgId);
        if (messageExist != null)
        {
            return Result.Failure<EmailMessageDto>("Message already exists");
        }

        var emailRequesterResult = await GetOrCreateEmailRequesterAsync(message.RequsterEmail, emailRequesters);
        if (emailRequesterResult.IsFailure)
        {
            return Result.Failure<EmailMessageDto>(emailRequesterResult.Error);
        }

        var emailRequester = emailRequesterResult.Value;

        var newConversation = EmailConversation.Create(emailBot.CompanyId, emailBot.Id, emailRequester.Id,
            message.MsgId, message.Subject, message.Date);
        if (newConversation.IsFailure)
        {
            return Result.Failure<EmailMessageDto>(newConversation.Error);
        }

        var createdConversation =
            await CreateConversationAsync(newConversation.Value, conversations, message.MsgId);
        if (createdConversation == null)
        {
            return Result.Failure<EmailMessageDto>("Conversation not created");
        }

        return await CreateEmailMessage(message, createdConversation.Id, emailRequester.Id);
    }

    private async Task<Result<Dictionary<string, List<ReceivedMessage>>>> GetConversationsByReplyIdsAsync(
        EmailBotDto emailBot, List<SimpleMessageInfo> ids)
    {
        var conversationsResult = await _emailImapService.GetConversationsByIds(emailBot.Email, emailBot.Password,
            emailBot.ImapPort, emailBot.ImapHost, ids);
        if (conversationsResult.IsFailure)
        {
            return Result.Failure<Dictionary<string, List<ReceivedMessage>>>(conversationsResult.Error);
        }

        return conversationsResult;
    }

    private async Task<Result<EmailRequesterDto>> GetOrCreateEmailRequesterAsync(
        string email,
        Dictionary<string, EmailRequesterDto> emailRequesters)
    {
        var findRequesterResult = await FindEmailRequesterAsync(email, emailRequesters);
        if (!findRequesterResult.IsFailure)
        {
            return findRequesterResult;
        }

        var emailRequesterResult = EmailRequester.Create(email);
        if (emailRequesterResult.IsFailure)
        {
            return Result.Failure<EmailRequesterDto>(emailRequesterResult.Error);
        }

        var createdRequester =
            await _emailRequesterRepository.CreateAsync<EmailRequesterDto>(emailRequesterResult.Value);
        emailRequesters[email] = createdRequester;

        return Result.Success(createdRequester);
    }

    private async Task<Result<EmailRequesterDto>> FindEmailRequesterAsync(string email,
        Dictionary<string, EmailRequesterDto> emailRequesters)
    {
        if (emailRequesters.TryGetValue(email, out var cachedRequester))
        {
            return Result.Success(cachedRequester);
        }

        var existingRequester = await _emailRequesterRepository.GetByEmailAsync<EmailRequesterDto>(email);
        if (existingRequester != null)
        {
            emailRequesters[email] = existingRequester;
            return Result.Success(existingRequester);
        }

        return Result.Failure<EmailRequesterDto>("Email requester not found");
    }

    private async Task<EmailConversationDto> CreateConversationAsync(
        EmailConversation conversation,
        Dictionary<string, EmailConversationDto> conversations,
        string messageId)
    {
        var createdConversation = await _emailConversationsRepository.CreateAsync<EmailConversationDto>(conversation);
        conversations[messageId] = createdConversation;
        return createdConversation;
    }

    private async Task<Result<EmailMessageDto>> CreateEmailMessage(
        ReceivedMessage message,
        int conversationId,
        int emailRequesterId)
    {
        var messageExist = await _emailMessagesRepository.GetByMessageIdAsync<EmailMessageDto>(message.MsgId);
        if (messageExist != null)
        {
            return Result.Failure<EmailMessageDto>("Message already exists");
        }

        var emailMessage = EmailMessage.Builder()
            .SetEmailConversationId(conversationId)
            .SetEmailRequesterId(emailRequesterId)
            .SetMessageId(message.MsgId)
            .SetBody(message.Body)
            .SetDate(message.Date)
            .SetMessageType(message.MessageType)
            .Build();

        if (emailMessage.IsFailure)
        {
            return Result.Failure<EmailMessageDto>(emailMessage.Error);
        }

        var emailMessageCreateResult = await _emailMessagesRepository.CreateAsync<EmailMessageDto>(emailMessage.Value);
        if (emailMessageCreateResult.IsFailure)
        {
            throw new Exception(emailMessageCreateResult.Error);
        }

        return emailMessageCreateResult.Value;
    }

    private async Task<Result<List<ReceivedMessage>>> CheckUpdates(string user, string password, int port, string host)
    {
        var newMessages = await _emailImapService.GetUnreadMessages(user, password, port, host);
        if (newMessages.IsFailure)
        {
            return Result.Failure<List<ReceivedMessage>>(newMessages.Error);
        }

        if (newMessages.Value.Count == 0)
        {
            return Result.Success<List<ReceivedMessage>>(new List<ReceivedMessage>());
        }

        return newMessages;
    }

    public async Task<Result<bool>> RemoveKeyAsync()
    {
        var result = await _cacheRepository.RemoveKey();
        if (result.IsFailure)
        {
            return Result.Failure<bool>(result.Error);
        }

        return result.Value;
    }
}