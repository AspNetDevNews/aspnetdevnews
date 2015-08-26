using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Models
{
    public class TwittedIssue: Issue
    {
        public ulong StatusID { get; set; }
    }
}
