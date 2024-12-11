using System.ComponentModel.DataAnnotations;
using SupportHub.Domain.Models;

namespace SupportHub.API.Contracts;

public class UserAndCompanyRegistrationRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(MailAdmin.MaxEmailLength)]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
    
    //TODO: Add CompanyNameLength
    [Required]
    /*[MaxLength(Company.MaxNameLength)]*/
    public string CompanyName { get; set; }
}