using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using GetImage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace TinnyMagicMiror.Controllers
{
    public class NewsfeedController : Controller
    {
        public IConfiguration _configuration { get; }
        public GetMirrorData _getMirrorData;
        public NewsfeedController(IConfiguration Configuration, GetMirrorData getMirrorData)
        {
            _configuration = Configuration;
            _getMirrorData = getMirrorData;

        }
        public IActionResult GetNewsDisplay()
        {
            if (_getMirrorData.MirrorNewsFeed.Count < 1)
            {
                GetNewsFeed();
            }
            var newsIndex = new Random().Next(0, _getMirrorData.MirrorNewsFeed.Count);

            var news = new { Header = _getMirrorData.MirrorNewsFeed[newsIndex].Header, Title = _getMirrorData.MirrorNewsFeed[newsIndex].Title };
            _getMirrorData.MirrorNewsFeed.RemoveAt(newsIndex);
            if (_getMirrorData.MirrorNewsFeed.Count < 1)
            {
                GetNewsFeed();
            }
            return Json(news);
        }

        public IActionResult GetNewsFeed()
        {
            var rssfeed = _configuration.GetSection("newsfeed").GetSection("feeds").GetChildren();//.First()["url"]
            _getMirrorData.MirrorNewsFeed.Clear();
            _getMirrorData.CurrentIndexNews = 0;
            if (rssfeed == null || rssfeed.Count() < 1)
            {
                return Json("");
            }

            foreach (var item in rssfeed)
            {
                var sourceurl = item["url"];
                var sourcetitle = item["title"];

                _getMirrorData.MirrorNewsFeed.AddRange(PopulateRssFeed(sourceurl, sourcetitle));
            }

            var newsfeed = JsonConvert.SerializeObject(_getMirrorData.MirrorNewsFeed);


            return Json(newsfeed);
        }
        private List<Feeds> PopulateRssFeed(string rssFeedUrl, string sourcetitle)
        {

            List<Feeds> feeds = new List<Feeds>();
            XDocument xDoc = XDocument.Load(rssFeedUrl);
            var titems = xDoc.Descendants("item");
            var items = from x in xDoc.Descendants("item")

                        select new
                        {
                            index = _getMirrorData.MirrorNewsFeed.Count(),
                            header = sourcetitle,
                            title = x.Element("title") != null ? x.Element("title").Value : "",
                            link = x.Element("link") != null ? x.Element("link").Value : "",
                            pubDate = x.Element("pubDate") != null ? x.Element("pubDate").Value : "",
                            description = x.Element("description") != null ? x.Element("description").Value : ""
                        };
            if (items != null)
            {
                feeds.AddRange(items.Select(i => new Feeds
                {
                    Index = i.index,
                    Header = i.header,
                    Title = i.title,
                    Link = i.link,
                    PublishDate = i.pubDate,
                    Description = i.description
                }));
            }
            return feeds;

        }

    }
    public class Feeds
    {
        public int Index { get; set; }
        public string Header { get; set; }
        public string Title { get; set; }

        public string Link { get; set; }
        public string PublishDate { get; set; }
        public string Description { get; set; }
    }
}