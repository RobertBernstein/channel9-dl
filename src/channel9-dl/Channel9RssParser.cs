using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace channel9_dl
{
   
    public class Channel9RssParser
    {
        public async Task<Channel9RssResult> Parse (Uri rssUri)
        {
            var result = new Channel9RssResult();
            result.SourceUrl = rssUri;

            try
            {
                using (var client = new HttpClient())
                {
                    result.RawXml = await client.GetStringAsync(rssUri);

                    using (var xmlReader = XmlReader.Create(new StringReader(result.RawXml), new XmlReaderSettings() { Async = false }))
                    {
                        var feedReader = new RssFeedReader(xmlReader);

                        while (await feedReader.Read())
                        {
                            switch (feedReader.ElementType)
                            {
                                // Read category
                                case SyndicationElementType.Category:
                                    ISyndicationCategory category = await feedReader.ReadCategory();
                                    break;

                                // Read Image
                                case SyndicationElementType.Image:
                                    ISyndicationImage image = await feedReader.ReadImage();
                                    break;

                                // Read Item
                                case SyndicationElementType.Item:

                                    // parse the syndication item
                                    ISyndicationItem item = await feedReader.ReadItem();
                                    result.SyndicationItems.Add(item);

                                    // then construct a session info
                                    var si = new SessionInfo();
                                    si.SessionID = item.Id.Substring(item.Id.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1);
                                    si.Title = item.Title;
                                    si.SessionSite = new Uri(item.Id);
                                    si.PublishDate = item.Published.DateTime;
                                    si.Presenter = item.Contributors.FirstOrDefault()?.Name;

                                    result.Sessions.Add(si);

                                    foreach (var v in item.Links)
                                    {
                                        if (!string.IsNullOrWhiteSpace(v.MediaType))
                                        {
                                            si.VideoRecordings.Add(new VideoRecording() { SessionInfo = si, Url = v.Uri, MediaType = v.MediaType, Length = v.Length });
                                        }
                                    }

                                    break;

                                // Read link
                                case SyndicationElementType.Link:
                                    ISyndicationLink link = await feedReader.ReadLink();
                                    break;

                                // Read Person
                                case SyndicationElementType.Person:
                                    ISyndicationPerson person = await feedReader.ReadPerson();
                                    break;

                                // Read content
                                default:
                                    ISyndicationContent content = await feedReader.ReadContent();
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Exceptions = new List<Exception>();
                result.Exceptions.Add(ex);
            }

            return result;
        }
    }
}
