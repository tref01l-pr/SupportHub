namespace SupportHub.API.Contracts;

public class EventOnMessageReceivedRequest
{
    public string EmailAddress { get; set; }
    public string HistoryId { get; set; }
    public EventMessageRequest[] Messages { get; set; }
}