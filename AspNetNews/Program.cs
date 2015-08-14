using AspNetNews.Repos;
using LinqToTwitter;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNews
{
    class Program
    {
        private static List<GhOrganization> Organizations { get; set; }
        private static List<string> Labels { get; set; }

        private static SingleUserAuthorizer authorizer =
                 new SingleUserAuthorizer
                 {
                     CredentialStore = new
                    SingleUserInMemoryCredentialStore
                     {
                         ConsumerKey = "UFulm1GvnQdFkgTcHmTpLAg6n",
                         ConsumerSecret = "2yZVuCo2sbZgKaatG6CwsE5Rr4unM5C3L2VkGQKatVBuO23Ukj",
                         AccessToken = "50950120-J1H5slds6tG08W5O4fnNuLjmqhYQkliK2C3JgYCJ9",
                         AccessTokenSecret = "h0GyII6AwgRIHlpj1Qo9kd6Ii2XxHYcDf1WPf6zZkJ8lb"
                     }
                 };

        static void Main(string[] args)
        {
            InitScanData();
            Work().Wait();
        }

        public static void InitScanData() {
            Organizations = new List<GhOrganization>();

            var aspNetOrg = new GhOrganization();
            Organizations.Add(aspNetOrg);
            aspNetOrg.Owner = "aspnet";
            aspNetOrg.Repositories = new List<string>();
            aspNetOrg.Repositories.Add("EntityFramework");
            aspNetOrg.Repositories.Add("KestrelHttpServer");
            aspNetOrg.Repositories.Add("Mvc");
            aspNetOrg.Repositories.Add("dnx");
            aspNetOrg.Repositories.Add("Logging");
            aspNetOrg.Repositories.Add("MusicStore");
            aspNetOrg.Repositories.Add("Razor");
            aspNetOrg.Repositories.Add("benchmarks");
            aspNetOrg.Repositories.Add("DependencyInjection");
            aspNetOrg.Repositories.Add("Testing");
            aspNetOrg.Repositories.Add("BasicMiddleware");
            aspNetOrg.Repositories.Add("Session");
            aspNetOrg.Repositories.Add("Docs");
            aspNetOrg.Repositories.Add("SignalR-Redis");
            aspNetOrg.Repositories.Add("DataProtection");
            aspNetOrg.Repositories.Add("Security");
            aspNetOrg.Repositories.Add("Diagnostics");
            aspNetOrg.Repositories.Add("UserSecrets");
            aspNetOrg.Repositories.Add("Performance");
            aspNetOrg.Repositories.Add("Scaffolding");
            aspNetOrg.Repositories.Add("Identity");
            aspNetOrg.Repositories.Add("Configuration");
            aspNetOrg.Repositories.Add("Hosting");
            aspNetOrg.Repositories.Add("ServerTests");
            aspNetOrg.Repositories.Add("dnvm");
            aspNetOrg.Repositories.Add("StaticFiles");
            aspNetOrg.Repositories.Add("WebSockets");
            aspNetOrg.Repositories.Add("Routing");
            aspNetOrg.Repositories.Add("Entropy");
            aspNetOrg.Repositories.Add("SignalR-Server");
            aspNetOrg.Repositories.Add("SignalR-SqlServer");
            aspNetOrg.Repositories.Add("Options");
            aspNetOrg.Repositories.Add("live.asp.net");
            aspNetOrg.Repositories.Add("Microsoft.Data.Sqlite");
            aspNetOrg.Repositories.Add("Localization");
            aspNetOrg.Repositories.Add("HttpClient");
            aspNetOrg.Repositories.Add("FileSystem");
            aspNetOrg.Repositories.Add("EventNotification");
            aspNetOrg.Repositories.Add("CORS");
            aspNetOrg.Repositories.Add("Antiforgery");
            aspNetOrg.Repositories.Add("Home");
            aspNetOrg.Repositories.Add("Caching");
            aspNetOrg.Repositories.Add("Universe");
            aspNetOrg.Repositories.Add("aspnet.xunit");
            aspNetOrg.Repositories.Add("vsweb-publish");
            aspNetOrg.Repositories.Add("Coherence");
            aspNetOrg.Repositories.Add("Common");
            aspNetOrg.Repositories.Add("SignalR-Client-Cpp");
            aspNetOrg.Repositories.Add("DnxTools");
            aspNetOrg.Repositories.Add("Proxy");
            aspNetOrg.Repositories.Add("Signing");
            aspNetOrg.Repositories.Add("xunit");
            aspNetOrg.Repositories.Add("vsweb-docs");
            aspNetOrg.Repositories.Add("EntityFramework.Docs");
            aspNetOrg.Repositories.Add("aspnet-docker");
            aspNetOrg.Repositories.Add("Announcements");
            aspNetOrg.Repositories.Add("jquery-ajax-unobtrusive");
            aspNetOrg.Repositories.Add("jquery-validation-unobtrusive");
            aspNetOrg.Repositories.Add("SignalR-ServiceBus");
            aspNetOrg.Repositories.Add("homebrew-dnx");
            aspNetOrg.Repositories.Add("ResponseCaching");
            aspNetOrg.Repositories.Add("BugTracker");
            aspNetOrg.Repositories.Add("SignalR-Client-Java");
            aspNetOrg.Repositories.Add("SignalR-Client-JS");
            aspNetOrg.Repositories.Add("ignalR-Client-Net");

            var nugetOrg = new GhOrganization();
            Organizations.Add(nugetOrg);
            nugetOrg.Owner = "nuget";
            nugetOrg.Repositories = new List<string>();
            nugetOrg.Repositories.Add("NuGet.VisualStudioExtension");
            nugetOrg.Repositories.Add("NuGet.PackageManagement");
            nugetOrg.Repositories.Add("NuGet3");
            nugetOrg.Repositories.Add("NuGet.V2");
            nugetOrg.Repositories.Add("NuGet.Jobs");
            nugetOrg.Repositories.Add("NuGetGallery");
            nugetOrg.Repositories.Add("NuGet.Services.Metadata");
            nugetOrg.Repositories.Add("NuGet.Services.Dashboard");
            nugetOrg.Repositories.Add("Home");
            nugetOrg.Repositories.Add("NuGetDocs");
            nugetOrg.Repositories.Add("NuGet.PackageIndex");
            nugetOrg.Repositories.Add("NuGet.Services.Build");
            nugetOrg.Repositories.Add("WebBackgrounder");
            nugetOrg.Repositories.Add("NuGet.Services.Search");
            nugetOrg.Repositories.Add("NuGet.Versioning");
            nugetOrg.Repositories.Add("NuGet.Protocol");
            nugetOrg.Repositories.Add("NuGet.Packaging");
            nugetOrg.Repositories.Add("NuGet.Warehouse");
            nugetOrg.Repositories.Add("json-ld.net");
            nugetOrg.Repositories.Add("NuGet.Services.Work");
            nugetOrg.Repositories.Add("NuGet.Schema");
            nugetOrg.Repositories.Add("NuGet.Services.Messaging");
            nugetOrg.Repositories.Add("NuGet.Client");
            nugetOrg.Repositories.Add("NuGet.CommandLine");
            nugetOrg.Repositories.Add("NuGet.Services.Gateway");
            nugetOrg.Repositories.Add("NuGet.Operations");
            nugetOrg.Repositories.Add("NuGet.Services.Platform");
            nugetOrg.Repositories.Add("NuGet.Services.Metrics");
            nugetOrg.Repositories.Add("PoliteCaptcha");
            nugetOrg.Repositories.Add("Media");
            nugetOrg.Repositories.Add("Samples");
            nugetOrg.Repositories.Add("OpsDashboard");
            nugetOrg.Repositories.Add("Concierge");
            nugetOrg.Repositories.Add("NuGetStats");

            Labels = new List<string>();
            //Labels.Add("Announcement");
            //Labels.Add("Breaking Change");
            //Labels.Add("Feedback Wanted");
            //Labels.Add("Up for Grabs");
            Labels.Add("question");

        }

        public static async Task Work() {
            var client = new GitHubClient(new ProductHeaderValue("AspNetNews"));

            var repositories = await client.Repository.GetAllForOrg("aspnet");
            

            var basicAuth = new Credentials("lucamorelli", "61Stress"); // NOTE: not real credentials
            client.Credentials = basicAuth;
            //var tokenAuth = new Credentials("token"); // NOTE: not real token
            //client.Credentials = tokenAuth;

            var request = new RepositoryIssueRequest();
            request.Since = DateTime.Now.AddMinutes(-15);
            //foreach( var label in Labels)
            //    request.Labels.Add(label);
            List<Issue> issues = new List<Issue>();

            using (var twitterCtx = new TwitterContext(authorizer))
            {
                var tweet = await twitterCtx.TweetAsync("prova");

                //Console.WriteLine(tweet.StatusID);

            }




            try
            {
                foreach (var organization in Organizations)
                {
                    foreach (var repository in organization.Repositories) { 
                        var queryResult = await client.Issue.GetAllForRepository(organization.Owner, repository, request);
                        foreach (var issue in queryResult) {
                            if (issue.PullRequest != null)
                                continue;

                            foreach (var refLabel in Labels) {

                                if (issue.Labels.Any(lab => lab.Name == refLabel)) { 
                                    issues.Add(issue);

                                }
                            }
                        }
                    }
                }

            }
            catch (Exception exc) {

            }
            int daMandare = issues.Count;

            if (daMandare > 0) {
            }
        }
    }
}
