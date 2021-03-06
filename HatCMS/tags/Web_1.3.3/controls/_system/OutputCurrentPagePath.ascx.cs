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
	public partial class OutputCurrentPagePath : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            CmsPage page = CmsContext.currentPage;

            writer.Write(page.Path);
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
