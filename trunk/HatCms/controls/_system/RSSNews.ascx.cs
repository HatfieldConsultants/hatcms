using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.controls
{
    public partial class RSSNews : System.Web.UI.UserControl
    {
        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsControlParameterDependency(this, new string[] { "rssurl" }));
            return ret.ToArray();
        }



        protected override void Render(HtmlTextWriter writer)
        {
            StringBuilder html = new StringBuilder();

            /// ORASECOM river basin SWISH news:  http://www.sadcwaterhub.org/articles/feed?keywords=orange%20OR%20ORASECOM%20OR%20%22Orange-Senqu%22
            string rssurl = "http://www.sadcwaterhub.org/articles/feed"; //all articles feed
            int cacheDuration_hours = 12;
            
            /// [template]: Output Template
            ///     template parameters: url, summaryOutput, newsArticleDetailsPage.Title, dateOfNews
            ///     {0} = url
            ///     {1} = snippet
            ///     {2} = title    
            ///     {3} = date
            /// [dateOutputFormat]: dateOutputFormat
            /// [summaryLength]: maxLengthOfSummary (integer)            
            string itemTemplate = "<div class=\"RSSNewsItem\">{3}: <a href=\"{0}\">{2}</a><br>{1}</div>";
            string dateOutputFormat = "MMMM d, yyyy";
            int maxLengthOfSummary = 115;

            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                rssurl = CmsControlUtils.getControlParameterKeyValue(this, "rssurl", rssurl);
                cacheDuration_hours = CmsControlUtils.getControlParameterKeyValue(this, "cacheduration_hours", cacheDuration_hours);            
                itemTemplate = CmsControlUtils.getControlParameterKeyValue(this, "template", itemTemplate);
                dateOutputFormat = CmsControlUtils.getControlParameterKeyValue(this, "dateFormat", dateOutputFormat);
                maxLengthOfSummary = CmsControlUtils.getControlParameterKeyValue(this, "summaryLength", maxLengthOfSummary);
                dateOutputFormat = CmsControlUtils.getControlParameterKeyValue(this, "dateOutputFormat", dateOutputFormat);
            }
            else
            {
                throw new ArgumentException("Invalid CmsTemplateEngineVersion");
            }

            Rss.RssFeed newsRss;
            // the RSS feed is cached to improve performance.
            if (cacheDuration_hours >= 0 && Cache[rssurl] != null)
            {
                newsRss = (Rss.RssFeed)Cache[rssurl];
            }
            else
            {
                newsRss = Rss.RssFeed.Read(rssurl);

                // add it to the cache
                if (cacheDuration_hours >= 0)
                    Cache.Insert(rssurl, newsRss, null, DateTime.Now.AddHours(cacheDuration_hours), System.Web.Caching.Cache.NoSlidingExpiration);
            }

            if (newsRss.Channels.Count == 0)
            {
                html.Append("<em>Error: could not retrieve News RSS from " + rssurl + "</em>");
            }
            else if (newsRss.Channels.Count > 0 && newsRss.Channels[0].Items.Count == 0)
            {
                html.Append("<em>There are no news items available to view</em>");
            }
            else
            {
                html.Append(GetArticleListHtml(newsRss.Channels[0].Items, itemTemplate, maxLengthOfSummary, dateOutputFormat));

            } // else
            writer.Write(html.ToString());

        }

        private string GetArticleListHtml(Rss.RssItemCollection items, string itemFormat, int snippetLength, string dateOutputFormat)
        {
            StringBuilder html = new StringBuilder();
            
            html.Append("<div class=\"RSSNews\">");

            foreach (Rss.RssItem item in items)
            {
                // -- get the items to include in the format string
                string url = item.Link.ToString();
                string snippet = item.Description;
                if (snippet.Length > snippetLength)
                    snippet = snippet.Substring(0, snippetLength);

                string title = item.Title;
                string date = item.PubDate_GMT.ToString(dateOutputFormat);

                // run string.Format
                string formatted = String.Format(itemFormat, url, snippet, title, date);

                html.Append(formatted);

            } // foreach

            html.Append("</div>");
            return html.ToString();
        }
    }
}