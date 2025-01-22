using System.Net.Mail;
using CSharpFunctionalExtensions;

namespace SupportHub.Domain.Models;

public class ReceivedMessage
{
    public const int MaxEmailLength = 320;
    public const int MaxSubjectLength = 5000;
    public const int MaxBodyLength = 15000;

    private ReceivedMessage(
        int id,
        string msgId,
        string requesterEmail,
        string emaiilBot,
        string subject,
        string body,
        DateTimeOffset date,
        string? replyToMsgId,
        MessageTypes messageType)
    {
        Id = id;
        MsgId = msgId;
        RequsterEmail = requesterEmail;
        EmailBot = emaiilBot;
        Subject = subject;
        Body = body;
        Date = date;
        ReplyToMsgId = replyToMsgId;
        MessageType = messageType;
    }

    public int Id { get; set; }
    public string MsgId { get; set; }
    public string RequsterEmail { get; set; }
    public string EmailBot { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTimeOffset Date { get; set; }
    public string? ReplyToMsgId { get; set; }
    public MessageTypes MessageType { get; set; }

    public string GetFrom() => MessageType == MessageTypes.Question ? RequsterEmail : EmailBot;
    public string GetTo() => MessageType == MessageTypes.Answer ? RequsterEmail : EmailBot;

    public static Result<ReceivedMessage> Create(
        string msgId,
        string from,
        string to,
        string botEmail,
        string? subject,
        string? body,
        DateTimeOffset date,
        string? replyToMsgId = null)
    {
        Result failure = Result.Success();

        if (string.IsNullOrWhiteSpace(msgId))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>($"ReceivedMessage {nameof(msgId)} can't be null or white space"));
        }

        if (string.IsNullOrWhiteSpace(from))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>($"ReceivedMessage {nameof(from)} can't be null or white space"));
        }
        else if (!IsValidEmail(from))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>($"Email is incorrect"));
        }
        else if (from.Length > MaxEmailLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>(
                    $"ReceivedMessage {nameof(from)} can`t be more than {MaxEmailLength} chars"));
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>($"ReceivedMessage {nameof(to)} can't be null or white space"));
        }
        else if (!IsValidEmail(to))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>($"Email is incorrect"));
        }
        else if (to.Length > MaxEmailLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>(
                    $"ReceivedMessage {nameof(to)} can`t be more than {MaxEmailLength} chars"));
        }

        if (string.IsNullOrWhiteSpace(botEmail))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>($"ReceivedMessage {nameof(botEmail)} can't be null or white space"));
        }
        else if (!IsValidEmail(botEmail))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>($"Email is incorrect"));
        }
        else if (botEmail.Length > MaxEmailLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>(
                    $"ReceivedMessage {nameof(botEmail)} can`t be more than {MaxEmailLength} chars"));
        }

        string? requesterEmail = null;
        if (from == botEmail)
        {
            requesterEmail = to; //Asnwer
        }
        else if (to == botEmail)
        {
            requesterEmail = from; //Question
        }
        else
        {
            //TODO: Check it because in case of pop3 we don't have equal emails
            requesterEmail = from;
            botEmail = to;
        }

        if (string.IsNullOrWhiteSpace(requesterEmail))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>(
                    $"ReceivedMessage {nameof(requesterEmail)} can't be null or white space"));
        }

        MessageTypes messageType = from == botEmail
            ? MessageTypes.Answer
            : MessageTypes.Question;

        if (subject != null)
        {
            if (subject.Length > MaxSubjectLength)
            {
                failure = Result.Combine(
                    failure,
                    Result.Failure<ReceivedMessage>(
                        $"ReceivedMessage {nameof(subject)} can`t be more than {MaxSubjectLength} chars"));
            }
        }
        else
        {
            subject = "null";
        }


        if (body != null)
        {
            if (body.Length > MaxBodyLength)
            {
                body = body.Substring(0, MaxBodyLength);
            }
        }
        else
        {
            body = "null";
        }

        date = date.ToUniversalTime();
        if (date > DateTimeOffset.UtcNow)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>(
                    $"ReceivedMessage {nameof(date)} can`t be more than current date {DateTimeOffset.Now} chars"));
        }

        if (replyToMsgId != null && string.IsNullOrWhiteSpace(replyToMsgId))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>(
                    $"ReceivedMessage {nameof(replyToMsgId)} can't be null or white space"));
        }

        if (failure.IsFailure)
        {
            return Result.Failure<ReceivedMessage>(failure.Error);
        }

        return new ReceivedMessage(
            0,
            msgId,
            requesterEmail!,
            botEmail,
            subject,
            body,
            date,
            replyToMsgId,
            messageType);
    }

    public static Result<ReceivedMessage> CreateDeletedMessage(string messageId, string requesterEmail, string botEmail,
        string replyTo = null)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            return Result.Failure<ReceivedMessage>("MessageId cannot be null or white space");
        }

        if (string.IsNullOrWhiteSpace(requesterEmail))
        {
            return Result.Failure<ReceivedMessage>("ReceivedMessage cannot be null or white space");
        }

        if (!IsValidEmail(requesterEmail))
        {
            return Result.Failure<ReceivedMessage>("RequesterEmail is incorrect");
        }

        if (requesterEmail.Length > MaxEmailLength)
        {
            return Result.Failure<ReceivedMessage>(
                $"ReceivedMessage {nameof(requesterEmail)} can`t be more than {MaxEmailLength} chars");
        }

        if (string.IsNullOrWhiteSpace(botEmail))
        {
            return Result.Failure<ReceivedMessage>("ReceivedMessage cannot be null or white space");
        }

        if (!IsValidEmail(botEmail))
        {
            return Result.Failure<ReceivedMessage>("BotEmail is incorrect");
        }

        if (requesterEmail.Length > MaxEmailLength)
        {
            return Result.Failure<ReceivedMessage>(
                $"ReceivedMessage {nameof(requesterEmail)} can`t be more than {MaxEmailLength} chars");
        }

        //TODO DateTime.Now should be replaced with the date of the message
        return new ReceivedMessage(
            0,
            messageId,
            requesterEmail,
            botEmail,
            "Deleted Message",
            "Deleted Message",
            DateTime.Now,
            replyTo,
            MessageTypes.Deleted);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return address.Address == email;
        }
        catch
        {
            return false;
        }
    }
}