namespace HatCMS.Controls
{
	using System;
    using System.Text;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using Hatfield.Web.Portal.Authentication;
    using Hatfield.Web.Portal;

	/// <summary>
	///		Summary description for Login.
	/// </summary>
	public partial class Login : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			string notFound = Guid.NewGuid().ToString();
            int targetPageId = PageUtils.getFromForm("target",Int32.MinValue);
            if (targetPageId < 0)
                targetPageId = CmsContext.HomePage.ID;

            CmsPage targetPage = CmsContext.getPageById(targetPageId);


            string ReturnUrl = PageUtils.getFromForm("ReturnUrl", "");

			CmsPage page = CmsContext.currentPage;

			string _errorMessage = "";

			string action = PageUtils.getFromForm("action","");
			string un = PageUtils.getFromForm("un","");
			string pw = PageUtils.getFromForm("pw","");

			
			// -- logoff action
			if (CmsContext.currentUserIsLoggedIn && action.ToLower().Trim() == "logoff")
			{
				WebPortalAuthentication.SignOut();
                CmsContext.setEditModeAndRedirect(CmsEditMode.View, targetPage);
			}

			// -- login action
			if (action.ToLower().Trim() == "login")
			{				
				WebPortalAuthentication auth = new WebPortalAuthentication(un,pw);
                string[] validRoleNames = new string[] { CmsConfig.getConfigValue("LoginUserRole", new Guid().ToString()), CmsConfig.getConfigValue("AuthorAccessUserRole", "Author"), CmsConfig.getConfigValue("AdminUserRole", "Administrator") };
                if (auth.CheckAuthentication(System.Web.HttpContext.Current, validRoleNames, CmsPortalApplication.GetInstance()))
                {
                    // load cookie for 100 days
                    auth.loadGroupsAndCookie(System.Web.HttpContext.Current, 60 * 24 * 100, true, CmsPortalApplication.GetInstance());
                    // redirect
                    if (ReturnUrl.Trim() != "")
                    {
                        Response.Redirect(ReturnUrl);
                    }
                    else
                    {                                                
                        CmsContext.setEditModeAndRedirect(CmsEditMode.View, targetPage);
                    }
                }
				_errorMessage = "Invalid Username or Password. Please try again.";
			}

            // string onloadJS = CmsPage.getOnloadJavascript("setLoginFocus");
            // use eventListeners wherever possible so that any other onload events are also fired.
			string onloadJS = @"				
				    // setLoginFocus			
					var el = document.getElementById('input_login_un');	
					el.focus();
				
			";

            page.HeadSection.AddJSOnReady(onloadJS);

            StringBuilder html = new StringBuilder();
            string formId = "loginForm";
            html.Append(page.getFormStartHtml(formId));
            html.Append("<p><strong>Login: </strong><br>");
			html.Append("<table>");
			if (_errorMessage != "")
			{
				html.Append("<tr>");
				html.Append("<td colspan=\"2\" align=\"center\">");
                html.Append("<span style=\"color: red;\">" + _errorMessage + "</span>");
				html.Append("</td>");
				html.Append("<tr>");
			}
			html.Append("<tr>");
            html.Append("<td>Username: </td><td><input id=\"input_login_un\" type=\"text\" value=\"" + un + "\" name=\"un\" size=\"40\"></td>");
			html.Append("</tr>");
			html.Append("<tr>");
			html.Append("<td>Password: </td><td><input type=\"password\" name=\"pw\" size=\"40\"></td>");
			html.Append("</tr>");
			html.Append("</table>");
            html.Append("<input type=\"hidden\" name=\"target\" value=\"" + targetPageId.ToString() + "\">");
            html.Append("<input type=\"hidden\" name=\"ReturnUrl\" value=\"" + ReturnUrl + "\">");
            html.Append("<input type=\"hidden\" name=\"action\" value=\"login\">");
            html.Append("<input type=\"submit\" value=\"login\">");

            html.Append(page.getFormCloseHtml(formId));
			writer.WriteLine(html.ToString());		
			
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
