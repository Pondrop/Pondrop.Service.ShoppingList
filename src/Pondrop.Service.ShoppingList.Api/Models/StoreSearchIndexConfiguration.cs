namespace Pondrop.Service.ShoppingList.Api.Models;

public class StoreSearchIndexConfiguration
{
    public const string Key = nameof(StoreSearchIndexConfiguration);

    public string BaseUrl { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public string IndexerName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ManagementKey { get; set; } = string.Empty;
}