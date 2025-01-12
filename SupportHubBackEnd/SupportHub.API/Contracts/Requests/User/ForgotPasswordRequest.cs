using System.ComponentModel.DataAnnotations;
using SupportHub.Domain.Models;

namespace SupportHub.API.Contracts;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(MailAdmin.MaxEmailLength)]
    public string Email { get; set; }

    public string ReturnUrl { get; set; }
}