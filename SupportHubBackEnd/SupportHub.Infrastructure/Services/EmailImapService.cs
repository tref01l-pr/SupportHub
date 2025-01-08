using CSharpFunctionalExtensions;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using SupportHub.API;
using SupportHub.Domain.Helpers;
using SupportHub.Domain.Interfaces;
using SupportHub.Domain.Models;

namespace SupportHub.Infrastructure.Services;

public class EmailImapService : IEmailImapService
{
    //imap.gmail.com 993
    public async Task<Result<List<ReceivedMessage>>> GetRecentMessages(string user, string password, int port, string host, int count)
    {
        try
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync(host, port, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(user, password);

                var inbox = client.Inbox;
                var inboxMessages = await FetchRecentMessagesAsync(inbox, count, user);
                
                var sent = client.GetFolder(SpecialFolder.Sent);
                var sentMessages = await FetchRecentMessagesAsync(sent, count, user);

                var allMessages = inboxMessages.Concat(sentMessages)
                    .OrderByDescending(m => m.Date)
                    .Take(count)
                    .ToList();

                await client.DisconnectAsync(true);
                return Result.Success(allMessages);
            }
        }
        catch (Exception ex)
        {
            return Result.Failure<List<ReceivedMessage>>(ex.Message);
        }
    }

    public async Task<Result<List<ReceivedMessage>>> GetUnreadMessages(string user, string password, int port, string host)
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

    public async Task<Result> TestImapConnectionAsync(string emailBot, string password, string imapHost, int imapPort)
    {
        try
        {
            using (var imap = new ImapClient())
            {
                await imap.ConnectAsync(imapHost, imapPort, true);

                await imap.AuthenticateAsync(emailBot, password);

                await imap.DisconnectAsync(true);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error during connection to imap: {ex.Message}");
        }
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
                    var allMessageUids = await inbox.SearchAsync(SearchQuery.All);

                    if (allMessageUids.Any())
                    {
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

        if (!messageMap.TryGetValue(messageId, out var summary))
        {
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

    private async Task<List<ReceivedMessage>> FetchRecentMessagesAsync(IMailFolder folder, int count, string botEmail)
    {
        await folder.OpenAsync(FolderAccess.ReadOnly);
        var messageCount = folder.Count;
        var start = Math.Max(0, messageCount - count);
        var uids = await folder.FetchAsync(start, -1,
            MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure);

        var messages = new List<ReceivedMessage>();

        foreach (var uid in uids)
        {
            var messageId = uid.Envelope.MessageId;
            var sender = uid.Envelope.From.Mailboxes.FirstOrDefault()?.Address;

            if (string.IsNullOrEmpty(sender))
            {
                throw new Exception("Sender email is empty.");
            }

            var inReplyTo = uid.Envelope.InReplyTo;

            TextPart? text = null;

            if (uid.TextBody is not null)
            {
                text = (TextPart)(await folder.GetBodyPartAsync(uid.UniqueId, uid.TextBody));
            }

            var receivedMessage = ReceivedMessage.Create(
                messageId,
                sender,
                uid.Envelope.To.Mailboxes.FirstOrDefault()?.Address,
                botEmail,
                uid.Envelope.Subject,
                text == null ? "null" : text.Text,
                uid.Date,
                inReplyTo);

            if (receivedMessage.IsFailure)
            {
                throw new Exception(receivedMessage.Error);
            }

            messages.Add(receivedMessage.Value);
        }

        return messages;
    }
}