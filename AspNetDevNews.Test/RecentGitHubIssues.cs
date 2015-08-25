using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AspNetDevNews.Services.Interfaces;
using AspNetDevNews.Services;
using AspNetDevNews.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using AspNetDevNews.Services.AzureTableStorage;

namespace AspNetDevNews.Test
{
    [TestClass]
    public class RecentGitHubIssues
    {
        private IssueReceiveService GetService(IGitHubService gitHubService)
        {
            var ghService = new IssueReceiveService(
                gitHubService: gitHubService,
                storageService: new AzureTableStorageService(),
                settingsService: new SettingsService(),
                twitterService: new TwitterService());
            return ghService;
        }

        [TestMethod,ExpectedException(typeof(ArgumentNullException)), TestCategory("RecentGitHubIssues")]
        public async Task OrganizationParameterCannotBeEmpty()
        {
            var ghService = new IssueReceiveService();
            var issues = await ghService.RecentGitHubIssues("", ".");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("RecentGitHubIssues")]
        public async Task RepositoryParameterCannotBeEmpty()
        {
            var ghService = new IssueReceiveService();
            var issues = await ghService.RecentGitHubIssues(".", "");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("RecentGitHubIssues")]
        public async Task OrganizationParameterCannotBeNull()
        {
            var ghService = new IssueReceiveService();
            var issues = await ghService.RecentGitHubIssues(null, ".");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("RecentGitHubIssues")]
        public async Task RepositoryParameterCannotBeNull()
        {
            var ghService = new IssueReceiveService();
            var issues = await ghService.RecentGitHubIssues(".", null);
        }


        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IssuesWithAtLeastOneLabelAreOk()
        {
            //var dummyGutHubService = new DummyGitHubService();
            //var ghService = GetService( dummyGutHubService );
            var serviceForLabels = new IssueReceiveService();

            foreach (var label in serviceForLabels.Labels) {
                var recentIssues = new List<Issue>();
                recentIssues.Add(new Issue { Labels = new[] { label } } );

                var gitMock = new Mock<IGitHubService>();
                gitMock.Setup(mock => mock.GetRecentIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(recentIssues);
                var ghService = this.GetService(gitMock.Object);

                var issues = await ghService.RecentGitHubIssues(".", ".");
                Assert.IsNotNull(issues);
                Assert.AreEqual(1,issues.Count);
            }
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IssuesWithOtherLabelAreKo()
        {
            //var dummyGutHubService = new DummyGitHubService();
            //var ghService = GetService(dummyGutHubService);

            var recentIssues = new List<Issue>();
            recentIssues.Add(new Issue { Labels = new[] { "pippo" } });

            var gitMock = new Mock<IGitHubService>();
            gitMock.Setup(mock => mock.GetRecentIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(recentIssues);
            var ghService = this.GetService(gitMock.Object);

            var issues = await ghService.RecentGitHubIssues(".", ".");
            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IssuesWithNoLabelAreKo()
        {
            var recentIssues = new List<Issue>();
            recentIssues.Add(new Issue { Labels = new string[] { } });

            var gitMock = new Mock<IGitHubService>();
            gitMock.Setup(mock => mock.GetRecentIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(recentIssues);
            var ghService = this.GetService(gitMock.Object);

            var issues = await ghService.RecentGitHubIssues(".", ".");
            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IfFailingInGetIssuesReturnsEmptyList()
        {
            var recentIssues = new List<Issue>();
            recentIssues.Add(new Issue { Labels = new string[] { } });

            var gitMock = new Mock<IGitHubService>();
            gitMock.Setup(mock => mock.GetRecentIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>())).Throws(new ApplicationException());
            var ghService = this.GetService(gitMock.Object);

            var issues = await ghService.RecentGitHubIssues(".", ".");
            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IfOneOkAndOneKoJustOneIsReturned()
        {
            var serviceForLabels = new IssueReceiveService();

            foreach (var label in serviceForLabels.Labels)
            {
                var recentIssues = new List<Issue>();
                recentIssues.Add(new Issue { Title = "Ok", Labels = new[] { label } });
                recentIssues.Add(new Issue { Title = "Ko", Labels = new[] { "pippo" } });

                var gitMock = new Mock<IGitHubService>();
                gitMock.Setup(mock => mock.GetRecentIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(recentIssues);
                var ghService = this.GetService(gitMock.Object);

                var issues = await ghService.RecentGitHubIssues(".", ".");
                Assert.IsNotNull(issues);
                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual("Ok", issues[0].Title);
            }
        }

    }
}
