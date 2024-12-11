using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Models;

namespace SupportHub.DataAccess.SqlServer.Repositories;

public class CacheRepository : ICacheRepository
{
    private readonly IDistributedCache _distributedCache;
    private const string CacheKey = "imapmessage";

    public CacheRepository(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<Result<List<ImapMessage>>> GetLastMessagesAsync()
    {
        try
        {
            string? cacheImapMessage = await _distributedCache.GetStringAsync(CacheKey);
            if (string.IsNullOrWhiteSpace(cacheImapMessage))
            {
                return new List<ImapMessage>();
            }

            List<ImapMessage>? imapMessages = JsonConvert.DeserializeObject<List<ImapMessage>>(
                cacheImapMessage,
                new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                });

            if (imapMessages == null)
            {
                return Result.Failure<List<ImapMessage>>("Deserialization failed");
            }

            return imapMessages;
        }
        catch (Exception e)
        {
            return Result.Failure<List<ImapMessage>>(e.Message);
        }
    }

    public async Task<Result<ImapMessage?>> GetLastMessageByRequester(string requester)
    {
        try
        {
            string? cacheImapMessage = await _distributedCache.GetStringAsync(CacheKey);
            if (string.IsNullOrWhiteSpace(cacheImapMessage))
            {
                return null;
            }

            ImapMessage[]? imapMessages = JsonConvert.DeserializeObject<ImapMessage[]>(
                cacheImapMessage,
                new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                });

            if (imapMessages == null)
            {
                return Result.Failure<ImapMessage?>("Deserialization failed");
            }

            var lastMessage = imapMessages.FirstOrDefault(m => m.Requester == requester);

            if (lastMessage == null)
            {
                return Result.Failure<ImapMessage?>("No message with such message");
            }

            return lastMessage;
        }
        catch (Exception e)
        {
            return Result.Failure<ImapMessage?>(e.Message);
        }
    }

    public async Task<Result<bool>> SetLastMessagesAsync(ImapMessage[] lastMessagesValue)
    {
        try
        {
            string serializedMessages = JsonConvert.SerializeObject(
                lastMessagesValue,
                new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                });

            await _distributedCache.SetStringAsync(CacheKey, serializedMessages);

            return true;
        }
        catch (Exception e)
        {
            return Result.Failure<bool>(e.Message);
        }
    }

    public async Task<Result<bool>> UpdateLastMessagesAsync(ImapMessage[] lastMessages)
    {
        try
        {
            string key = CacheKey;

            ImapMessage[] existingMessages;

            string? cacheImapMessage = await _distributedCache.GetStringAsync(key);
            if (string.IsNullOrWhiteSpace(cacheImapMessage))
            {
                return Result.Failure<bool>("Key was not found");
            }

            existingMessages = JsonConvert.DeserializeObject<ImapMessage[]>(
                cacheImapMessage,
                new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                }) ?? Array.Empty<ImapMessage>();

            var updatedMessages = existingMessages.ToList();
            foreach (var lastMessage in lastMessages)
            {
                var index = updatedMessages.FindIndex(msg => msg.Requester == lastMessage.Requester);
                if (index >= 0)
                {
                    updatedMessages[index] = lastMessage;
                }
                else
                {
                    updatedMessages.Add(lastMessage);
                }
            }

            updatedMessages = updatedMessages.OrderByDescending(msg => msg.Date).ToList();

            string serializedMessage = JsonConvert.SerializeObject(
                updatedMessages.ToArray(),
                new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                });

            await _distributedCache.SetStringAsync(key, serializedMessage);

            return true;
        }
        catch (Exception e)
        {
            return Result.Failure<bool>(e.Message);
        }
    }

    public async Task<Result<bool>> RemoveKey()
    {
        try
        {
            await _distributedCache.RemoveAsync(CacheKey);
            return true;
        }
        catch (Exception e)
        {
            return Result.Failure<bool>(e.Message);
        }
    }
}