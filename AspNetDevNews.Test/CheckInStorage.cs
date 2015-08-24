using AspNetDevNews.Models;
using AspNetDevNews.Services;
using AspNetDevNews.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Test
{
    [TestClass]
    public class CheckInStorage
    {
        private IssueReceiveService GetService(IStorageService storageService)
        {
            var ghService = new IssueReceiveService(
                gitHubService: new GitHubService(),
                storageService: storageService,
                settingsService: new SettingsService(),
                twitterService: new TwitterService());
            return ghService;
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("CheckInStorage")]
        public async Task IfOrganizationIsNullRaisesAnException()
        {
            var ghService = new IssueReceiveService();
            List<Issue> test = new List<Issue>();

            var issues = await ghService.CheckInStorage(null,".", test);

        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("CheckInStorage")]
        public async Task IfOrganizationIsEmptyRaisesAnException()
        {
            var ghService = new IssueReceiveService();
            List<Issue> test = new List<Issue>();

            var issues = await ghService.CheckInStorage("", ".", test);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("CheckInStorage")]
        public async Task IfRepositoryIsEmptyRaisesAnException() {
            var ghService = new IssueReceiveService();
            List<Issue> test = new List<Issue>();

            var issues = await ghService.CheckInStorage(".", "", test);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("CheckInStorage")]
        public async Task IfRepositoryIsNullRaisesAnException()
        {
            var ghService = new IssueReceiveService();
            List<Issue> test = new List<Issue>();

            var issues = await ghService.CheckInStorage(".", null, test);
        }

        [TestMethod, TestCategory("CheckInStorage")]
        public async Task IfIssuesListIsNullReturnEmptyList()
        {
            var ghService = new IssueReceiveService();
            List<Issue> test = new List<Issue>();

            var issues = await ghService.CheckInStorage(".", ".", null);
            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod, TestCategory("CheckInStorage")]
        public async Task IfIssuesListIsEmptyReturnEmptyList()
        {
            var ghService = new IssueReceiveService();
            List<Issue> test = new List<Issue>();

            var issues = await ghService.CheckInStorage(".", ".", test);
            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod, TestCategory("CheckInStorage")]
        public async Task IfTheListIsEmptyAnEmptyListIsReturned()
        {
            //var dummyStorage = new DummyStorageService();
            //var ghService = this.GetService(dummyStorage);
            var organization = "org";
            var repository = "repo";

            List<Issue> storageIssues = new List<Issue>();
            storageIssues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });

            var issues = new List<Issue>();

            var gitMock = new Mock<IStorageService>();
            gitMock.Setup(mock => mock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Returns(storageIssues);
            var ghService = this.GetService(gitMock.Object);

            var cleanedIssues = await ghService.CheckInStorage(".", ".", issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(0, cleanedIssues.Count);
        }

        [TestMethod, TestCategory("CheckInStorage")]
        public async Task AllTheKeysInTheListAreSearchedInTheStorage()
        {
            var issues = new List<Issue>();
            issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 1 });
            issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 2 });
            var numKeys = 0;

            var gitMock = new Mock<IStorageService>();
            gitMock.Setup(mock => mock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Callback((string org, string repo, IList<string> keys) => numKeys = keys.Count)
                .Returns(issues);
            var ghService = this.GetService(gitMock.Object);

            await ghService.CheckInStorage("org", "repo", issues);
            Assert.AreEqual(issues.Count, numKeys);

        }

        [TestMethod, ExpectedException(typeof(ApplicationException)), TestCategory("CheckInStorage")]
        public async Task IfFromRepositoriesDifferentFromParametersRaisesAnException()
        {
            var ghService = new IssueReceiveService();

            var issues = new List<Issue>();
            issues.Add(new Issue { Organization = "org", Repository = "repo2", Number = 1 });
            issues.Add(new Issue { Organization = "org", Repository = "repo2", Number = 2 });

            var cleanedIssues = await ghService.CheckInStorage("org", "repo", issues);
        }

        [TestMethod, TestCategory("CheckInStorage")]
        public async Task IfFailsInGetReturnsEmptyList()
        {
            var issues = new List<Issue>();
            issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 1 });

            var gitMock = new Mock<IStorageService>();
            gitMock.Setup(mock => mock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Throws(new ApplicationException());
            var ghService = this.GetService(gitMock.Object);

            var cleanedIssues = await ghService.CheckInStorage("org", "repo", issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(0, cleanedIssues.Count);
        }

    }
}
