namespace HatCMS.controls
{
	using System;
    using System.Collections.Generic;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
    using Hatfield.Web.Portal;

	/// <summary>
	///		Summary description for SortSubPagesPopup.
	/// </summary>
	public partial class SortSubPagesPopup : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/sortSelectList.js"));
            return ret.ToArray();
        }


		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{

            
			
			// -- get the target page
			int targetPageId = PageUtils.getFromForm("target",Int32.MinValue);
            if (targetPageId < 0)
			{
				writer.WriteLine("Error: invalid target page!");
				return;
			}
            CmsPage page = CmsContext.getPageById(targetPageId);

            if (!page.currentUserCanWrite)
            {
                writer.WriteLine("Access Denied");
                return;
            }

			string html = "<Strong>Sort Sub-Pages</Strong><br>";

			// -- process the submitted form
			string action = PageUtils.getFromForm("action","");
			if (action.ToLower() == "dosort")
			{
				string[] newOrderIds = PageUtils.getFromForm("order");
				// writer.WriteLine(String.Join(",",newOrderIds)+"<p>");
				for(int i = 0; i < newOrderIds.Length; i++)
				{
					int id = Convert.ToInt32(newOrderIds[i]);
					CmsPage tempPage = CmsContext.getPageById(id);
					if (tempPage.ID != -1)
					{
						tempPage.setSortOrdinal(i);
					}
				} // for

				html = html + "<script>"+Environment.NewLine;
				html = html + "function go(url){"+Environment.NewLine;
				html = html + "opener.location.href = url;"+Environment.NewLine;
				html = html + "window.close();\n}";
				html = html + "</script>"+Environment.NewLine;
				html = html + "<p><center>Sub-Pages Successfully Sorted<p>";
				html = html + "<input type=\"button\" value=\"close this window\" onclick=\"go('"+page.Url+"')\">";				
				html = html + "</center>";

				writer.WriteLine(html);
				return;

			} // if action = doSort


			// -- render the form
            CmsPage currentPage = CmsContext.currentPage;
            
            currentPage.HeadSection.AddJavascriptFile("js/_system/sortSelectList.js");
            

            string formId = "sortSubPagesForm";
            html = html + currentPage.getFormStartHtml(formId, "selectall('order');");
			html = html + "<table align=\"center\" border=\"0\">";
			html = html + "<tr>";
			html = html + "<td valign=\"top\">";

			int size = Math.Min((int)page.ChildPages.Length,20);
			html = html + "<select name=\"order\" size=\""+size.ToString()+"\" multiple=\"multiple\" id=\"order\" onmousewheel=\"mousewheel(this);\" ondblclick=\"selectnone(this);\">";
				// <option value="13" id="a01">0Red 1</option>
			foreach(CmsPage childPage in page.ChildPages)
			{
                html = html + "<option value=\"" + childPage.ID + "\">" + childPage.Title + "</option>" + Environment.NewLine;
			}
				
			html = html + "</select>";
			html = html + "</td>";
			html = html + "<td valign=\"middle\">";
			html = html + "<input type=\"button\" value=\"Move to Top\" onclick=\"top('order');\" style=\"width: 100px;\" /><br><br>";
			html = html + "<input type=\"button\" value=\"Move Up\" onclick=\"up('order');\"  style=\"width: 100px;\" /><br>";
			html = html + "<input type=\"button\" value=\"Move Down\" onclick=\"down('order');\"  style=\"width: 100px;\" /><br><br>";
			html = html + "<input type=\"button\" value=\"Move to Bottom\" onclick=\"bottom('order');\"  style=\"width: 100px;\" /><br><br>";
			html = html + "</td>";
			html = html + "</tr>";
			html = html + "</table>";
			html = html + "<input type=\"hidden\" name=\"action\" value=\"doSort\">";
			html = html + "<input type=\"hidden\" name=\"target\" value=\""+targetPageId.ToString()+"\">";
			
			html = html + "<input type=\"submit\" value=\"Save Order\">";
            html = html + currentPage.getFormCloseHtml(formId);
			

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
