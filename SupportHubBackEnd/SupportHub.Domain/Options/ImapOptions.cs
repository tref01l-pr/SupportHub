namespace SupportHub.API;

public class ImapOptions
{
    public const string Imap = "Imap";

    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
}