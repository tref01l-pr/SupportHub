using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace SupportHub.Domain.Models;

public class Company
{
    public const int MaxNameLength = 320;
    public const int MaxUrlLength = 64;

    private Company(
        int id,
        string name,
        string url)
    {
        Id = id;
        Name = name;
        Url = url;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }

    public static Result<Company> Create(
        string name,
        string url)
    {
        Result failure = Result.Success();

        if (string.IsNullOrWhiteSpace(name))
        {
            failure = Result.Failure<Company>($"Company {nameof(name)} can't be null or white space");
        }
        else if (name.Length > MaxNameLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<Company>($"Company {nameof(name)} can`t be more than {MaxNameLength} chars"));
        }
        else if (HasNameIncorrectCharacters(name))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<Company>($"Company {nameof(name)} can only contain letters, spaces, hyphens, periods, commas, and underscores"));
        }
        
        if (string.IsNullOrWhiteSpace(url))
        {
            url = NormalizeName(name);
        }
        else if (url.Length > MaxUrlLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<Company>($"Company {nameof(url)} can`t be more than {MaxUrlLength} chars"));
        }
        else if (HasUrlIncorrectCharacters(url))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<Company>($"Company {nameof(url)} can only contain letters, numbers, and hyphens"));
        }

        if (failure.IsFailure)
        {
            return Result.Failure<Company>(failure.Error);
        }

        return new Company(
            0,
            name,
            url);
    }

    private static bool HasNameIncorrectCharacters(string name)
    {
        return Regex.IsMatch(name, @"[^a-zA-Z\ \-\.\,\(_)]");
    }
    
    private static bool HasUrlIncorrectCharacters(string url)
    {
        return Regex.IsMatch(url, @"[^a-zA-Z0-9\-]");
    }

    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var normalized = Regex.Replace(name, @"([A-Z])", "-$1");;
        
        normalized = normalized.ToLower();

        normalized = Regex.Replace(normalized, @"[^a-zA-Z]", "-");

        while (normalized.Contains("--"))
        {
            normalized = normalized.Replace("--", "-");
        }
        
        normalized = normalized.Trim('-');

        return normalized;
    }
}