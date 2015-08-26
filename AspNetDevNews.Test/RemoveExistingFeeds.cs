using AspNetDevNews.Models;
using AspNetDevNews.Services;
using AspNetDevNews.Services.Feeds;
using AspNetDevNews.Services.Interfaces;
using Autofac.Extras.Moq;
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
        [TestMethod, TestCategory("RemoveExistingFeeds")]
        public async Task IfListIsNullReturnEmptyList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();
                List<FeedItem> test = null;

                var issues = sut.RemoveExisting(test);

                Assert.IsNotNull(issues);
                Assert.AreEqual(0, issues.Count);
            }
        }

        [TestMethod, TestCategory("RemoveExistingFeeds")]
        public async Task IfFailsInGetReturnsEmptyList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var issues = new List<FeedItem>();
                issues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.1", Title = "prova" });

                var gitMock = new Mock<IStorageService>();
                gitMock.Setup(myMock => myMock.GetBatchWebLinks(It.IsAny<string>(), It.IsAny<IList<string>>())).Throws(new ApplicationException());

                var sut = mock.Create<IssueReceiveService>();

                var cleanedIssues = sut.RemoveExisting(issues);
                Assert.IsNotNull(cleanedIssues);
                Assert.AreEqual(0, cleanedIssues.Count);
            }
        }

        [TestMethod, TestCategory("RemoveExistingFeeds")]
        public async Task AllTheKeysInTheListAreRequested() {
            using (var mock = AutoMock.GetLoose())
            {
                var issues = new List<FeedItem>();
                issues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.1", Title = "prova" });
                issues.Add(new FeedItem { Id = "http://localhost/2", Feed = "Http:/127.0.0.1", Title = "prova2" });
                var numKeys = 0;

                mock.Mock<IStorageService>()
                    .Setup(myMock => myMock.GetBatchWebLinks(It.IsAny<string>(), It.IsAny<IList<string>>()))
                    .Callback((string org, IList<string> keys) => numKeys = keys.Count)
                    .Returns(issues);

                var sut = mock.Create<IssueReceiveService>();

                sut.RemoveExisting(issues);
                Assert.AreEqual(issues.Count, numKeys);
            }
        }

        [TestMethod, ExpectedException(typeof(ApplicationException)), TestCategory("RemoveExistingFeeds")]
        public async Task IfFromDifferentFeedsRaisesAnException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var issues = new List<FeedItem>();
                issues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.2", Title = "prova" });
                issues.Add(new FeedItem { Id = "http://localhost/2", Feed = "Http:/127.0.0.1", Title = "prova2" });

                var sut = mock.Create<IssueReceiveService>();

                var cleanedIssues = sut.RemoveExisting(issues);
            }
        }

        [TestMethod, TestCategory("RemoveExistingFeeds")]
        public async Task IfThePostAlreadyInStorageIsRemovedFromList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                List<FeedItem> storageIssues = new List<FeedItem>();
                storageIssues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.2", Title = "prova" });

                var issues = new List<FeedItem>();
                issues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.1", Title = "prova" });
                issues.Add(new FeedItem { Id = "http://localhost/2", Feed = "Http:/127.0.0.1", Title = "prova2" });

                mock.Mock<IStorageService>()
                    .Setup(myMock => myMock.GetBatchWebLinks(It.IsAny<string>(), It.IsAny<IList<string>>()))
                    .Returns(storageIssues);

                var sut = mock.Create<IssueReceiveService>();

                var cleanedIssues = sut.RemoveExisting(issues);

                Assert.IsNotNull(cleanedIssues);
                Assert.AreEqual(1, cleanedIssues.Count);
                Assert.AreNotEqual(issues[0].Title, cleanedIssues[0].Title);
            }
        }

        [TestMethod, TestCategory("RemoveExistingFeeds")]
        public async Task IfThePostIsNotInStorageIsManteinedInList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                List<FeedItem> storageIssues = new List<FeedItem>();
                storageIssues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.2", Title = "prova" });

                var issues = new List<FeedItem>();
                issues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.1", Title = "prova" });
                issues.Add(new FeedItem { Id = "http://localhost/2", Feed = "Http:/127.0.0.1", Title = "prova2" });

                mock.Mock<IStorageService>()
                    .Setup(myMock => myMock.GetBatchWebLinks(It.IsAny<string>(), It.IsAny<IList<string>>()))
                    .Returns(storageIssues);

                var sut = mock.Create<IssueReceiveService>();

                var cleanedIssues = sut.RemoveExisting(issues);

                Assert.IsNotNull(cleanedIssues);
                Assert.AreEqual(1, cleanedIssues.Count);
                Assert.AreEqual(issues[1].Title, cleanedIssues[0].Title);
            }
        }

        [TestMethod, TestCategory("RemoveExistingFeeds")]
        public async Task IfTheListIsEmptyAnEmptyListIsReturned()
        {
            using (var mock = AutoMock.GetLoose())
            {
                List<FeedItem> storageIssues = new List<FeedItem>();
                storageIssues.Add(new FeedItem { Id = "http://localhost", Feed = "Http:/127.0.0.1", Title = "prova" });

                var issues = new List<FeedItem>();

                mock.Mock<IStorageService>()
                    .Setup(myMock => myMock.GetBatchWebLinks(It.IsAny<string>(), It.IsAny<IList<string>>()))
                    .Returns(storageIssues);

                var sut = mock.Create<IssueReceiveService>();

                var cleanedIssues = sut.RemoveExisting(issues);
                Assert.IsNotNull(cleanedIssues);
                Assert.AreEqual(0, cleanedIssues.Count);
            }
        }


    }
}
