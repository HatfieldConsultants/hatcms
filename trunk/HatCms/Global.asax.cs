using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.Web.Security;
using System.Threading;
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

        private ConsoleTraceListener _traceListener;
        private static System.Threading.Timer _timer = null;
        
        /// <summary>
        /// time-interval (milliseconds) to wait between calls to RunBackgroundPeriodicTasks(). 
        /// default value is every hour (3600000 ms)
        /// </summary>
        private const int _backgroundTimerPeriod_ms = 60 * 60 * 1000; 
        
        
        /// <summary>
		/// constructor (never needs to be called)
		/// </summary>
		public Global()
		{
			InitializeComponent();
		}	
		
		protected void Application_Start(Object sender, EventArgs e)
		{            
            // -- initialize the console trace listener
            _traceListener = new ConsoleTraceListener();
            System.Diagnostics.Trace.Listeners.Add(_traceListener);            

            // -- run all OnApplicationStart background tasks
            CmsBackgroundTaskUtils.RunAllApplicationStartBackgroundTasks();

            // -- initialize Period Background Task timer
            int dueTime_ms = 60 * 1000; // milliseconds to wait before calling RunBackgroundPeriodicTasks for first time.            

            _timer = new System.Threading.Timer(RunBackgroundPeriodicTasks, null, dueTime_ms, _backgroundTimerPeriod_ms);
            
		}
        
		protected void Session_Start(Object sender, EventArgs e)
		{
            Console.WriteLine("Application - Session_Start");
            CmsContext.Application_Start_Session();
		}

        private static void RunBackgroundPeriodicTasks(object state)
        {
            // -- ensure we don't call this function again until RunBackgroundTasks has returned
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            try
            {
                CmsBackgroundTaskUtils.RunAllApplicablePeriodicTasks();
                
            }
            catch (Exception ex)
            {
                Console.Write("Background Task Error: Something went wrong. Reason: {0} Stack: {1}", ex.Message, ex.StackTrace);
            }

            
            _timer.Change(_backgroundTimerPeriod_ms, _backgroundTimerPeriod_ms);
        }

        
        /// <summary>
        /// The Application_BeginRequest function is where all the URL re-writing magic happens.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            Console.WriteLine("Application - Application_BeginRequest");

            ProcessSwfUploadFixes(); // this needs to be before the UrlsToNotRemap is checked.

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

            CmsContext.StartNewRequest();            

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
            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                return;

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
            string techEmail = CmsConfig.getConfigValue("TechnicalAdministratorEmail", "");
            string smtpServer = CmsConfig.getConfigValue("smtpServer", "");
            if (techEmail.IndexOf("@") > 0 && smtpServer.Trim() != "")
            {
                Hatfield.Web.Portal.ApplicationUtils.Application_Error_StandardEmailSender(HttpContext.Current, techEmail, new string[] { techEmail }, smtpServer);
            }
        }

		
		protected void Application_End(Object sender, EventArgs e)
		{
            // -- run all OnApplicationStart background tasks
            CmsBackgroundTaskUtils.RunAllApplicationEndBackgroundTasks();

            // -- get rid of the periodic background events timer.
            if (_timer != null)
                _timer.Dispose();

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

