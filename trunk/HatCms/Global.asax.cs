using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.Web.Security;

using Hatfield.Web.Portal;

namespace HatCMS 
{
	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global : System.Web.HttpApplication
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

        private class ConsoleTraceListener : System.Diagnostics.TraceListener
        {
            public override void Write(string message)
            {
                Console.Write("Trace: "+message);
            }

            public override void WriteLine(string message)
            {
                Console.WriteLine("Trace: " + message);
            }
        }

        private ConsoleTraceListener traceListener;
        
        /// <summary>
		/// constructor (never needs to be called)
		/// </summary>
		public Global()
		{
			InitializeComponent();
		}	
		
		protected void Application_Start(Object sender, EventArgs e)
		{            
            traceListener = new ConsoleTraceListener();
            System.Diagnostics.Trace.Listeners.Add(traceListener);            
		}
        
		protected void Session_Start(Object sender, EventArgs e)
		{
            Console.WriteLine("Application - Session_Start");
            CmsContext.Application_Start_Session();
		}

        /// <summary>
        /// a list of URLs (no leading slash or app path) that the remapping engine should not process.
        /// filenames not handled by the ASP.Net runtime (such as .js or .htm files) do not need to be listed here.
        /// However, any file that you want to execute on its own which normally runs through the .Net runtime needs to be listed
        /// here. These include *.aspx, *.ashx files.
        /// </summary>
        public static readonly string[] urlsToNotRemap = new string[] 
            {                
                "rss.ashx",
                "pdf.ashx",
                "xmlSiteMap.ashx",
                "_system/renderControl.ashx",
                "_system/showThumb.aspx",
                "setup/default.aspx",                                
                "_system/FCKHelpers/InlineImageBrowser2.aspx",
                "_system/FCKHelpers/InlineLinkBrowser.aspx",
                "_system/FCKHelpers/InlineUserFileBrowser.aspx",
                "_system/FlashObject/PopupFlashObjectBrowser.aspx",
                "js/_system/FCKEditor/editor/filemanager/browser/default/connectors/aspx/connector.aspx",
                "_system/MasterPage/EmbeddedPage.aspx",                
                "_system/SingleImage/SingleImageEditor.aspx",
                "_system/FCKHelpers/DeleteResourcePopup.aspx",
                "_system/swfUpload/SwfUploadTarget.aspx",
                "_system/ckhelpers/InlinePageBrowser.aspx",
                "_system/ckhelpers/InlineFileBrowser.aspx",
                "_system/Calendar/SimpleCalendarJsonData.ashx",
                "_system/Browser/browser_json.ashx",
                "_system/Browser/InlineBrowser.aspx",
            };
        /// <summary>
        /// The Application_BeginRequest function is where all the URL re-writing magic happens.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            Console.WriteLine("Application - Application_BeginRequest");

            CmsContext.StartNewRequest();

            ProcessSwfUploadFixes();


            // get the requested page path
            string baseProcessingPage = CmsContext.ApplicationPath + "default.aspx";
            string pagePath = Request.Url.AbsolutePath.Substring(CmsContext.ApplicationPath.Length);

            // do not process the urls in "urlsToNotProcess" array.
            if (StringUtils.IndexOf(urlsToNotRemap, pagePath, StringComparison.CurrentCultureIgnoreCase) != -1)
            {
                return;
            }


            // only re-map ASPX files
            string extension = System.IO.Path.GetExtension(pagePath);
            if (String.Compare(extension, ".aspx", true) != 0)
                return;

            // remove extension
            pagePath = pagePath.Substring(0, pagePath.Length - ".aspx".Length);

            if (String.Compare(pagePath, "default", true) == 0)
                pagePath = "/";


            if (!pagePath.StartsWith("/"))
                pagePath = "/" + pagePath;

            string queryString = "p=" + pagePath; // note: no starting "?"
            if (Request.Url.Query != "")
            {
                string query = Request.Url.Query;
                if (query.StartsWith("?"))
                {
                    // replace beginning "?" with "&"
                    query = query.Substring(1);
                    query = "&" + query;
                }
                queryString += query;
            }
            
            // send the request to the default.aspx page with the page path in the querystring.
            Context.RewritePath(baseProcessingPage, null, queryString);

        }

        private void ProcessSwfUploadFixes()
        {
            try
            {
                string session_param_name = "ASPSESSID";
                string session_cookie_name = "ASP.NET_SESSIONID";

                if (HttpContext.Current.Request.Form[session_param_name] != null)
                {
                    UpdateCookie(session_cookie_name, HttpContext.Current.Request.Form[session_param_name]);
                }
                else if (HttpContext.Current.Request.QueryString[session_param_name] != null)
                {
                    UpdateCookie(session_cookie_name, HttpContext.Current.Request.QueryString[session_param_name]);
                }
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                Response.Write("Error Initializing Session");
            }

            try
            {
                string auth_param_name = "AUTHID";
                string auth_cookie_name = FormsAuthentication.FormsCookieName;

                if (HttpContext.Current.Request.Form[auth_param_name] != null)
                {
                    UpdateCookie(auth_cookie_name, HttpContext.Current.Request.Form[auth_param_name]);
                }
                else if (HttpContext.Current.Request.QueryString[auth_param_name] != null)
                {
                    UpdateCookie(auth_cookie_name, HttpContext.Current.Request.QueryString[auth_param_name]);
                }

            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                Response.Write("Error Initializing Forms Authentication");
            }
        }

        private void UpdateCookie(string cookie_name, string cookie_value)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(cookie_name);
            if (cookie == null)
            {
                cookie = new HttpCookie(cookie_name);
                HttpContext.Current.Request.Cookies.Add(cookie);
            }
            cookie.Value = cookie_value;
            HttpContext.Current.Request.Cookies.Set(cookie);
        }
        
		protected void Application_EndRequest(Object sender, EventArgs e)
		{
            Console.WriteLine("Application_EndRequest");
		}
        /*
		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{

		}
        */
         protected void Session_End(Object sender, EventArgs e)
		{
            
             Console.WriteLine("Application - Session_End - "+(sender.GetType()).Name.ToString());
             Console.WriteLine("Session_End: session has " + Session.Keys.Count + " keys");
             int num = 1;
             foreach (string key in Session.Keys)
             {
                 Console.WriteLine(num.ToString()+"; Session[" + key + "] = " + Session[key].ToString());
                 num++;
             }
             
		}
        
        protected void Application_Error(Object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError().GetBaseException();

            Console.WriteLine(" == Application_Error == ");
            Console.WriteLine("URL: " + Request.Url.ToString());
            Console.WriteLine("MESSAGE: " + ex.Message);
            Console.WriteLine("SOURCE: " + ex.Source);
            Console.WriteLine("FORM: " + Request.Form.ToString());

            Console.WriteLine("TARGETSITE: " + ex.TargetSite);
            Console.WriteLine("STACKTRACE: " + ex.StackTrace);
            try
            {
                if (Session != null)
                {
                    Console.WriteLine("SESSION: session has " + Session.Keys.Count + " keys");
                    int num = 1;
                    foreach (string key in Session.Keys)
                    {
                        Console.WriteLine(num.ToString() + "; Session[" + key + "] = " + Session[key].ToString());
                        num++;
                    }
                }
            }
            catch { }

            if (HttpRuntime.Cache != null)
            {
                Console.WriteLine("Cache has " + HttpRuntime.Cache.Count + " items");
                int num2 = 1;

                IDictionaryEnumerator enumerator = HttpRuntime.Cache.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string key = (String)((DictionaryEntry)enumerator.Current).Key;
                    Console.WriteLine(num2.ToString() + " Cache[" + key + "] = " + Context.Cache[key].ToString());
                    num2++;
                } // while
            }

            if (System.Reflection.Assembly.GetAssembly(typeof(System.Web.HttpRuntime)).GetType("System.Web.HttpRuntime").GetMember("Cache").Length > 0)
                Console.WriteLine("HttpRuntime has a Cache member");
            if (System.Reflection.Assembly.GetAssembly(typeof(System.Web.HttpRuntime)).GetType("System.Web.HttpRuntime").GetMember("InternalCache").Length > 0)
                Console.WriteLine("HttpRuntime has a InternalCache member");
            
            

            Console.WriteLine(" ==== ");
        }

		
		protected void Application_End(Object sender, EventArgs e)
		{
            Console.WriteLine("Application_End");
		}
		
		#region Web Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
