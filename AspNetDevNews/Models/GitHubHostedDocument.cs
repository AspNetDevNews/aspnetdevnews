using AspNetDevNews.Helpers;
using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Models
{
    public class GitHubHostedDocument: ITableStorageKeyGet, IIsTweetable
    {
        public string FileName { get; set; }
        public string Status { get; set; }
        public DateTimeOffset TsCommit { get; set; }
        public string Commit { get; set; }
        public string Organization { get; set; }
        public string Repository { get; set; }

        public string GetPartitionKey()
        {
            return Organization + "+" + Repository;
        }
        public string GetRowKey()
        {
            return Commit + "+" + FileName.Replace("/", "-");
        }

        private string GetDocumentName() {
            int position = FileName.LastIndexOf("/", StringComparison.Ordinal);
            string documentName = string.Empty;
            if (position != -1)
                documentName = FileName.Substring(position+1);
            else
                documentName = FileName;
            return documentName.Substring(0, documentName.Length - 4);
        }
        
        public string GetTwitterText()
        {
            string url = UrlFormatter.GetWorkingUrl(Organization, Repository, FileName);
            string documentName = GetDocumentName();
            string tweetStatus = string.Empty;

            if (Status.ToLower() == "modified")
                tweetStatus = "update";
            else
                tweetStatus = Status.ToLower();

            return $"[doc {tweetStatus}]: {documentName} " + url;
        }

    }
}
