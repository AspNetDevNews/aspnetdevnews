using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.AzureTableStorage
{
    public class LinkEntity: TableEntity
    {
        public LinkEntity(string feed, string id)
            : base(feed, id)
        { }

        public LinkEntity() { }

        public DateTime PublishDate { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string StatusId { get; set; }

    }
}
