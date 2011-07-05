using System;
using System.Web;
using System.Security;
using System.Security.Principal;
using System.Xml;
using System.Collections;
using System.IO;
using System.Web.UI;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using MySql.Data.MySqlClient;

namespace Hatfield.Web.Portal.Authentication
{
    /// <summary>
    /// Summary description for WebPortal.
    /// </summary>
    public class WebPortalAuthentication
    {
        public static char groupDelimiter = '|';

        private string _un;
        private string _pw;
        public WebPortalAuthentication(string un, string pw)
        {
            _un = un;
            _pw = pw;
        }

        public bool CheckAuthentication(HttpContext context, PortalApplication portalApp)
        {
            return WebPortalUser.CheckLogin(_un, _pw, portalApp);
        }

        /// <summary>
        /// if the user is in any one of the validRoleNames, authentication will proceed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="validRoleNames"></param>
        /// <returns></returns>
        public bool CheckAuthentication(HttpContext context, string[] validRoleNames, PortalApplication portalApp)
        {            
            if (WebPortalUser.CheckLogin(_un, _pw, portalApp))
            {
                WebPortalUser u = WebPortalUser.FetchUser(_un, portalApp);

                foreach (string requiredRoleName in validRoleNames)
                {
                    bool b = u.inRole(requiredRoleName);
                    if (b)
                    {
                        u.SetLastLoginInDatabaseToNow();
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckAuthentication(HttpContext context, string requiredRoleName, PortalApplication portalApp)
        {
            if (WebPortalUser.CheckLogin(_un, _pw, portalApp))
            {
                WebPortalUser u = WebPortalUser.FetchUser(_un, portalApp);
                bool b = u.inRole(requiredRoleName);
                if (b)
                {
                    u.SetLastLoginInDatabaseToNow();
                    return true;
                }
            }
            return false;
        }

        public void loadGroupsAndCookie(HttpContext context, int cookieTimeoutMinutes, bool persistCookie, PortalApplication portalApp)
        {
            // -- Retrieve the user's groups
            WebPortalUser user = WebPortalUser.FetchUser(_un, portalApp);
            WebPortalUserRole[] Roles = user.userRoles;
            string groups = "";
            for (int i = 0; i < Roles.Length; i++)
            {
                WebPortalUserRole role = Roles[i];
                groups = groups + role.Name;
                if (i < Roles.Length - 1)
                {
                    groups = groups + groupDelimiter;
                }
            } // for			


            // -- Create the authetication ticket
            FormsAuthenticationTicket authTicket =
                new FormsAuthenticationTicket(1,  // version
                _un,
                DateTime.Now,
                DateTime.Now.AddMinutes(cookieTimeoutMinutes),
                persistCookie, groups);

            // Now encrypt the ticket.
            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
            // Create a cookie and add the encrypted ticket to the
            // cookie as data.
            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            if (authTicket.IsPersistent)
                authCookie.Expires = authTicket.Expiration;

            Console.Write(authCookie.Path);
            Console.Write(authCookie.Domain);

            // Add the cookie to the outgoing cookies collection.
            context.Response.Cookies.Add(authCookie);

        } // loadGroupsAndCookie

        public static void processAuthenticateRequest(HttpContext context)
        {
            // Extract the forms authentication cookie
            string cookieName = FormsAuthentication.FormsCookieName;
            HttpCookie authCookie = context.Request.Cookies[cookieName];

            if (null == authCookie)
            {
                // There is no authentication cookie.
                return;
            }
            FormsAuthenticationTicket authTicket = null;
            try
            {
                authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch
            {
                // Log exception details (omitted for simplicity)
                return;
            }

            if (null == authTicket)
            {
                // Cookie failed to decrypt.
                return;
            }

            // When the ticket was created, the UserData property was assigned a
            // pipe delimited string of group names.
            string[] groups = authTicket.UserData.Split(new char[] { groupDelimiter });

            // Create an Identity object
            GenericIdentity id = new GenericIdentity(authTicket.Name, "WebPortalAuthentication");

            // This principal will flow throughout the request.
            GenericPrincipal principal = new GenericPrincipal(id, groups);
            // Attach the new principal object to the current HttpContext object
            context.User = principal;

        }

        public static void SignOut()
        {
            FormsAuthentication.SignOut();
        }

    }
}
