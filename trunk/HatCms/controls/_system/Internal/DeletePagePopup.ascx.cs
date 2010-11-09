namespace HatCMS.controls
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
    using Hatfield.Web.Portal;

	/// <summary>
	///		Summary description for DeletePagePopup.
	/// </summary>
	public partial class DeletePagePopup : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{

            if (! CmsContext.currentUserCanAuthor)
			{
				writer.WriteLine("Access Denied");
				return;
			}
			
			string html = "<p><center>";
            int targetPageId = PageUtils.getFromForm("target", Int32.MinValue);
            if (targetPageId < 0)
			{
				html = html + "<span style=\"color: red\">Invalid Target parameter. No page to delete.</span>";
			}
			else
			{
                if (!CmsContext.pageExists(targetPageId))
				{
					html = html + "<span style=\"color: red\">Target page does not exist. No page to delete.</span>";
				}
				else
				{
					CmsPageDb db = new CmsPageDb();
                    CmsPage page = CmsContext.getPageById(targetPageId);
					bool success = db.deletePage(page);
					if (!success)
					{
						html = html + "<span style=\"color: red\">Database error: could not delete page.</span>";
					}
					else
					{
						string script = "<script>"+Environment.NewLine;
						script = script + "function go(url){"+Environment.NewLine;
						script = script + "opener.location.href = url;"+Environment.NewLine;
						script = script + "window.close();\n}";
						script = script + "</script>"+Environment.NewLine;
						html = html + "<span style=\"color: green; font-weight: bold;\">The Page has successfully been deleted.</span>";
                        html = html + "<p><input type=\"button\" onclick=\"go('" + page.ParentPage.Url + "');\" value=\"close this window\">";
						writer.WriteLine(script+html);
						return;
					}
				}
			}
			html = html + "<p><input type=\"button\" onclick=\"window.close();\" value=\"close this window\">";
			html = html + "</center>";
			writer.WriteLine(html);
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
