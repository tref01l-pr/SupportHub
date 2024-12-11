using SupportHub.Domain.Models;

namespace SupportHub.API.Contracts;

using System.ComponentModel.DataAnnotations;
using SupportHub.Domain.Models;

public class MailAdminRegistrationRequest
{
    [Required]
    [MaxLength(MailAdmin.MaxLengthNickname)]
    public string Nickname { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(MailAdmin.MaxEmailLength)]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}