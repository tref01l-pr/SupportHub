﻿using System.Text;

namespace SupportHub.UnitTests;

public class StringFixture
{
    private static readonly string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenerateRandomString(int length)
    {
        var rnd = new Random();

        var builder = new StringBuilder(1, int.MaxValue);

        var result = builder.Append(Enumerable.Repeat(_chars, length)
                .Select(x => x[rnd.Next(x.Length)])
                .ToArray())
            .ToString();

        return result;
    }
}