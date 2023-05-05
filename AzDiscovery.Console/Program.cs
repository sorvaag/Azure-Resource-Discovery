using AzDiscovery.Console;
using AzDiscovery.Core;
using AzDiscovery.Core.Extensions;
using Azure;
using Azure.Identity;
using BetterConsoleTables;

Console.WriteLine("Starting demo...");

//////////////////////////////////////////////////////////////////////
// Option 1: Authenticate using a Visual Studio authenticated session
//////////////////////////////////////////////////////////////////////
var azDiscovery = new ResourceApi(new DefaultAzureCredential());


//////////////////////////////////////////////////////////////////////
// Option 2: Authenticate using a service principal
//////////////////////////////////////////////////////////////////////
//var secrets = SecretManager.ReadSection<ApplicationSecrets>("ApplicationSecrets");
//var azDiscovery = new AzDiscovery.Core.Resources(new ClientSecretCredential(secrets.TenantId, secrets.ClientId, secrets.ClientSecret));


// Query the Azure Resource Graph with a basic query
// The order by clause should be included to ensure consistent paging on large datasets
Console.WriteLine("Basic ARG Query - checking for Virtual Machines");
Console.WriteLine("***********************************************");
Console.WriteLine("Running query...");

var queryResult = azDiscovery.GetDiscoveredResources("resources | where type =~ 'microsoft.compute/virtualmachines' | order by name asc", skip: 0, take: 10);

Console.WriteLine($"Results: {queryResult.TotalResults }");
Table outputTable = new("Resource", "Location", "Subscription","Resource Group", "Type", "Tags");
foreach (var resource in queryResult.AzureResources)
{
    outputTable.AddRow(resource.Name, resource.Location, resource.SubscriptionName ?? "", resource.ResourceGroup, resource.Type, resource.Tags.FormatDictionaryAsString());
}
Console.Write(outputTable.ToString());
Console.WriteLine("");
Console.WriteLine("press any key for next query");
Console.ReadKey();


Console.WriteLine("");
Console.WriteLine("ARG Query with join onto Subscription Table - All Resource Types");
Console.WriteLine("****************************************************************");
Console.WriteLine("Running query...");

// Query the Azure Resource Graph with a more complex query - joining the subscriptions table to get the subscription name - no resource type filter
// Results are paged for large datasets - you can control this with the pageSize parameter
string query = "resources " +
                "| join kind = leftouter (resourcecontainers | where type =~ 'microsoft.resources/subscriptions' | project SubscriptionName = name, subscriptionId) on subscriptionId " +
                "| project id, name, type, tenantId, subscriptionId, SubscriptionName, resourceGroup, location, tags " +
                "| order by name asc";

queryResult = azDiscovery.GetDiscoveredResources(query, skip: 0, take: 10);

Console.WriteLine($"Results: {queryResult.TotalResults}");
outputTable = new("Resource", "Location", "Subscription", "Resource Group", "Type", "Tags");
foreach (var resource in queryResult.AzureResources)
{
    outputTable.AddRow(resource.Name, resource.Location, resource.SubscriptionName, resource.ResourceGroup, resource.Type, resource.Tags.FormatDictionaryAsString());
}
Console.Write(outputTable.ToString());
Console.WriteLine("");
Console.WriteLine("press any key for next query");
Console.ReadKey();


Console.WriteLine("");
Console.WriteLine("ARG Query with join onto Subscription Table with Resource Type Filters");
Console.WriteLine("**********************************************************************");
Console.WriteLine("Running query...");

// Query the Azure Resource Graph with a more complex query - joining the subscriptions table to get the subscription name - with a resource type filter
query = "resources " +
        "| where type in~ ('microsoft.hdinsight/clusters', 'microsoft.cdn/profiles', 'microsoft.logic/workflows', 'microsoft.logic/integrationserviceenvironments', 'microsoft.containerinstance/containergroups', 'microsoft.apimanagement/service')" +
        "| join kind = leftouter (resourcecontainers | where type =~ 'microsoft.resources/subscriptions' | project SubscriptionName = name, subscriptionId) on subscriptionId" +
        "| project id, name, type, tenantId, subscriptionId, SubscriptionName, resourceGroup, location, tags" +
        "| order by name asc";


// The next section works through the paged result set
// It checks for the skipToken to determine if there are additional results
// The demo also has a hardcoded 5 page limit (pageNumber < 6)
bool hasadditionalPages;
int pageNumber = 1;
do
{
    var pagedResult = azDiscovery.GetDiscoveredResources(query, skip: ((pageNumber - 1) * 5), take: 5);

    Console.WriteLine($"Results: {pagedResult.TotalResults}");
    Console.WriteLine($"Page: {pageNumber}");
    Console.WriteLine("##########");
    outputTable = new("Resource", "Location", "Subscription", "Resource Group", "Type", "Tags");
    foreach (var resource in pagedResult.AzureResources)
    {
        outputTable.AddRow(resource.Name, resource.Location, resource.SubscriptionName, resource.ResourceGroup, resource.Type, resource.Tags.FormatDictionaryAsString());
    }
    Console.Write(outputTable.ToString());

    hasadditionalPages = !string.IsNullOrEmpty(pagedResult.SkipToken);
    pageNumber++;

} while (hasadditionalPages && pageNumber < 6);


Console.WriteLine("");
Console.ReadKey();

return; // exit the demo here...


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
    queryResult = azDiscovery.GetDiscoveredResources(query, managementGroups: managementGroupFilters, skip: 0, take: 50);
    Console.WriteLine("ARG Query with Management Group Filters");
    Console.WriteLine("*******************************");
    Console.WriteLine($"Results: {queryResult.TotalResults}");
    outputTable = new("Resource", "Location", "Subscription", "Resource Group", "Type", "Tags");
    foreach (var resource in queryResult.AzureResources)
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
    queryResult = azDiscovery.GetDiscoveredResources(query, subscriptions: subscriptionFilters, skip: 0, take: 50);
    Console.WriteLine("ARG Query with Subscription Filters");
    Console.WriteLine("*******************************");
    Console.WriteLine($"Results: {queryResult.TotalResults}");
    outputTable = new("Resource", "Location", "Subscription", "Resource Group", "Type", "Tags");
    foreach (var resource in queryResult.AzureResources)
    {
        outputTable.AddRow(resource.Name, resource.Location, resource.SubscriptionName, resource.ResourceGroup, resource.Type, resource.Tags.FormatDictionaryAsString());
    }
    Console.Write(outputTable.ToString());
    Console.WriteLine("");
}

Console.WriteLine("The end!");