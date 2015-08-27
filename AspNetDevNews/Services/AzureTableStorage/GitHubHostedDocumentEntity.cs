using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.AzureTableStorage
{
    public class GitHubHostedDocumentEntity : TableEntity
    {
        public GitHubHostedDocumentEntity(string orgRepo, string commitDocument)
            : base(orgRepo, commitDocument)
        { }

        public GitHubHostedDocumentEntity() { }

        public string FileName { get; set; }
        public string Status { get; set; }
        public DateTime TsCommit { get; set; }
        public string Commit { get; set; }
        public string Organization { get; set; }
        public string Repository { get; set; }
        public string StatusId { get; set; }
    }
}
