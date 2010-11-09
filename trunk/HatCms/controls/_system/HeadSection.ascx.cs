namespace HatCMS.controls
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
    using HatCMS.Placeholders;

	/// <summary>
	///		Summary description for HeadSection.
	/// </summary>
	public partial class HeadSection : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
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
