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

        var sentMessage = await _messagesService.SendEmailMessageAsync(request.EmailConversationId, CompanyId.Value, request.Body, UserId.Value);
        if (sentMessage.IsFailure)
        {
            _logger.LogError("{errors}", sentMessage.Error);
            return BadRequest(sentMessage.Error);
        }

        return Ok("Message was sent))");
    }

    //TODO Remake this method to use database for taking massages
    [HttpGet("get-last-messages")]
    public async Task<IActionResult> GetLastMessages()
    {
        var resultMessages = await _messagesService.GetLastMessagesAsync(_imapOptions);
        if (resultMessages.IsFailure)
        {
            return BadRequest(resultMessages.Error);
        }

        return Ok(resultMessages.Value);
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

    //TODO Delete this method
    [HttpPost("send-test-message")]
    public async Task<IActionResult> SendTestMessage([FromBody] SendTestEmailRequest request)
    {
        var sentMessage = await _messagesService.SendTestEmailMessageAsync(request.SmtpId, request.Message, request.Email);
        if (sentMessage.IsFailure)
        {
            return BadRequest(sentMessage.Error);
        }
        
        return Ok("Message was sent))");
    }
    
    [HttpPost("event-on-message-received")]
    public async Task<IActionResult> EventOnMessageReceived()
    {
        //get messageId from event
        //get message from romanListSender
        //get new messages from imap for this emailBot
        
        var result = await _messagesService.EventOnMessageReceivedAsync(_imapOptions);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        
        var messages = await _emailConversationsRepository.GetAllAsync<EmailConversationWithMessagesDto>();

        return Ok(messages);
    }
}