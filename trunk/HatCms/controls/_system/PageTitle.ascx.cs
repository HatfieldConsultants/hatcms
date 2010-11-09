namespace HatCMS.controls
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Collections.Specialized;
    using HatCMS.Placeholders;

	/// <summary>
	///		Summary description for LogonStatus.
	/// </summary>
	public partial class PageTitleControl : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
            CmsPage p = CmsContext.currentPage;
            string title = p.Title;
            if (title.Trim() == "")
                title = p.MenuTitle;
            
            // -- @TODO: once AlternateViews is implemented, this should be updated to use that mechanism instead of this implementation.

            if (PageFiles.isPageFilesPage(p) && PageFiles.currentViewRenderMode == PageFiles.RenderMode.SingleFile)
            {
                PageFilesItemData fileData = PageFiles.getCurrentPageFilesItemData();
                title = fileData.Title;
            }
            // -- Contacts
            else if (Contacts.isContactsPage(p) && Contacts.currentViewRenderMode == Contacts.PlaceholderDisplayMode.SingleContact)
            {
                ContactData c = Contacts.getCurrentContactData();
                //@@TODO: this should use Contacts.getNameDisplayOutput().
                title = c.firstName + " " + c.lastName;
            }
            

            writer.WriteLine(title);
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
