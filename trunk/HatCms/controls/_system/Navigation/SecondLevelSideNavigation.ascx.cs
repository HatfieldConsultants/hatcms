namespace HatCMS.Controls._system
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
	public partial class SecondLevelSideNavigation : System.Web.UI.UserControl
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
			CmsPage currentPage = CmsContext.currentPage;
			CmsPage homePage = CmsContext.HomePage;			

			if (currentPage.ID == homePage.ID)
			{				
				writer.Write("<div id=\"SideNav\"></div>");
				return;
			}

			// -- get the secondLevelMainPage
            CmsPage secondLevelMainPage = currentPage;
			while(secondLevelMainPage.Level > 1)
				secondLevelMainPage = secondLevelMainPage.ParentPage;
			
			int maxLevels = 100;
            maxLevels = CmsConfig.getConfigValue("SideNavMaxLevels", maxLevels);

            bool outputAllChildren = CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "OutputAllChildren", false);
			
			string html = "<div id=\"SideNav\">";

			if (secondLevelMainPage.ChildPages.Length > 0)
			{
                html += "<ul class=\"level0\">"+Environment.NewLine + recursiveRender(secondLevelMainPage, 0, maxLevels, outputAllChildren) + "\n</ul>"+Environment.NewLine;
			}
			html += "</div>";
			
			writer.Write(html);
		} // Render

		

		private string recursiveRender(CmsPage page, int currentLevel, int maxLevel, bool outputAllChildren)
		{
			StringBuilder html = new StringBuilder();
			if(page.ID == -1 || currentLevel > maxLevel)
				return "";

            if (! page.isVisibleForCurrentUser || ! page.ShowInMenu )
				return "";
		
			bool outputChildren = false;
            if (currentLevel < 1 || page.isChildSelected() || page.Path == CmsContext.currentPage.Path || outputAllChildren)
				outputChildren = true;
			
			string CSSClass = ""; // 
            if (page.Path == CmsContext.currentPage.Path || (currentLevel == maxLevel && page.isChildSelected()))
			{
                if (page.isSelfSelected())
                {
                    CSSClass = "selected current";
                }
                else
                {
                    CSSClass = "selected";
                }
			}

			string title = page.MenuTitle;
			if (title == "")
				title = page.Title;

            html.Append("<li class=\"level" + page.Level.ToString() + " " + CSSClass + "\">");
            html.Append("<a class=\"level" + page.Level.ToString() + " " + CSSClass + "\" href=\"" + page.Url + "\">");
            html.Append(title);
            html.Append("</a></li>");
			
			if (page.ChildPages.Length > 0 && outputChildren)
			{
				
				html.Append("<ul class=\"level"+(currentLevel+1).ToString()+"\">"+Environment.NewLine);
				foreach(CmsPage subPage in page.ChildPages)
				{
                    html.Append(recursiveRender(subPage, currentLevel + 1, maxLevel, outputAllChildren));
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
