namespace ChuckPilot.Core;
public class History
{
	public string ConversationId { get; set; }
	public List<HistoryMessage> Messages { get; set; }
	public History()
	{
		Messages = new List<HistoryMessage>();
	}
}

public class HistoryMessage
{
	public string id { get; set; }
	public string conversationId { get; set; }
	public string role { get; set; }
	public string content { get; set; }
}

