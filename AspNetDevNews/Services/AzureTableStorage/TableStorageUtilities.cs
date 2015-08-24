using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.AzureTableStorage
{
    public static class TableStorageUtilities
    {
        public static string GetTableQuerySetString(string partitionKey, IList<string> rowKeys) {
            string filter = "(PartitionKey eq '" + partitionKey + "') and (";
            string rowFilter = string.Empty;
            foreach (var chiave in rowKeys)
            {
                if (!string.IsNullOrWhiteSpace(rowFilter))
                    rowFilter += " or ";

                rowFilter += " (RowKey eq '" + chiave + "') ";
            }
            filter += rowFilter + ")";
            return filter;
        }

        public static String EncodeToKey(String originalKey)
        {
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(originalKey);
            var base64 = System.Convert.ToBase64String(keyBytes);
            return base64.Replace('/', '_');
        }

        public static String DecodeFromKey(String encodedKey)
        {
            var base64 = encodedKey.Replace('_', '/');
            byte[] bytes = System.Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
