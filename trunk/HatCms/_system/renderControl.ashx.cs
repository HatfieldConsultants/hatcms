using System;
using System.Text;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using Hatfield.Web.Portal;
using HatCMS.Placeholders;
using System.Web.SessionState;

namespace HatCMS
{
    /// <summary>
    /// An .ASPX page that renders a CMS control to the response stream.
    /// </summary>
    [WebService(Namespace = "http://www.hatcms.net/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class RenderControlHandlerPage : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            string pagePath = "";
            if (context.Request.QueryString["p"] != null)
            {
                pagePath = context.Request.QueryString["p"];
            }

            string controlPath = "";
            if (context.Request.QueryString["c"] != null)
            {
                controlPath = context.Request.QueryString["c"];
            }

            CmsPage pageToRenderControlFor = CmsContext.getPageByPath(pagePath);
            if (pageToRenderControlFor.ID < 0)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Error: CMS page not found");
                context.Response.Flush();
                context.Response.End();
                return;
            }            
          
            string appPath = context.Request.ApplicationPath;
            if (!appPath.EndsWith("/"))
                appPath += "/";


            string html = pageToRenderControlFor.TemplateEngine.renderControlToString(controlPath);

            
            context.Response.ContentType = "text/html";
            context.Response.Write(html);


            
        } // ProcessRequest

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
