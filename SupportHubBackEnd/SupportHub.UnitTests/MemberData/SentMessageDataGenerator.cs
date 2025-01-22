using SupportHub.Domain.Models;

namespace SupportHub.UnitTests.MemberData;

public class SentMessageDataGenerator
{
    public static IEnumerable<object[]> GenerateSetInvalidAllParameters(int testCount)
    {
        var rnd = new Random();


        for (int i = 0; i < testCount; i++)
        {
            // to

            var toInvalidLength = rnd.Next(EmailMessage.MaxEmailLength, EmailMessage.MaxEmailLength + 4);
            var invalidToData = Enumerable.Range(0, 5)
                .Select(x => StringFixture.GenerateRandomString(toInvalidLength))
                .ToArray();

            var invalidTo = BaseDataGenerator.MakeInvalidString(invalidToData);

            // subject
            var subjectInvalidLength = rnd.Next(ReceivedMessage.MaxSubjectLength, ReceivedMessage.MaxSubjectLength + 4);

            var invalidSubjectData = Enumerable.Range(0, 5)
                .Select(x => StringFixture.GenerateRandomString(subjectInvalidLength))
                .ToArray();

            var invalidSubject = BaseDataGenerator.MakeInvalidString(invalidSubjectData);

            // body

            var bodyInvalidLength = rnd.Next(ReceivedMessage.MaxBodyLength, ReceivedMessage.MaxBodyLength + 4);

            var invalidBodyData = Enumerable.Range(0, 5)
                .Select(x => StringFixture.GenerateRandomString(bodyInvalidLength))
                .ToArray();

            var invalidBody = BaseDataGenerator.MakeInvalidString(invalidBodyData);

            // date
            var invalidDate = DateTimeOffset.UtcNow.AddDays(rnd.Next(1, 4));

            yield return new object[]
            {
                Guid.Empty,
                invalidTo,
                invalidSubject,
                invalidBody,
                invalidDate,
            };
        }
    }
}