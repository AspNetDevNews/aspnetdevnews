using AspNetDevNews.Helpers;
using AspNetDevNews.Models;
using AspNetDevNews.Services;
using AspNetDevNews.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;

namespace AspNetDevNews.Test
{
    [TestClass]
    public class CheckInStorage
    {
        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("CheckInStorage")]
        public async Task IfOrganizationIsNullRaisesAnException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                List<Issue> test = new List<Issue>();

                var issues = sut.CheckInStorage(null, ".", test);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("CheckInStorage")]
        public async Task IfOrganizationIsEmptyRaisesAnException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();
                List<Issue> test = new List<Issue>();

                var issues = sut.CheckInStorage("", ".", test);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("CheckInStorage")]
        public async Task IfRepositoryIsEmptyRaisesAnException() {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                List<Issue> test = new List<Issue>();

                var issues = sut.CheckInStorage(".", "", test);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException)), TestCategory("CheckInStorage")]
        public async Task IfRepositoryIsNullRaisesAnException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                List<Issue> test = new List<Issue>();

                var issues = sut.CheckInStorage(".", null, test);
            }
        }

        [TestMethod, TestCategory("CheckInStorage")]
        public async Task IfIssuesListIsNullReturnEmptyList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                List<Issue> test = new List<Issue>();

                var issues = sut.CheckInStorage(".", ".", null);
                Assert.IsNotNull(issues);
                Assert.AreEqual(0, issues.Count);
            }
        }

        [TestMethod, TestCategory("CheckInStorage")]
        public async Task IfIssuesListIsEmptyReturnEmptyList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                List<Issue> test = new List<Issue>();

                var issues = sut.CheckInStorage(".", ".", test);
                Assert.IsNotNull(issues);
                Assert.AreEqual(0, issues.Count);
            }
        }

        [TestMethod, TestCategory("CheckInStorage")]
        public async Task IfTheListIsEmptyAnEmptyListIsReturned()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                var organization = "org";
                var repository = "repo";

                List<Issue> storageIssues = new List<Issue>();
                storageIssues.Add(new Issue { Organization = organization, Repository = repository, Number = 1 });

                var issues = new List<Issue>();

                var stgMock = new Mock<IStorageService>();
                stgMock.Setup(myMock => myMock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                    .Returns(storageIssues);


                mock.Provide<IStorageService>(stgMock.Object);

                var cleanedIssues = sut.CheckInStorage(".", ".", issues);
                Assert.IsNotNull(cleanedIssues);
                Assert.AreEqual(0, cleanedIssues.Count);
            }
        }

        [TestMethod, TestCategory("CheckInStorage")]
        public async Task AllTheKeysInTheListAreSearchedInTheStorage()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var issues = new List<Issue>();
                issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 1 });
                issues.Add(new Issue { Organization = "org", Repository = "repo", Number = 2 });
                var numKeys = 0;

                mock.Mock<IStorageService>()
                    .Setup(myMock => myMock.GetBatchIssues(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                    .Callback((string org, string repo, IList<string> keys) => numKeys = keys.Count)
                    .Returns(issues);

                var sut = mock.Create<IssueReceiveService>();

                var check = sut.CheckInStorage("org", "repo", issues);
                Assert.AreEqual(issues.Count, numKeys);
            }
        }

        [TestMethod, ExpectedException(typeof(ApplicationException)), TestCategory("CheckInStorage")]
        public async Task IfFromRepositoriesDifferentFromParametersRaisesAnException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var issues = new List<Issue>();
                issues.Add(new Issue { Organization = "org", Repository = "repo2", Number = 1 });
                issues.Add(new Issue { Organization = "org", Repository = "repo2", Number = 2 });

                var sut = mock.Create<IssueReceiveService>();

                var cleanedIssues = sut.CheckInStorage("org", "repo", issues);
            }
        }

        [TestMethod, TestCategory("CheckInStorage")]
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

                var cleanedIssues = sut.CheckInStorage("org", "repo", issues);
                Assert.IsNotNull(cleanedIssues);
                Assert.AreEqual(0, cleanedIssues.Count);
            }
        }

    }
}
