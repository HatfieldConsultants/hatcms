using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.Web.Security;
using System.Threading;
using Hatfield.Web.Portal;
using SharpArch.Core;
using SharpArch.Data.NHibernate;
using SharpArch.Web.NHibernate;
using NHibernate;
using System.IO;
using HatCMS.Core;


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
        private WebSessionStorage webSessionStorage;                    
        /// <summary>
		/// constructor (never needs to be called)
		/// </summary>
		public Global()
		{
			InitializeComponent();
		}	
		
		protected void Application_Start(Object sender, EventArgs e)
		{
            DatabaseServiceLocatorInitializer.Init();
            Console.WriteLine("application starts...");

            CmsUserInterface WebUIConfiguration = new CmsUserInterface(
                new showThumbPage(), 
                new HatCMS.WebEditor.Helpers.PopupFlashObjectBrowser(), 
                CMSUserInterfaceDependencies.CollectUserInterfaceDependencies);

            CmsContext.Application_Start(WebUIConfiguration);                      
		}
        
		protected void Session_Start(Object sender, EventArgs e)
		{            
            CmsContext.Application_Start_Session();
		}

        public override void Init()
        {
            base.Init();
            webSessionStorage = new WebSessionStorage(this);

        }

        
        /// <summary>
        /// The Application_BeginRequest function 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {            
            CmsContext.Application_BeginRequest(Context);

            NHibernateInitializer.Instance().InitializeNHibernateOnce(delegate
            {
                InitializeNHibernateSession();
            });
        }

        private NHibernate.Cfg.Configuration InitializeNHibernateSession()
        {
            return NHibernateSession.Init(webSessionStorage, new string[] { "HatCMS.Data" }, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hibernate.cfg.xml"));
        }

        
		protected void Application_EndRequest(Object sender, EventArgs e)
		{
            
		}
        /*
		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{

		}
        */
         protected void Session_End(Object sender, EventArgs e)
		{
            
		}
        
        protected void Application_Error(Object sender, EventArgs e)
        {
            CmsContext.Application_Error(Context);
        }

		
		protected void Application_End(Object sender, EventArgs e)
		{
            CmsContext.Application_End();            
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

