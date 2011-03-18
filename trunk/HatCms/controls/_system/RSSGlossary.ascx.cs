using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HatCMS.Placeholders;

namespace HatCMS.Controls
{
    public partial class RSSGlossary : System.Web.UI.UserControl
    {
        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsControlParameterDependency(this, new string[] {"rssurl"}));            
            return ret.ToArray();
        }

        private GlossaryData[] ToGlossaryData(Rss.RssItemCollection items)
        {
            List<GlossaryData> ret = new List<GlossaryData>();
            foreach (Rss.RssItem item in items)
            {
                GlossaryData g = new GlossaryData();
                g.word = item.Title;
                g.description = item.Description;

                ret.Add(g);
            } // foreach
            return ret.ToArray();
        }

        private string[] getCharsWithData(GlossaryData[] items)
        {
            List<string> ret = new List<string>();
            foreach (GlossaryData item in items)
            {
                string c = item.word[0].ToString();
                if (ret.IndexOf(c) < 0)
                    ret.Add(c);
            } // foreach

            return ret.ToArray();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            StringBuilder html = new StringBuilder();

            string swh = "http://www.sadcwaterhub.org/glossary/feed?lang_tid[0]=2";
            string url = CmsControlUtils.getControlParameterKeyValue(this, "rssurl", swh);

            int cacheDuration_hours = CmsControlUtils.getControlParameterKeyValue(this, "cacheduration_hours", 12);

            Rss.RssFeed glossaryRss;
            // the RSS feed is cached to improve performance.
            if (cacheDuration_hours >= 0 && Cache[url] != null)
            {
                glossaryRss = (Rss.RssFeed)Cache[url];
            }
            else
            {
                glossaryRss = Rss.RssFeed.Read(url);

                // add it to the cache
                if (cacheDuration_hours >= 0)
                    Cache.Insert(url, glossaryRss, null, DateTime.Now.AddHours(cacheDuration_hours), System.Web.Caching.Cache.NoSlidingExpiration);
            }

            if (glossaryRss.Channels.Count == 0)
            {
                html.Append("<em>Error: could not retrieve Glossary from SADC Water Hub</em>");
            }
            else
            {
                GlossaryData[] items = ToGlossaryData(glossaryRss.Channels[0].Items);
                GlossaryPlaceholderData phData = new GlossaryPlaceholderData();
                phData.SortOrder = GlossaryPlaceholderData.GlossarySortOrder.byWord;
                phData.ViewMode = GlossaryPlaceholderData.GlossaryViewMode.PagePerLetter;

                string[] charsWithData = getCharsWithData(items);
                string letterToDisplay = Glossary.getLetterToDisplay(phData);

                html.Append(Glossary.GetHtmlDisplay(CmsContext.currentPage, items, phData, charsWithData, letterToDisplay));
                
            } // else
            writer.Write(html.ToString());

        }
    }
}