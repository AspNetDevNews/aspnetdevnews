using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Models
{
    public class Issue: ITableStorageKeyGet, IIsTweetable
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string []Labels { get; set; }
        public string Organization { get; set; }
        public string Repository { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Number { get; set; }
        public DateTime ?UpdatedAt { get; set; }
        public string Body { get; set; }
        public string State { get; set; }
        public int Comments { get; set; }

        public string GetPartitionKey() {
            return Organization + "+" + Repository;
        }
        public string GetRowKey()
        {
            return Number.ToString();
        }

        public string PlainLabels {
            get {
                return this.Labels != null ? string.Join(";", this.Labels).ToLower() : string.Empty;
            }
        }

        public string GetTwitterText() {
            string title = this.Title.Trim();
            if (!title.EndsWith(".", StringComparison.Ordinal))
                title += ".";
            if (title.Length > 85)
                title = title.Substring(0, 85);
            return "[" + PlainLabels + "]: " + title + " " + this.Url;
        }

    }
}
