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

            // do not process the urls in "URLsToNotRemap" configuration entry.
            if (StringUtils.IndexOf(CmsConfig.URLsToNotRemap, pagePath, StringComparison.CurrentCultureIgnoreCase) != -1)
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
            Hatfield.Web.Portal.ApplicationUtils.Application_Error_StandardEmailSender(HttpContext.Current, "jsuwala@hatfieldgroup.com", new string[] { "jsuwala@hatfieldgroup.com"}, "mx.hatfieldgroup.com");
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

