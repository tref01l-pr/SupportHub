using CSharpFunctionalExtensions;
using SupportHub.Domain.Helpers;

namespace SupportHub.Domain.Models;

public class EmailRequester
{
    public const int MaxEmailLength = 320;
    
    public int Id { get; init; }
    public string Email { get; private set; }
    
    private EmailRequester(int id, string email)
    {
        Id = id;
        Email = email;
    }
    
    public static Result<EmailRequester> Create(string email)
    {
        Result failure = Result.Success();
        
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
        
        if (failure.IsFailure)
        {
            return Result.Failure<EmailRequester>(failure.Error);
        }

        return new EmailRequester(
            0,
            email);
    }
}