namespace Pondrop.Service.ShoppingList.Application.Models;

public class DaprEventTopicConfiguration
{
    public const string Key = nameof(DaprEventTopicConfiguration);
    
    public string AppId { get; set; } = string.Empty;
    public string MethodName { get; set; } = string.Empty;
    public string EventTopic { get; set; } = string.Empty;
}
