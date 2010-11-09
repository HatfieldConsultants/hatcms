using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Web.Services;
using HatCMS.placeholders.NewsDatabase;

namespace HatCMS
{
    /// <summary>
    /// Serves an XML SiteMap for this site (http://www.sitemaps.org). Note that this file needs to be in the root of the site it serves (http://www.sitemaps.org/protocol.php#location)
    /// </summary>
    [WebService(Namespace = "http://www.hatcms.net/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class XMLSiteMapHandlerPage : IHttpHandler
    {

        private string W3CTimeFormatString = "yyyy-MM-ddTHH:mm:sszzz"; // YYYY-MM-DDThh:mm:ssTZD (eg 1997-07-16T19:20:30+01:00)

        private void OutputPageXml(CmsPage page, XmlWriter writer, string rootUrl)
        {

            writer.WriteStartElement("url");// <url>
            writer.WriteStartElement("loc"); // <loc>
            
            string pageUrl = rootUrl + page.Url;
            writer.WriteValue(pageUrl);

            writer.WriteEndElement(); // </loc>

            writer.WriteStartElement("lastmod");  // <lastmod>
            
            writer.WriteValue(page.LastUpdatedDateTime.ToString(W3CTimeFormatString));
            writer.WriteEndElement(); // </lastmod>

            writer.WriteEndElement(); // </url>
            
            if (NewsArticleAggregator.isNewsArticleAggregator(page))
            {
                CmsLanguage lang;
                try
                {
                    lang = CmsContext.currentLanguage;
                }
                catch
                {
                    lang = CmsConfig.Languages[0];
                }
                NewsArticleDb db = new NewsArticleDb();
                NewsArticleDb.NewsArticleAggregatorData aggregator = db.fetchNewsAggregator(page, 1, lang, true);
                NewsArticleDb.NewsArticleDetailsData[] thisYearsNews = db.getNewsDetailsByYear(aggregator.YearToDisplay, lang);

                CmsPageDb pageDb = new CmsPageDb();
                foreach (NewsArticleDb.NewsArticleDetailsData d in thisYearsNews)
                {
                    CmsPage newsDetailPage = pageDb.getPage(d.PageId);
                    string itemUrl = rootUrl + newsDetailPage.getUrl(lang);
                    writer.WriteStartElement("url");// <url>
                    writer.WriteStartElement("loc"); // <loc>

                    writer.WriteValue(itemUrl);

                    writer.WriteEndElement(); // </loc>

                    writer.WriteStartElement("lastmod");  // <lastmod>

                    writer.WriteValue(page.LastUpdatedDateTime.ToString(W3CTimeFormatString));
                    writer.WriteEndElement(); // </lastmod>                    

                    writer.WriteEndElement(); // </url>
                } // foreach
            }
#if IncludeOldJobDatabasePlaceholder
            else if (JobDatabase.isJobDatabasePage(page))
            {
                JobDatabaseDb db = new JobDatabaseDb();
                JobDetailsData[] jobs = db.getJobDetailsByLocation("", false);
                foreach (JobDetailsData j in jobs)
                {                    
                    string itemUrl = rootUrl + page.Url;
                    if (itemUrl.IndexOf("?") == -1)
                        itemUrl += "?";
                    else
                        itemUrl += "&";
                    itemUrl += JobDatabase.CurrentJobIdFormName + "=" + j.JobDetailsId.ToString();

                    writer.WriteStartElement("url");// <url>
                    writer.WriteStartElement("loc"); // <loc>

                    writer.WriteValue(itemUrl);

                    writer.WriteEndElement(); // </loc>

                    writer.WriteStartElement("lastmod");  // <lastmod>

                    writer.WriteValue(page.LastUpdatedDateTime.ToString(W3CTimeFormatString));
                    writer.WriteEndElement(); // </lastmod>

                    writer.WriteEndElement(); // </url>
                
                } // foreach
            }   
#endif
        } // AddPageNodes

        public void ProcessRequest(HttpContext context)
        {

            System.Web.HttpRequest r = context.Request;

            string rootUrl = r.Url.Scheme + "://" + r.Url.Host;
            if (!r.Url.IsDefaultPort)
                rootUrl += ":" + r.Url.Port.ToString();

            context.Response.BufferOutput = false;
            context.Response.ContentType = "text/xml";
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = System.Text.Encoding.UTF8;
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(context.Response.OutputStream, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            foreach (int pageId in allPages.Keys)
            {
                CmsPage page = allPages[pageId];
                
                OutputPageXml(page, writer, rootUrl);
            }

            writer.WriteEndElement(); // urlSet
            writer.WriteEndDocument();

            writer.Flush(); // very very important (will not work if this isn't included!)
            
            context.Response.Flush();
            context.Response.End();

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
