using HackerNewsInterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
//using Xunit.Sdk;

namespace HackerTestCases
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var logger = LoggerFactory.Create(options => { }).CreateLogger<HackerNewsInterface.Controllers.HackerNewsController>();
            HackerNewsInterface.Controllers.HackerNewsController controller = new(logger);
            ActionResult<int> maxcount = controller.GetMaxStoryCount();

            Assert.IsNotNull(maxcount);
            Assert.IsInstanceOfType(maxcount.Value, typeof(int));
            Assert.IsTrue(maxcount.Value >= 0);

            ActionResult<int> grabret = controller.GrabRecentArticles("3000");

            Assert.IsNotNull(grabret);
            Assert.IsInstanceOfType(grabret.Value, typeof(int));
            Assert.IsTrue(grabret.Value == 0);

            ActionResult<ConcurrentBag<HackerNews>> grabbag = controller.GetLatestPull();

            Assert.IsNotNull(grabbag);
            Assert.IsNotNull(grabbag.Value);
            Assert.IsInstanceOfType(grabbag.Value, typeof(ConcurrentBag<HackerNews>));

            ConcurrentBag<HackerNews> localbag = grabbag.Value;
            foreach(HackerNews bag in localbag)
            {
                Assert.IsNotNull(bag.ArticleID);
                Assert.IsInstanceOfType(bag.ArticleID, typeof(string));
                Assert.IsTrue(bag.ArticleID.Length > 0);
                Assert.IsNotNull(bag.ArticleTitle);
                Assert.IsInstanceOfType(bag.ArticleTitle, typeof(string));
                Assert.IsTrue(bag.ArticleTitle.Length > 0);
                Assert.IsNotNull(bag.ArticleUrl);
                Assert.IsInstanceOfType(bag.ArticleUrl, typeof(string));
                Assert.IsTrue(bag.ArticleUrl.Length > 0);
            }
        }
    }
}