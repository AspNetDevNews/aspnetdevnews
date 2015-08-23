using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.AzureTableStorage
{
    public class ExecutionEntity : TableEntity
    {
        public ExecutionEntity(string operation, string timestamp)
            : base(operation, timestamp)
        { }

        public ExecutionEntity() { }

        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public int TwittedIsseues { get; set; }
        public int CheckedRepositories { get; set; }
        public int UpdatedIssues { get; set; }
    }
}
