﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Models
{
    public class TwittedIssue
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string[] Labels { get; set; }
        public string Organization { get; set; }
        public string Repository { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Number { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ulong StatusID { get; set; }
        public string Body { get; set; }
        public string State { get; internal set; }
        public int Comments { get; internal set; }

        public string GetPartitionKey()
        {
            return Organization + "+" + Repository;
        }
        public string GetRowKey()
        {
            return Number.ToString();
        }
    }
}
