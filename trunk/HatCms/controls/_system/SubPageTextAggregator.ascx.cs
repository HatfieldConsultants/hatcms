namespace HatCMS.Controls
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Text;
    using Hatfield.Web.Portal;

	/// <summary>
	///		Summary description for SubPageTextAggregator.
	/// </summary>
	public partial class SubPageTextAggregator : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			StringBuilder html = new StringBuilder();
			
			CmsPage parentPage = CmsContext.currentPage;
			foreach(CmsPage subPage in parentPage.ChildPages)
			{
                if (subPage.isVisibleForCurrentUser)
                {

                    html.Append("<li>");

                    string val = subPage.renderAllPlaceholdersToString(CmsContext.currentLanguage);
                    val = PageUtils.StripTags(val);
                    if (val.Length > 100)
                        val = val.Substring(0, 100) + " ...<br>";
                    html.Append(val);

                    html.Append("<br><a href=\"" + subPage.Url + "\">read this article</a>");
                    html.Append("</li>");
                }

			} // foreach subPage

			writer.WriteLine(html.ToString());
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
