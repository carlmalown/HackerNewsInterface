namespace HackerNewsInterface
{
    public class HackerNews
    {
        public string? ArticleID { get; set; }
        public string? ArticleTitle { get; set; }
        public string? ArticleUrl { get; set; }

        public HackerNews(string articleID, string? articleTitle, string? articleUrl)
        {
            ArticleID = articleID;
            ArticleTitle = articleTitle;
            ArticleUrl = articleUrl;
        }
    }
}