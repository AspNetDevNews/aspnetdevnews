using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.Interfaces
{
    public interface ISettingsService
    {
        string GitHubAppName { get;  }
        string GitHubUserId { get; }
        string GitHubPassword { get;  }

        string TwittedIssuesATAccountName { get; }
        string TwittedIssuesATAccountKey { get; }

        string TwitterConsumerKey { get; }
        string TwitterConsumerSecret { get;  }
        string TwitterAccessToken { get;  }
        string TwitterAccessTokenSecret { get; }
        DateTimeOffset Since { get; }
    }
}
