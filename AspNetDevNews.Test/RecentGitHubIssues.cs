using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AspNetDevNews.Services.Interfaces;
using AspNetDevNews.Services;
using AspNetDevNews.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using AspNetDevNews.Services.AzureTableStorage;
using AspNetDevNews.Services.Feeds;
using Autofac.Extras.Moq;

namespace AspNetDevNews.Test
{
    [TestClass]
    public class RecentGitHubIssues
    {
        [TestMethod,ExpectedException(typeof(ArgumentNullException)), TestCategory("RecentGitHubIssues")]
        public async Task OrganizationParameterCannotBeEmpty()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();
                var issues = await sut.RecentGitHubIssues("", ".");
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("RecentGitHubIssues")]
        public async Task RepositoryParameterCannotBeEmpty()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();
                var issues = await sut.RecentGitHubIssues(".", "");
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("RecentGitHubIssues")]
        public async Task OrganizationParameterCannotBeNull()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();
                var issues = await sut.RecentGitHubIssues(null, ".");
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("RecentGitHubIssues")]
        public async Task RepositoryParameterCannotBeNull()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();
                var issues = await sut.RecentGitHubIssues(".", null);
            }
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IssuesWithAtLeastOneLabelAreOk()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var serviceForLabels = new IssueReceiveService();

                foreach (var label in serviceForLabels.Labels)
                {
                    var recentIssues = new List<Issue>();
                    recentIssues.Add(new Issue { Labels = new[] { label } });

                    mock.Mock<IGitHubService>()
                        .Setup(myMock => myMock.GetRecentIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(recentIssues);

                    var sut = mock.Create<IssueReceiveService>();

                    var issues = await sut.RecentGitHubIssues(".", ".");
                    Assert.IsNotNull(issues);
                    Assert.AreEqual(1, issues.Count);
                }
            }
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IssuesWithOtherLabelAreKo()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var recentIssues = new List<Issue>();
                recentIssues.Add(new Issue { Labels = new[] { "pippo" } });

                mock.Mock<IGitHubService>()
                    .Setup(myMock => myMock.GetRecentIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(recentIssues);

                var sut = mock.Create<IssueReceiveService>();

                var issues = await sut.RecentGitHubIssues(".", ".");
                Assert.IsNotNull(issues);
                Assert.AreEqual(0, issues.Count);
            }
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IssuesWithNoLabelAreKo()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var recentIssues = new List<Issue>();
                recentIssues.Add(new Issue { Labels = new string[] { } });

                mock.Mock<IGitHubService>()
                    .Setup(myMock => myMock.GetRecentIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(recentIssues);

                var sut = mock.Create<IssueReceiveService>();

                var issues = await sut.RecentGitHubIssues(".", ".");

                Assert.IsNotNull(issues);
                Assert.AreEqual(0, issues.Count);
            }
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IfFailingInGetIssuesReturnsEmptyList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var recentIssues = new List<Issue>();
                recentIssues.Add(new Issue { Labels = new string[] { } });

                mock.Mock<IGitHubService>()
                    .Setup(myMock => myMock.GetRecentIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
                    .Throws(new ApplicationException());

                var sut = mock.Create<IssueReceiveService>();

                var issues = await sut.RecentGitHubIssues(".", ".");

                Assert.IsNotNull(issues);
                Assert.AreEqual(0, issues.Count);
            }
        }

        [TestMethod, TestCategory("RecentGitHubIssues")]
        public async Task IfOneOkAndOneKoJustOneIsReturned()
        {
            using (var mock = AutoMock.GetLoose())
            {

                var serviceForLabels = new IssueReceiveService();

                foreach (var label in serviceForLabels.Labels)
                {
                    var recentIssues = new List<Issue>();
                    recentIssues.Add(new Issue { Title = "Ok", Labels = new[] { label } });
                    recentIssues.Add(new Issue { Title = "Ko", Labels = new[] { "pippo" } });

                    mock.Mock<IGitHubService>()
                        .Setup(myMock => myMock.GetRecentIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
                        .ReturnsAsync(recentIssues);

                    var sut = mock.Create<IssueReceiveService>();

                    var issues = await sut.RecentGitHubIssues(".", ".");

                    Assert.IsNotNull(issues);
                    Assert.AreEqual(1, issues.Count);
                    Assert.AreEqual("Ok", issues[0].Title);
                }
            }
        }

    }
}
