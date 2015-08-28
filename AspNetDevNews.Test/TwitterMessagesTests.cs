using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AspNetDevNews.Models;

namespace AspNetDevNews.Test
{
    [TestClass]
    public class TwitterMessagesTests
    {
        [TestMethod,TestCategory("Tweets")]
        public void AspNetDocAdded()
        {
            var documento = new GitHubHostedDocument();
            documento.Commit = "5e6f9037bb7acf4411bf9e0320b0c5383692ef42";
            documento.FileName = "aspnet/frameworks/index.rst";
            documento.Organization = "aspnet";
            documento.Repository = "Docs";
            documento.Status = "added";
            documento.TsCommit = new DateTime(2015, 08, 27, 00, 02, 12);
            var messaggio = documento.GetTwitterText();

            Assert.AreEqual("[doc added]: index http://docs.asp.net/en/latest/frameworks/index.html", messaggio);
        }

        [TestMethod, TestCategory("Tweets")]
        public void AspNetDocModified()
        {
            var documento = new GitHubHostedDocument();
            documento.Commit = "5e6f9037bb7acf4411bf9e0320b0c5383692ef42";
            documento.FileName = "aspnet/index.rst";
            documento.Organization = "aspnet";
            documento.Repository = "Docs";
            documento.Status = "modified";
            documento.TsCommit = new DateTime(2015, 08, 27, 00, 02, 12);
            var messaggio = documento.GetTwitterText();

            Assert.AreEqual("[doc update]: index http://docs.asp.net/en/latest/index.html", messaggio);
        }

        [TestMethod, TestCategory("Tweets")]
        public void EntityFrameworkDoc()
        {
            var documento = new GitHubHostedDocument();
            documento.Commit = "c5da04b8e571563eb68b8e4c893351288286f4cf";
            documento.FileName = "docs/getting-started/x-plat/guide.rst";
            documento.Organization = "aspnet";
            documento.Repository = "EntityFramework.Docs";
            documento.Status = "modified";
            documento.TsCommit = new DateTime(2015, 07, 29, 22, 59, 25);
            var messaggio = documento.GetTwitterText();

            Assert.AreEqual("[doc update]: guide http://ef.readthedocs.org/en/latest/getting-started/x-plat/guide.html", messaggio);
        }

        [TestMethod, TestCategory("Tweets")]
        public void DotNetCoreDoc()
        {
            var documento = new GitHubHostedDocument();
            documento.Commit = "f3786b9c554b6d84a88382cf7deea3294a92e61a";
            documento.FileName = "docs/getting-started/installing-core-linux.rst";
            documento.Organization = "dotnet";
            documento.Repository = "core-docs";
            documento.Status = "modified";
            documento.TsCommit = new DateTime(2015, 06, 30, 13, 55, 46);
            var messaggio = documento.GetTwitterText();

            Assert.AreEqual("[doc update]: installing-core-linux http://dotnet.readthedocs.org/en/latest/getting-started/installing-core-linux.html", messaggio);
        }


        [TestMethod, TestCategory("Tweets")]
        public void Blog()
        {
            var feed = new FeedItem();
            feed.Feed = "http://webdevblogs.azurewebsites.net/master.xml";
            feed.Id = "http://www.asp.net/vnext/overview/aspnet-vnext/create-a-web-api-with-mvc-6";
            feed.PublishDate = new DateTime(2015, 08, 27);
            feed.Summary = "Note: An updated version of this article is located here.";
            feed.Title = "Create a Web API in MVC 6";
            var messaggio = feed.GetTwitterText();

            Assert.AreEqual("[blog]: Create a Web API in MVC 6. http://www.asp.net/vnext/overview/aspnet-vnext/create-a-web-api-with-mvc-6", messaggio);
        }

        [TestMethod, TestCategory("Tweets")]
        public void Video()
        {
            var feed = new FeedItem();
            feed.Feed = "http://webdevblogs.azurewebsites.net/master.xml";
            feed.Id = "https://channel9.msdn.com/Shows/Azure-Friday/Azure-API-Management-Policy-Expressions-102-JSON-Web-Tokens";
            feed.PublishDate = new DateTime(2015, 08, 20, 23, 0, 0); 
            feed.Summary = "<p>Scott talks to Vladimir Vinogradsky in this three-part series on Azure API Management Policy Expressions. This second&nbsp;episode&nbsp;talks about how JSON Web Tokens work and&nbsp;shows some of the online tools you'll use to express&nbsp;policies and&nbsp;then apply them with a Policy&nbsp;Definition.&nbsp;</p> <img src=\"http://m.webtrends.com/dcs1wotjh10000w0irc493s0e_6x1g/njs.gif?dcssip=channel9.msdn.com&dcsuri=https://channel9.msdn.com/Niners/Glucose/Posts/RSS&WT.dl=0&WT.entryid=Entry:RSSView:61bc3f20285e4c94a895a4ea009182d5\">";
            feed.Title = "Azure API Management Policy Expressions 102 - JSON Web Tokens";
            var messaggio = feed.GetTwitterText();

            Assert.AreEqual("[video]: Azure API Management Policy Expressions 102 - JSON Web Tokens. https://channel9.msdn.com/Shows/Azure-Friday/Azure-API-Management-Policy-Expressions-102-JSON-Web-Tokens", messaggio);
        }

        [TestMethod, TestCategory("Tweets")]
        public void AspNetIssue()
        {
            var issue = new Issue();
            issue.Body = "Looks like other commands are in sorted order:\r\n\r\n![image](https://cloud.githubusercontent.com/assets/6559784/8552498/536e8fe8-2492-11e5-81f0-f65561a8c72c.png)\r\n";
            issue.Comments = 5;
            issue.CreatedAt = new DateTime(2015, 7, 7, 19, 24, 59);
            issue.Labels = new string[] { "up-for-grabs" };
            issue.Number = 2201;
            issue.Organization = "aspnet";
            issue.Repository = "dnx";
            issue.State = "Open";
            issue.Title = "Put the 'feeds' command help text in sorted order";
            issue.UpdatedAt = new DateTime(2015, 8, 27, 20, 27, 08);
            issue.Url = "https://github.com/aspnet/dnx/issues/2201";
            var messaggio = issue.GetTwitterText();

            Assert.AreEqual("[up-for-grabs]: Put the 'feeds' command help text in sorted order. https://github.com/aspnet/dnx/issues/2201", messaggio);
        }

    }
}
