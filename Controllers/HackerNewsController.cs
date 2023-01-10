using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace HackerNewsInterface.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HackerNewsController : ControllerBase
    {
        //private
        private int maxStories { get; set; } = 0;

        private readonly ILogger<HackerNewsController> _logger;

        private static ConcurrentBag<HackerNews> hackerArticleBag = new ConcurrentBag<HackerNews>();

        private class FetchingDataThread 
        {
            private ILogger<HackerNewsController> _logger;

            private int indexStx { get; set; }
            private int indexEtx { get; set; }
            public bool finished { get; set; }
            public int currentindex { get; set; }

            public FetchingDataThread(int indexStx, int indexEtx, ILogger<HackerNewsController> logger)
            {
                this.indexStx = indexStx;
                this.indexEtx = indexEtx;
                this.finished = false;
                _logger = logger;
            }

            public void ThreadProc()
            {
                for (int i = indexStx; i <= indexEtx; ++i)
                {
                    this.currentindex = i;
                    var client = new RestClient($"https://hacker-news.firebaseio.com/v0/item/{i}.json");
                    client.Options.MaxTimeout = -1;
                    var request = new RestRequest();
                    RestResponse response = client.Execute(request);


                    if (response == null || response.Content == null)
                        continue;

                    Rootobject r = JsonConvert.DeserializeObject<Rootobject>(response.Content) ?? new Rootobject();
                    if (r == null || r.type == null || ( (!"story".Equals(r.type) ) || r.url == null || r.url.Length <=1) )
                        continue;

                    _logger.Log(LogLevel.Information, $"index {i}, id {r.id}, title {r.title}, url {r.url}");
                    hackerArticleBag.Add(new HackerNews(r.id, r.title, r.url));
                }
                this.finished = true;
            }
        }

        public HackerNewsController(ILogger<HackerNewsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            return "test";
        }

        [HttpGet]
        [Route("[action]")]
        public ActionResult<ConcurrentBag<HackerNews>> GetArticleBag()
        {
            return hackerArticleBag;
        }
        [HttpGet]
        [Route("[action]")]
        public ActionResult<int> GetMaxStoryCount()
        {
            var client = new RestClient("https://hacker-news.firebaseio.com/v0/maxitem.json");
            client.Options.MaxTimeout = -1;
            var request = new RestRequest();
            RestResponse response = client.Execute(request);
            if (response.Content != null)
                this.maxStories = int.Parse(response.Content);

            return maxStories;
        }
        [HttpGet]
        [Route("[action]")]
        public ActionResult<int> GetNewestStories()
        {
            List<HackerNews> hackerArticleCollection = new List<HackerNews>();

            for (int i = 1; i <= this.maxStories; ++i)
            {
                var client = new RestClient($"https://hacker-news.firebaseio.com/v0/item/{i}.json");
                client.Options.MaxTimeout = -1;
                var request = new RestRequest();
                RestResponse response = client.Execute(request);

                if (response.Content == null)
                    continue;

                Rootobject r = JsonConvert.DeserializeObject<Rootobject>(response.Content);
                if (!r.type.Equals("story") && r.url == null)
                    continue;
                hackerArticleCollection.Add(new HackerNews(r.id, r.title, r.url));
            }
            return 0;
        }

        [HttpGet]
        [Route("[action]")]
        public ActionResult<int> FillOutCollection()
        {
            List<Thread> threadslist = new List<Thread>();
            List<FetchingDataThread> fetchingthreadslist = new List<FetchingDataThread>();
            for (int v = 1; v <= maxStories; v += maxStories / 666) {

                FetchingDataThread fdt = new FetchingDataThread(v, v+(maxStories / 666), _logger);
                fetchingthreadslist.Add(fdt);
                Thread t = new Thread(fdt.ThreadProc);
                t.Start();
                threadslist.Add(t);
                break;
            }

            bool stillrunning = true;
            int index = 0;
            while (stillrunning)
            {
                Thread.Sleep(60000);

                _logger.Log(LogLevel.Information, $"Current Fetching Thread We are Checking [{fetchingthreadslist[index].currentindex}] state [{fetchingthreadslist[index].finished}]");
                Console.WriteLine(fetchingthreadslist[index].currentindex);
                if (fetchingthreadslist[index].finished)
                {
                    index++;
                    if (index == fetchingthreadslist.Count)
                        stillrunning = false;
                }
            }
            return 0;
        }
        [HttpGet]
        [Route("[action]")]
        public ActionResult<ConcurrentBag<HackerNews>> GetLatestPull()
        {
            return hackerArticleBag;
        }
        [HttpGet("GrabRecentArticles/{count?}")]
        [Route("[action]")]
        public ActionResult<int> GrabRecentArticles(string count = "100")
        {
            int startdex = maxStories - int.Parse(count);
            int enddex = maxStories;
            int fetchsize = int.Parse(count);
            List<Thread> threadslist = new List<Thread>();
            List<FetchingDataThread> fetchingthreadslist = new List<FetchingDataThread>();
            for (int v = startdex; v <= enddex;)
            {
                FetchingDataThread fdt = new FetchingDataThread(v, v+=fetchsize, _logger);
                fetchingthreadslist.Add(fdt);
                Thread t = new Thread(fdt.ThreadProc);
                t.Start();
                threadslist.Add(t);
            }

            bool stillrunning = true;
            int index = 0;
            while (stillrunning)
            {
                Thread.Sleep(500);

                _logger.Log(LogLevel.Information, $"Current Fetching Thread We are Checking [{fetchingthreadslist[index].currentindex}] state [{fetchingthreadslist[index].finished}]");

                Console.WriteLine(fetchingthreadslist[index].currentindex);
                if (fetchingthreadslist[index].finished)
                {
                    index++;
                    if (index == fetchingthreadslist.Count)
                        stillrunning = false;
                }
            }
            return 0;
        }


        private class Rootobject
        {
            //public string by { get; set; }
            //public int descendants { get; set; }
            public string? id { get; set; }
            //public int[] kids { get; set; }
            //public int score { get; set; }
            //public int time { get; set; }
            public string? title { get; set; }
            public string? type { get; set; }
            public string? url { get; set; }
        }
    }
}
