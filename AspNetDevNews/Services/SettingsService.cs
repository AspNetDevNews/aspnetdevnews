using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services
{
    public class SettingsService: ISettingsService
    {
        public SettingsService() {
            GitHubAppName = ConfigurationManager.AppSettings["GitHubAppName"];
            GitHubUserId = ConfigurationManager.AppSettings["GitHubUserId"];
            GitHubPassword = ConfigurationManager.AppSettings["GitHubPassword"];

            TwittedIssuesATAccountName = ConfigurationManager.AppSettings["TwittedIssuesATAccountName"];
            TwittedIssuesATAccountKey = ConfigurationManager.AppSettings["TwittedIssuesATAccountKey"];

            TwitterConsumerKey = ConfigurationManager.AppSettings["TwitterConsumerKey"];
            TwitterConsumerSecret = ConfigurationManager.AppSettings["TwitterConsumerSecret"];
            TwitterAccessToken = ConfigurationManager.AppSettings["TwitterAccessToken"];
            TwitterAccessTokenSecret = ConfigurationManager.AppSettings["TwitterAccessTokenSecret"];

        }
        // GitHub Client Parameters
        public string GitHubAppName { get; private set; }
        public string GitHubPassword { get; private set; }
        public string GitHubUserId { get; private set; }
        // Storage Account Parameters
        public string TwittedIssuesATAccountName { get; private set; }
        public string TwittedIssuesATAccountKey { get; private set; }

        public string TwitterConsumerKey { get; private set; }
        public string TwitterConsumerSecret { get; private set; }
        public string TwitterAccessToken { get; private set; }
        public string TwitterAccessTokenSecret { get; private set; }


    }
}
