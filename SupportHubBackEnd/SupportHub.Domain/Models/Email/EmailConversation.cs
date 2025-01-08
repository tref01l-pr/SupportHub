using CSharpFunctionalExtensions;

namespace SupportHub.Domain.Models;

public class EmailConversation
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int EmailBotId { get; set; }
    public int EmailRequesterId { get; set; }
    public string MsgId { get; set; }
    public string Subject { get; set; }

    private EmailConversation(
        int id,
        int companyId,
        int emailBotId,
        int emailRequesterId,
        string msgId,
        string subject)
    {
        Id = id;
        CompanyId = companyId;
        EmailBotId = emailBotId;
        EmailRequesterId = emailRequesterId;
        MsgId = msgId;
        Subject = subject;
    }

    public static Result<EmailConversation> Create(
        int companyId,
        int emailBotId,
        int emailRequesterId,
        string msgId,
        string subject)
    {
        Result failure = Result.Success();

        if (companyId <= 0)
        {
            failure = Result.Failure<EmailConversation>(
                $"EmailConversation {nameof(companyId)} can't be less than or equal to 0");
        }

        if (emailBotId <= 0)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailConversation>(
                    $"EmailConversation {nameof(emailBotId)} can't be less than or equal to 0"));
        }

        if (emailRequesterId <= 0)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailConversation>(
                    $"EmailConversation {nameof(emailRequesterId)} can't be less than or equal to 0"));
        }

        if (string.IsNullOrWhiteSpace(msgId))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailConversation>(
                    $"EmailConversation {nameof(msgId)} can't be null or empty"));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            subject = "No subject";
        }

        if (failure.IsFailure)
        {
            return Result.Failure<EmailConversation>(failure.Error);
        }

        return new EmailConversation(0, companyId, emailBotId, emailRequesterId, msgId, subject);
    }
}