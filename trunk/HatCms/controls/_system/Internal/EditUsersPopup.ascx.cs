namespace HatCMS.controls
{
	using System;
	using System.Text;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Collections.Specialized;
    using Hatfield.Web.Portal;
    using System.Collections.Generic;
    using HatCMS.Placeholders;
    using Hatfield.Web.Portal.Authentication;

	/// <summary>
	///		Summary description for CreateNewPagePopup.
	/// </summary>
    public partial class EditUsersPopup : System.Web.UI.UserControl
	{

        

        protected void Page_Load(object sender, System.EventArgs e)
        {
            

        } // page_load

        private enum PageDisplayMode { ListUsers, AddUser, EditSelectedUser };

        private WebPortalUserRole[] getAllAvailableRoles()
        {
            List<WebPortalUserRole> ret = new List<WebPortalUserRole>();
            string adminUserRoleName = CmsConfig.getConfigValue("AdminUserRole", "Administrator");
            ret.Add(WebPortalUserRole.Fetch(adminUserRoleName));
            string authorUserRoleName = CmsConfig.getConfigValue("AuthorAccessUserRole", "Author");
            if (String.Compare(adminUserRoleName, authorUserRoleName, true) != 0)
            {
                ret.Add(WebPortalUserRole.Fetch(authorUserRoleName));
            }
            
            string nothing = Guid.NewGuid().ToString();
            CmsZone homePageZone = (new CmsZoneDb()).fetchByPage(CmsContext.HomePage);
            WebPortalUser dummyPublicUser = new WebPortalUser(-1, nothing, nothing, new WebPortalUserRole[] { WebPortalUserRole.dummyPublicUserRole });
            bool requireAnonLogin = homePageZone.canRead(dummyPublicUser);            
            
            string loginRole = CmsConfig.getConfigValue("LoginUserRole", nothing);
            if (requireAnonLogin && loginRole != nothing && String.Compare(loginRole, authorUserRoleName, true) != 0 && String.Compare(loginRole, adminUserRoleName, true) != 0)
            {
                ret.Add(WebPortalUserRole.Fetch(loginRole));
            }
            return ret.ToArray();
        }


        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if (!CmsContext.currentUserIsSuperAdmin)
            {
                writer.Write("Access denied");
                return;
            }

            StringBuilder html = new StringBuilder();
            html.Append("<h1>Manage Website Users</h1>");

            PageDisplayMode displayMode = (PageDisplayMode)PageUtils.getFromForm("display", typeof(PageDisplayMode), PageDisplayMode.ListUsers);
            switch (displayMode)
            {
                case PageDisplayMode.ListUsers:
                    html.Append(getListUsersDisplay());
                    break;
                case PageDisplayMode.AddUser:
                    html.Append(getEditUserDisplay(Int32.MinValue, CmsContext.currentPage));
                    break;
                case PageDisplayMode.EditSelectedUser:
                    html.Append(getEditUserDisplay(PageUtils.getFromForm("uid", Int32.MinValue), CmsContext.currentPage));
                    break;
            } // switch

            writer.Write(html.ToString());
        } // Render

        private string getListUsersDisplay()
        {
            StringBuilder html = new StringBuilder();

            WebPortalUserRole[] allRoles = getAllAvailableRoles();
            
            CmsPage currPage = CmsContext.currentPage;
            
            html.Append("<table cellpadding=\"2\">"+Environment.NewLine);
            foreach (WebPortalUserRole role in allRoles)
            {
                html.Append("<tr><td style=\"background-color: #CCC;\" colspan=\"2\"><strong>" + role.Name + " - " + role.Description + "</strong></td></tr>" + Environment.NewLine);
                WebPortalUser[] users = WebPortalUser.FetchAll(role, CmsPortalApplication.GetInstance());
                if (users.Length == 0)
                {
                    html.Append("<tr><td><em>there are no users with this security level</em></td></tr>" + Environment.NewLine);
                }
                else
                {
                    foreach (WebPortalUser user in users)
                    {
                        string editUrl = getPageDisplayUrl(user, currPage, PageDisplayMode.EditSelectedUser );
                        html.Append("<tr><td>" + user.UserName + "</td><td><a href=\"" + editUrl + "\">edit</a></td></tr>" + Environment.NewLine);
                    } // foreach user
                }

            } // foreach role

            html.Append("</table>");

            html.Append("(<a href=\"" + getPageDisplayUrl(new WebPortalUser(), currPage, PageDisplayMode.AddUser) + "\">add a new user</a>)");

            return html.ToString();
        }

        private string getPageDisplayUrl(WebPortalUser user, CmsPage currentPage, PageDisplayMode displayMode)
        {
            Dictionary<string, string> pageParams = new Dictionary<string, string>();
            pageParams.Add("display", Enum.GetName(typeof(PageDisplayMode), displayMode));
            pageParams.Add("uid", user.uid.ToString());
            return currentPage.getUrl(pageParams);
        }

        private string getEditUserDisplay(int userId, CmsPage page)
        {
            string _errorMessage = "";
            string _successMessage = "";
            
            bool isEditingExisting = false;
            WebPortalUser user = WebPortalUser.FetchUser(userId, CmsPortalApplication.GetInstance());
            if (user != null)            
                isEditingExisting = true;            
            else
                user = new WebPortalUser();

            string userRole = "";
            if (user.userRoles.Length > 0)
            {
                userRole = getBestMatchingUserRoleName(getAllAvailableRoles(), user.userRoles);
            }

            string formaction = PageUtils.getFromForm("formaction", "");
            if (string.Compare(formaction, "saveupdates", true) == 0)
            {
                string un = PageUtils.getFromForm("username", user.UserName);
                if (un.Trim() == "")
                {
                    _errorMessage = "Please specify a username";
                    
                }

                if (_errorMessage == "" && !isEditingExisting && WebPortalUser.FetchUser(un, CmsPortalApplication.GetInstance()) != null)
                {
                    _errorMessage = "A user with the username '" + un + "' already exists. Please use another username.";
                    
                }

                string pw = PageUtils.getFromForm("password", user.Password);
                if (_errorMessage == "" && pw.Trim() == "")
                {
                    _errorMessage = "Blank passwords are not allowed.";                    
                }
                /*
                if (pw1 != pw2)
                {
                    errorMessage = "Passwords do not match.";
                    return;
                }*/

                string selRole = PageUtils.getFromForm("roles", userRole);
                if (selRole.Trim() == "")
                {
                    _errorMessage = "Please select the user's access level";
                }

                if (_errorMessage == "" && WebPortalUserRole.Fetch(selRole) == null)
                {
                    _errorMessage = "Invalid security group '" + selRole + "' (does not exist)";                    
                }


                if (_errorMessage == "")
                {
                    user.UserName = un;
                    user.Password = pw;

                    bool b = false;

                    user.ClearAllUserRoles();
                    user.AddUserRole(WebPortalUserRole.Fetch(selRole));
                    b = user.SaveToDatabase();
                    if (!b)
                        _errorMessage = "Fatal Error: could not save user to database.";
                    else
                        _successMessage = "User '" + un + "' has been saved.";

                }
            } // if saveUpdates

            StringBuilder html = new StringBuilder();
            string formId = "EditUsers";
            html.Append(page.getFormStartHtml(formId));
            if (_errorMessage != "")
            {
                html.Append("<p style=\"color: red;\">" + _errorMessage + "</p>");
            }
            if (_successMessage != "")
            {
                html.Append("<p style=\"color: green;\">" + _successMessage + "  - <a href=\"" + getPageDisplayUrl(new WebPortalUser(), page, PageDisplayMode.ListUsers) + "\">back to user list</a></p>");
            }
            html.Append("<table>");
            // -- User name
            html.Append("<tr><td>Username: </td><td>" + Environment.NewLine);
            if (!isEditingExisting)
                html.Append(PageUtils.getInputTextHtml("username", "username", user.UserName, 30, 255));
            else
                html.Append(user.UserName);
            html.Append("</td></tr>" + Environment.NewLine);

            // -- Password
            html.Append("<tr><td>Password: </td><td>");
            html.Append(PageUtils.getInputTextHtml("password", "password", user.Password, 30, 255));
            html.Append("</td></tr>" + Environment.NewLine);

            
            
            NameValueCollection roleOpts = new NameValueCollection();
            foreach (WebPortalUserRole role in getAllAvailableRoles())
            {
                roleOpts.Add(role.Name, role.Name + " - " + role.Description);                
            }
            html.Append("<tr><td>Access Level: </td><td>");
            html.Append(PageUtils.getRadioListHtml("roles", "role", roleOpts, userRole, "", "<br />")); 
            html.Append("</td></tr>" + Environment.NewLine);

            html.Append("</table>");

            html.Append(PageUtils.getHiddenInputHtml("formaction", "saveupdates"));
            html.Append(PageUtils.getHiddenInputHtml("uid", userId.ToString()));
            html.Append(PageUtils.getHiddenInputHtml("display", Enum.GetName(typeof(PageDisplayMode), PageDisplayMode.EditSelectedUser)));

            html.Append("<input type=\"submit\" value=\"save\">");
            html.Append(" <input type=\"button\" value=\"cancel\" onclick=\"window.location = '" + page.Url + "'\">");
            html.Append(page.getFormCloseHtml(formId));

            if (isEditingExisting)
            {
                formId = "delUser";
                html.Append(page.getFormStartHtml(formId));
                html.Append(PageUtils.getHiddenInputHtml("formaction", "deleteuser"));
                html.Append(PageUtils.getHiddenInputHtml("uid", userId.ToString()));
                html.Append(PageUtils.getHiddenInputHtml("display", Enum.GetName(typeof(PageDisplayMode), PageDisplayMode.EditSelectedUser)));

                html.Append("<p align=\"right\"><input type=\"submit\" value=\"delete user\"></p>");
                html.Append(page.getFormCloseHtml(formId));
            }
            
            return html.ToString();
        }


        /// <summary>
        /// returns string.empty is nothing matches
        /// </summary>
        /// <param name="rolesToReturn"></param>
        /// <param name="rolesToChooseFrom"></param>
        /// <returns></returns>
        private string getBestMatchingUserRoleName(WebPortalUserRole[] rolesToReturn, WebPortalUserRole[] rolesToChooseFrom)
        {
            if (rolesToReturn.Length == 1)
                return rolesToReturn[0].Name;

            foreach (WebPortalUserRole roleToReturn in rolesToReturn)
            {
                foreach (WebPortalUserRole roleToTest in rolesToChooseFrom)
                {
                    if (String.Compare(roleToReturn.Name, roleToTest.Name, true) == 0)
                        return roleToReturn.Name;
                }
            } // foreach
            return "";
        }



		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			// Response.Write("onInit!!!");
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

