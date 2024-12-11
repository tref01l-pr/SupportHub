using SupportHub.Domain.Models;

namespace SupportHub.UnitTests.MemberData;

public class ImapMessageDataGenerator
{
    public static IEnumerable<object[]> GenerateSetInvalidAllParameters(int testCount)
    {
        var rnd = new Random();

        for (int i = 0; i < testCount; i++)
        {
            //requester

            var requesterInvalidLength = rnd.Next(ImapMessage.MaxEmailLength, ImapMessage.MaxEmailLength + 4);
            var invalidRequesterData = Enumerable.Range(0, 5)
                .Select(x => StringFixture.GenerateRandomString(requesterInvalidLength))
                .ToArray();

            var invalidRequester = BaseDataGenerator.MakeInvalidString(invalidRequesterData);

            //from

            var fromInvalidLength = rnd.Next(ReceivedMessage.MaxEmailLength, ReceivedMessage.MaxEmailLength + 4);
            var invalidFromData = Enumerable.Range(0, 5)
                .Select(x => StringFixture.GenerateRandomString(fromInvalidLength))
                .ToArray();

            var invalidFrom = BaseDataGenerator.MakeInvalidString(invalidFromData);

            // subject
            var subjectInvalidLength = rnd.Next(ReceivedMessage.MaxSubjectLength, ReceivedMessage.MaxSubjectLength + 4);

            var invalidSubjectData = Enumerable.Range(0, 5)
                .Select(x => StringFixture.GenerateRandomString(subjectInvalidLength))
                .ToArray();

            var invalidSubject = BaseDataGenerator.MakeInvalidString(invalidSubjectData);

            //body

            var bodyInvalidLength = rnd.Next(ReceivedMessage.MaxBodyLength, ReceivedMessage.MaxBodyLength + 4);

            var invalidBodyData = Enumerable.Range(0, 5)
                .Select(x => StringFixture.GenerateRandomString(bodyInvalidLength))
                .ToArray();

            var invalidBody = BaseDataGenerator.MakeInvalidString(invalidBodyData);

            // date
            var invalidDate = DateTimeOffset.Now.AddDays(rnd.Next(1, 4));

            // messageStatus

            var messageStatus = (MessageTypes)(-1 * rnd.Next(1, 1000));

            yield return new object[]
            {
                invalidRequester,
                invalidFrom,
                invalidSubject,
                invalidBody,
                invalidDate,
                messageStatus,
            };
        }
    }
}