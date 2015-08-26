using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Models
{
    public class TwittedGitHubHostedDocument: GitHubHostedDocument, IHasTweetInfo
    {
        public ulong StatusID { get; set; }
    }
}
