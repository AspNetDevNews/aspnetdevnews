using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AspNetDevNews.Services.Interfaces;
using AspNetDevNews.Services;
using AspNetDevNews.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetDevNews.Test.Support;

namespace AspNetDevNews.Test
{
    [TestClass]
    public class RecentGitHubIssues
    {
        private IssueReceiveService GetService(IGitHubService gitHubService)
        {
            var ghService = new IssueReceiveService(
                gitHubService: gitHubService,
                storageService: new DummyStorageService(),
                settingsService: new OneDaySettings(),
                twitterService: new DummyTwitterService());
            return ghService;
        }

        private IssueReceiveService GetService() {
            var dummyGutHubService = new DummyGitHubService();
            return GetService(dummyGutHubService);
        }

        [TestMethod,ExpectedException(typeof(ArgumentNullException)), TestCategory("RecentGitHubIssues")]
        public async Task OrganizationParameterCannotBeEmpty()
        {
            var ghService = this.GetService();
            var issues = await ghService.RecentGitHubIssues("", ".");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("RecentGitHubIssues")]
        public async Task RepositoryParameterCannotBeEmpty()
        {
            var ghService = this.GetService();
            var issues = await ghService.RecentGitHubIssues(".", "");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("RecentGitHubIssues")]
        public async Task OrganizationParameterCannotBeNull()
        {
            var ghService = this.GetService();
            var issues = await ghService.RecentGitHubIssues(null, ".");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("RecentGitHubIssues")]
        public async Task RepositoryParameterCannotBeNull()
        {
            var ghService = this.GetService();
            var issues = await ghService.RecentGitHubIssues(".", null);
        }


        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IssuesWithAtLeastOneLabelAreOk()
        {
            var dummyGutHubService = new DummyGitHubService();
            var ghService = GetService( dummyGutHubService );

            foreach (var label in ghService.Labels) {
                dummyGutHubService.RecentIssues = new List<Issue>();
                dummyGutHubService.RecentIssues.Add(new Issue { Labels = new[] { label } } );
                var issues = await ghService.RecentGitHubIssues(".", ".");
                Assert.IsNotNull(issues);
                Assert.AreEqual(1,issues.Count);
            }
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IssuesWithOtherLabelAreKo()
        {
            var dummyGutHubService = new DummyGitHubService();
            var ghService = GetService(dummyGutHubService);

            dummyGutHubService.RecentIssues.Add(new Issue { Labels = new[] { "pippo" } });
            var issues = await ghService.RecentGitHubIssues(".", ".");
            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IssuesWithNoLabelAreKo()
        {
            var dummyGutHubService = new DummyGitHubService();
            var ghService = GetService(dummyGutHubService);

            dummyGutHubService.RecentIssues.Add(new Issue { Labels = new string[] { } });
            var issues = await ghService.RecentGitHubIssues(".", ".");
            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IfFailingInGetIssuesReturnsEmptyList()
        {
            var dummyGutHubService = new DummyGitHubService();
            var ghService = GetService(dummyGutHubService);

            dummyGutHubService.RecentIssues.Add(new Issue { Labels = new string[] { } });
            var issues = await ghService.RecentGitHubIssues(".", ".");
            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IfOneOkAndOneKoJustOneIsReturned()
        {
            var dummyGutHubService = new DummyGitHubService();
            var ghService = GetService(dummyGutHubService);

            foreach (var label in ghService.Labels)
            {
                dummyGutHubService.RecentIssues = new List<Issue>();
                dummyGutHubService.RecentIssues.Add(new Issue { Title = "Ok", Labels = new[] { label } });
                dummyGutHubService.RecentIssues.Add(new Issue { Title = "Ko", Labels = new[] { "pippo" } });

                var issues = await ghService.RecentGitHubIssues(".", ".");
                Assert.IsNotNull(issues);
                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual("Ok", issues[0].Title);
            }
        }

    }
}
