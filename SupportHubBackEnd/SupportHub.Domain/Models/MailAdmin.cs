using CSharpFunctionalExtensions;

namespace SupportHub.Domain.Models;

public class MailAdmin
{
    public const int MaxLengthNickname = 50;
    public const int MaxEmailLength = 320;

    private MailAdmin(Guid courseAdminId, string nickname)
    {
        CourseAdminId = courseAdminId;
        Nickname = nickname;
    }

    public Guid CourseAdminId { get; }

    public string Nickname { get; }

    public static Result<MailAdmin> Create(
        Guid courseAdminId,
        string nickname)
    {
        Result failure = Result.Success();
        if (courseAdminId == Guid.Empty)
        {
            failure = Result.Failure<MailAdmin>(
                $"{nameof(courseAdminId)} is not be empty!");
        }

        if (string.IsNullOrWhiteSpace(nickname))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<MailAdmin>($"{nameof(nickname)} is not be null or whitespace"));
        }

        if (!string.IsNullOrWhiteSpace(nickname)
            && nickname.Length > MaxLengthNickname)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<MailAdmin>($"{nameof(nickname)} is not be more than {MaxLengthNickname} chars"));
        }

        if (failure.IsFailure)
        {
            return Result.Failure<MailAdmin>(failure.Error);
        }

        var mailAdmin = new MailAdmin(courseAdminId, nickname);

        return mailAdmin;
    }
}