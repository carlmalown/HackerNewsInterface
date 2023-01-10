using log4net;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit.Sdk;

namespace HackerTestCases
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var logger = LoggerFactory.Create(options => { }).CreateLogger<HackerNewsInterface.Controllers.HackerNewsController>();
            //Logger<HackerNewsInterface.Controllers.HackerNewsController> logger = new Logger<>();
            HackerNewsInterface.Controllers.HackerNewsController controller = new(logger);
            controller.GetMaxStoryCount();
            //controller.FillOutCollection();
            controller.GrabRecentArticles("300");
        }
    }
}