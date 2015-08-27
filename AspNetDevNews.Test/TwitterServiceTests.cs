using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
using LinqToTwitter;
using System.Threading.Tasks;
using System.Threading;
using AspNetDevNews.Services;
using Autofac;
using AspNetDevNews.Services.Interfaces;
using AspNetDevNews.Helpers;

namespace AspNetDevNews.Test
{
	[TestClass]
	public class TwitterServiceTests
	{
		const string SingleStatusResponse = @"{
		  ""retweeted"":false,
		  ""in_reply_to_screen_name"":null,
		  ""possibly_sensitive"":false,
		  ""retweeted_status"":{
			 ""retweeted"":false,
			 ""in_reply_to_screen_name"":null,
			 ""possibly_sensitive"":false,
			 ""contributors"":null,
			 ""coordinates"":null,
			 ""place"":null,
			 ""user"":{
				""id"":41754227,
				""profile_image_url"":""http:\/\/a0.twimg.com\/profile_images\/565139568\/redshirt_normal.jpg"",
				""url"":""http:\/\/weblogs.asp.net\/scottgu"",
				""created_at"":""Fri May 22 04:39:35 +0000 2009"",
				""followers_count"":57222,
				""default_profile"":true,
				""profile_background_color"":""C0DEED"",
				""lang"":""en"",
				""utc_offset"":-28800,
				""name"":""Scott Guthrie"",
				""profile_background_image_url"":""http:\/\/a0.twimg.com\/images\/themes\/theme1\/bg.png"",
				""location"":""Redmond, WA"",
				""profile_link_color"":""0084B4"",
				""listed_count"":4390,
				""verified"":false,
				""protected"":false,
				""profile_use_background_image"":true,
				""is_translator"":false,
				""following"":false,
				""description"":""I live in Seattle and build a few products for Microsoft"",
				""profile_text_color"":""333333"",
				""statuses_count"":3054,
				""screen_name"":""scottgu"",
				""profile_image_url_https"":""https:\/\/si0.twimg.com\/profile_images\/565139568\/redshirt_normal.jpg"",
				""time_zone"":""Pacific Time (US & Canada)"",
				""profile_background_image_url_https"":""https:\/\/si0.twimg.com\/images\/themes\/theme1\/bg.png"",
				""friends_count"":86,
				""default_profile_image"":false,
				""contributors_enabled"":false,
				""profile_sidebar_border_color"":""C0DEED"",
				""id_str"":""41754227"",
				""geo_enabled"":false,
				""favourites_count"":44,
				""profile_background_tile"":false,
				""notifications"":false,
				""show_all_inline_media"":false,
				""profile_sidebar_fill_color"":""DDEEF6"",
				""follow_request_sent"":false
			 },
			 ""retweet_count"":393,
			 ""id_str"":""184793217231880192"",
			 ""in_reply_to_user_id"":null,
			 ""favorited"":false,
			 ""in_reply_to_status_id_str"":null,
			 ""in_reply_to_status_id"":null,
			 ""source"":""web"",
			 ""created_at"":""Wed Mar 28 00:05:10 +0000 2012"",
			 ""in_reply_to_user_id_str"":null,
			 ""truncated"":false,
			 ""id"":184793217231880192,
			 ""geo"":null,
			 ""text"":""I just blogged about http:\/\/t.co\/YWHGwOq6 MVC, Web API, Razor and Open Source - Now with Contributions: http:\/\/t.co\/qpevLMZd""
		  },
		  ""contributors"":null,
		  ""coordinates"":null,
		  ""place"":null,
		  ""user"":{
			 ""id"":15411837,
			 ""profile_image_url"":""http:\/\/a0.twimg.com\/profile_images\/1728197892\/n536783050_1693444_2739826_normal.jpg"",
			 ""url"":""http:\/\/www.mayosoftware.com"",
			 ""created_at"":""Sun Jul 13 04:35:50 +0000 2008"",
			 ""followers_count"":1102,
			 ""default_profile"":false,
			 ""profile_background_color"":""0099B9"",
			 ""lang"":""en"",
			 ""utc_offset"":-25200,
			 ""name"":""Joe Mayo"",
			 ""profile_background_image_url"":""http:\/\/a0.twimg.com\/profile_background_images\/13330711\/200xColor_2.png"",
			 ""location"":""Denver, CO"",
			 ""profile_link_color"":""0099B9"",
			 ""listed_count"":112,
			 ""verified"":false,
			 ""protected"":false,
			 ""profile_use_background_image"":true,
			 ""is_translator"":false,
			 ""following"":true,
			 ""description"":""Independent .NET Consultant; author of 6 books; Microsoft Visual C# MVP"",
			 ""profile_text_color"":""3C3940"",
			 ""statuses_count"":1906,
			 ""screen_name"":""JoeMayo"",
			 ""profile_image_url_https"":""https:\/\/si0.twimg.com\/profile_images\/1728197892\/n536783050_1693444_2739826_normal.jpg"",
			 ""time_zone"":""Mountain Time (US & Canada)"",
			 ""profile_background_image_url_https"":""https:\/\/si0.twimg.com\/profile_background_images\/13330711\/200xColor_2.png"",
			 ""friends_count"":211,
			 ""default_profile_image"":false,
			 ""contributors_enabled"":false,
			 ""profile_sidebar_border_color"":""5ED4DC"",
			 ""id_str"":""15411837"",
			 ""geo_enabled"":true,
			 ""favourites_count"":44,
			 ""profile_background_tile"":false,
			 ""notifications"":true,
			 ""show_all_inline_media"":false,
			 ""profile_sidebar_fill_color"":""95E8EC"",
			 ""follow_request_sent"":false
		  },
		  ""retweet_count"":393,
		  ""id_str"":""184835136037191681"",
		  ""in_reply_to_user_id"":null,
		  ""favorited"":false,
		  ""in_reply_to_status_id_str"":null,
		  ""in_reply_to_status_id"":null,
		  ""source"":""web"",
		  ""created_at"":""Wed Mar 28 02:51:45 +0000 2012"",
		  ""in_reply_to_user_id_str"":null,
		  ""truncated"":false,
		  ""id"":184835136037191681,
		  ""geo"":null,
		  ""text"":""RT @scottgu: I just blogged about http:\/\/t.co\/YWHGwOq6 MVC, Web API, Razor and Open Source - Now with Contributions: http:\/\/t.co\/qpevLMZd""
	   }";

		const string MediaResponse = @"{
			""media_id"": 521449660083609601,
			""media_id_string"": ""521449660083609601"",
			""size"": 6955,
			""image"": {
				""w"": 100,
				""h"": 100,
				""image_type"": ""image\/png""
			}
		}";

		private static IContainer Container { get; set; }

        [TestInitialize]
		public void Initialize()
		{
                AutoMapperHelper.InitMappings();
                Container = AutoFacHelper.InitAutoFac();
        }

        async Task<TwitterContext> InitializeTwitterContext()
		{
			//await Task.Delay(1);
			var authMock = new Mock<IAuthorizer>();
			var execMock = new Mock<ITwitterExecute>();
			execMock = new Mock<ITwitterExecute>();

			var tcsAuth = new TaskCompletionSource<IAuthorizer>();
			tcsAuth.SetResult(authMock.Object);

			var tcsResponse = new TaskCompletionSource<string>();
			tcsResponse.SetResult(SingleStatusResponse);

			var tcsMedia = new TaskCompletionSource<string>();
			tcsMedia.SetResult(MediaResponse);

			execMock.SetupGet(exec => exec.Authorizer).Returns(authMock.Object);
			execMock.Setup(exec =>
				exec.PostToTwitterAsync<Status>(
					It.IsAny<string>(),
					It.IsAny<IDictionary<string, string>>(),
					It.IsAny<CancellationToken>()))
				.Returns(tcsResponse.Task);
			execMock.Setup(exec =>
				exec.PostMediaAsync(
					It.IsAny<string>(),
					It.IsAny<IDictionary<string, string>>(),
					It.IsAny<byte[]>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(tcsMedia.Task);
			var ctx = new TwitterContext(execMock.Object);
			return ctx;
		}

		//[TestMethod]
		public void MockStorageTable()
		{
			//using (var mock = AutoMock.GetLoose())
			//{
			//    var isCalled = false;

			//    mock.Mock<CloudTable>()
			//        .Setup(myMock => myMock.ExecuteBatchAsync(It.IsAny<TableBatchOperation>()))
			//        .Callback((TableBatchOperation batch) => isCalled = true)
			//        .ReturnsAsync(new List<TableResult>());

			//    var sut = mock.Create<AzureTableStorageService>();

			//    sut.Store(issues: null);
			//}
		}

		[TestMethod]
		public async Task StoringIssuesWithANullListTheCallIsBlocked()
		{
			var tweetMock = await InitializeTwitterContext();

			object[]TSservices = { Container.Resolve<ISettingsService>(),
				Container.Resolve<IStorageService>(),
				Container.Resolve<ISessionLogger>()};

			var svcMock = new Mock<TwitterService>( TSservices );
			svcMock.Protected()
				.Setup<TwitterContext>("GetTwitterContext")
				.Returns(tweetMock);

			await svcMock.Object.Send(links: null);

			svcMock.Protected()
				.Verify<TwitterContext>("GetTwitterContext", Times.Never());
		}

		[TestMethod]
		public async Task StoringIssuesWithAEmptyListTheCallIsBlocked()
		{
			var tweetMock = await InitializeTwitterContext();

			object[] TSservices = { Container.Resolve<ISettingsService>(),
				Container.Resolve<IStorageService>(),
				Container.Resolve<ISessionLogger>()};

			var svcMock = new Mock<TwitterService>(TSservices);
			svcMock.Protected()
				.Setup<TwitterContext>("GetTwitterContext")
				.Returns(tweetMock);

			await svcMock.Object.Send(new List<Models.Issue>());

			svcMock.Protected()
				.Verify<TwitterContext>("GetTwitterContext", Times.Never());
		}

		[TestMethod]
		public async Task StoringIssuesWithAListWithValuesTheCallIsDone()
		{
			var tweetMock = await InitializeTwitterContext();

			object[] TSservices = { Container.Resolve<ISettingsService>(),
				Container.Resolve<IStorageService>(),
				Container.Resolve<ISessionLogger>()};

			var svcMock = new Mock<TwitterService>(TSservices);
			svcMock.Protected()
				.Setup<TwitterContext>("GetTwitterContext")
				.Returns(tweetMock);

			var issues = new List<Models.Issue>();
			var issue = new Models.Issue();
			issue.Body = "body";
			issue.Comments = 1;
			issue.CreatedAt = DateTime.Now;
			issue.Labels = new string[] { "label1", "label2" };
			issue.Number = 101;
			issue.Organization = "org";
			issue.Repository = "repo";
			issue.State = "opened";
			issue.Title = "title";
			issue.UpdatedAt = DateTime.Now.AddDays(1);
			issue.Url = "http://localhost";

			issues.Add(issue);

			await svcMock.Object.Send(issues);

			svcMock.Protected()
				.Verify<TwitterContext>("GetTwitterContext", Times.Once());
		}

		[TestMethod]
		public async Task IfTwoIssuesHaveProducesTheSameOneTheSecondIsSkipped()
		{
            AutoMapperHelper.InitMappings();
            using (var Container = AutoFacHelper.InitAutoFac()) { 

                var tweetMock = await InitializeTwitterContext();

			    object[] TSservices = { Container.Resolve<ISettingsService>(),
				    Container.Resolve<IStorageService>(),
				    Container.Resolve<ISessionLogger>()};

			    var issues = new List<Models.Issue>();
			    var issue1 = new Models.Issue();
			    issue1.Body = "body";
			    issue1.Comments = 1;
			    issue1.CreatedAt = DateTime.Now;
			    issue1.Labels = new string[] { "label1", "label2" };
			    issue1.Number = 101;
			    issue1.Organization = "org";
			    issue1.Repository = "repo";
			    issue1.State = "opened";
			    issue1.Title = "title";
			    issue1.UpdatedAt = DateTime.Now.AddDays(1);
			    issue1.Url = "http://localhost";

			    issues.Add(issue1);

			    var issue2 = new Models.Issue();
			    issue2.Body = "body";
			    issue2.Comments = 1;
			    issue2.CreatedAt = DateTime.Now;
			    issue2.Labels = new string[] { "label1", "label2" };
			    issue2.Number = 101;
			    issue2.Organization = "org2";
			    issue2.Repository = "repo2";
			    issue2.State = "opened";
			    issue2.Title = "title";
			    issue2.UpdatedAt = DateTime.Now.AddDays(1);
			    issue2.Url = "http://localhost";

			    issues.Add(issue2);

			    var svcMock = new Mock<TwitterService>(TSservices);
			    svcMock.Protected()
				    .Setup<TwitterContext>("GetTwitterContext")
				    .Returns(ctx.Object);


			    var issuesSent = await svcMock.Object.Send(issues);

			    svcMock.Protected()
				    .Verify<TwitterContext>("GetTwitterContext", Times.Once());
			    Assert.AreEqual(1, issuesSent.Count);
			    Assert.AreEqual("org", issuesSent[0].Organization);
            }
        }

		readonly string expectedUploadUrl = "https://api.twitter.com/1.1/statuses/update_with_media.json";

		private Mock<TwitterContext> ctx;
		private Mock<ITwitterExecute> execMock;
		private Mock<IRequestProcessor<Status>> statusReqProc;

		readonly byte[] imageBytes = new byte[] { 0xFF };

		string status = "test";
		bool possiblySensitive = true;
		decimal latitude = 37.78215m;
		ulong inReplyToStatusID = 23030327348932ul;

		[TestMethod]
		public async Task IfRaiseExceptionSendingIsSkippedAndProcessingContinue()
		{
			statusReqProc = new Mock<IRequestProcessor<Status>>();
			statusReqProc.Setup(reqProc => reqProc.ProcessResults(It.IsAny<string>()))
			.Returns(new List<Status> { new Status { Text = "Test" } });

			execMock = new Mock<ITwitterExecute>();
			var authMock = new Mock<IAuthorizer>();
			var tcsResponse = new TaskCompletionSource<string>();
			tcsResponse.SetResult(SingleStatusResponse);
			execMock = new Mock<ITwitterExecute>();
			execMock.SetupGet(exec => exec.Authorizer).Returns(authMock.Object);
			execMock.Setup(
				exec => exec.PostMediaAsync(
					It.IsAny<string>(),
					It.IsAny<Dictionary<string, string>>(),
					It.IsAny<byte[]>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
					.Returns(tcsResponse.Task);

			ctx = new Mock<TwitterContext>(execMock.Object);

			object[] TSservices = { Container.Resolve<ISettingsService>(),
				Container.Resolve<IStorageService>(),
				Container.Resolve<ISessionLogger>()};

			var issues = new List<Models.Issue>();
			var issue1 = new Models.Issue();
			issue1.Body = "body";
			issue1.Comments = 1;
			issue1.CreatedAt = DateTime.Now;
			issue1.Labels = new string[] { "label1", "label2" };
			issue1.Number = 101;
			issue1.Organization = "org";
			issue1.Repository = "repo";
			issue1.State = "opened";
			issue1.Title = "title";
			issue1.UpdatedAt = DateTime.Now.AddDays(1);
			issue1.Url = "http://localhost";

			issues.Add(issue1);

			var issue2 = new Models.Issue();
			issue2.Body = "body2";
			issue2.Comments = 1;
			issue2.CreatedAt = DateTime.Now;
			issue2.Labels = new string[] { "label1", "label2" };
			issue2.Number = 101;
			issue2.Organization = "org2";
			issue2.Repository = "repo2";
			issue2.State = "opened";
			issue2.Title = "title";
			issue2.UpdatedAt = DateTime.Now.AddDays(1);
			issue2.Url = "http://localhost";

			issues.Add(issue2);

			var issue3 = new Models.Issue();
			issue3.Body = "body3";
			issue3.Comments = 1;
			issue3.CreatedAt = DateTime.Now;
			issue3.Labels = new string[] { "label3", "label2" };
			issue3.Number = 101;
			issue3.Organization = "org2";
			issue3.Repository = "repo2";
			issue3.State = "opened";
			issue3.Title = "title";
			issue3.UpdatedAt = DateTime.Now.AddDays(1);
			issue3.Url = "http://localhost";

			issues.Add(issue3);

			var message = issue2.GetTwitterText();

			ctx.Setup(mock => mock.TweetAsync(It.IsAny<string>()))
				.ThrowsAsync(new InvalidOperationException());


			var svcMock = new Mock<TwitterService>(TSservices);
			svcMock.Protected()
				.Setup<TwitterContext>("GetTwitterContext")
				.Returns(ctx.Object);


			var issuesSent = await svcMock.Object.Send(issues);

			svcMock.Protected()
				.Verify<TwitterContext>("GetTwitterContext", Times.Once());
			Assert.AreEqual(1, issuesSent.Count);
			Assert.AreEqual("org", issuesSent[0].Organization);
		}


	}
}
