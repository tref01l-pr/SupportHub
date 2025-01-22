using AutoFixture;
using SupportHub.Domain.Models;
using SupportHub.UnitTests.MemberData;

namespace SupportHub.UnitTests;

public class ImapMessageTests
{
    private readonly Fixture _fixture;

    public ImapMessageTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Create_ShouldReturnNewImapMessage()
    {
        Random rnd = new Random();
        // arrange
        var requester = "protsayroman228@gmail.com";
        var from = "romanlistsender@gmail.com";
        var subject = _fixture.Create<string>();
        var body = _fixture.Create<string>();
        var date = DateTimeOffset.UtcNow;
        var messageStatus = MessageTypes.Question;

        // act
        var result = ImapMessage.Create(requester, from, subject, body, date, messageStatus);

        // assert
        Assert.NotNull(result.Value);
        Assert.False(result.IsFailure);
        Assert.Equal(requester, result.Value.Requester);
        Assert.Equal(from, result.Value.From);
        Assert.Equal(subject, result.Value.Subject);
        Assert.Equal(body, result.Value.Body);
        Assert.Equal(date, result.Value.Date);
        Assert.Equal(messageStatus, result.Value.MessageTypes);
    }

    [Theory]
    [MemberData(
        nameof(ImapMessageDataGenerator.GenerateSetInvalidAllParameters),
        parameters: 100,
        MemberType = typeof(ImapMessageDataGenerator))]
    public void Create_AllParametersIsInvalid_ShouldReturnErrors(
        string invalidRequester,
        string invalidFrom,
        string invalidSubject,
        string invalidBody,
        DateTimeOffset invalidDate,
        MessageTypes messageTypes)
    {
        // arrange
        // act
        var result = ImapMessage.Create(
            invalidRequester,
            invalidFrom,
            invalidSubject,
            invalidBody,
            invalidDate,
            messageTypes);

        /*if (result.IsFailure)
        {
            throw new Exception(result.Error);
        }*/

        // assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.True(result.Error.Length != 0);
    }
}