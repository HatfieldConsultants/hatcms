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
    public partial class RSSDocuments : System.Web.UI.UserControl
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



        protected override void Render(HtmlTextWriter writer)
        {
            StringBuilder html = new StringBuilder();

            /// ORASECOM river basin SWISH documents:  http://www.sadcwaterhub.org/documents/feed?source_tid_op=or&type_tid_op=or&icp_tid_op=or&rbo_tid_op=or&river_basin_tid_op=or&river_basin_tid[0]=68&title=
            string swh = "http://www.sadcwaterhub.org/documents/feed"; //all documents feed
            string url = CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "rssurl", swh);

            int cacheDuration_hours = CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "cacheduration_hours", 12);

            Rss.RssFeed documentsRss;
            // the RSS feed is cached to improve performance.
            System.Web.Caching.Cache Cache = System.Web.Hosting.HostingEnvironment.Cache;

            if (cacheDuration_hours >= 0 && Cache[url] != null)
            {
                documentsRss = (Rss.RssFeed)Cache[url];
            }
            else
            {
                documentsRss = Rss.RssFeed.Read(url);

                // add it to the cache
                if (cacheDuration_hours >= 0)
                    Cache.Insert(url, documentsRss, null, DateTime.Now.AddHours(cacheDuration_hours), System.Web.Caching.Cache.NoSlidingExpiration);
            }

            if (documentsRss.Channels.Count == 0)
            {
                html.Append("<em>Error: could not retrieve Documents RSS from "+url+"</em>");
            }
            else
            {
                FileLibraryAggregator2.FileAggItem[] files = FileLibraryAggregator2.FileAggItem.FromRSSItems(documentsRss.Channels[0].Items);

                files = FileLibraryAggregator2.FileAggItem.RemoveDuplicates(new List<FileLibraryAggregator2.FileAggItem>(files));
                files = FileLibraryAggregator2.FileAggItem.SortFilesByTitle(files);
                
                
                FileLibraryAggregator2.RenderParameters renderParameters = new FileLibraryAggregator2.RenderParameters();
                renderParameters.fileLinkMode = FileLibraryAggregator2.RenderParameters.FileLinkMode.LinkToFile;
                renderParameters.ListingTitle = CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "listingtitle", renderParameters.ListingTitle);
                renderParameters.ShowByCategory = CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "showbycategory", renderParameters.ShowByCategory);


                html.Append(FileLibraryAggregator2.RenderToHtmlList(files, renderParameters, false));
                
            } // else
            writer.Write(html.ToString());

        }
        
    }
}