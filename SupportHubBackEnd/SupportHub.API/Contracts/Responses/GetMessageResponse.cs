using SupportHub.Domain.Models;

namespace SupportHub.API.Contracts;

public class GetMessageResponse
{
    public string Requester { get; set; }
    
    public string From { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }

    public DateTimeOffset Date { get; set; }

    public MessageTypes MessageTypes { get; set; }
}