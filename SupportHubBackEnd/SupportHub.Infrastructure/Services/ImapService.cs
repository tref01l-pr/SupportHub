using CSharpFunctionalExtensions;
using SupportHub.Domain.Options;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using SupportHub.API;
using SupportHub.Domain.Dtos.EmailBotDtos;
using SupportHub.Domain.Helpers;
using SupportHub.Domain.Interfaces;
using SupportHub.Domain.Models;

namespace SupportHub.Infrastructure.Services;

public class ImapService : IImapService
{
    //imap.gmail.com 993
    public async Task<Result<List<MimeMessage>>> GetMessagesAsync(EmailBotDto emailBotDto)
    {
        try
        {
            var client = new ImapClient();

            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(emailBotDto.ImapHost, emailBotDto.ImapPort, true);

            await client.AuthenticateAsync(emailBotDto.Email, emailBotDto.Password);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            List<MimeMessage> mimeMessages = new List<MimeMessage>();

            for (int i = 0; i < inbox.Count; i++)
            {
                var message = await inbox.GetMessageAsync(i);
                mimeMessages.Add(message);
            }

            await client.DisconnectAsync(true);

            return mimeMessages;
        }
        catch (Exception e)
        {
            return Result.Failure<List<MimeMessage>>(e.Message);
        }
    }

    public async Task<Result<List<ImapMessage>>> GetLastMessage(ImapOptions imapOptions)
    {
        try
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync("imap.gmail.com", imapOptions.Port, true);
                await client.AuthenticateAsync(imapOptions.User, imapOptions.Password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                // Get all messages sorted by date (newest first)
                var allMessages =
                    (await inbox.FetchAsync(0, -1,
                        MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope |
                        MessageSummaryItems.BodyStructure))
                    .OrderByDescending(summary => summary.Date);

                var latestMessagesByUser = new List<ImapMessage>();

                foreach (var message in allMessages)
                {
                    var sender = message.Envelope.From.Mailboxes.Single().Address;
                    if (!string.IsNullOrEmpty(sender) && !latestMessagesByUser.Any(m => m.From == sender))
                    {
                        var text = (TextPart)(await inbox.GetBodyPartAsync(message.UniqueId, message.TextBody));
                        var imapMessage = ImapMessage.Create(
                            message.Envelope.From.Mailboxes.Single().Address,
                            message.Envelope.From.Mailboxes.Single().Address,
                            message.Envelope.Subject,
                            text.Text,
                            message.Date,
                            MessageTypes.Question);

                        if (imapMessage.IsFailure)
                        {
                            return Result.Failure<List<ImapMessage>>(imapMessage.Error);
                        }

                        latestMessagesByUser.Add(imapMessage.Value);
                    }
                }


                await client.DisconnectAsync(true);
                return latestMessagesByUser;
            }
        }
        catch (Exception e)
        {
            return Result.Failure<List<ImapMessage>>(e.Message);
        }
    }

    public async Task<Result<List<ImapMessage>>> GetMessagesFromRequester(ImapOptions imapOptions, string email)
    {
        try
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync("imap.gmail.com", imapOptions.Port, true);
                await client.AuthenticateAsync(imapOptions.User, imapOptions.Password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                var allMessages =
                    (await inbox.FetchAsync(0, -1,
                        MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope |
                        MessageSummaryItems.BodyStructure))
                    .Where(m => m.Envelope.From.Mailboxes.Single().Address == email)
                    .OrderByDescending(summary => summary.Date);

                if (!allMessages.Any())
                {
                    return Result.Failure<List<ImapMessage>>("There is no lists from that email");
                }

                var userMessages = new List<ImapMessage>();

                foreach (var message in allMessages)
                {
                    var text = (TextPart)(await inbox.GetBodyPartAsync(message.UniqueId, message.TextBody));
                    var imapMessage = ImapMessage.Create(
                        message.Envelope.From.Mailboxes.Single().Address,
                        message.Envelope.From.Mailboxes.Single().Address,
                        message.Envelope.Subject,
                        text.Text,
                        message.Date,
                        MessageTypes.Question);

                    if (imapMessage.IsFailure)
                    {
                        return Result.Failure<List<ImapMessage>>(imapMessage.Error);
                    }

                    userMessages.Add(imapMessage.Value);
                }

                await client.DisconnectAsync(true);
                return userMessages;
            }
        }
        catch (Exception e)
        {
            return Result.Failure<List<ImapMessage>>(e.Message);
        }
    }

    public async Task<Result<List<string>>> GetMessagesBody(ImapOptions imapOptions,
        Dictionary<string, IMessageSummary> allMessages)
    {
        try
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync("imap.gmail.com", imapOptions.Port, true);
                await client.AuthenticateAsync(imapOptions.User, imapOptions.Password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                List<string> resultBodies = new List<string>();

                foreach (var message in allMessages)
                {
                    var text = (TextPart)(await inbox.GetBodyPartAsync(message.Value.UniqueId, message.Value.TextBody));
                    resultBodies.Add(text.Text);
                }

                await client.DisconnectAsync(true);
                return resultBodies;
            }
        }
        catch (Exception e)
        {
            return Result.Failure<List<string>>(e.Message);
        }
    }

    public async Task<Result<List<EmailMessage>>> GetAllSentMessages(ImapOptions imapOptions)
    {
        try
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync("imap.gmail.com", imapOptions.Port, true);
                await client.AuthenticateAsync(imapOptions.User, imapOptions.Password);

                var sentFolder = client.GetFolder(SpecialFolder.Sent);
                await sentFolder.OpenAsync(FolderAccess.ReadOnly);

                var allMessages =
                    (await sentFolder.FetchAsync(0, -1,
                        MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope |
                        MessageSummaryItems.BodyStructure))
                    .OrderByDescending(summary => summary.Date).ToList();

                var newMessages = new List<EmailMessage>();

                foreach (var t in allMessages)
                {
                    var sender = t.Envelope.From.Mailboxes.Single().Address;
                    if (string.IsNullOrEmpty(sender))
                    {
                        return Result.Failure<List<EmailMessage>>("Sender name is empty!!!");
                    }

                    string text = "null";

                    if (t.TextBody is not null)
                    {
                        text = ((TextPart)await sentFolder.GetBodyPartAsync(t.UniqueId, t.TextBody)).ToString();
                    }


                    //TODO сделать добавление в базу данных
                    /*var sentMessage = EmailMessage.Create(
                        Guid.Empty,
                        t.Envelope.To.Mailboxes.Single().Address,
                        t.Envelope.Subject,
                        text,
                        t.Date);

                    if (sentMessage.IsFailure)
                    {
                        return Result.Failure<List<EmailMessage>>(sentMessage.Error);
                    }

                    newMessages.Add(sentMessage.Value);*/
                }

                await client.DisconnectAsync(true);
                return newMessages;
            }
        }
        catch (Exception e)
        {
            return Result.Failure<List<EmailMessage>>(e.Message);
        }
    }

    public async Task<Result<List<ReceivedMessage>>> GetUnreadMessages(string user, string password, int port,
        string host)
    {
        try
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync(host, port, true);
                await client.AuthenticateAsync(user, password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite);

                var unreadMessageUids = await inbox.SearchAsync(SearchQuery.NotSeen);

                if (unreadMessageUids.Count == 0)
                {
                    return Result.Success(new List<ReceivedMessage>());
                }

                var limitedUids = unreadMessageUids.ToList();

                var summaries = await inbox.FetchAsync(limitedUids,
                    MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure);

                var unreadMessages = new List<ReceivedMessage>();

                foreach (var summary in summaries)
                {
                    var messageId = summary.Envelope.MessageId;
                    var sender = summary.Envelope.From.Mailboxes.FirstOrDefault()?.Address;
                    if (string.IsNullOrEmpty(sender))
                    {
                        return Result.Failure<List<ReceivedMessage>>("Sender email is empty.");
                    }

                    var inReplyTo = summary.Envelope.InReplyTo;

                    TextPart? text = null;

                    if (summary.TextBody is not null)
                    {
                        text = (TextPart)(await inbox.GetBodyPartAsync(summary.UniqueId, summary.TextBody));
                    }

                    var receivedMessage = ReceivedMessage.Create(
                        messageId,
                        sender,
                        summary.Envelope.To.Mailboxes.FirstOrDefault()?.Address,
                        user,
                        summary.Envelope.Subject,
                        text == null ? "null" : text.Text,
                        summary.Date,
                        inReplyTo);

                    if (receivedMessage.IsFailure)
                    {
                        return Result.Failure<List<ReceivedMessage>>(receivedMessage.Error);
                    }

                    unreadMessages.Add(receivedMessage.Value);

                    await inbox.AddFlagsAsync(summary.UniqueId, MessageFlags.Seen, true);
                }

                await client.DisconnectAsync(true);
                return Result.Success(unreadMessages);
            }
        }
        catch (Exception e)
        {
            return Result.Failure<List<ReceivedMessage>>(e.Message);
        }
    }

    public async Task<Result<Dictionary<string, List<ReceivedMessage>>>> GetConversationsByIds(string botEmail,
        string emailBotPassword, int emailBotImapPort, string emailBotImapHost, List<SimpleMessageInfo> messagesInfo)
    {
        try
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync(emailBotImapHost, emailBotImapPort, true);
                await client.AuthenticateAsync(botEmail, emailBotPassword);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                var summaries = await inbox.FetchAsync(0, -1,
                    MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure);

                var messageMap = summaries
                    .Where(s => s.Envelope.MessageId != null)
                    .ToDictionary(s => s.Envelope.MessageId, s => s);

                var conversations = new Dictionary<string, List<ReceivedMessage>>();
                foreach (var messageInfo in messagesInfo)
                {
                    string? rootMessageId = FindRootMessageId(messageInfo.Id, messageMap);

                    if (rootMessageId == null)
                    {
                        continue;
                    }

                    if (conversations.ContainsKey(rootMessageId))
                    {
                        continue;
                    }

                    var processedMessageIds = new HashSet<string>();

                    var result = LoadConversationMessages(
                        rootMessageId,
                        messageMap,
                        inbox,
                        processedMessageIds,
                        botEmail,
                        messageInfo.RequesterEmail);
                    if (!result.Any())
                    {
                        // didn't find the root message
                    }

                    conversations[rootMessageId] = result;
                }

                await client.DisconnectAsync(true);

                return conversations;
            }
        }
        catch (Exception e)
        {
            return Result.Failure<Dictionary<string, List<ReceivedMessage>>>(e.Message);
        }
    }


    private string? FindRootMessageId(string messageId, Dictionary<string, IMessageSummary> messageMap)
    {
        var currentId = messageId;

        while (messageMap.TryGetValue(currentId, out var summary) && summary.Envelope.InReplyTo != null)
        {
            currentId = summary.Envelope.InReplyTo;
        }

        var isReferenced = messageMap.Values.Any(m => m.Envelope.InReplyTo == currentId);

        return isReferenced ? currentId : null;
    }

    private List<ReceivedMessage> LoadConversationMessages(
        string messageId,
        Dictionary<string, IMessageSummary> messageMap,
        IMailFolder inbox,
        HashSet<string> processedMessageIds,
        string botEmail,
        string requesterEmail,
        string replyTo = null)
    {
        var conversationMessages = new List<ReceivedMessage>();

        if (processedMessageIds.Contains(messageId))
        {
            return conversationMessages;
        }

        // Если сообщение не найдено в мапе, считаем его удаленным
        if (!messageMap.TryGetValue(messageId, out var summary))
        {
            // Если это удаленное сообщение, создаем для него заглушку
            var deletedReceivedMessage = ReceivedMessage.CreateDeletedMessage(
                messageId,
                requesterEmail,
                botEmail,
                replyTo);

            if (deletedReceivedMessage.IsSuccess)
            {
                conversationMessages.Add(deletedReceivedMessage.Value);
            }

            var replies = messageMap.Values
                .Where(m => m.Envelope.InReplyTo == messageId)
                .ToList();

            foreach (var reply in replies)
            {
                conversationMessages.AddRange(LoadConversationMessages(reply.Envelope.MessageId, messageMap, inbox,
                    processedMessageIds, botEmail, requesterEmail, replyTo));
            }

            return conversationMessages;
        }

        // Если сообщение найдено, добавляем его в цепочку
        processedMessageIds.Add(messageId);

        var text = summary.TextBody != null
            ? (TextPart)inbox.GetBodyPart(summary.UniqueId, summary.TextBody)
            : null;

        var receivedMessage = ReceivedMessage.Create(
            summary.Envelope.MessageId,
            summary.Envelope.From.Mailboxes.FirstOrDefault()?.Address,
            summary.Envelope.To.Mailboxes.FirstOrDefault()?.Address,
            botEmail,
            summary.Envelope.Subject,
            text?.Text ?? string.Empty,
            summary.Date,
            summary.Envelope.InReplyTo);

        if (receivedMessage.IsSuccess)
        {
            conversationMessages.Add(receivedMessage.Value);

            var replies = messageMap.Values
                .Where(m => m.Envelope.InReplyTo == messageId)
                .ToList();

            foreach (var reply in replies)
            {
                conversationMessages.AddRange(LoadConversationMessages(reply.Envelope.MessageId, messageMap, inbox,
                    processedMessageIds, botEmail, requesterEmail, messageId));
            }
        }

        return conversationMessages;
    }

    public async Task<Result> MarkAllMessagesAsReadAsync(ImapOptions imapOptions)
    {
        try
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync("imap.gmail.com", imapOptions.Port, true);
                await client.AuthenticateAsync(imapOptions.User, imapOptions.Password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite);

                if (inbox.Count > 0)
                {
                    // Получение всех уникальных идентификаторов сообщений
                    var allMessageUids = await inbox.SearchAsync(SearchQuery.All);

                    if (allMessageUids.Any())
                    {
                        // Пометить все сообщения как прочитанные
                        await inbox.AddFlagsAsync(allMessageUids, MessageFlags.Seen, true);
                        Console.WriteLine("All messages marked as read.");
                    }
                    else
                    {
                        Console.WriteLine("No messages found in inbox.");
                    }
                }
                else
                {
                    Console.WriteLine("Inbox is empty.");
                }

                await client.DisconnectAsync(true);
                return Result.Success();
            }
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }
}