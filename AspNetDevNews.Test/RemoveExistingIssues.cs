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
    public class RemoveExistingIssues
    {
        [TestMethod, TestCategory("RemoveExistingIssues")]
        public async Task IfListIsNullReturnEmptyList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                List<Issue> test = null;

                var sut = mock.Create<IssueReceiveService>();

                var issues = await sut.RemoveExisting(test);

                Assert.IsNotNull(issues);
                Assert.AreEqual(0, issues.Count);
            }
        }

        [TestMethod, TestCategory("RemoveExistingIssues")]
        public async Task IfFailsInGetReturnsEmptyList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var issues = new List<Issue>();
                issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 1 });

                mock.Mock<IStorageService>()
                    .Setup(myMock => myMock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                    .Throws(new ApplicationException());

                var sut = mock.Create<IssueReceiveService>();

                var cleanedIssues = await sut.RemoveExisting(issues);
                Assert.IsNotNull(cleanedIssues);
                Assert.AreEqual(0, cleanedIssues.Count);
            }
        }

        [TestMethod, TestCategory("RemoveExistingIssues")]
        public async Task AllTheKeysInTheListAreRequested() {
            using (var mock = AutoMock.GetLoose())
            {
                var issues = new List<Issue>();
                issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 1 });
                issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 2 });
                var numKeys = 0;

                mock.Mock<IStorageService>()
                    .Setup(myMock => myMock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                    .Callback( (string org, string repo, IList<string> keys) => numKeys = keys.Count)
                    .Returns(issues);

                var sut = mock.Create<IssueReceiveService>();

                await sut.RemoveExisting(issues);
                Assert.AreEqual(issues.Count, numKeys);
            }
        }

        [TestMethod, ExpectedException(typeof(ApplicationException)), TestCategory("RemoveExistingIssues")]
        public async Task IfFromDifferentRepositoriesRaisesAnException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var issues = new List<Issue>();
                issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 1 });
                issues.Add(new Issue { Organization = "org", Repository = "repo2", Number = 1 });
                var sut = mock.Create<IssueReceiveService>();

                var cleanedIssues = await sut.RemoveExisting(issues);
            }
        }

        [TestMethod, TestCategory("RemoveExistingIssues")]
        public async Task IfTheIssuesAlreadyInStorageIsRemovedFromList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var organization = "org";
                var repository = "repo";

                List<Issue> storageIssues = new List<Issue>();
                storageIssues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });

                var issues = new List<Issue>();
                issues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });
                issues.Add(new Issue { Organization = organization, Repository = repository, Number = 2 });

                mock.Mock<IStorageService>()
                    .Setup(myMock => myMock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                    .Returns(storageIssues);

                var sut = mock.Create<IssueReceiveService>();

                var cleanedIssues = await sut.RemoveExisting(issues);

                Assert.IsNotNull(cleanedIssues);
                Assert.AreEqual(1, cleanedIssues.Count);
                Assert.AreNotEqual(issues[0].Number, cleanedIssues[0].Number);
            }
        }

        [TestMethod, TestCategory("RemoveExistingIssues")]
        public async Task IfTheIssuesIsNotInStorageIsManteinedInList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var organization = "org";
                var repository = "repo";

                List<Issue> storageIssues = new List<Issue>();
                storageIssues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });

                var issues = new List<Issue>();
                issues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });
                issues.Add(new Issue { Organization = organization, Repository = repository, Number = 2 });

                mock.Mock<IStorageService>()
                    .Setup(myMock => myMock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                    .Returns(storageIssues);

                var sut = mock.Create<IssueReceiveService>();

                var cleanedIssues = await sut.RemoveExisting(issues);
                Assert.IsNotNull(cleanedIssues);
                Assert.AreEqual(1, cleanedIssues.Count);
                Assert.AreEqual(issues[1].Number, cleanedIssues[0].Number);
            }
        }

        [TestMethod, TestCategory("RemoveExistingIssues")]
        public async Task IfTheListIsEmptyAnEmptyListIsReturned()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var organization = "org";
                var repository = "repo";

                List<Issue> storageIssues = new List<Issue>();
                storageIssues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });

                var issues = new List<Issue>();

                mock.Mock<IStorageService>()
                    .Setup(myMock => myMock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                    .Returns(storageIssues);

                var sut = mock.Create<IssueReceiveService>();

                var cleanedIssues = await sut.RemoveExisting(issues);
                Assert.IsNotNull(cleanedIssues);
                Assert.AreEqual(0, cleanedIssues.Count);
            }
        }


    }
}
