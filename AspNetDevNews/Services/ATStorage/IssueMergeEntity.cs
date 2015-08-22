using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.ATStorage
{
    public class IssueMergeEntity : TableEntity
    {
        public IssueMergeEntity(string orgRepository, string issueNumber)
            : base(orgRepository, issueNumber)
        { }

        public IssueMergeEntity() { }

        public string Title { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string State { get; internal set; }
        public int Comments { get; internal set; }
    }
}
