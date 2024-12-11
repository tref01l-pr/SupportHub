using System.Text;

namespace SupportHub.API;

using System.Security.Claims;
using CSharpFunctionalExtensions;
using JWT.Algorithms;
using JWT.Builder;
using Domain.Options;

public class JwtHelper
{
    public static string CreateAccessToken(UserInformation information, JWTSecretOptions options)
    {
        var accsessToken = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(Encoding.UTF8.GetBytes(options.Secret))
            .ExpirationTime(DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds())
            .AddClaim(ClaimTypes.Name, information.Nickname)
            .AddClaim(ClaimTypes.NameIdentifier, information.UserId)
            .AddClaim(ClaimTypes.Role, information.Role)
            .AddClaim("CompanyId", information.CompanyId)
            .AddClaim("CompanyName", information.CompanyName)
            .WithVerifySignature(true)
            .Encode();

        return accsessToken;
    }

    public static string CreateRefreshToken(UserInformation information, JWTSecretOptions options)
    {
        var refreshToken = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(options.Secret)
            .ExpirationTime(DateTimeOffset.UtcNow.AddMonths(1).ToUnixTimeSeconds())
            .AddClaim(ClaimTypes.Name, information.Nickname)
            .AddClaim(ClaimTypes.NameIdentifier, information.UserId)
            .AddClaim(ClaimTypes.Role, information.Role)
            .AddClaim("CompanyId", information.CompanyId)
            .AddClaim("CompanyName", information.CompanyName)
            .WithVerifySignature(true)
            .Encode();

        return refreshToken;
    }

    public static Result<UserInformation> GetPayloadFromJWTTokenV2(string token, JWTSecretOptions options)
    {
        var payload = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(options.Secret)
            .MustVerifySignature()
            .Decode<UserInformation>(token);

        return payload;
    }

    public static IDictionary<string, object> GetPayloadFromJWTToken(string token, JWTSecretOptions options)
    {
        var payload = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(options.Secret)
            .MustVerifySignature()
            .Decode<IDictionary<string, object>>(token);

        return payload;
    }

    public static Result<UserInformation> ParseUserInformation(IDictionary<string, object> payload)
    {
        Result failure = Result.Success();

        if (!payload.TryGetValue(ClaimTypes.NameIdentifier, out var nameIdentifierValue))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>("User id is not found."));
        }

        var nameIdentifierValueStr = nameIdentifierValue?.ToString();

        if (string.IsNullOrWhiteSpace(nameIdentifierValueStr))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>("User id can't be null"));
        }

        if (!Guid.TryParse(nameIdentifierValueStr, out var userId))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>(
                    $"{nameof(userId)} is can't parsing."));
        }

        if (!payload.TryGetValue(ClaimTypes.Role, out var roleValue))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>("Role is not found."));
        }

        var role = roleValue?.ToString();

        if (string.IsNullOrWhiteSpace(role))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>(
                    $"{nameof(role)} is can't parsing."));
        }

        if (!payload.TryGetValue(ClaimTypes.Name, out var nicknameValue))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>("Nickname is not found."));
        }

        var nickname = nicknameValue?.ToString();
        if (string.IsNullOrWhiteSpace(nickname))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>(
                    $"{nameof(nickname)} is can't parsing."));
        }

        if (!payload.TryGetValue("CompanyId", out var companyIdValue))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>("Company id is not found."));
        }

        var companyIdValueStr = companyIdValue?.ToString();
        if (string.IsNullOrWhiteSpace(companyIdValueStr))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>("Company id can't be null"));
        }

        if (!int.TryParse(companyIdValueStr, out var companyId))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>(
                    $"{nameof(companyId)} is can't parsing."));
        }

        if (!payload.TryGetValue("CompanyName", out var companyNameValue))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>("Company name is not found."));
        }

        var companyName = companyNameValue?.ToString();

        if (string.IsNullOrWhiteSpace(companyName))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>(
                    $"{nameof(companyName)} is can't parsing."));
        }

        if (failure.IsFailure)
        {
            return Result.Failure<UserInformation>(failure.Error);
        }

        return new UserInformation(nickname, userId, role, companyId, companyName);
    }
}