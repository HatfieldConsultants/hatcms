namespace HatCMS.controls
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Collections.Specialized;

	/// <summary>
	///		Summary description for FloatingEditMenu.
	/// </summary>
	public partial class NoEditFloatingEditMenu : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			if (CmsContext.currentWebPortalUser != null && CmsContext.currentEditMode == CmsEditMode.View
				&& CmsContext.currentUserCanAuthor)
			{
				string html = "";
				CmsPage page = CmsContext.currentPage;
				
                page.HeadSection.AddJavascriptFile("js/_system/FloatingEditMenu.js");                

				html = html + "<div id=\"editConsole\" ondblclick=\"OpenCloseDiv('editConsoleOptions')\" style=\"PADDING-RIGHT: 0px; PADDING-LEFT: 0px; Z-INDEX: 10; LEFT: 580px; PADDING-BOTTOM: 0px; WIDTH: 200px; PADDING-TOP: 0px; POSITION: absolute; TOP: 30px; BACKGROUND-COLOR: transparent; TEXT-ALIGN: left\">";
				html = html + "<table class=\"wbcedit\" onmouseover=\"drag('editConsole')\" onfocus=\"this.blur()\" cellSpacing=\"0\" cellPadding=\"0\" border=\"0\">";
				html = html + "<tr>";
				html = html + "<td style=\"cursor: move;  font-family: arial; padding-left: 5px; padding-right: 5px; background-color: green; text-align: center;\">";
				html = html + "<span style=\"font-weight: bold;\">Edit Menu</span><span style=\"font-size: 8pt;\"><br><nobr>(drag here to move menu)</span></nobr></TD>";
				html = html + "</tr>";
				html = html + "</table>";
				html = html + "<div class=\"wbcedit\" id=\"editConsoleOptions\">";
				html = html + "<table borderColor=\"green\" cellSpacing=\"0\" cellPadding=\"5\" width=\"100%\" border=\"2\">";
				html = html + "	<tr>";
				html = html + "<td nowrap=\"nowrap\" style=\"background: yellow; font-size: 10pt; font-family: arial;\" >";							

				// -- No Edit notice
				NameValueCollection paramList = new NameValueCollection();
				html = html + "<strong>This page can not be edited</strong><br>";				

				// -- Logoff link
				paramList.Clear();			
				paramList.Add("target",page.ID.ToString());
				paramList.Add("action","logoff");
				string LogoffUrl = CmsContext.getUrlByPagePath(CmsConfig.getConfigValue("LoginPath","/_admin/login"), paramList);
				html = html + "<a href=\""+LogoffUrl+"\"><strong>Logoff</strong></a><br>";


				html = html + "</td>";
				html = html + "</tr>";
				html = html + "</table>";
				html = html + "</div>";
				html = html + "</div>";
				writer.WriteLine(html);
			} // if logged on AND in View Mode
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
