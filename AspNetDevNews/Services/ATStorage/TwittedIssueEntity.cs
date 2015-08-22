using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.ATStorage
{
    public class TwittedIssueEntity : TableEntity
    {
        public TwittedIssueEntity(string orgRepository, string issueNumber)
            : base(orgRepository, issueNumber)
        { }

        public TwittedIssueEntity() { }

        public string Title { get; set; }
        public string Url { get; set; }
        public string Labels { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ulong TweetId { get; set; }
        public string Body { get; set; }
        public DateTime TwittedAt { get; set; }
    }
}
