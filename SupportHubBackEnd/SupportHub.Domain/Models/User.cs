﻿using System.Net.Mail;
using CSharpFunctionalExtensions;

namespace SupportHub.Domain.Models;

public class User
{
    public Guid Id { get; }

    public string Email { get; }

    public string UserName { get; }

    private User(Guid id, string email, string userName)
    {
        Id = id;
        Email = email;
        UserName = userName;
    }

    public static Result<User> Create(string email, string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return Result.Failure<User>("UserName cannot be empty");
        }
        if (IsValidEmail(email) == false)
        {
            return Result.Failure<User>("Email is incorrect");
        }

        return new User(Guid.Empty, email, userName);
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