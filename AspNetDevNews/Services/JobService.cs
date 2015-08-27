using AspNetDevNews.Models;
using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services
{
    public class JobService: IJobService
    {
        public IEnumerable<string> Organizations
        {
            get { return organizations; }
        }

        public IEnumerable<string> Feeds
        {
            get { return new List<string> { "http://webdevblogs.azurewebsites.net/master.xml" }; }
        }

        public IList<GitHubRepo> DocsRepo
        {
            get
            {
                List<GitHubRepo> repos = new List<GitHubRepo>();
                repos.Add(new GitHubRepo { Organization = "aspnet", Repository = "Docs" });
                repos.Add(new GitHubRepo { Organization = "aspnet", Repository = "EntityFramework.Docs" });
                repos.Add(new GitHubRepo { Organization = "dotnet", Repository = "core-docs" });
                return repos;
            }
        }

        private static string []labels = { "Announcement", "Breaking Change", "Feedback Wanted", "Up for Grabs", "up-for-grabs", "help wanted", "feedback-requested" };
        private string []organizations = { "aspnet", "nuget" };

        public IList<string> Labels
        {
            get { return labels; }
        }

        public JobService( ISettingsService settings) {
        }
    }
}
