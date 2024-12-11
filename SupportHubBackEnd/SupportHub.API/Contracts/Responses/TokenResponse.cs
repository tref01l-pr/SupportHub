namespace SupportHub.API.Contracts;

public class TokenResponse
{
    public Guid Id { get; set; }

    public string Role { get; set; }

    public string AccessToken { get; set; }

    public string Nickname { get; set; }

    public string Email { get; set; }
}