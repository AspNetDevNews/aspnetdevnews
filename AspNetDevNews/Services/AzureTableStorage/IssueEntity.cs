using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.AzureTableStorage
{
    public class IssueEntity : TableEntity
    {
        public IssueEntity(string orgRepository, string issueNumber)
            : base(orgRepository, issueNumber)
        {
        }

        public IssueEntity() { }

        public string Title { get; set; }
        public string Url { get; set; }
        public string Labels { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string StatusId { get; set; }
        public string Body { get; set; }
        public DateTime TwittedAt { get; set; }
        public string State { get; set; }
        public int Comments { get; set; }
    }
}
