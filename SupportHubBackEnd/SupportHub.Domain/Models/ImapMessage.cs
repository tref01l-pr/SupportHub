using System.Net.Mail;
using CSharpFunctionalExtensions;
using MailKit;

namespace SupportHub.Domain.Models;

public class ImapMessage
{
    public const int MaxEmailLength = 320;
    public const int MaxSubjectLength = 5000;
    public const int MaxBodyLength = 15000;

    public ImapMessage(
        string requester,
        string from,
        string subject,
        string body,
        DateTimeOffset date,
        MessageTypes messageTypes)
    {
        Requester = requester;
        From = from;
        Subject = subject;
        Body = body;
        Date = date;
        MessageTypes = messageTypes;
    }

    public string Requester { get; set; }

    public string From { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }

    public DateTimeOffset Date { get; set; }

    public MessageTypes MessageTypes { get; set; }

    public static Result<ImapMessage> Create(
        string requester,
        string? from,
        string subject,
        string body,
        DateTimeOffset date,
        MessageTypes messageTypes)
    {
        Result failure = Result.Success();

        if (string.IsNullOrWhiteSpace(requester))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ImapMessage>($"ImapMessage {nameof(requester)} can't be null or white space"));
        }
        else if (!IsValidEmail(requester))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ImapMessage>($"ImapMessage {nameof(requester)} is not an email"));
        }
        else if (requester.Length > MaxEmailLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ImapMessage>(
                    $"ReceivedMessage {nameof(requester)} can`t be more than {MaxEmailLength} chars"));
        }

        if (string.IsNullOrWhiteSpace(from))
        {
            if (messageTypes == MessageTypes.Answer)
            {
                from = "deleted@account.com";
            }
            else
            {
                failure = Result.Combine(
                    failure,
                    Result.Failure<ImapMessage>($"ImapMessage {nameof(from)} can't be null or white space"));
            }
        }
        else if (!IsValidEmail(from))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ImapMessage>($"ImapMessage {nameof(from)} is not an email"));
        }
        else if (from.Length > MaxEmailLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ImapMessage>(
                    $"ImapMessage {nameof(from)} can`t be more than {MaxEmailLength} chars"));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            subject = "null";
        }
        else if (subject.Length > MaxSubjectLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ImapMessage>(
                    $"ImapMessage {nameof(subject)} can`t be more than {MaxSubjectLength} chars"));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            body = "null";
        }
        else if (body.Length > MaxBodyLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ImapMessage>(
                    $"ImapMessage {nameof(body)} can`t be more than {MaxBodyLength} chars"));
        }

        if (DateTimeOffset.Now < date)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ImapMessage>($"ImapMessage {nameof(date)} can't be null or white space"));
        }

        if (failure.IsFailure)
        {
            return Result.Failure<ImapMessage>(failure.Error);
        }

        return new ImapMessage(
            requester,
            from,
            subject,
            body,
            date,
            messageTypes);
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