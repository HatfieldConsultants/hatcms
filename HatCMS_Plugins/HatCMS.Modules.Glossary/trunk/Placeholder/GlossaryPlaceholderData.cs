using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Modules.Glossary
{
    public class GlossaryPlaceholderData
    {
        public enum GlossaryDataSource { RssFeed, LocalDatabase }
        public enum GlossarySortOrder { byId, byDescription, byWord }
        public enum GlossaryViewMode { SinglePageWithJumpList, PagePerLetter }

        public int GlossaryId = Int32.MinValue;
        public GlossarySortOrder SortOrder = GlossarySortOrder.byWord;
        public GlossaryViewMode ViewMode = GlossaryViewMode.PagePerLetter;

        public static GlossaryDataSource DataSource
        {
            get
            {                
                // -- default is for the LocalDatabase to be the Glossary's data source.
                string display = CmsConfig.getConfigValue("Glossary:DataSource", "");
                if (string.Compare(GlossaryDataSource.RssFeed.ToString(), display.Trim(), true) == 0)
                {
                    string rssUrl = getRssDataSourceUrl();
                    if (rssUrl.Trim() != "")
                        return GlossaryDataSource.RssFeed;
                }
                
                return GlossaryDataSource.LocalDatabase;
            }
        }

        public static string getRssDataSourceUrl()
        {
            // SADC water hub glossary via RSS: "http://www.sadcwaterhub.org/glossary/feed?lang_tid[0]=2"
            string url = CmsConfig.getConfigValue("Glossary:RSSUrl", "");
            return url.Trim();
        }

        public static string getRssDataPersistentVariableName()
        {
            return "Glossary:RSS_Data_" + getRssDataSourceUrl();
        }
        
    }
}
