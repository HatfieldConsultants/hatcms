namespace HatCMS.Controls._system
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Text;
    using HatCMS.Placeholders;

	/// <summary>
	///		Summary description for Breadcrumb.
	/// </summary>
	public partial class Breadcrumb : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

        /// <summary>
        /// For the breadcrumb "You are here" text, retrieve from config file according to current language.
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string getYouAreHereText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("Breadcrumb.YouAreHere", "You are here", lang);
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            CmsPage page = CmsContext.currentPage;
            StringBuilder sb = new StringBuilder();

            bool first = true;            

            // -- handle all parent pages
            while (page.ID != -1)
            {
                string displayTitle = page.MenuTitle;
                if (displayTitle == "")
                    displayTitle = page.Title;

                if (first)
                {
                    sb.Insert(0, "<strong>" + displayTitle + "</strong>");
                    first = false;
                }
                else
                {
                    sb.Insert(0, "<a href=\"" + page.Url + "\">" + displayTitle + "</a> > ");
                }
                page = page.ParentPage;
            }

            string youAreHere = Breadcrumb.getYouAreHereText(CmsContext.currentLanguage);
            writer.Write("<div id=\"breadcrumb\">" + youAreHere + " : " + sb.ToString() + "</div>");
        } // Render


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
