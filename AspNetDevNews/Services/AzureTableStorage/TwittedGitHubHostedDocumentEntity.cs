using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.AzureTableStorage
{
    public class TwittedGitHubHostedDocumentEntity : TableEntity
    {
        public TwittedGitHubHostedDocumentEntity(string orgRepo, string commitDocument)
            : base(orgRepo, commitDocument)
        { }

        public TwittedGitHubHostedDocumentEntity() { }

        public string FileName { get; set; }
        public string Status { get; set; }
        public DateTime TsCommit { get; set; }
        public string Commit { get; set; }
        public string Organization { get; set; }
        public string Repository { get; set; }
        public string StatusId { get; set; }
    }
}
