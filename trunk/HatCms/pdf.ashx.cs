using System;
using System.IO;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using Hatfield.Web.Portal;
using HatCMS.Placeholders;

namespace HatCMS
{
    /// <summary>
    /// Renders a CMS page as a PDF file.
    /// </summary>
    [WebService(Namespace = "http://www.hatcms.net")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class PDFHandlerPage : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {

            string pagePath = "";
            if (context.Request.QueryString["p"] != null)
            {
                pagePath = context.Request.QueryString["p"];
            }

            bool landscape = false;
            if (context.Request.QueryString["landscape"] != null && context.Request.QueryString["landscape"] == "1")
            {
                landscape = true;
            }

            CmsPage pageToRenderPDFFor = CmsContext.getPageByPath(pagePath);
            if (pageToRenderPDFFor.ID < 0)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Error: CMS page not found");
                context.Response.Flush();
                context.Response.End();
            }
            else if (PageRedirect.isPageRedirectPage(pageToRenderPDFFor))
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Error: can not make PDF out of a redirection page");
                context.Response.Flush();
                context.Response.End();
            }            

            System.Web.HttpRequest r = context.Request;

            string rootUrl = r.Url.Scheme + "://" + r.Url.Host;
            if (!r.Url.IsDefaultPort)
                rootUrl += ":" + r.Url.Port.ToString();
           
            WebSupergoo.ABCpdf6.XSettings.License = "393-927-439-276-8499-969";

            string urlToRender = rootUrl + pageToRenderPDFFor.Url;

            string cacheKey = urlToRender.ToLower();
            byte[] pdfBinary = getPDFFromCache(context, cacheKey);
            if (pdfBinary == null)
            {
                pdfBinary = UrlToPdf(urlToRender, 1100, landscape);
                if (pdfBinary.Length > 0)
                {
                    addToCache(context, cacheKey, pdfBinary);
                }
            }

            string outputFilename = pageToFilename(pageToRenderPDFFor);

            context.Response.ContentType = "application/pdf";
            context.Response.AppendHeader("content-disposition", "attachment; filename=" + outputFilename);
            context.Response.AppendHeader("Content-Transfer-Encoding", "binary");
            context.Response.AppendHeader("Content-Length", pdfBinary.Length.ToString()); 
            context.Response.BinaryWrite(pdfBinary);
            context.Response.Flush();
            context.Response.End();            
            
        }

        private byte[] getPDFFromCache(System.Web.HttpContext context, string cachekey)
        {
            if (context.Cache[cachekey] != null)
                return (byte[])context.Cache[cachekey];
            return null;
        }

        private void addToCache(System.Web.HttpContext context, string cachekey, byte[] pdfContent)
        {
            context.Cache.Insert(cachekey, pdfContent);
        }


        private string pageToFilename(CmsPage page)
        {
            string ret = page.Title;
            if (ret == "")
                ret = page.MenuTitle;
            
            foreach (char c in Path.GetInvalidPathChars())
            {
                ret = ret.Replace(c, '~');
            }

            ret = ret.Replace(Path.DirectorySeparatorChar.ToString(), "-");
            ret = ret.Replace(Path.AltDirectorySeparatorChar.ToString(), "-");
            ret = ret.Replace(Path.VolumeSeparatorChar.ToString(), "");
            ret = ret.Replace(Path.PathSeparator.ToString(), "");
            ret = ret.Replace(".", "");
            ret = ret.Replace(" ", "+");

            if (ret.Length >= 240)
            {
                ret = ret.Substring(ret.Length - 240);
            }

            ret = ret + ".pdf";            

            return ret;
        }

        private byte[] UrlToPdf(string url, int BrowserRenderWidth, bool landscape)
        {

            using (WebSupergoo.ABCpdf6.Doc theDoc = new WebSupergoo.ABCpdf6.Doc())
            {
                try
                {
                    if (landscape)
                    {
                        // -- make landscape
                        double w = theDoc.MediaBox.Width;
                        double h = theDoc.MediaBox.Height;
                        double l = theDoc.MediaBox.Left;
                        double b = theDoc.MediaBox.Bottom;
                        theDoc.Transform.Rotate(90, l, b);
                        theDoc.Transform.Translate(w, 0);

                        // rotate our rectangle
                        theDoc.Rect.Width = h;
                        theDoc.Rect.Height = w;

                    }
                    // -- setup the conversion params
                    theDoc.Rect.Inset(10, 10);
                    theDoc.HtmlOptions.AddForms = false;
                    theDoc.HtmlOptions.AddLinks = true;
                    theDoc.HtmlOptions.AddMovies = true;
                    theDoc.HtmlOptions.AutoTruncate = false;
                    theDoc.HtmlOptions.BrowserWidth = BrowserRenderWidth;
                    theDoc.HtmlOptions.FontEmbed = true;
                    theDoc.HtmlOptions.Paged = true;
                    theDoc.HtmlOptions.TargetLinks = true;

                    bool requireAnonLogin = CmsConfig.getConfigValue("RequireAnonLogin", false);
                    if (requireAnonLogin && CmsContext.currentUserIsLoggedIn)
                    {
                        theDoc.HtmlOptions.LogonName = CmsContext.currentWebPortalUser.UserName;
                        theDoc.HtmlOptions.LogonPassword = CmsContext.currentWebPortalUser.Password;
                    }
                    theDoc.Page = theDoc.AddPage();

                    // make a cache busting URL
                    string urlDelim = "?";
                    if (url.IndexOf("?") > 0)
                        urlDelim = "&";
                    string cacheBustingUrl = url+urlDelim+"pdfgen_nocache="+DateTime.Now.Ticks.ToString();

                    int theID = theDoc.AddImageUrl(cacheBustingUrl);
                    

                    while (true)
                    {
                        theDoc.FrameRect(); // add a black border
                        if (!theDoc.Chainable(theID))
                            break;
                        theDoc.Page = theDoc.AddPage();
                        

                        theID = theDoc.AddImageToChain(theID);

                    }

                    for (int i = 1; i <= theDoc.PageCount; i++)
                    {
                        theDoc.PageNumber = i;
                        theDoc.Flatten();
                    }

                    if (landscape)
                    {
                        int rotId = theDoc.GetInfoInt(theDoc.Root, "Pages");
                        theDoc.SetInfo(rotId, "/Rotate", "90"); // rotate the view 90 degrees; http://www.webgoo.com/helppdf6net/default.html?page=source%2F3-concepts%2F2-objectinfo.htm
                    }
                    byte[] ret =  theDoc.GetData();

                    theDoc.Clear();

                    return ret;
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
                return new byte[0];
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
