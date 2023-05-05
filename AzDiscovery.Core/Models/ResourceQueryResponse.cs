namespace AzDiscovery.Core.Models
{
    public class ResourceQueryResponse
    {
        public long TotalResults { get; set; }
        public List<AzureResource> AzureResources { get; set; } = new List<AzureResource>();
        public string SkipToken { get; set; } = string.Empty;
    }
}