using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.AzureTableStorage
{
    public class TwittedLinkEntity: TableEntity
    {
        public TwittedLinkEntity(string feed, string id)
            : base(feed, id)
        { }

        public TwittedLinkEntity() { }

        public DateTime PublishDate { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string StatusId { get; set; }

    }
}
