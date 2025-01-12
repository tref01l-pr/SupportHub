using System.Security.Claims;
using Newtonsoft.Json;

namespace SupportHub.API;

public class UserInformation
{
    public UserInformation(string userName, Guid userId, string role, int companyId, string companyName)
    {
        UserName = userName;
        UserId = userId;
        Role = role;
        CompanyId = companyId;
        CompanyName = companyName;
    }

    [JsonProperty(ClaimTypes.Name)]
    public string UserName { get; init; }

    [JsonProperty(ClaimTypes.NameIdentifier)]
    public Guid UserId { get; init; }

    [JsonProperty(ClaimTypes.Role)]
    public string Role { get; init; }
    
    [JsonProperty("CompanyId")]
    public int CompanyId { get; init; }
    
    [JsonProperty("CompanyName")]
    public string CompanyName { get; init; }
}