using System;
using System.Data;
using System.Web;
using System.Web.Caching;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Net;

namespace HatCMS.Tools
{
    /// <summary>
    /// A service that proxies information from external sites. Host names must be in the "Proxy.ApprovedHosts" list in the configuration file.
    /// 
    /// Note: to improve this class using a disk caching system, look at http://concurrentcache.codeplex.com/releases/view/46598 and 
    /// DownloadManager here :http://aspnetrsstoolkit.codeplex.com/SourceControl/changeset/view/27560#1313580 
    /// </summary>
    [WebService(Namespace = "http://hatcms.net/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class cachingProxy : IHttpHandler
    {
        public static string[] ApprovedHosts
        {
            get
            {
                string toSplit = CmsConfig.getConfigValue("Proxy.ApprovedHosts", "");
                return toSplit.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static int IndexOfHost(string hostName)
        {
            return Hatfield.Web.Portal.StringUtils.IndexOf(ApprovedHosts, hostName, StringComparison.CurrentCulture);
        }

        public static string getProxiedUrl(string externalUrl)
        {
            Uri u = new Uri(externalUrl);
            
            int hostIndex = IndexOfHost(u.Host);
            string path = u.PathAndQuery;

            string url = CmsContext.ApplicationPath + "_system/tools/cachingProxy.ashx?hostIndex=" + hostIndex.ToString() + "&path=" + path;
            return url;
        }

        /// <summary>
        /// the number of minutes that requested items should be cached for.
        /// If zero or less than zero, no caching is done.
        /// </summary>
        public static int CacheDuration_Minutes
        {
            get
            {
                return CmsConfig.getConfigValue("Proxy.CacheDuration_Minutes", 60);
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            int hostIndex = Int32.MinValue;
            if (context.Request["hostIndex"] == null || 
                context.Request["hostIndex"].ToString() == "" ||
               !Int32.TryParse(context.Request["hostIndex"].ToString(), out hostIndex) ||
               hostIndex < 0 || hostIndex >= ApprovedHosts.Length
                )
            {
                context.Response.Write("Error: hostIndex must be specified");
                context.Response.End();
            }

            string hostPath = context.Request["path"].ToString();

            string hostName = cachingProxy.ApprovedHosts[hostIndex];
            string url = "http://" + hostName + hostPath;
            
            string contentType = context.Request["type"].ToString();

            // We don't want to buffer because we want to save memory
            context.Response.Buffer = false;

            // Serve from cache if available
            if (CacheDuration_Minutes>= 0 && context.Cache[url] != null)
            {
                context.Response.BinaryWrite(context.Cache[url] as byte[]);
                context.Response.Flush();
                return;
            }
            using (WebClient client = new WebClient())
            {
                if (!string.IsNullOrEmpty(contentType))
                    client.Headers["Content-Type"] = contentType;

                client.Headers["Accept-Encoding"] = "gzip";
                client.Headers["Accept"] = "*/*";
                client.Headers["Accept-Language"] = "en-US";
                client.Headers["User-Agent"] =
                       "Mozilla/5.0 (Windows; U; Windows NT 6.0; " +
                       "en-US; rv:1.8.1.6) Gecko/20070725 Firefox/2.0.0.6";
                
                byte[] data = client.DownloadData(url);

                // -- add the data to the cache
                if (CacheDuration_Minutes >= 0)
                {
                    context.Cache.Insert(url, data, null,
                                Cache.NoAbsoluteExpiration,
                                TimeSpan.FromMinutes(CacheDuration_Minutes),
                                CacheItemPriority.Normal, null);
                }

                if (!context.Response.IsClientConnected) return;


                // Deliver content type, encoding and length
                // as it is received from the external URL
                if (client.ResponseHeaders["Content-Type"] != null)
                    context.Response.ContentType = client.ResponseHeaders["Content-Type"].ToString();

                string contentEncoding = client.ResponseHeaders["Content-Encoding"];
                string contentLength = client.ResponseHeaders["Content-Length"];

                if (!string.IsNullOrEmpty(contentEncoding))
                    context.Response.AppendHeader("Content-Encoding", contentEncoding);
                if (!string.IsNullOrEmpty(contentLength))
                    context.Response.AppendHeader("Content-Length", contentLength);

                // Transmit the exact bytes downloaded
                context.Response.BinaryWrite(data);

            }
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}
