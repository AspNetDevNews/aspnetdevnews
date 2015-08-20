using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.ATStorage
{
    public class ExceptionEntity : TableEntity
    {
        public ExceptionEntity(string operation, string timestamp)
            : base(operation, timestamp)
        { }

        public ExceptionEntity() { }

        public DateTime CreatedAt { get; set; }
        public string Exception { get; set; }
        public string Operation { get; set; }
        public string TwitPartitionKey { get; set; }
        public string TwitRowKey { get; set; }
    }

}
