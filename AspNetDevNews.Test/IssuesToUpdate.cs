﻿using AspNetDevNews.Models;
using AspNetDevNews.Services;
using AspNetDevNews.Services.AzureTableStorage;
using AspNetDevNews.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetDevNews.Test
{
    [TestClass]
    public class IssuesToUpdate
    {
        private IssueReceiveService GetService(IGitHubService gitHubService)
        {
            var ghService = new IssueReceiveService(
                gitHubService: new GitHubService(),
                storageService: new AzureTableStorageService(),
                settingsService: new SettingsService(),
                twitterService: new TwitterService());
            return ghService;
        }

        [TestInitialize]
        public void Initialize() {
        }


        [TestCleanup]
        public void CleanUp() {

        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public async Task IfGitHubIssuesListIsNullReturnEmptyList()
        {
            var ghService = new IssueReceiveService();

            var issues = await ghService.IssuesToUpdate(null, new List<Models.Issue>());

            Assert.IsNotNull(issues);
            Assert.AreEqual(0,issues.Count);
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public async Task IfStorageIssuesListIsNullReturnEmptyList()
        {
            var ghService = new IssueReceiveService();

            var issues = await ghService.IssuesToUpdate(new List<Models.Issue>(), null);

            Assert.IsNotNull(issues);
            Assert.AreEqual(0,issues.Count);
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public async Task IfHaveDifferentKeysReturnEmptyList()
        {
            var gitHubIssues = new List<Issue>();
            gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1 });

            var storageIssues = new List<Issue>();
            storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 2 });

            var ghService = new IssueReceiveService();

            var issues = await ghService.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);
            Assert.IsNotNull(issues);
            Assert.AreEqual(0,issues.Count);
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public async Task IfSameIssueAndDifferentTitleTheGitHubIssueIsReported()
        {
            //var dummyGutHubService = new DummyGitHubService();
            //var ghService = this.GetService(dummyGutHubService);

            var gitHubIssues = new List<Issue>();
            gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Title = "Title 1" });

            var storageIssues = new List<Issue>();
            storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Title = "Title 2" });

            var ghService = new IssueReceiveService();

            var issues = await ghService.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);

            Assert.IsNotNull(issues);
            Assert.AreEqual(1,issues.Count);
            Assert.AreEqual(gitHubIssues[0],issues[0]);
        }

        [TestMethod,TestCategory("IssuesToUpdate")]
        public async Task IfSameIssueAndDifferentUpdateTheGitHubIssueIsReported()
        {
            //var dummyGutHubService = new DummyGitHubService();
            //var ghService = this.GetService(dummyGutHubService);

            var gitHubIssues = new List<Issue>();
            gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, UpdatedAt = DateTime.Now });

            var storageIssues = new List<Issue>();
            storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, UpdatedAt = DateTime.Now.AddHours(1) });

            var ghService = new IssueReceiveService();

            var issues = await ghService.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);

            Assert.IsNotNull(issues);
            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual(gitHubIssues[0], issues[0]);
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public async Task IfSameIssueAndDifferentStateTheGitHubIssueIsReported()
        {
            //var dummyGutHubService = new DummyGitHubService();
            //var ghService = this.GetService(dummyGutHubService);

            var gitHubIssues = new List<Issue>();
            gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, State = "Open" });

            var storageIssues = new List<Issue>();
            storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, State = "Closed" });

            var ghService = new IssueReceiveService();

            var issues = await ghService.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);
            Assert.IsNotNull(issues);
            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual(gitHubIssues[0], issues[0]);
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public async Task IfSameIssueAndDifferentCommentsTheGitHubIssueIsReported()
        {
            //var dummyGutHubService = new DummyGitHubService();
            //var ghService = this.GetService(dummyGutHubService);

            var gitHubIssues = new List<Issue>();
            gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Comments = 0 });

            var storageIssues = new List<Issue>();
            storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Comments = 1 });

            var ghService = new IssueReceiveService();

            var issues = await ghService.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);
            Assert.IsNotNull(issues);
            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual(gitHubIssues[0], issues[0]);
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public async Task IfIssuesAreEqualNothinIsReported()
        {
            //var dummyGutHubService = new DummyGitHubService();
            //var ghService = this.GetService(dummyGutHubService);

            var gitHubIssues = new List<Issue>();
            gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Title = "Title 1", UpdatedAt = DateTime.Today, Comments = 1 });

            var storageIssues = new List<Issue>();
            storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Title = "Title 1", UpdatedAt = DateTime.Today, Comments = 1 });

            var ghService = new IssueReceiveService();

            var issues = await ghService.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);

            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod, TestCategory("IssuesToUpdate")]
        public async Task OtherFieldsAreIgnored()
        {
            //var dummyGutHubService = new DummyGitHubService();
            //var ghService = this.GetService(dummyGutHubService);

            var gitHubIssues = new List<Issue>();
            gitHubIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Url = "url1", Labels = new [] { "1" }, CreatedAt = DateTime.Now,
                Body = "Body 1"});

            var storageIssues = new List<Issue>();
            storageIssues.Add(new Issue { Organization = "Org", Repository = "Repo", Number = 1, Url = "url2", Labels = new[] { "2" }, CreatedAt = DateTime.Now.AddHours(1),
                Body = "Body 2"});

            var ghService = new IssueReceiveService();

            var issues = await ghService.IssuesToUpdate(issues: gitHubIssues, lastStored: storageIssues);
            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count);
        }

    }
}