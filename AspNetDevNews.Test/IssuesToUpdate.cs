﻿using AspNetDevNews.Helpers;
using AspNetDevNews.Models;
using AspNetDevNews.Services;
using AspNetDevNews.Services.AzureTableStorage;
using AspNetDevNews.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;

namespace AspNetDevNews.Test
{
    [TestClass]
    public class IssuesToUpdate
    {
        [TestInitialize]
        public void Initialize()
        {
        }

        [TestCleanup]
        public void CleanUp() {

        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public void IfGitHubIssuesListIsNullReturnEmptyList()
        {
            using (var mock = AutoMock.GetLoose()) {

                var sut = mock.Create<IssueReceiveService>();

                var issues = sut.IssuesToUpdate(null, new List<Models.Issue>());

                Assert.IsNotNull(issues);
                Assert.AreEqual(0,issues.Count);
            }
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public void IfStorageIssuesListIsNullReturnEmptyList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                var issues = sut.IssuesToUpdate(new List<Models.Issue>(), null);

                Assert.IsNotNull(issues);
                Assert.AreEqual(0, issues.Count);
            }
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public void IfHaveDifferentKeysReturnEmptyList()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                var gitHubIssues = new List<Issue>();
                gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1 });

                var storageIssues = new List<Issue>();
                storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 2 });

                var issues = sut.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);
                Assert.IsNotNull(issues);
                Assert.AreEqual(0, issues.Count);
            }
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public void IfSameIssueAndDifferentTitleTheGitHubIssueIsReported()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                var gitHubIssues = new List<Issue>();
                gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Title = "Title 1" });

                var storageIssues = new List<Issue>();
                storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Title = "Title 2" });

                var issues = sut.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);

                Assert.IsNotNull(issues);
                Assert.AreEqual(1,issues.Count);
                Assert.AreEqual(gitHubIssues[0],issues[0]);
            }
        }

        [TestMethod,TestCategory("IssuesToUpdate")]
        public void IfSameIssueAndDifferentUpdateTheGitHubIssueIsReported()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                var gitHubIssues = new List<Issue>();
                gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, UpdatedAt = DateTime.Now });

                var storageIssues = new List<Issue>();
                storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, UpdatedAt = DateTime.Now.AddHours(1) });

                var issues = sut.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);

                Assert.IsNotNull(issues);
                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual(gitHubIssues[0], issues[0]);
            }
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public void IfSameIssueAndDifferentStateTheGitHubIssueIsReported()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                var gitHubIssues = new List<Issue>();
                gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, State = "Open" });

                var storageIssues = new List<Issue>();
                storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, State = "Closed" });

                var issues = sut.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);
                Assert.IsNotNull(issues);
                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual(gitHubIssues[0], issues[0]);
            }
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public void IfSameIssueAndDifferentCommentsTheGitHubIssueIsReported()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                var gitHubIssues = new List<Issue>();
                gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Comments = 0 });

                var storageIssues = new List<Issue>();
                storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Comments = 1 });

                var issues = sut.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);
                Assert.IsNotNull(issues);
                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual(gitHubIssues[0], issues[0]);
            }
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public void IfIssuesAreEqualNothinIsReported()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                var gitHubIssues = new List<Issue>();
                gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Title = "Title 1", UpdatedAt = DateTime.Today, Comments = 1 });

                var storageIssues = new List<Issue>();
                storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Title = "Title 1", UpdatedAt = DateTime.Today, Comments = 1 });

                var issues = sut.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);

                Assert.IsNotNull(issues);
                Assert.AreEqual(0, issues.Count);
            }
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public void OtherFieldsAreIgnored()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<IssueReceiveService>();

                var gitHubIssues = new List<Issue>();
                gitHubIssues.Add(new Issue
                {
                    Organization = "Org",
                    Repository = "Repo",
                    Number = 1,
                    Url = "url1",
                    Labels = new[] { "1" },
                    CreatedAt = DateTime.Now,
                    Body = "Body 1"
                });

                var storageIssues = new List<Issue>();
                storageIssues.Add(new Issue
                {
                    Organization = "Org",
                    Repository = "Repo",
                    Number = 1,
                    Url = "url2",
                    Labels = new[] { "2" },
                    CreatedAt = DateTime.Now.AddHours(1),
                    Body = "Body 2"
                });

                var issues = sut.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);
                Assert.IsNotNull(issues);
                Assert.AreEqual(0, issues.Count);
            }
        }

    }
}
