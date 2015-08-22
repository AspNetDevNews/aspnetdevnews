using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Test.Support
{
    public class OneDaySettings : ISettingsService
    {
        public string GitHubAppName
        {
            get { return string.Empty; }
        }

        public string GitHubPassword
        {
            get { return string.Empty; }
        }

        public string GitHubUserId
        {
            get { return string.Empty; }
        }

        public DateTimeOffset Since
        {
            get
            {
                return new DateTimeOffset(DateTime.Now.AddDays(-1));
            }
        }

        public string TwittedIssuesATAccountKey
        {
            get { return string.Empty; }
        }

        public string TwittedIssuesATAccountName
        {
            get { return string.Empty; }
        }

        public string TwitterAccessToken
        {
            get { return string.Empty; }
        }

        public string TwitterAccessTokenSecret
        {
            get { return string.Empty; }
        }

        public string TwitterConsumerKey
        {
            get { return string.Empty; }
        }

        public string TwitterConsumerSecret
        {
            get { return string.Empty; }
        }
    }
}
