using System.ComponentModel.DataAnnotations;
using SupportHub.Domain.Models;

namespace SupportHub.API.Contracts;

public class MessagesFromRequesterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(EmailMessage.MaxEmailLength)]
    public int RequesterId { get; set; }
}