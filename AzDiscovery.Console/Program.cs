using AzDiscovery.Console;
using AzDiscovery.Core;
using AzDiscovery.Core.Extensions;
using Azure.Identity;
using BetterConsoleTables;

Console.WriteLine("Starting demo...");

//////////////////////////////////////////////////////////////////////
// Option 1: Authenticate using a Visual Studio authenticated session
//////////////////////////////////////////////////////////////////////
var azDiscovery = new AzDiscovery.Core.Resources(new DefaultAzureCredential());


//////////////////////////////////////////////////////////////////////
// Option 2: Authenticate using a service principal
//////////////////////////////////////////////////////////////////////
//var secrets = SecretManager.ReadSection<ApplicationSecrets>("ApplicationSecrets");
//var azDiscovery = new AzDiscovery.Core.Resources(new ClientSecretCredential(secrets.TenantId, secrets.ClientId, secrets.ClientSecret));


// Query the Azure Resource Graph with a basic query
// The order by clause should be included to ensure consistent paging on large datasets
var results = azDiscovery.GetDiscoveredResources("resources | where type =~ 'microsoft.compute/virtualmachines' | order by name asc");

Console.WriteLine("Basic ARG Query");
Console.WriteLine("*******************************");
Console.WriteLine($"Results: {results.Count}");
Table outputTable = new("Resource", "Location", "Subscription","Resource Group", "Type", "Tags");
foreach (var resource in results)
{
    outputTable.AddRow(resource.Name, resource.Location, resource.SubscriptionName ?? "", resource.ResourceGroup, resource.Type, resource.Tags.FormatDictionaryAsString());
}
Console.Write(outputTable.ToString());
Console.WriteLine("");


// Query the Azure Resource Graph with a more complex query - joining the subscriptions table to get the subscription name - no resource type filter
// Results are paged for large datasets - you can control this with the pageSize parameter
string query = "resources " +
                "| join kind = leftouter (resourcecontainers | where type =~ 'microsoft.resources/subscriptions' | project SubscriptionName = name, subscriptionId) on subscriptionId " +
                "| project id, name, type, tenantId, subscriptionId, SubscriptionName, resourceGroup, location, tags " +
                "| order by name asc";

results = azDiscovery.GetDiscoveredResources(query, pageSize: 500);
Console.WriteLine("ARG Query with join onto Subscription Table");
Console.WriteLine("*******************************");
Console.WriteLine($"Results: {results.Count}");
outputTable = new("Resource", "Location", "Subscription", "Resource Group", "Type", "Tags");
foreach (var resource in results)
{
    outputTable.AddRow(resource.Name, resource.Location, resource.SubscriptionName, resource.ResourceGroup, resource.Type, resource.Tags.FormatDictionaryAsString());
}
Console.Write(outputTable.ToString());
Console.WriteLine("");


// Query the Azure Resource Graph with a more complex query - joining the subscriptions table to get the subscription name - with a resource type filter
query = "resources " +
        "| where type in~ ('microsoft.compute/virtualmachines', 'microsoft.storage/storageaccounts', 'microsoft.network/publicipaddresses', 'microsoft.compute/disks')" +
        "| join kind = leftouter (resourcecontainers | where type =~ 'microsoft.resources/subscriptions' | project SubscriptionName = name, subscriptionId) on subscriptionId" +
        "| project id, name, type, tenantId, subscriptionId, SubscriptionName, resourceGroup, location, tags" +
        "| order by name asc";

results = azDiscovery.GetDiscoveredResources(query, pageSize: 500);
Console.WriteLine("ARG Query with join onto Subscription Table and Multiple Resource Type Filters");
Console.WriteLine("*******************************");
Console.WriteLine($"Results: {results.Count}");
outputTable = new("Resource", "Location", "Subscription", "Resource Group", "Type", "Tags");
foreach (var resource in results)
{
    outputTable.AddRow(resource.Name, resource.Location, resource.SubscriptionName, resource.ResourceGroup, resource.Type, resource.Tags.FormatDictionaryAsString());
}
Console.Write(outputTable.ToString());
Console.WriteLine("");


////////////////////////////////////////////////////////////////////////////////////////
///// the next queries will fail without proper management group and subscription Id's.
////////////////////////////////////////////////////////////////////////////////////////

// Filter the Results by Management Group
query = "resources " +
        "| where type in~ ('microsoft.compute/virtualmachines', 'microsoft.storage/storageaccounts', 'microsoft.network/publicipaddresses', 'microsoft.compute/disks')" +
        "| join kind = leftouter (resourcecontainers | where type =~ 'microsoft.resources/subscriptions' | project SubscriptionName = name, subscriptionId) on subscriptionId" +
        "| project id, name, type, tenantId, subscriptionId, SubscriptionName, resourceGroup, location, tags" +
        "| order by name asc";

var managementGroupFilters = new List<string>
{
    "" // MG Id Goes here
};

if (managementGroupFilters.Count > 0 && !string.IsNullOrEmpty(managementGroupFilters[0]))
{
    results = azDiscovery.GetDiscoveredResources(query, managementGroups: managementGroupFilters, pageSize: 500);
    Console.WriteLine("ARG Query with Management Group Filters");
    Console.WriteLine("*******************************");
    Console.WriteLine($"Results: {results.Count}");
    outputTable = new("Resource", "Location", "Subscription", "Resource Group", "Type", "Tags");
    foreach (var resource in results)
    {
        outputTable.AddRow(resource.Name, resource.Location, resource.SubscriptionName, resource.ResourceGroup, resource.Type, resource.Tags.FormatDictionaryAsString());
    }
    Console.Write(outputTable.ToString());
    Console.WriteLine("");
}


// Filter the Results by Subscription
var subscriptionFilters = new List<string>
{
    "" // Subscription Id Goes here
};

if (subscriptionFilters.Count > 0 && !string.IsNullOrEmpty(subscriptionFilters[0]))
{
    results = azDiscovery.GetDiscoveredResources(query, subscriptions: subscriptionFilters, pageSize: 500);
    Console.WriteLine("ARG Query with Subscription Filters");
    Console.WriteLine("*******************************");
    Console.WriteLine($"Results: {results.Count}");
    outputTable = new("Resource", "Location", "Subscription", "Resource Group", "Type", "Tags");
    foreach (var resource in results)
    {
        outputTable.AddRow(resource.Name, resource.Location, resource.SubscriptionName, resource.ResourceGroup, resource.Type, resource.Tags.FormatDictionaryAsString());
    }
    Console.Write(outputTable.ToString());
    Console.WriteLine("");
}

Console.WriteLine("The end!");