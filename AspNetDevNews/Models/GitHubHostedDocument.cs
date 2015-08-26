using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Models
{
    public class GitHubHostedDocument
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

        private string GetSection() {
            if (FileName.ToLower().StartsWith("aspnet/", StringComparison.Ordinal))
                return "aspnet";
            else if (FileName.ToLower().StartsWith("mvc/", StringComparison.Ordinal))
                return "mvc";
            else
                return string.Empty;
        }

        private string GetDocumentUrl() {
            string section = GetSection();

            if (!string.IsNullOrWhiteSpace(section))
            {
                string url = FileName.Substring(section.Length + 1);
                url = url.Substring(0, url.Length - 4);
                if (section == "aspnet")
                    url = "http://docs.asp.net/en/latest/" + url;
                else if (section == "mvc")
                    url = "http://docs.asp.net/projects/mvc/en/latest/" + url;
                url += ".html";

                return url;

            }
            else
                return string.Empty;
        }

        private string GetDocumentName() {
            int position = FileName.LastIndexOf("/");
            string documentName = string.Empty;
            if (position != -1)
                documentName = FileName.Substring(position+1);
            else
                documentName = FileName;
            return documentName.Substring(0, documentName.Length - 4);
        }
        
        public string GetTwitterText()
        {

            string url = GetDocumentUrl();
            string documentName = GetDocumentName();

            return "[document " + Status + "]: " + documentName + " " + url;
        }

    }
}
