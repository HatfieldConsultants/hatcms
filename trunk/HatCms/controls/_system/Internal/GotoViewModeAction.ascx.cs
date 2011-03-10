namespace HatCMS.Controls
{
	using System;
	using System.Collections.Specialized;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

    using Hatfield.Web.Portal;

	/// <summary>
	///		Summary description for switchToEdit.
	/// </summary>
	public partial class gotoViewMode : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
						
			int targetPageId = PageUtils.getFromForm("target",Int32.MinValue);
            CmsPage targetPage = CmsContext.getPageById(targetPageId);

			string appendToTargetUrl = PageUtils.getFromForm("appendToTargetUrl","");
			NameValueCollection paramList = new NameValueCollection();
			if (appendToTargetUrl.Trim() != "")
			{
				// -- split by | (pipe), and then by = (equals).
				string[] parts = appendToTargetUrl.Split(new char[] {'|'});
				foreach(string s in parts)
				{
					string[] subParts = s.Split(new char[]{'='});
					if (subParts.Length == 2)
						paramList.Add(subParts[0], subParts[1]);
				}
			}

            CmsContext.setEditModeAndRedirect(CmsEditMode.View, targetPage, paramList);
			
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
