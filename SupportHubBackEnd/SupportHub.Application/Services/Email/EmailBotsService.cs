using AutoMapper;
using CSharpFunctionalExtensions;
using SupportHub.Domain.Dtos.EmailBotDtos;
using SupportHub.Domain.Interfaces;
using SupportHub.Domain.Interfaces.Application;
using SupportHub.Domain.Interfaces.Application.Email;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Interfaces.DataAccess.Email;
using SupportHub.Domain.Interfaces.Infrastructure;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Services.Email;

public class EmailBotsService : IEmailBotsService
{
    private static int NUMBER_OF_MESSAGES_TO_FETCH_ON_INITIALIZE = 100;
    
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IEmailMessagesRepository _emailMessagesRepository;
    private readonly IEmailBotsRepository _emailBotsRepository;
    private readonly IEmailSmtpService _emailSmtpService;
    private readonly IEmailImapService _emailImapService;
    private readonly IMapper _mapper;
    private readonly IMessagesService _messagesService;


    public EmailBotsService(
        ITransactionsRepository transactionsRepository,
        IEmailMessagesRepository emailMessagesRepository,
        IEmailBotsRepository emailBotsRepository,
        IEmailSmtpService emailSmtpService,
        IEmailImapService emailImapService,
        IMessagesService messagesService,
        IMapper mapper)
    {
        _transactionsRepository = transactionsRepository;
        _emailMessagesRepository = emailMessagesRepository;
        _emailBotsRepository = emailBotsRepository;
        _emailSmtpService = emailSmtpService;
        _emailImapService = emailImapService;
        _messagesService = messagesService;
        _mapper = mapper;
    }

    public async Task<Result<TProjectTo>> GetByIdAsync<TProjectTo>(int id)
    {
        try
        {
            var emailBot = await _emailBotsRepository.GetByIdAsync<TProjectTo>(id);
            return emailBot;
        }
        catch (Exception e)
        {
            return Result.Failure<TProjectTo>(e.Message);
        }
    }

    public async Task<Result<TProjectTo>> GetByEmailAsync<TProjectTo>(string email)
    {
        try
        {
            var emailBot = await _emailBotsRepository.GetByEmailAsync<TProjectTo>(email);
            return emailBot;
        }
        catch (Exception e)
        {
            return Result.Failure<TProjectTo>(e.Message);
        }
    }

    public async Task<Result<TProjectTo>> CreateAsync<TProjectTo>(EmailBot emailBot)
    {
        try
        {
            var emailBotExists = await _emailBotsRepository.GetByEmailAsync<EmailBot>(emailBot.Email);
            if (emailBotExists != null)
            {
                throw new Exception("email bot already exists");
            }

            var testSmtpConnectionResult = await _emailSmtpService.TestSmtpConnectionAsync(
                emailBot.Email,
                emailBot.Password,
                emailBot.SmtpHost,
                emailBot.SmtpPort);

            if (testSmtpConnectionResult.IsFailure)
            {
                throw new Exception(testSmtpConnectionResult.Error);
            }

            var testImapConnectionResult = await _emailImapService.TestImapConnectionAsync(
                emailBot.Email,
                emailBot.Password,
                emailBot.ImapHost,
                emailBot.ImapPort);

            if (testImapConnectionResult.IsFailure)
            {
                throw new Exception(testImapConnectionResult.Error);
            }

            var creationResult = await _emailBotsRepository.CreateAsync<EmailBotDto>(emailBot);

            var initializeFetchMessages = await _messagesService.AddMessageOnInitialize(creationResult, NUMBER_OF_MESSAGES_TO_FETCH_ON_INITIALIZE);
            if (initializeFetchMessages.IsFailure)
            {
                throw new Exception(initializeFetchMessages.Error);
            }

            var result = await _emailBotsRepository.GetByIdAsync<TProjectTo>(creationResult.Id);
            if (result == null)
            {
                throw new Exception("email bot was not created");
            }
            
            return result;
        }
        catch (Exception e)
        {
            return Result.Failure<TProjectTo>(e.Message);
        }
    }

    public async Task<Result<TProjectTo>> UpdateAsync<TProjectTo>(EmailBot emailBot)
    {
        try
        {
            var updateResult = await _emailBotsRepository.UpdateAsync<TProjectTo>(emailBot);
            return updateResult;
        }
        catch (Exception e)
        {
            return Result.Failure<TProjectTo>(e.Message);
        }
    }

    public async Task<Result<TProjectTo>> DeleteAsync<TProjectTo>(int id)
    {
        try
        {
            var deleteResult = await _emailBotsRepository.DeleteAsync<TProjectTo>(id);
            return deleteResult;
        }
        catch (Exception e)
        {
            return Result.Failure<TProjectTo>(e.Message);
        }
    }
}