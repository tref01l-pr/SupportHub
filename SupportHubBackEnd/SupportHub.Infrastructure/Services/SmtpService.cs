using CSharpFunctionalExtensions;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;
using SupportHub.Domain.Dtos.EmailBotDtos;
using SupportHub.Domain.Interfaces.Infrastructure;
using SupportHub.Domain.Models;

namespace SupportHub.Infrastructure.Services;

public class SmtpService : ISmtpService
{
    /*"smtp.gmail.com", 587*/
    public async Task<Result<string>> SendReplyMessageAsync(EmailBotDto emailBot, EmailMessage emailMessage,
        string emailRequesterEmail, string msgId)
    {
        try
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress("", emailBot.Email));
            email.To.Add(new MailboxAddress("", emailRequesterEmail));

            email.Subject = emailMessage.Subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = emailMessage.Body
            };

            if (!string.IsNullOrWhiteSpace(msgId))
            {
                if (!msgId.StartsWith("<") || !msgId.EndsWith(">"))
                {
                    msgId = $"<{msgId}>";
                }
                // email.Headers.Add(HeaderId.InReplyTo, msgId);
                // email.Headers.Add(HeaderId.References, msgId);
                email.InReplyTo = msgId;
                email.References.Add(msgId);
            }

            string generatedMessageId = email.MessageId;

            using (var smtp = new SmtpClient())
            {
                await smtp.ConnectAsync(emailBot.SmtpHost, emailBot.SmtpPort, false);
                await smtp.AuthenticateAsync(emailBot.Email, emailBot.Password);

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }

            return generatedMessageId;
        }
        catch (Exception e)
        {
            return Result.Failure<string>(e.Message);
        }
    }

    public async Task<Result> SendMessageAsync(EmailBotDto emailBot, string subject, string body, string emailRequesterEmail)
    {
        try
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress("", emailBot.Email));
            email.To.Add(new MailboxAddress("", emailRequesterEmail));

            email.Subject = subject;
            var domain = emailBot.Email.Split('@')[1];
            var messageId = MimeUtils.GenerateMessageId(domain);
            email.MessageId = messageId;
            
            email.Body = new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = body
            };

            using (var smtp = new SmtpClient())
            {
                await smtp.ConnectAsync(emailBot.SmtpHost, emailBot.SmtpPort, false);
                await smtp.AuthenticateAsync(emailBot.Email, emailBot.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }
}