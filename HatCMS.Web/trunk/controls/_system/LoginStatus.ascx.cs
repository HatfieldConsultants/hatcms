namespace HatCMS.Controls
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Collections.Specialized;

    using Hatfield.Web.Portal;

	/// <summary>
	///		Summary description for LogonStatus.
	/// </summary>
	public partial class LoginStatus : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			if(CmsContext.currentWebPortalUser != null)
			{
                CmsPage page = CmsContext.currentPage;
                NameValueCollection paramList = new NameValueCollection();
                paramList.Add("target", page.Id.ToString());
                paramList.Add("action", "logoff");
                string logoffUrl = CmsContext.getUrlByPagePath(CmsConfig.getConfigValue("LoginPath","/_login"), paramList);

                writer.WriteLine("You are logged in as " + CmsContext.currentWebPortalUser.UserName + " (<a href=\"" + logoffUrl + "\">logoff</a>)");
			}
			else if (! PageUtils.ClientIsMakingOfflineVersion)
			{
				CmsPage page = CmsContext.currentPage;
				NameValueCollection paramList = new NameValueCollection();
				paramList.Add("target",page.Id.ToString());
                string logonUrl = CmsContext.getUrlByPagePath(CmsConfig.getConfigValue("LoginPath", "/_login"), paramList);

				writer.WriteLine("Not logged on: <a href=\""+logonUrl+"\">log on here</a>");
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
