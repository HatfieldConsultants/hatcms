using System;
using System.Web;
using System.Web.Services;
using Rss;

namespace HatCMS
{
    /// <summary>
    /// Renders News and Job items as RSS feeds.
    /// </summary>
    [WebService(Namespace = "http://www.hatcms.net/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class RSSHandlerPage : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {

            string pagePath = "";
            if (context.Request.QueryString["p"] != null)
            {
                pagePath = context.Request.QueryString["p"];
            }
            CmsLanguage pageLang = CmsConfig.Languages[0];
            if (CmsConfig.Languages.Length > 1 && context.Request.QueryString["l"] != null)
            {
                string langCode = context.Request.QueryString["l"];
                CmsLanguage testLang = CmsLanguage.GetFromHaystack(langCode, CmsConfig.Languages);
                if (!testLang.isInvalidLanguage)
                    pageLang = testLang;
            }

            CmsPage pageToRenderRSSFor = CmsContext.getPageByPath(pagePath, pageLang);
            if (pageToRenderRSSFor.ID < 0 || !pageToRenderRSSFor.currentUserCanRead)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Error: CMS page not found");
                context.Response.Flush();
                context.Response.End();
            }            
            else
            {
                // -- generate the RssFeed
                RssFeed rssFeed = new RssFeed(System.Text.UTF8Encoding.UTF8);                
                rssFeed.Version = RssVersion.RSS20;                

                
                // -- setup the RSS channel
                string rssTitle = pageToRenderRSSFor.getTitle(pageLang);
                string rssDescription = pageToRenderRSSFor.getSearchEngineDescription(pageLang);
                Uri rssLink = new Uri(pageToRenderRSSFor.getUrl(CmsUrlFormat.FullIncludingProtocolAndDomainName, pageLang), UriKind.RelativeOrAbsolute);
                RssChannel rssChannel = new RssChannel(rssTitle, rssDescription, rssLink);
                rssChannel.Generator = "HatCMS: https://code.google.com/p/hatcms/";

                // -- call "GetRssFeedItems()" for each placeholder.
                CmsPlaceholderDefinition[] phDefs = pageToRenderRSSFor.getAllPlaceholderDefinitions();
                
                foreach (CmsPlaceholderDefinition phDef in phDefs)
                {                    
                    RssItem[] items = Placeholders.PlaceholderUtils.GetRssFeedItems(phDef.PlaceholderType, pageToRenderRSSFor, phDef, pageLang);
                    foreach (RssItem item in items)
                        rssChannel.Items.Add(item);
                }

                rssFeed.Channels.Add(rssChannel);

                context.Response.ContentType = "application/rss+xml";
                rssFeed.Write(context.Response.OutputStream);
                context.Response.Flush();
                context.Response.End();
            }

            
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
