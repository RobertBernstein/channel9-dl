using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace channel9_dl
{
    public class HtmlScraper
    {
        public async Task<string> GetWebResource(Uri url)
        {
            using (var client = new HttpClient())
            {
                return await client.GetStringAsync(url);
            }
        }


        // scraping the page with regex worked best
        public Uri GetListOfMediaFromChannel9PageWithRegex(string html)
        {
            var listofAllUrls = LinkFinder.Find(html);

            var filteredList = listofAllUrls.Where(n => n.Text == "High Quality MP4");

            if (filteredList.Count() != 0)
            {
                return new Uri(filteredList.FirstOrDefault().Href);
            }

            filteredList = listofAllUrls.Where(n => n.Text == "Mid Quality MP4");

            if (filteredList.Count() != 0)
            {
                return new Uri(filteredList.FirstOrDefault().Href);
            }

            filteredList = listofAllUrls.Where(n => n.Text == "Low Quality MP4");

            if (filteredList.Count() != 0)
            {
                return new Uri(filteredList.FirstOrDefault().Href);
            }

            return null;
        }

        // this seemed like the correct way to do this but it failed on some pages
        public Uri GetListOfMediaFromChannel9PageWithHtmlAgilityPack(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);


            var videoDownloadDiv = doc.GetElementbyId("video-download");
            var videoDownloadDivUl = videoDownloadDiv.ChildNodes.Where(n => n.Name == "ul").FirstOrDefault();
            var videoDownloadDivUlLi = videoDownloadDivUl.ChildNodes.Where(n => n.Name == "li");

            var list = new List<PageMediaItem>();

            foreach (var li in videoDownloadDivUlLi)
            {
                var items = li.ChildNodes.Where(n => n.Name == "div").FirstOrDefault();
                var anchorTag = items.ChildNodes.Where(n => n.Name == "a").FirstOrDefault();

                var name = anchorTag.InnerText;
                var url = anchorTag.Attributes.Where(a => a.Name == "href").FirstOrDefault().Value;

                Console.WriteLine($"{name} ==> {url}");

                list.Add(new PageMediaItem() { MediaType = name, Url = url });
            }

            var selectedUrl = list.Where(n => n.MediaType.Contains("High") && n.MediaType.Contains("MP4"));
            if (selectedUrl.Count() != 0)
            {
                return new Uri(selectedUrl.FirstOrDefault().Url);
            }

            selectedUrl = list.Where(n => n.MediaType.Contains("Mid") && n.MediaType.Contains("MP4"));
            if (selectedUrl.Count() != 0)
            {
                return new Uri(selectedUrl.FirstOrDefault().Url);
            }

            selectedUrl = list.Where(n => n.MediaType.Contains("MP4"));
            if (selectedUrl.Count() != 0)
            {
                return new Uri(selectedUrl.FirstOrDefault().Url);
            }

            return new Uri(list.FirstOrDefault().Url);
        }
    }

    public class PageMediaItem
    {
        public string MediaType { get; set; }
        public string Url { get; set; }
    }



    public struct LinkItem
    {
        public string Href;
        public string Text;

        public override string ToString()
        {
            return Href + "\n\t" + Text;
        }
    }


    static class LinkFinder
    {
        public static List<LinkItem> Find(string file)
        {
            List<LinkItem> list = new List<LinkItem>();

            // 1.
            // Find all matches in file.
            MatchCollection m1 = Regex.Matches(file, @"(<a.*?>.*?</a>)",
                RegexOptions.Singleline);

            // 2.
            // Loop over each match.
            foreach (Match m in m1)
            {
                string value = m.Groups[1].Value;
                LinkItem i = new LinkItem();

                // 3.
                // Get href attribute.
                Match m2 = Regex.Match(value, @"href=\""(.*?)\""",
                RegexOptions.Singleline);
                if (m2.Success)
                {
                    i.Href = m2.Groups[1].Value;
                }

                // 4.
                // Remove inner tags from text.
                string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
                RegexOptions.Singleline);
                i.Text = t;

                list.Add(i);
            }
            return list;
        }
    }
}
