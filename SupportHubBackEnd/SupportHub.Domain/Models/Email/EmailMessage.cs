using System.Net.Mail;
using CSharpFunctionalExtensions;

namespace SupportHub.Domain.Models;

public class EmailMessage
{
    public const int MaxEmailLength = 320;
    public const int MaxSubjectLength = 5000;
    public const int MaxBodyLength = 15000;

    private EmailMessage(
        int id,
        int emailConversationId,
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
    public int EmailRequesterId { get; set; }
    public Guid? UserId { get; set; }
    public string? MessageId { get; set; }
    public string? Subject { get; set; }
    public string Body { get; set; }
    public DateTimeOffset Date { get; set; }
    public MessageTypes MessageType { get; set; }
    public static EmaiilMessageBuilder Builder() => new EmaiilMessageBuilder();

    public class EmaiilMessageBuilder
    {
        private int _id;
        private int _emailConversationId;
        private int _emailRequesterId;
        private Guid? _userId = null;
        private string? _messageId;
        private string? _subject = null;
        private string _body;
        private DateTimeOffset _date;
        private MessageTypes _messageType;

        public EmaiilMessageBuilder SetId(int id)
        {
            _id = id;
            return this;
        }

        public EmaiilMessageBuilder SetEmailConversationId(int conversationId)
        {
            _emailConversationId = conversationId;
            return this;
        }

        public EmaiilMessageBuilder SetEmailRequesterId(int emailRequesterId)
        {
            _emailRequesterId = emailRequesterId;
            return this;
        }

        public EmaiilMessageBuilder SetUserId(Guid userId)
        {
            _userId = userId;
            return this;
        }

        public EmaiilMessageBuilder SetMessageId(string messageId)
        {
            _messageId = messageId;
            return this;
        }

        public EmaiilMessageBuilder SetSubject(string subject)
        {
            _subject = subject;
            return this;
        }


        public EmaiilMessageBuilder SetBody(string body)
        {
            _body = body;
            return this;
        }

        public EmaiilMessageBuilder SetDate(DateTimeOffset date)
        {
            _date = date;
            return this;
        }

        public EmaiilMessageBuilder SetMessageType(MessageTypes messageType)
        {
            _messageType = messageType;
            return this;
        }


        public Result<EmailMessage> Build()
        {
            var validationResult = ValidateEmailMessageData(
                _emailConversationId,
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

        if (date > DateTimeOffset.Now)
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