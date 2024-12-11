namespace SupportHub.Domain.Options;

public class GoogleApiOptions
{
    public const string GoogleApi = "GoogleApi";


    public string ClientId { get; set; } = string.Empty;

    public string ProjectId { get; set; } = string.Empty;

    public string AuthUri { get; set; } = string.Empty;

    public string TokenUri { get; set; } = string.Empty;

    public string AuthProviderCertUrl { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;
}