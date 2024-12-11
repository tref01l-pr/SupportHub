using AutoFixture;
using SupportHub.Domain.Models;
using SupportHub.UnitTests.MemberData;

namespace SupportHub.UnitTests;

public class EmailMessageTests
{
    private readonly Fixture _fixture;

    public EmailMessageTests()
    {
        _fixture = new Fixture();
    }

    /*[Fact]
    public async Task Create_ShouldReturnNewReceivedMessage()
    {
        Random rnd = new Random();
        // arrange
        var userId = Guid.NewGuid();
        var to = "romanlistsender@gmail.com";
        var subject = _fixture.Create<string>();
        var body = _fixture.Create<string>();
        var date = DateTimeOffset.Now;

        // act
        var result = EmailMessage.Create(userId, to, subject, body, date);

        // assert
        Assert.NotNull(result.Value);
        Assert.False(result.IsFailure);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(to, result.Value.To);
        Assert.Equal(subject, result.Value.Subject);
        Assert.Equal(body, result.Value.Body);
        Assert.Equal(date, result.Value.Date);
    }

    [Theory]
    [MemberData(
        nameof(SentMessageDataGenerator.GenerateSetInvalidAllParameters),
        parameters: 10,
        MemberType = typeof(SentMessageDataGenerator))]
    public void Create_AllParametersIsInvalid_ShouldReturnErrors(
        Guid invalidUserId,
        string invalidTo,
        string invalidSubject,
        string invalidBody,
        DateTimeOffset invalidDate)
    {
        // arrange
        // act
        var result = EmailMessage.Create(
            invalidUserId,
            invalidTo,
            invalidSubject,
            invalidBody,
            invalidDate);

        // assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.True(result.Error.Length != 0);
    }*/
}