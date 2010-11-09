using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS
{
    public class ContentUtils
    {
        /// <summary>
        /// returns the subset of fileLinksToFind that are found in HtmlHaystack
        /// </summary>
        /// <param name="HtmlHaystack"></param>
        /// <param name="fileLinksToFind"></param>
        /// <returns></returns>
        public static string[] FindFileLinksInHtml(string HtmlHaystack, string[] fileLinksToFind)
        {

            List<string> ret = new List<string>();
            System.Web.HttpServerUtility server = System.Web.HttpContext.Current.Server;
            string appPath = CmsContext.ApplicationPath;

            string html = HtmlHaystack.Trim();
            if (html == "")
            {
                return new string[0];                
            }

            foreach (string url in fileLinksToFind)
            {
                if (ret.IndexOf(url) > -1) // skip already found urls
                    break;

                List<string> searchFor = new List<string>();

                searchFor.Add("href=\"" + System.Uri.EscapeUriString(url) + "\"");
                searchFor.Add("href=\"" + System.Uri.EscapeUriString(appPath + url) + "\"");
                searchFor.Add("href='" + System.Uri.EscapeUriString(url) + "'");
                searchFor.Add("href='" + System.Uri.EscapeUriString(appPath + url) + "'");

                searchFor.Add("href=\"" + url + "\"");
                searchFor.Add("href=\"" + appPath + url + "\"");
                searchFor.Add("href='" + url + "'");
                searchFor.Add("href='" + appPath + url + "'");

                if (server != null)
                {
                    searchFor.Add("href=\"" + server.UrlEncode(url) + "\"");
                    searchFor.Add("href=\"" + server.UrlEncode(appPath + url) + "\"");
                    searchFor.Add("href='" + server.UrlEncode(url) + "'");
                    searchFor.Add("href='" + server.UrlEncode(appPath + url) + "'");
                }

                searchFor.Add("src=\"" + (url) + "\"");
                searchFor.Add("src=\"" + (appPath + url) + "\"");
                searchFor.Add("src='" + (url) + "'");
                searchFor.Add("src='" + (appPath + url) + "'");


                searchFor.Add("src=\"" + System.Uri.EscapeUriString(url) + "\"");
                searchFor.Add("src=\"" + System.Uri.EscapeUriString(appPath + url) + "\"");
                searchFor.Add("src='" + System.Uri.EscapeUriString(url) + "'");
                searchFor.Add("src='" + System.Uri.EscapeUriString(appPath + url) + "'");

                searchFor.Add("src=\"" + System.Uri.EscapeUriString("showThumb.aspx?file=" + url));
                searchFor.Add("src=\"" + System.Uri.EscapeUriString(appPath + "showThumb.aspx?file=" + appPath + url));
                searchFor.Add("src=\"" + System.Uri.EscapeUriString(appPath + "showThumb.aspx?file=" + url));
                searchFor.Add("src='" + System.Uri.EscapeUriString("showThumb.aspx?file=" + url));
                searchFor.Add("src='" + System.Uri.EscapeUriString(appPath + "showThumb.aspx?file=" + appPath + url));
                searchFor.Add("src='" + System.Uri.EscapeUriString(appPath + "showThumb.aspx?file=" + url));

                foreach (string searchString in searchFor)
                {                    
                    if (html.IndexOf(searchString, StringComparison.CurrentCultureIgnoreCase) > -1)
                    {
                        ret.Add(url);
                        break;
                    }
                } // foreach

            } // foreach

            return ret.ToArray();


        }        
    }
}
