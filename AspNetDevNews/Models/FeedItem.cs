using AspNetDevNews.Services.AzureTableStorage;
using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Models
{
    public class FeedItem: ITableStorageKeyGet, IIsTweetable
    {
        public string Id { get; set; }
        public DateTime PublishDate { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Feed { get; set; }

        public string GetTwitterText()
        {
            string title = this.Title.Trim();
            if (!title.EndsWith(".", StringComparison.Ordinal))
                title += ".";

            string contentType = "blog";
            if (Id.Contains("youtube.com") || Id.Contains("channel9.msdn.com") || Id.Contains("vimeo.com"))
                contentType = "video";

            return $"[{contentType}]: {title} " + this.Id;
        }

        public string GetPartitionKey()
        {
            return TableStorageUtilities.EncodeToKey(Feed);
        }

        public string GetRowKey()
        {
            return TableStorageUtilities.EncodeToKey(Id);
        }
    }
}
