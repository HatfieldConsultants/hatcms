namespace HatCMS.controls
{
	using System;
    using System.Collections.Generic;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

	/// <summary>
	///		Summary description for StartEditForm.
	/// </summary>
	public partial class StartEditForm : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/beforeUnload.js"));
            return ret.ToArray();
        }


        public static string FormId = "HatCmsEditForm";


        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if (CmsContext.currentUserCanAuthor && CmsContext.currentEditMode == CmsEditMode.Edit)
            {
                writer.WriteLine(CmsContext.currentPage.getFormStartHtml(FormId, "submitting = true;"));
                CmsContext.currentPage.HeadSection.AddJavascriptFile("js/_system/beforeUnload.js");
            }
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
