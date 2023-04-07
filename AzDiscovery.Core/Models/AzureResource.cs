namespace AzDiscovery.Core.Models
{
    public class AzureResource
    {
        public AzureResource()
        {
            Tags = new Dictionary<string, string>();
        }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? ResourceGroup { get; set; }
        public Guid? SubscriptionId { get; set; }
        public string? SubscriptionName { get; set; }
        public string? Type { get; set; }
        public Dictionary<string, string>? Tags { get; set; }
    }
}