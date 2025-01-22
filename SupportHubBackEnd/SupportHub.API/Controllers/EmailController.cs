using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SupportHub.API.Contracts;
using SupportHub.Domain.Dtos.EmailBotDtos;
using SupportHub.Domain.Dtos.EmailConversationDtos;
using SupportHub.Domain.Interfaces.Application;
using SupportHub.Domain.Interfaces.Application.Email;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Models;

namespace SupportHub.API.Controllers;

[Authorize]
[Route("/{companyName}/[controller]/")]
public class EmailController : BaseController
{
    private readonly ILogger<EmailController> _logger;
    private readonly IMapper _mapper;
    private readonly SmtpOptions _smtpOptions;
    private readonly ImapOptions _imapOptions;
    private readonly IMessagesService _messagesService;
    private readonly IEmailBotsService _emailBotsService;
    private readonly IEmailConversationsRepository _emailConversationsRepository;

    public EmailController(
        ILogger<EmailController> logger,
        IMapper mapper,
        IOptions<SmtpOptions> smtpOptions,
        IOptions<ImapOptions> imapOptions,
        IMessagesService messagesService,
        IEmailBotsService emailBotsService,
        IEmailConversationsRepository emailConversationsRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _smtpOptions = smtpOptions.Value;
        _imapOptions = imapOptions.Value;
        _messagesService = messagesService;
        _emailBotsService = emailBotsService;
        _emailConversationsRepository = emailConversationsRepository;
    }

    //TODO Remake this method to use SMTP from database
    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage([FromBody] SentEmailMessageRequest request)
    {
        if (UserId.Value == Guid.Empty)
        {
            return BadRequest("Incorrect UserId. It cannot be empty");
        }

        var sentMessage = await _messagesService.SendEmailMessageAsync(request.EmailConversationId, CompanyId.Value,
            request.Body, UserId.Value);
        if (sentMessage.IsFailure)
        {
            _logger.LogError("{errors}", sentMessage.Error);
            return BadRequest(sentMessage.Error);
        }

        return Ok("Message was sent))");
    }

    [HttpGet("get-last-conversations")]
    public async Task<IActionResult> GetLastMessages()
    {
        if (CompanyId.IsFailure)
        {
            return BadRequest(CompanyId.Error);
        }

        if (CompanyId.Value <= 0)
        {
            return BadRequest("CompanyId cannot be less than or equal to 0");
        }

        var resultMessages =
            await _messagesService.GetLastConversationsByCompanyIdAsync<EmailConversationWithLastUpdateMessagesDto>(
                CompanyId.Value);

        if (resultMessages.IsFailure)
        {
            return BadRequest(resultMessages.Error);
        }

        return Ok(resultMessages.Value);
    }

    [HttpGet("get-conversation-by-id")]
    public async Task<IActionResult> GetConversationById(int id)
    {
        if (CompanyId.IsFailure)
        {
            return BadRequest(CompanyId.Error);
        }

        if (CompanyId.Value <= 0)
        {
            return BadRequest("CompanyId cannot be less than or equal to 0");
        }

        var conversation = await _messagesService.GetConversationById<EmailConversationWithMessagesDto>(id);
        if (conversation.IsFailure)
        {
            return BadRequest(conversation.Error);
        }

        if (conversation.Value == null)
        {
            return BadRequest("Conversation not found");
        }

        if (conversation.Value.CompanyId != CompanyId.Value)
        {
            return BadRequest("Conversation not found");
        }

        return Ok(conversation.Value);
    }

    [HttpPost("create-email-bot")]
    public async Task<IActionResult> CreateEmailBot([FromBody] CreateEmailBotRequest botRequest)
    {
        if (CompanyId.IsFailure)
        {
            return BadRequest(CompanyId.Error);
        }

        if (CompanyId.Value <= 0)
        {
            return BadRequest("CompanyId cannot be less than or equal to 0");
        }


        var emailBot = EmailBot.Create(
            CompanyId.Value,
            botRequest.Email,
            botRequest.Password,
            botRequest.SmtpPort,
            botRequest.SmtpHost,
            botRequest.ImapPort,
            botRequest.ImapHost);

        if (emailBot.IsFailure)
        {
            return BadRequest(emailBot.Error);
        }

        var creationResult = await _emailBotsService.CreateAsync<EmailBotDto>(emailBot.Value);

        if (creationResult.IsFailure)
        {
            return BadRequest(creationResult.Error);
        }

        return Ok(creationResult.Value);
    }

    [HttpPut("update-email-bot")]
    public async Task<IActionResult> UpdateEmailBot([FromBody] UpdateBotRequest updateBotRequest)
    {
        if (CompanyId.IsFailure)
        {
            return BadRequest(CompanyId.Error);
        }

        if (CompanyId.Value <= 0)
        {
            return BadRequest("CompanyId cannot be less than or equal to 0");
        }

        var emailBotExists = await _emailBotsService.GetByIdAsync<EmailBot>(updateBotRequest.Id);
        if (emailBotExists.IsFailure)
        {
            return BadRequest(emailBotExists.Error);
        }

        if (emailBotExists.Value == null)
        {
            return BadRequest("Email bot not found");
        }

        if (emailBotExists.Value.CompanyId != CompanyId.Value)
        {
            return BadRequest("Email bot not found");
        }

        var emailBot = EmailBot.Create(
            CompanyId.Value,
            emailBotExists.Value.Email,
            updateBotRequest.Password,
            updateBotRequest.SmtpPort,
            updateBotRequest.SmtpHost,
            updateBotRequest.ImapPort,
            updateBotRequest.ImapHost);

        if (emailBot.IsFailure)
        {
            return BadRequest(emailBot.Error);
        }

        var updateResult = await _emailBotsService.UpdateAsync<EmailBotDto>(emailBot.Value);
        if (updateResult.IsFailure)
        {
            return BadRequest(updateResult.Error);
        }

        return Ok(updateResult.Value);
    }
    
    [HttpDelete("delete-email-bot")]
    public async Task<IActionResult> DeleteEmailBot(int id)
    {
        if (CompanyId.IsFailure)
        {
            return BadRequest(CompanyId.Error);
        }

        if (CompanyId.Value <= 0)
        {
            return BadRequest("CompanyId cannot be less than or equal to 0");
        }

        var emailBotExists = await _emailBotsService.GetByIdAsync<EmailBot>(id);
        if (emailBotExists.IsFailure)
        {
            return BadRequest(emailBotExists.Error);
        }

        if (emailBotExists.Value == null)
        {
            return BadRequest("Email bot not found");
        }

        if (emailBotExists.Value.CompanyId != CompanyId.Value)
        {
            return BadRequest("Email bot not found");
        }

        var deleteResult = await _emailBotsService.DeleteAsync<EmailBotDto>(id);
        if (deleteResult.IsFailure)
        {
            return BadRequest(deleteResult.Error);
        }

        return Ok("Email bot was deleted");
    }

    [HttpGet("get-email-bots")]
    public async Task<IActionResult> GetEmailBots()
    {
        if (CompanyId.IsFailure)
        {
            return BadRequest(CompanyId.Error);
        }

        if (CompanyId.Value <= 0)
        {
            return BadRequest("CompanyId cannot be less than or equal to 0");
        }

        var emailBots = await _emailBotsService.GetByCompanyIdAsync<EmailBotDto>(CompanyId.Value);
        if (emailBots.IsFailure)
        {
            return BadRequest(emailBots.Error);
        }

        return Ok(emailBots.Value);
    }

    [HttpGet("get-conversations")]
    public async Task<IActionResult> GetConversations()
    {
        if (CompanyId.IsFailure)
        {
            return BadRequest(CompanyId.Error);
        }

        if (CompanyId.Value <= 0)
        {
            return BadRequest("CompanyId cannot be less than or equal to 0");
        }

        var conversations = await _emailConversationsRepository.GetAllAsync<EmailConversationWithMessagesDto>();
        return Ok(conversations);
    }

    [AllowAnonymous]
    [HttpPost("event-on-message-received")]
    public async Task<IActionResult> EventOnMessageReceived()
    {
        //get messageId from event
        //get message from romanListSender
        //get new messages from imap for this emailBot
        _logger.LogInformation("Event on message received");

        var result = await _messagesService.EventOnMessageReceivedAsync(_imapOptions);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok("Success!");
    }
}