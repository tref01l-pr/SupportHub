using CSharpFunctionalExtensions;

namespace SupportHub.Domain.Models;

public class Company
{
    public const int MaxNameLength = 320;

    private Company(
        int id,
        string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }

    public static Result<Company> Create(
        string name)
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

        if (failure.IsFailure)
        {
            return Result.Failure<Company>(failure.Error);
        }

        return new Company(
            0,
            name);
    }
}