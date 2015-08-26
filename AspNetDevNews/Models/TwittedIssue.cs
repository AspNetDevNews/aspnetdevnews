using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Models
{
    public class TwittedIssue: Issue, IHasTweetInfo
    {
        public ulong StatusID { get; set; }
    }
}
