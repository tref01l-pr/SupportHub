using CSharpFunctionalExtensions;
using SupportHub.Domain.Options;
using SupportHub.API;
using SupportHub.Domain.Dtos.EmailBotDtos;
using SupportHub.Domain.Models;

namespace SupportHub.Domain.Interfaces.Infrastructure;

public interface ISmtpService
{
    public Task<Result<string>> SendReplyMessageAsync(EmailBotDto emailBot, EmailMessage emailMessage, string emailRequesterEmail, string msgId);
    public Task<Result> SendMessageAsync(EmailBotDto emailBot, string subject, string body, string emailRequesterEmail);
}