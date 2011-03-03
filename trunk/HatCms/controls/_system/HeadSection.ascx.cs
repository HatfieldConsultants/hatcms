namespace HatCMS.controls
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
            ret.Add(new CmsConfigItemDependency("PrinterAndPdfVer.pdfVer"));
            ret.Add(new CmsConfigItemDependency("PrinterAndPdfVer.printerVer"));
            ret.Add(new CmsConfigItemDependency("PrinterAndPdfVer.printerCss"));
            ret.Add(new CmsConfigItemDependency("PrinterAndPdfVer.placeAfterDom"));
            ret.Add(new CmsConfigItemDependency("PrinterAndPdfVer.pdfIcon"));
            ret.Add(new CmsConfigItemDependency("PrinterAndPdfVer.printerIcon"));

            ret.Add(CmsFileDependency.UnderAppPath("js/_system/printerAndPdfVersion.js", new DateTime(2011, 03, 03)));
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

                cssUrl = cssUrl.Replace("~", CmsContext.ApplicationPath);
                html.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + cssUrl + "\" />" + Environment.NewLine);
                
                // -- note: do not use @import for CSS; The problem is if you use "@import" IE downloads the CSS later and progressive rendering is delayed. 
                // --       http://developer.yahoo.net/blog/archives/2007/07/high_performanc_4.html
                // html += "<style type=\"text/css\">@import url(" + cssUrl + ");</style>";
            }

            // -- output any meta tags
            string description = CmsContext.currentPage.SearchEngineDescription.Trim();
            if (description != "")
            {
                html.Append(Environment.NewLine);
                html.Append("<meta name=\"description\" content=\"" + Server.HtmlEncode(description) + "\">");
                html.Append(Environment.NewLine);
            }

            addScriptForPrinterAndPdfVersion();
            

            return html.ToString();
        }

        /// <summary>
        /// Load the javascript during window.onload
        /// - if print flag is 0 (or not set), render PDF and Printer icons
        /// - if print flag is 1, render the page as a printer-friendly version
        /// </summary>
        /// <returns></returns>
        protected void addScriptForPrinterAndPdfVersion()
        {
            bool printerVer = CmsConfig.getConfigValue("PrinterAndPdfVer.printerVer", false);
            bool pdfVer = CmsConfig.getConfigValue("PrinterAndPdfVer.pdfVer", false);
            if (printerVer == false && pdfVer == false)
                return;

            CmsPage currentPage = CmsContext.currentPage;

            currentPage.HeadSection.AddJavascriptFile("js/_system/printerAndPdfVersion.js");
            currentPage.HeadSection.AddJSStatements("_printerVer = " + printerVer.ToString().ToLower() + ";" + EOL);                                    

            if (PageUtils.getFromForm("print", 0) == 1)
            {
                currentPage.HeadSection.AddJSStatements("_printerCss = '" + CmsConfig.getConfigValue("PrinterAndPdfVer.printerCss", "").Replace("~", CmsContext.ApplicationPath) + "';" + EOL);
                currentPage.HeadSection.AddJSOnReady("renderAsPrintVersion( _printerVer, _printerCss );");
                
            }
            else
            {
                currentPage.HeadSection.AddJSStatements("_pdfVer = " + pdfVer.ToString().ToLower() + ";" + EOL);
                currentPage.HeadSection.AddJSStatements("_placeAfterDom = '" + CmsConfig.getConfigValue("PrinterAndPdfVer.placeAfterDom", "") + "';" + EOL);
                currentPage.HeadSection.AddJSStatements("_printerIcon = '" + CmsConfig.getConfigValue("PrinterAndPdfVer.printerIcon", "").Replace("~", CmsContext.ApplicationPath) + "';" + EOL);
                currentPage.HeadSection.AddJSStatements("_pdfIcon = '" + CmsConfig.getConfigValue("PrinterAndPdfVer.pdfIcon", "").Replace("~", CmsContext.ApplicationPath) + "';" + EOL);
                currentPage.HeadSection.AddJSOnReady("addPrinterAndPdfIcon( _printerVer, _printerIcon, _pdfVer, _pdfIcon, _placeAfterDom ); " + EOL);
            }            
            
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
