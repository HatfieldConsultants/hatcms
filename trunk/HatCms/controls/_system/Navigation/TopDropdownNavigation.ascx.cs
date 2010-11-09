namespace HatCMS.controls._system
{
	using System;
    using System.Collections.Generic;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Text;

	/// <summary>
	///		Summary description for Breadcrumb.
	/// </summary>
	public partial class TopDropdownNavigation : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			
		}

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/TopNav.js"));
            return ret.ToArray();
        }


		/// <summary>
		/// Renders the SideNavigation control to the HtmlTextWriter
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			CmsPage homePage = CmsContext.HomePage;
			
			int maxLevels = 100;
            maxLevels = CmsConfig.getConfigValue("TopNavMaxLevels", maxLevels);						
			
			string EOL = Environment.NewLine;
			StringBuilder html = new StringBuilder();
			html.Append("<div id=\"TopNav\">"+EOL);

			if (!CmsContext.currentPage.HeadSection.isBlockRegisteredForOutput("TopDropdownNavigation"))
			{
                CmsContext.currentPage.HeadSection.registerBlockForOutput("TopDropdownNavigation");
                CmsContext.currentPage.HeadSection.AddJavascriptFile("js/_system/TopNav.js");                
                CmsContext.currentPage.HeadSection.AddJSOnReady("TopNavInit();");                
			}
						

			html.Append("<ul id=\"TopNavList\">"+EOL);
			
			// -- select home page if the current page is hidden
			string HomeCSSClass = "";
            if (homePage.Path == CmsContext.currentPage.Path || ! CmsContext.currentPage.isVisibleForCurrentUser) // CmsContext.currentUserIsSuperAdmin ||
				HomeCSSClass = "class = \"mainCurrent\" ";

			string homeTitle = homePage.MenuTitle;
			if (homeTitle == "")
				homeTitle = homePage.Title;

            html.Append("<li><a " + HomeCSSClass + " id=\"TopNav_" + homePage.ID.ToString() + "\" href=\"" + homePage.Url + "\">" + homeTitle + "</a></li>" + EOL);
			foreach(CmsPage p in homePage.ChildPages)
			{
                if (p.isVisibleForCurrentUser)
				{
					string CSSClass = "";
					if (p.Path == CmsContext.currentPage.Path || childIsSelected(p))
						CSSClass = "class = \"mainCurrent\" ";
					
					string title = p.MenuTitle;
					if (title == "")
						title = p.Title;

                    html.Append("<li><a " + CSSClass + " id=\"TopNav_" + p.ID.ToString() + "\" href=\"" + p.Url + "\">" + title + "</a></li>" + EOL);
				}
			}
            html.Append("</ul>" + EOL);
			html.Append("</div>");
			
			// -- SubMenus
			foreach(CmsPage p in homePage.ChildPages)
			{
				if (p.ChildPages.Length > 0 && p.isVisibleForCurrentUser)
				{
					string id = p.ID.ToString();
                    html.Append("<div class=\"TopNavSub\" style=\"display:none;\" id=\"TopNav_sub" + id + "\">" + EOL);
					html.Append("<ul>"+EOL);
					foreach(CmsPage subPage in p.ChildPages)
					{
                        if (subPage.isVisibleForCurrentUser)
						{
							string subTitle = subPage.MenuTitle;
							if (subTitle == "")
								subTitle = subPage.Title;
                            html.Append("<li><a href=\"" + subPage.Url + "\">" + subTitle + "</a></li>" + EOL);
						}
					} // foreach
                    html.Append("</ul>" + EOL);
					html.Append("</div>");
				}
			} // foreach page under
			
			writer.Write(html.ToString());
		} // Render

		private bool childIsSelected(CmsPage page)
		{
            return page.isChildSelected();

        } // childIsSelected
		


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
