namespace HatCMS.Controls
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
    using HatCMS.Placeholders;
    using Hatfield.Web.Portal;
    using System.Text;
    using System.Collections.Generic;

	/// <summary>
	///		Summary description for HeadSection.
	/// </summary>
	public partial class HeadSection : System.Web.UI.UserControl
	{
        protected static string EOL = Environment.NewLine;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();            
            string cssUrl = CmsConfig.getConfigValue("cssUrl", "").Replace("~", "");
            if (cssUrl != "")            
                ret.Add(CmsFileDependency.UnderAppPath(cssUrl));

            ret.Add(new CmsConfigItemDependency("PrinterAndPdfVer.printerCss"));

            string printcssUrl = CmsConfig.getConfigValue("PrinterAndPdfVer.printerCss", "").Replace("~", "");
            if (printcssUrl != "")
                ret.Add(CmsFileDependency.UnderAppPath(printcssUrl));
            

            return ret.ToArray();
        }

        private string getDisplayTitle()
        {
            return CmsContext.currentPage.Title;
        }

        private string getHtml()
        {
            string titlePrefix = CmsConfig.getConfigValue("pageTitlePrefix", "");

            string titlePostfix = CmsConfig.getConfigValue("pageTitlePostfix", "");

            System.Text.StringBuilder html = new System.Text.StringBuilder();
            html.Append("<title>" + titlePrefix + getDisplayTitle() + titlePostfix + "</title>");
            string cssUrl = CmsConfig.getConfigValue("cssUrl", "");
            if (cssUrl != null && cssUrl != "")
            {
                cssUrl = cssUrl.Replace("~", "");
                CmsContext.currentPage.HeadSection.AddCSSFile(CSSGroup.FrontEnd, cssUrl);                
            }

            string printCss = CmsConfig.getConfigValue("PrinterAndPdfVer.printerCss", "");
            if (CmsContext.currentUserIsRequestingPrintFriendlyVersion && printCss != null && printCss.Trim() != "")
            {
                printCss = printCss.Replace("~", CmsContext.ApplicationPath);
                CmsContext.currentPage.HeadSection.AddCSSFile(CSSGroup.FrontEnd, printCss);                
            }

            // -- output any meta tags
            string description = CmsContext.currentPage.SearchEngineDescription.Trim();
            if (description != "")
            {
                html.Append(Environment.NewLine);
                html.Append("<meta name=\"description\" content=\"" + Server.HtmlEncode(description) + "\">");
                html.Append(Environment.NewLine);
            }
                    
            // -- generator meta tag (bug #134)
            html.Append("<meta name=\"generator\" content=\"HatCMS "+CmsContext.currentHatCMSCoreVersion.ToString()+" - Open Source Content Management\" />");

            return html.ToString();
        }


        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {            
            writer.Write(getHtml());                             
        }


        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}
		#endregion
	}
}
