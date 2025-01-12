using CSharpFunctionalExtensions;
using SupportHub.API;
using SupportHub.Domain.Dtos.EmailBotDtos;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.Infrastructure;

public interface IEmailSmtpService
{
    Task<Result<string>> SendReplyMessageAsync(EmailBotDto emailBot, EmailMessage emailMessage, string emailRequesterEmail, string msgId);
    Task<Result> SendMessageAsync(EmailBotDto emailBot, string subject, string body, string emailRequesterEmail);
    Task<Result> TestSmtpConnectionAsync(string emailBot, string password, string smtpHost, int smtpPort);
    Task<Result> SendForgetPasswordToken(SmtpOptions options, string email, string token, string returnUrl, Guid userId);
}