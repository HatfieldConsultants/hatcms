namespace HatCMS
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Text;

	/// <summary>
	///		Summary description for Breadcrumb.
	/// </summary>
	public partial class SideNavigation : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			
		}

		/// <summary>
		/// Renders the SideNavigation control to the HtmlTextWriter
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			CmsPage page = CmsContext.HomePage;
			
			int maxLevels = 100;
            maxLevels = CmsConfig.getConfigValue("SideNavMaxLevels", maxLevels);
			
			string html = "<div id=\"SideNav\">\n<ul class=\"level0\">"+Environment.NewLine+recursiveRender(page,0,maxLevels)+"\n</ul>\n</div>";
			
			writer.Write(html);
		} // Render

		private bool childIsSelected(CmsPage page)
		{
            return page.isChildSelected();
		} // childIsSelected

		private string recursiveRender(CmsPage page, int currentLevel, int maxLevel)
		{
			StringBuilder html = new StringBuilder();
			if(page.Id == -1 || currentLevel > maxLevel)
				return "";

            if (! page.isVisibleForCurrentUser)
				return "";
		
						
			string CSSClass = ""; // 
			if (page.Path == CmsContext.currentPage.Path || (currentLevel == maxLevel && childIsSelected(page)))
			{
				CSSClass = "class=\"selected\"";
			}

			string title = page.MenuTitle;
			if (title == "")
				title = page.Title;
			
			html.Append("<li><a "+CSSClass+" href=\""+page.Url+"\">"+title+"</a></li>");
			
			if (page.ChildPages.Length > 0)
			{
				html.Append("<ul class=\"level"+(currentLevel+1).ToString()+"\">"+Environment.NewLine);
				foreach(CmsPage subPage in page.ChildPages)
				{
					html.Append(recursiveRender(subPage, currentLevel+1, maxLevel));
				}
				html.Append("</ul>"+Environment.NewLine);
			}
			
			return html.ToString();
		} // recursiveRender


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
