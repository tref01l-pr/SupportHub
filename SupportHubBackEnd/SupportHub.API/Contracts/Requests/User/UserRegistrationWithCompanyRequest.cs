using System.ComponentModel.DataAnnotations;
using SupportHub.Domain.Models;

namespace SupportHub.API.Contracts;

public class UserRegistrationWithCompanyRequest : UserRegistrationRequest
{
    [Required]
    [MaxLength(Company.MaxNameLength)]
    public string CompanyName { get; set; }
}