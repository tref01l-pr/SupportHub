using CSharpFunctionalExtensions;

namespace SupportHub.Domain.Models;

public class Session
{
    public const int MaxLengthToken = 2048;

    private Session(Guid userId, string accessToken, string refreshToken)
    {
        UserId = userId;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public Guid UserId { get; }

    public string AccessToken { get; }

    public string RefreshToken { get; }

    public static Result<Session> Create(
        Guid userId,
        string accessToken,
        string refreshToken)
    {
        Result failure = Result.Success();
        if (userId == Guid.Empty)
        {
            failure = Result.Failure<Session>(
                $"{nameof(userId)} is not be empty!");
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<Session>(
                    $"{nameof(accessToken)} is not be null or whitespace"));
        }
        else if (accessToken.Length > MaxLengthToken)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<Session>($"Course {nameof(accessToken)} can`t be more than {MaxLengthToken} chars"));
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<Session>(
                    $"{nameof(refreshToken)} is not be null or whitespace"));
        }
        else if (refreshToken.Length > MaxLengthToken)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<Session>($"Course {nameof(refreshToken)} can`t be more than {MaxLengthToken} chars"));
        }

        if (failure.IsFailure)
        {
            return Result.Failure<Session>(failure.Error);
        }

        var session = new Session(userId, accessToken, refreshToken);

        return session;
    }
}