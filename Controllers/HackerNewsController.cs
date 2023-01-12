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

            /*
                Simple thread class to make the fetching of the articles multi threaded.
                I went with using a ConcurrentBag object since that object is thread safe.
             */
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

        /*
            This gets the max article count.
         */
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

        /*
            This was the intial test for just getting the articles, I was curious how long it would take
            and it took a very long time for it to finish and made my tiny laptop very mad.
            I would then see if I could multithread it so that I could make it do a bunch of get calls at once
            this had better results and I could fetch 1000 articles every 2 minutes or so but that was still not fast
            enough to make it part of the final solution.
         */
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


        /*
            This is where I tried to see if I could fetch all the articles and 
            keep a local copy of just the data that I wanted. This worked but it 
            took such a long time that I decided against it and went with limiting the pull.
            if this hacker api had a better api call that would let you grab sets of articles it
            could be a faster way of interacting with it but with only being able to get 1 article
            at a time its just too much of a burden to do.
         */
        [HttpGet]
        [Route("[action]")]
        public ActionResult<int> FillOutCollection()
        {
            try
            {
                List<Thread> threadslist = new List<Thread>();
                List<FetchingDataThread> fetchingthreadslist = new List<FetchingDataThread>();
                try
                {
                    for (int v = 1; v <= maxStories; v += maxStories / 666)
                    {

                        FetchingDataThread fdt = new FetchingDataThread(v, v + (maxStories / 666), _logger);
                        fetchingthreadslist.Add(fdt);
                        Thread t = new Thread(fdt.ThreadProc);
                        t.Start();
                        threadslist.Add(t);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, $"Thread Creation Loop Error {ex.Message}");
                    return 3;
                }
                bool stillrunning = true;
                int index = 0;
                try
                {
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
                } catch(Exception ex) 
                {
                    _logger.Log(LogLevel.Error, $"Waiting Loop Error {ex.Message}");
                    return 2;
                }
            } catch(Exception ex) 
            {
                _logger.Log(LogLevel.Error, $"Error {ex.Message}");
                return 1;
            }
            return 0;
        }

        /*
            This was created so that I could have it fetch the articles durring the initial page load.
            Then on the actual page anytime it is refreshed we are just getting a copy of what was already
            fetched reducing the number of get calls happening behind the scences. 
         */
        [HttpGet]
        [Route("[action]")]
        public ActionResult<ConcurrentBag<HackerNews>> GetLatestPull()
        {
            return hackerArticleBag;
        }

        /*
            This is what handles fetching all the articles we want, we can set a limit on the count,
            and it fetches from the max article backwards.
         */
        [HttpGet("GrabRecentArticles/{count?}")]
        [Route("[action]")]
        public ActionResult<int> GrabRecentArticles(string count = "100")
        {
            try
            {
                this.GetMaxStoryCount();
            } catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Max Story Count Error {ex.Message}");
                return 4;
            }
            try
            {
                int startdex = maxStories - int.Parse(count);
                int enddex = maxStories;
                int fetchsize = 20;//int.Parse(count);
                List<Thread> threadslist = new List<Thread>();
                List<FetchingDataThread> fetchingthreadslist = new List<FetchingDataThread>();
                try
                {
                    for (int v = startdex; v <= enddex;)
                    {
                        FetchingDataThread fdt = new FetchingDataThread(v, v + fetchsize, _logger);
                        v += fetchsize;
                        fetchingthreadslist.Add(fdt);
                        Thread t = new Thread(fdt.ThreadProc);
                        t.Start();
                        threadslist.Add(t);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, $"Thread Creation Loop Error {ex.Message}");
                    return 3;
                }
                bool stillrunning = true;
                int index = 0;

                try
                {
                    while (stillrunning)
                    {
                        Thread.Sleep(500);

                        _logger.Log(LogLevel.Information, $"Current Fetching Thread[{index}] of [{fetchingthreadslist.Count}] We are Checking [{fetchingthreadslist[index].currentindex}] state [{fetchingthreadslist[index].finished}]");

                        Console.WriteLine(fetchingthreadslist[index].currentindex);
                        if (fetchingthreadslist[index].finished)
                        {
                            index++;
                            if (index == fetchingthreadslist.Count)
                                stillrunning = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, $"Waiting Loop Error {ex.Message}");
                    return 2;
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error {ex.Message}");
                return 1;
            }
            return 0;
        }

        /*
            This is my json object for loading in the data from the api get request.
         */
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
