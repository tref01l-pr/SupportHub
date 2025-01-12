using System.ComponentModel.DataAnnotations;
using SupportHub.Domain.Models;

namespace SupportHub.API.Contracts;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(MailAdmin.MaxEmailLength)]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}