using System;
using System.Web;
using System.Web.Services;
using HatCMS.placeholders.NewsDatabase;

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

            CmsPage pageToRenderRSSFor = CmsContext.getPageByPath(pagePath);
            if (pageToRenderRSSFor.ID < 0)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Error: CMS page not found");
                context.Response.Flush();
                context.Response.End();
            }

            System.Web.HttpRequest r = context.Request;

            string rootUrl = r.Url.Scheme + "://" + r.Url.Host;
            if (!r.Url.IsDefaultPort)
                rootUrl += ":" + r.Url.Port.ToString();

            string rfc822format = "ddd, d MMM yyyy h:mm:ss tt zzz";
            rfc822format = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";

            if (NewsArticleAggregator.isNewsArticleAggregator(pageToRenderRSSFor))
            {
                int year = DateTime.Now.Year;
                if (context.Request.QueryString["y"] != null && context.Request.QueryString["y"] != "")
                {
                    try
                    {
                        year = Convert.ToInt32(context.Request.QueryString["y"]);
                    } catch {}
                }
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
                NewsArticleDb.NewsArticleDetailsData[] thisYearsNews = db.getNewsDetailsByYear(year, lang);
                string FeedTitle = CmsConfig.getConfigValue("pageTitlePrefix", "") + "News for " + year.ToString() + CmsConfig.getConfigValue("pageTitlePostfix", "");

                System.Text.StringBuilder rss = new System.Text.StringBuilder();
                
                rss.Append("<?xml version=\"1.0\"?>");
                rss.Append("<rss version=\"2.0\">");
                rss.Append("<channel>");
                rss.Append("<link>" + rootUrl + "</link>");
                rss.Append("<title>" + context.Server.HtmlEncode(FeedTitle) + "</title>");
                // <lastBuildDate>Mon, 14 Jul 2008 11:32:33 PM -0800</lastBuildDate>

                CmsPageDb pageDb = new CmsPageDb();
                foreach (NewsArticleDb.NewsArticleDetailsData d in thisYearsNews)
                {
                    CmsPage page = pageDb.getPage(d.PageId);
                    rss.Append("<item>");
                    rss.Append("<title>" + context.Server.HtmlEncode(page.Title) + "</title>");
                    rss.Append("<link>" + page.getUrl(lang) + "</link>");
                    DateTime utc = d.DateOfNews.ToUniversalTime();                    

                    // http://stackoverflow.com/questions/284775/how-do-i-parse-and-convert-datetimes-to-the-rfc-822-date-time-format

                    rss.Append("<pubDate>" + utc.ToString(rfc822format) + "</pubDate>");

                    string newsContent = page.renderPlaceholdersToString("HtmlContent", lang);
                    rss.Append("<description>" + context.Server.HtmlEncode(newsContent) + "</description>");

                    rss.Append("</item>");
                } // foreach
                rss.Append("</channel>");
                rss.Append("</rss>");

                context.Response.ContentType = "application/rss+xml";
                context.Response.Write(rss.ToString());
                context.Response.Flush();
                context.Response.End();
            }
#if IncludeOldJobDatabasePlaceholder
            else if (JobDatabase.isJobDatabasePage(pageToRenderRSSFor))
            {
                JobDatabaseDb db = new JobDatabaseDb();
                JobDetailsData[] jobs = db.getJobDetailsByLocation("", false);

                string FeedTitle = CmsConfig.getConfigValue("pageTitlePrefix", "") + "Job Postings Available" + CmsConfig.getConfigValue("pageTitlePostfix", "");

                System.Text.StringBuilder rss = new System.Text.StringBuilder();
                rss.Append("<?xml version=\"1.0\"?>");
                rss.Append("<rss version=\"2.0\">");
                rss.Append("<channel>");
                rss.Append("<link>" + rootUrl + "</link>");
                rss.Append("<title>" + context.Server.HtmlEncode(FeedTitle) + "</title>");
                // <lastBuildDate>Mon, 14 Jul 2008 11:32:33 PM -0800</lastBuildDate>
                foreach (JobDetailsData j in jobs)
                {
                    rss.Append("<item>");
                    rss.Append("<title>" + context.Server.HtmlEncode(j.Title + " in " + j.Location) + "</title>");

                    string itemUrl = rootUrl + pageToRenderRSSFor.Url;
                    if (itemUrl.IndexOf("?") == -1)
                        itemUrl += "?";
                    else
                        itemUrl += "&";
                    itemUrl += JobDatabase.CurrentJobIdFormName + "=" + j.JobDetailsId.ToString();


                    rss.Append("<link>" + itemUrl + "</link>");
                    DateTime utc = j.LastUpdatedDateTime.ToUniversalTime();

                    rss.Append("<pubDate>" + utc.ToString(rfc822format) + "</pubDate>");
                    rss.Append("<description>" + context.Server.HtmlEncode(j.HtmlJobDescription) + "</description>");

                    rss.Append("</item>");
                } // foreach
                rss.Append("</channel>");
                rss.Append("</rss>");

                // -- output the RSS
                context.Response.ContentType = "application/rss+xml";
                context.Response.Write(rss.ToString());
                context.Response.Flush();
                context.Response.End();
            } // Job Database
#endif
            else
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Error: CMS page specified does not have an RSS renderer");
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
