using AutoFixture;
using SupportHub.Domain.Models;
using SupportHub.UnitTests.MemberData;

namespace SupportHub.UnitTests;

public class ReceivedMessageTests
{
    private readonly Fixture _fixture;

    public ReceivedMessageTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Create_ShouldReturnNewReceivedMessage()
    {
        Random rnd = new Random();
        // arrange
        var msgId = _fixture.Create<string>();;
        var msgIdStr = msgId.ToString();
        var from = "protsayroman228@gmail.com";
        var to = "romanlistsender@gmail.com";
        var subject = _fixture.Create<string>();
        var body = _fixture.Create<string>();
        var date = DateTimeOffset.Now;
        var botEmail = "romanlistsender@gmail.com";

        // act
        var result = ReceivedMessage.Create(msgIdStr, from, to, botEmail, subject, body, date, null);

        // assert
        Assert.NotNull(result.Value);
        Assert.False(result.IsFailure);
        Assert.Equal(msgId, result.Value.MsgId);
        /*Assert.Equal(from, result.Value.From);
        Assert.Equal(to, result.Value.To);*/
        Assert.Equal(subject, result.Value.Subject);
        Assert.Equal(body, result.Value.Body);
        Assert.Equal(date, result.Value.Date);
    }

    [Theory]
    [MemberData(
        nameof(ReceivedMessageDataGenerator.GenerateSetInvalidAllParameters),
        parameters: 10,
        MemberType = typeof(ReceivedMessageDataGenerator))]
    public void Create_AllParametersIsInvalid_ShouldReturnErrors(
        string invalidMessageIdStr,
        string invalidFrom,
        string invalidTo,
        string invalidSubject,
        string invalidBody,
        DateTimeOffset invalidDate)
    {
        // arrange
        // act
        var result = ReceivedMessage.Create(
            invalidMessageIdStr,
            invalidFrom,
            invalidTo,
            invalidTo,
            invalidSubject,
            invalidBody,
            invalidDate,
            null);

        // assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.True(result.Error.Length != 0);
    }
}