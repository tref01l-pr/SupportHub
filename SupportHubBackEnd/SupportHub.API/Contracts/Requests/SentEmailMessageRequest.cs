using System.ComponentModel.DataAnnotations;
using SupportHub.Domain.Models;

namespace SupportHub.API.Contracts;

public class SentEmailMessageRequest
{
    [Required]
    public int EmailConversationId { get; set; }

    [Required]
    public int EmailRequesterId { get; set; }

    [Required]
    [StringLength(EmailMessage.MaxBodyLength)]
    public string Body { get; set; }
}