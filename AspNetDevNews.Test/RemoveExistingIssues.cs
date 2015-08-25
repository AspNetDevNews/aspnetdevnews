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
    public class RemoveExistingIssues
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

        [TestMethod, TestCategory("RemoveExistingIssues")]
        public async Task IfListIsNullReturnEmptyList()
        {
            var ghService = new IssueReceiveService();
            List<Issue> test = null;

            var issues = await ghService.RemoveExisting(test);

            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod, TestCategory("RemoveExistingIssues")]
        public async Task IfFailsInGetReturnsEmptyList()
        {
            //var failingStorage = new FailingInGetStorageService();
            //var ghService = this.GetService(failingStorage);

            var issues = new List<Issue>();
            issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 1});

            var gitMock = new Mock<IStorageService>();
            gitMock.Setup(mock => mock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                //            gitMock.Setup(mock => mock.GetRecentIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
                .Throws(new ApplicationException());
            var ghService = this.GetService(gitMock.Object);

            var cleanedIssues = await ghService.RemoveExisting(issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(0, cleanedIssues.Count);
        }

        [TestMethod, TestCategory("RemoveExistingIssues")]
        public async Task AllTheKeysInTheListAreRequested() {
            var issues = new List<Issue>();
            issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 1 });
            issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 2 });
            var numKeys = 0;

            var gitMock = new Mock<IStorageService>();
            gitMock.Setup(mock => mock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Callback( (string org, string repo, IList<string> keys) => numKeys = keys.Count)
                .Returns(issues);
            var ghService = this.GetService(gitMock.Object);

            await ghService.RemoveExisting(issues);
            Assert.AreEqual(issues.Count, numKeys);

        }

        [TestMethod, ExpectedException(typeof(ApplicationException)), TestCategory("RemoveExistingIssues")]
        public async Task IfFromDifferentRepositoriesRaisesAnException()
        {
            var ghService = new IssueReceiveService();

            var issues = new List<Issue>();
            issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 1 });
            issues.Add(new Issue { Organization = "org", Repository = "repo2", Number = 1 });

            var cleanedIssues = await ghService.RemoveExisting(issues);
        }

        [TestMethod, TestCategory("RemoveExistingIssues")]
        public async Task IfTheIssuesAlreadyInStorageIsRemovedFromList()
        {
            //var dummyStorage = new DummyStorageService();
            //var ghService = this.GetService(dummyStorage);
            var organization = "org";
            var repository = "repo";

            List<Issue> storageIssues = new List<Issue>();
            storageIssues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });

            var issues = new List<Issue>();
            issues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });
            issues.Add(new Issue { Organization = organization, Repository = repository, Number = 2 });

            var gitMock = new Mock<IStorageService>();
            gitMock.Setup(mock => mock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Returns(storageIssues);
            var ghService = this.GetService(gitMock.Object);

            var cleanedIssues = await ghService.RemoveExisting(issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(1, cleanedIssues.Count);
            Assert.AreNotEqual(issues[0].Number, cleanedIssues[0].Number);
        }

        [TestMethod, TestCategory("RemoveExistingIssues")]
        public async Task IfTheIssuesIsNotInStorageIsManteinedInList()
        {
            //var dummyStorage = new DummyStorageService();
            //var ghService = this.GetService(dummyStorage);
            var organization = "org";
            var repository = "repo";

            List<Issue> storageIssues = new List<Issue>();
            storageIssues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });

            var issues = new List<Issue>();
            issues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });
            issues.Add(new Issue { Organization = organization, Repository = repository, Number = 2 });

            var gitMock = new Mock<IStorageService>();
            gitMock.Setup(mock => mock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Returns(storageIssues);
            var ghService = this.GetService(gitMock.Object);

            var cleanedIssues = await ghService.RemoveExisting(issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(1, cleanedIssues.Count);
            Assert.AreEqual(issues[1].Number, cleanedIssues[0].Number);
        }

        [TestMethod, TestCategory("RemoveExistingIssues")]
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

            var cleanedIssues = await ghService.RemoveExisting(issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(0, cleanedIssues.Count);
        }


    }
}
