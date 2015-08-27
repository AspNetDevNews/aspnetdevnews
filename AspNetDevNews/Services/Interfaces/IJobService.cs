using AspNetDevNews.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.Interfaces
{
    public interface IJobService
    {
        IEnumerable<string> Organizations { get; }

        IEnumerable<string> Feeds { get;  }

        IList<GitHubRepo> DocsRepo { get;  }

        IList<string> Labels { get; }
    }
}
