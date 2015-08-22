using AspNetDevNews.Models;
using AspNetDevNews.Services;
using AspNetDevNews.Services.Interfaces;
using AspNetDevNews.Test.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Test
{
    [TestClass]
    public class RemoveExisting
    {
        private IssueReceiveService GetService(IStorageService storageService)
        {
            var ghService = new IssueReceiveService(
                gitHubService: new DummyGitHubService(),
                storageService: storageService,
                settingsService: new OneDaySettings(),
                twitterService: new DummyTwitterService());
            return ghService;
        }

        private IssueReceiveService GetService()
        {
            var dummyStorageService = new DummyStorageService();
            dummyStorageService.Existing = new List<Issue>();

            return GetService(dummyStorageService);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("RemoveExisting")]
        public async Task IfListIsNullReturnEmptyList()
        {
            var ghService = this.GetService();
            var issues = await ghService.RecentGitHubIssues("", ".");
        }

        [TestMethod, TestCategory("RemoveExisting")]
        public async Task IfFailsInGetReturnsEmptyList()
        {
            var failingStorage = new FailingInGetStorageService();
            var ghService = this.GetService(failingStorage);

            var issues = new List<Issue>();
            issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 1});

            var cleanedIssues = await ghService.RemoveExisting(issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(0, cleanedIssues.Count);
        }

        [TestMethod, ExpectedException(typeof(ApplicationException)), TestCategory("RemoveExisting")]
        public async Task IfFromDifferentRepositoriesRaisesAnException()
        {
            var failingStorage = new DummyStorageService();
            var ghService = this.GetService(failingStorage);

            var issues = new List<Issue>();
            issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 1 });
            issues.Add(new Issue { Organization = "org", Repository = "repo2", Number = 1 });

            var cleanedIssues = await ghService.RemoveExisting(issues);
        }

        [TestMethod, TestCategory("RemoveExisting")]
        public async Task IfTheIssuesAlreadyInStorageIsRemovedFromList()
        {
            var dummyStorage = new DummyStorageService();
            var ghService = this.GetService(dummyStorage);
            var organization = "org";
            var repository = "repo";

            dummyStorage.Existing.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });

            var issues = new List<Issue>();
            issues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });
            issues.Add(new Issue { Organization = organization, Repository = repository, Number = 2 });

            var cleanedIssues = await ghService.RemoveExisting(issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(1, cleanedIssues.Count);
            Assert.AreNotEqual(issues[0].Number, cleanedIssues[0].Number);
        }

        [TestMethod, TestCategory("RemoveExisting")]
        public async Task IfTheIssuesIsNotInStorageIsRemovedFromList()
        {
            var dummyStorage = new DummyStorageService();
            var ghService = this.GetService(dummyStorage);
            var organization = "org";
            var repository = "repo";

            dummyStorage.Existing.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });

            var issues = new List<Issue>();
            issues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });
            issues.Add(new Issue { Organization = organization, Repository = repository, Number = 2 });

            var cleanedIssues = await ghService.RemoveExisting(issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(1, cleanedIssues.Count);
            Assert.AreEqual(issues[1].Number, cleanedIssues[0].Number);
        }

        [TestMethod, TestCategory("RemoveExisting")]
        public async Task IfTheListIsEmptyAnEmptyListIsReturned()
        {
            var dummyStorage = new DummyStorageService();
            var ghService = this.GetService(dummyStorage);
            var organization = "org";
            var repository = "repo";

            dummyStorage.Existing.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });

            var issues = new List<Issue>();

            var cleanedIssues = await ghService.RemoveExisting(issues);
            Assert.IsNotNull(cleanedIssues);
            Assert.AreEqual(0, cleanedIssues.Count);
        }


    }
}
