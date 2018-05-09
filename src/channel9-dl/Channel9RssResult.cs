using Microsoft.SyndicationFeed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace channel9_dl
{
    public class Channel9RssResult
    {
        public Channel9RssResult()
        {
            this.SyndicationItems = new List<ISyndicationItem>();
            this.Sessions = new List<SessionInfo>();
        }

        public Uri SourceUrl { get; set; }
        public string RawXml { get; set; }
        public IList<ISyndicationItem> SyndicationItems { get; set; }
        public IList<SessionInfo> Sessions { get; set; }

        public List<Exception> Exceptions { get; set; }

        public override string ToString()
        {
            return $"{SyndicationItems?.Count()} Items with {Exceptions?.Count()} Exceptions";
        }
    }
}
