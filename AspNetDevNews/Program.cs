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

            DateTime dtInizio = DateTime.Now;
            int twitted = 0;
            int checkedRepositories = 0;

            foreach (var organization in ghService.Organizations) {
                var repositories = await ghService.Repositories(organization);
                checkedRepositories += repositories.Count();
                foreach (var repository in repositories) {
                    // get recent created or modified issues
                    var issues = await ghService.RecentGitHubIssues(organization, repository);
                    // if no issues are reported from github, go next repository
                    if (issues == null || issues.Count == 0)
                        continue;
                    // get the latest issues archived
                    var lastStored = await ghService.RecentStorageIssues(organization, repository);
                    // check for updates
                    var changed = await ghService.IssuesToUpdate(issues, lastStored);
                    // if updated ones, merge the changes
                    await ghService.Merge(changed);
                    // remove from the list the ones already in the storage, keeping just the ones 
                    // I have to tweet and store
                    issues = await ghService.RemoveExisting(issues);
                    // publish the new issues
                    var twittedIssues = await ghService.PublishNewIssues(issues);
                    // store in the storage the data about the new issues
                    await ghService.StorePublishedIssues(twittedIssues);
                    twitted += twittedIssues.Count;
                }
            }

            DateTime dtFine = DateTime.Now;
            var stgService = new AzureTableStorageService();
            await stgService.ReportExection(dtInizio, dtFine, twitted, checkedRepositories);
        }
    }
}
