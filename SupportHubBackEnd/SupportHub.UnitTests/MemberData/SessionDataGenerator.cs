﻿using SupportHub.Domain.Models;

namespace SupportHub.UnitTests.MemberData;

public class SessionDataGenerator
{
    public static IEnumerable<object[]> GenerateSetInvalidUserIdAccessTokenRefreshToken(
        int testCount)
    {
        var rnd = new Random();
        var userId = Guid.NewGuid();

        for (int i = 0; i < testCount; i++)
        {
            var invalidStringLength = rnd.Next(
                Session.MaxLengthToken + 1,
                int.MaxValue / 1000);

            var invalidData = Enumerable.Range(0, 5)
                .Select(x => StringFixture.GenerateRandomString(invalidStringLength))
                .ToArray();

            var accessToken = BaseDataGenerator.MakeInvalidString(invalidData);
            var refreshToken = BaseDataGenerator.MakeInvalidString(invalidData);

            yield return new object[]
            {
                userId, accessToken, refreshToken
            };
        }
    }
}