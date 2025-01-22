using CSharpFunctionalExtensions;

namespace SupportHub.Domain.Models;

public class EmailMessage
{
    public const int MaxEmailLength = 320;
    public const int MaxSubjectLength = 5000;
    public const int MaxBodyLength = 25000;

    private EmailMessage(
        int id,
        int emailConversationId,
        int emailBotId,
        int emailRequesterId,
        Guid? userId,
        string? messageId,
        string? subject,
        string body,
        DateTimeOffset date,
        MessageTypes messageType)
    {
        Id = id;
        EmailConversationId = emailConversationId;
        EmailBotId = emailBotId;
        EmailRequesterId = emailRequesterId;
        UserId = userId;
        MessageId = messageId;
        Subject = subject;
        Body = body;
        Date = date;
        MessageType = messageType;
    }

    public int Id { get; init; }
    public int EmailConversationId { get; set; }
    public int EmailBotId { get; set; }
    public int EmailRequesterId { get; set; }
    public Guid? UserId { get; set; }
    public string? MessageId { get; set; }
    public string? Subject { get; set; }
    public string Body { get; set; }
    public DateTimeOffset Date { get; set; }
    public MessageTypes MessageType { get; set; }
    public static EmailMessageBuilder Builder() => new EmailMessageBuilder();

    public class EmailMessageBuilder
    {
        private int _id;
        private int _emailConversationId;
        private int _emailBotId;
        private int _emailRequesterId;
        private Guid? _userId = null;
        private string? _messageId;
        private string? _subject = null;
        private string _body;
        private DateTimeOffset _date;
        private MessageTypes _messageType;

        public EmailMessageBuilder SetId(int id)
        {
            _id = id;
            return this;
        }

        public EmailMessageBuilder SetEmailConversationId(int conversationId)
        {
            _emailConversationId = conversationId;
            return this;
        }

        public EmailMessageBuilder SetEmailBotId(int emailBotId)
        {
            _emailBotId = emailBotId;
            return this;
        }

        public EmailMessageBuilder SetEmailRequesterId(int emailRequesterId)
        {
            _emailRequesterId = emailRequesterId;
            return this;
        }

        public EmailMessageBuilder SetUserId(Guid userId)
        {
            _userId = userId;
            return this;
        }

        public EmailMessageBuilder SetMessageId(string messageId)
        {
            _messageId = messageId;
            return this;
        }

        public EmailMessageBuilder SetSubject(string subject)
        {
            _subject = subject;
            return this;
        }


        public EmailMessageBuilder SetBody(string body)
        {
            _body = body;
            return this;
        }

        public EmailMessageBuilder SetDate(DateTimeOffset date)
        {
            _date = date.ToUniversalTime();
            return this;
        }

        public EmailMessageBuilder SetMessageType(MessageTypes messageType)
        {
            _messageType = messageType;
            return this;
        }


        public Result<EmailMessage> Build()
        {
            var validationResult = ValidateEmailMessageData(
                _emailConversationId,
                _emailBotId,
                _emailRequesterId,
                _userId,
                _messageId,
                _subject,
                _body,
                _date,
                _messageType);

            if (validationResult.IsFailure)
            {
                return Result.Failure<EmailMessage>(validationResult.Error);
            }

            return new EmailMessage(
                _id,
                _emailConversationId,
                _emailBotId,
                _emailRequesterId,
                _userId,
                _messageId,
                _subject,
                _body,
                _date,
                _messageType
            );
        }
    }

    private static Result ValidateEmailMessageData(
        int emailConversationId,
        int emailBotId,
        int emailRequesterId,
        Guid? userId,
        string messageId,
        string? subject,
        string body,
        DateTimeOffset date,
        MessageTypes messageType)
    {
        if (emailConversationId <= 0)
        {
            return Result.Failure("EmailConversationId must be greater than zero.");
        }

        if (emailBotId <= 0)
        {
            return Result.Failure("EmailBotId must be greater than zero.");
        }

        if (emailRequesterId <= 0)
        {
            return Result.Failure("EmailRequesterId must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return Result.Failure("Body cannot be null or empty.");
        }

        if (body.Length > MaxBodyLength)
        {
            return Result.Failure($"Body cannot be longer than {MaxBodyLength} characters.");
        }

        if (subject != null)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Result.Failure("Subject cannot be empty if provided.");
            }

            if (subject.Length > MaxSubjectLength)
            {
                return Result.Failure($"Subject cannot be longer than {MaxSubjectLength} characters.");
            }
        }

        if (date == default)
        {
            return Result.Failure("Date must be a valid value.");
        }

        if (date > DateTimeOffset.UtcNow)
        {
            return Result.Failure("Date cannot be in the future.");
        }

        switch (messageType)
        {
            case MessageTypes.Question:
                return ValidateQuestionMessage(userId);
            case MessageTypes.Answer:
                return ValidateAnswerMessage(userId);
            case MessageTypes.Deleted:
                return ValidateDeletedMessage(userId);
            default:
                return Result.Failure("MessageType is not supported.");
        }
    }

    private static Result ValidateDeletedMessage(Guid? userId)
    {
        if (userId != null)
        {
            return Result.Failure("UserId must be null for MessageTypes.Deleted.");
        }

        return Result.Success();
    }

    private static Result ValidateQuestionMessage(Guid? userId)
    {
        if (userId != null)
        {
            return Result.Failure("UserId must be null for MessageTypes.Question.");
        }

        return Result.Success();
    }

    private static Result ValidateAnswerMessage(Guid? userId)
    {
        if (userId == null)
        {
            return Result.Success();
        }

        if (userId == Guid.Empty)
        {
            return Result.Failure("UserId must be a valid value for MessageTypes.Answer.");
        }

        return Result.Success();
    }
}