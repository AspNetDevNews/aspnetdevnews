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
    public class RemoveExistingFeeds
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

        [TestMethod, TestCategory("RemoveExistingFeeds")]
        public async Task IfListIsNullReturnEmptyList()
        {
            var ghService = new IssueReceiveService();
            List<FeedItem> test = null;

            var issues = await ghService.RemoveExisting(test);

            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod, TestCategory("RemoveExistingFeeds")]
        public async Task IfFailsInGetReturnsEmptyList()
        {
            var issues = new List<FeedItem>();
            issues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.1", Title = "prova"});

            var gitMock = new Mock<IStorageService>();
//            gitMock.Setup(mock => mock.GetRecentIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>())).Throws(new ApplicationException());
            gitMock.Setup(mock => mock.GetBatchWebLinks(It.IsAny<string>(), It.IsAny<IList<string>>())).Throws(new ApplicationException());
            var ghService = this.GetService(gitMock.Object);

            var cleanedIssues = await ghService.RemoveExisting(issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(0, cleanedIssues.Count);
        }

        [TestMethod, TestCategory("RemoveExistingFeeds")]
        public async Task AllTheKeysInTheListAreRequested() {
            var issues = new List<FeedItem>();
            issues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.1", Title = "prova" });
            issues.Add(new FeedItem { Id = "http://localhost/2", Feed = "Http:/127.0.0.1", Title = "prova2" });
            var numKeys = 0;

            var gitMock = new Mock<IStorageService>();
            gitMock.Setup(mock => mock.GetBatchWebLinks(It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Callback( (string org, IList<string> keys) => numKeys = keys.Count)
                .Returns(issues);
            var ghService = this.GetService(gitMock.Object);

            await ghService.RemoveExisting(issues);
            Assert.AreEqual(issues.Count, numKeys);

        }

        [TestMethod, ExpectedException(typeof(ApplicationException)), TestCategory("RemoveExistingFeeds")]
        public async Task IfFromDifferentFeedsRaisesAnException()
        {
            var ghService = new IssueReceiveService();

            var issues = new List<FeedItem>();
            issues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.2", Title = "prova" });
            issues.Add(new FeedItem { Id = "http://localhost/2", Feed = "Http:/127.0.0.1", Title = "prova2" });

            var cleanedIssues = await ghService.RemoveExisting(issues);
        }

        [TestMethod, TestCategory("RemoveExistingFeeds")]
        public async Task IfThePostAlreadyInStorageIsRemovedFromList()
        {
            var organization = "org";
            var repository = "repo";

            List<FeedItem> storageIssues = new List<FeedItem> ();
            storageIssues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.2", Title = "prova" });

            var issues = new List<FeedItem>();
            issues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.1", Title = "prova" });
            issues.Add(new FeedItem { Id = "http://localhost/2", Feed = "Http:/127.0.0.1", Title = "prova2" });

            var gitMock = new Mock<IStorageService>();
            gitMock.Setup(mock => mock.GetBatchWebLinks(It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Returns(storageIssues);
            var ghService = this.GetService(gitMock.Object);

            var cleanedIssues = await ghService.RemoveExisting(issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(1, cleanedIssues.Count);
            Assert.AreNotEqual(issues[0].Title, cleanedIssues[0].Title);
        }

        [TestMethod, TestCategory("RemoveExistingFeeds")]
        public async Task IfThePostIsNotInStorageIsManteinedInList()
        {
            var organization = "org";
            var repository = "repo";

            List<FeedItem> storageIssues = new List<FeedItem>();
            storageIssues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.2", Title = "prova" });

            var issues = new List<FeedItem>();
            issues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.1", Title = "prova" });
            issues.Add(new FeedItem { Id = "http://localhost/2", Feed = "Http:/127.0.0.1", Title = "prova2" });

            var gitMock = new Mock<IStorageService>();
            gitMock.Setup(mock => mock.GetBatchWebLinks(It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Returns(storageIssues);
            var ghService = this.GetService(gitMock.Object);

            var cleanedIssues = await ghService.RemoveExisting(issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(1, cleanedIssues.Count);
            Assert.AreEqual(issues[1].Title, cleanedIssues[0].Title);
        }

        [TestMethod, TestCategory("RemoveExistingFeeds")]
        public async Task IfTheListIsEmptyAnEmptyListIsReturned()
        {
            var organization = "org";
            var repository = "repo";

            List<FeedItem> storageIssues = new List<FeedItem>();
            storageIssues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.1", Title = "prova" });

            var issues = new List<FeedItem>();

            var gitMock = new Mock<IStorageService>();
            gitMock.Setup(mock => mock.GetBatchWebLinks(It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Returns(storageIssues);
            var ghService = this.GetService(gitMock.Object);

            var cleanedIssues = await ghService.RemoveExisting(issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(0, cleanedIssues.Count);
        }


    }
}
