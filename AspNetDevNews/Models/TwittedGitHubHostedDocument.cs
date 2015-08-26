using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Models
{
    public class TwittedGitHubHostedDocument: GitHubHostedDocument
    {
        public ulong StatusID { get; set; }
    }
}
