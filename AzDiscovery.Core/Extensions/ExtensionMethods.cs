namespace AzDiscovery.Core.Extensions
{
    public static class ExtensionMethods
    {
        public static string FormatDictionaryAsString(this Dictionary<string, string> dictionary)
        {
            if (dictionary == null || dictionary.Count < 1)
            {
                return string.Empty;
            }
            
            // Format the dictionary as a string
            string result = string.Empty;
            foreach (var item in dictionary)
            {
                result += $"{item.Key}:{item.Value}; ";
            }
            return result;
        }
    }
}