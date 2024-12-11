using CSharpFunctionalExtensions;
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
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IEmailMessagesRepository _emailMessagesRepository;
    private readonly IEmailBotsRepository _emailBotsRepository;

    public EmailBotsService(
        ITransactionsRepository transactionsRepository,
        IEmailMessagesRepository emailMessagesRepository,
        IEmailBotsRepository emailBotsRepository)
    {
        _transactionsRepository = transactionsRepository;
        _emailMessagesRepository = emailMessagesRepository;
        _emailBotsRepository = emailBotsRepository;
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

            var creationResult = await _emailBotsRepository.CreateAsync<TProjectTo>(emailBot);
            return creationResult;
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