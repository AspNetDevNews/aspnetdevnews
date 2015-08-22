using AspNetDevNews.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: updated the UpdatedAt field at every update
//       store the number of comments
namespace AspNetDevNews
{
    class Program
    {
        static void Main(string[] args)
        {
            Work().Wait();
        }

        public static async Task Work() {
            var ghService = new IssueReceiveService();
            var twService = new TwitterService();
            var stgService = new AzureTableStorageService();

            DateTime dtInizio = DateTime.Now;
            int twitted = 0;
            int checkedRepositories = 0;

            foreach (var organization in ghService.Organizations) {
                var repositories = await ghService.Repositories(organization);
                checkedRepositories += repositories.Count();
                foreach (var repository in repositories) {
                    var issues = await ghService.RecentIssues(organization, repository);
                    if(issues.Count() > 0) {
                        issues = await stgService.RemoveExisting(issues);
                        var twittedIssues = await twService.SendIssues(issues);
                        await stgService.Store(twittedIssues);
                        twitted += twittedIssues.Count;
                    }
                }
            }

            DateTime dtFine = DateTime.Now;
            await stgService.ReportExection(dtInizio, dtFine, twitted, checkedRepositories);
        }
    }
}
