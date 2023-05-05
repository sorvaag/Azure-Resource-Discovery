using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AzDiscovery.Core.Models;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Newtonsoft.Json;

// Reference: https://learn.microsoft.com/en-us/azure/governance/resource-graph/first-query-dotnet 

namespace AzDiscovery.Core
{
    public class ResourceApi
    {
        private readonly ArmClient _armClient;

        public ResourceApi(TokenCredential azureTokenCredential)
        {
            _armClient = new ArmClient(azureTokenCredential);
        }

        public ResourceQueryResponse GetDiscoveredResources(string query, 
                                                            List<string>? managementGroups = null, 
                                                            List<string>? subscriptions = null, 
                                                            int skip = 0, 
                                                            int take = 100, 
                                                            string? skipToken = "")
        {
            var tenant = _armClient.GetTenants().First();
            var result = new ResourceQueryResponse();
            var request = new ResourceQueryContent(query)
            {
                Options = new ResourceQueryRequestOptions
                {
                    Top = take,
                    Skip = skip
                }
            };

            // Check if we have a skip token
            if (!string.IsNullOrEmpty(skipToken))
            {
                request.Options.SkipToken = skipToken;
            }

            // Add any Management Group filters
            if (managementGroups != null && managementGroups.Count > 0)
            {
                foreach (var mg in managementGroups)
                {
                    request.ManagementGroups.Add(mg);
                }
            }

            // Add any Subscription filters
            if (subscriptions != null && subscriptions.Count > 0)
            {
                foreach (var sub in subscriptions)
                {
                    request.Subscriptions.Add(sub);
                }
            }

            // Execute the Query
            var response = tenant.GetResources(request);

            // First result - if we have a large response, it will be paged
            if (response.Value.Data != null)
            {
                var tempResult = JsonConvert.DeserializeObject<List<AzureResource>>(response.Value.Data.ToString());
                if (tempResult != null && tempResult.Count > 0)
                {
                    result.AzureResources.AddRange(tempResult);
                    result.TotalResults = response.Value.TotalRecords;
                    result.SkipToken = response.Value.SkipToken;
                }
            }

            // Loop through the remaining pages of results
            //while (response.Value.SkipToken != null)
            //{
            //    request.Options.SkipToken = response.Value.SkipToken;
            //    response = tenant.GetResources(request);
            //    var tempResult = JsonConvert.DeserializeObject<List<AzureResource>>(response.Value.Data.ToString());
            //    if (tempResult != null && tempResult.Count > 0)
            //    {
            //        resources.AddRange(tempResult);
            //    }
            //}


            // Return the results
            return result;
        }
    }
}