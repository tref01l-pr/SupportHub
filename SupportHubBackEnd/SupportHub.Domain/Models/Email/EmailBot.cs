using CSharpFunctionalExtensions;
using SupportHub.Domain.Helpers;

namespace SupportHub.Domain.Models;

public class EmailBot
{
    public const int MaxPortLength = 65535; //not including 0 and 65535
    public const int MaxEmailLength = 320;
    public const int PasswordLength = 16;

    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpHost { get; set; }
    public string ImapHost { get; set; }
    public int ImapPort { get; set; }
    

    private EmailBot(
        int id,
        int companyId,
        string email,
        string password,
        int smtpPort,
        string smtpHost,
        int imapPort,
        string imapHost)
    {
        Id = id;
        CompanyId = companyId;
        Email = email;
        Password = password;
        SmtpPort = smtpPort;
        SmtpHost = smtpHost;
        ImapPort = imapPort;
        ImapHost = imapHost;
    }

    public static Result<EmailBot> Create(
        int companyId,
        string email,
        string smtpPassword,
        int smtpPort,
        string smtpHost,
        int imapPort,
        string imapHost)
    {
        Result failure = Result.Success();

        if (companyId <= 0)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailBot>($"EmailBot {nameof(companyId)} can't be less than or equal to 0"));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailBot>($"EmailBot {nameof(email)} can't be null or white space"));
        }
        else if (!EmailValidator.IsValidEmail(email))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailBot>($"EmailBot {nameof(email)} is not an email"));
        }
        else if (email.Length > MaxEmailLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailBot>(
                    $"EmailBot {nameof(email)} can`t be more than {MaxEmailLength} chars"));
        }

        if (string.IsNullOrWhiteSpace(smtpPassword))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailBot>($"EmailBot {nameof(smtpPassword)} can't be null or white space"));
        }
        else if (smtpPassword.Length != PasswordLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailBot>(
                    $"EmailBot {nameof(smtpPassword)} should be {PasswordLength} chars"));
        }

        if (smtpPort is <= 0 or > MaxPortLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailBot>(
                    $"EmailBot {nameof(smtpPort)} can't be less than or equal to 0 or more than {MaxPortLength}"));
        }

        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailBot>($"EmailBot {nameof(smtpHost)} can't be null or white space"));
        }
        
        if (imapPort is <= 0 or > MaxPortLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailBot>(
                    $"EmailBot {nameof(imapPort)} can't be less than or equal to 0 or more than {MaxPortLength}"));
        }

        if (string.IsNullOrWhiteSpace(imapHost))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<EmailBot>($"EmailBot {nameof(imapHost)} can't be null or white space"));
        }

        if (failure.IsFailure)
        {
            return Result.Failure<EmailBot>(failure.Error);
        }

        return new EmailBot(
            0,
            companyId,
            email,
            smtpPassword,
            smtpPort,
            smtpHost,
            imapPort,
            imapHost);
    }
}