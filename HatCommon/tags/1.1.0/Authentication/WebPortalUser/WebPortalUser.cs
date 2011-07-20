using System;
using System.Security.Principal;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Hatfield.Web.Portal
{
    /// <summary>
    /// Summary description for WebPortalUser.
    /// </summary>
    public class WebPortalUser
    {
        public int uid;

        /// <summary>
        /// note: NEVER use the UserName as a primary key - always link to the uid!!
        /// </summary>
        public string UserName;
        public string Password;
        public string FullName;
        public string EmailAddress;
        public DateTime LastLogin;

        /// <summary>
        /// An ArrayList of WebPortalUserRole objects
        /// </summary>
        private List<WebPortalUserRole> Roles;

        private List<PortalApplicationPermission> ApplicationPermissions;

        /// <summary>
        /// Permissions for ALL applications. Use .hasPermissionTo() to
        /// see if a user has application specific permissions.
        /// </summary>
        public PortalApplicationPermission[] Permissions
        {
            get { return ApplicationPermissions.ToArray(); }
        }

        /// <summary>
        /// adds a permission to the user's Permissions list. Does NOT save this addition to the database.
        /// </summary>
        /// <param name="role"></param>
        public void AddUserPermission(PortalApplicationPermission permission)
        {
            foreach (PortalApplicationPermission p in Permissions)
            {
                if (p.ID == permission.ID)
                    return;
            }

            ApplicationPermissions.Add(permission);
        }

        public void RemoveUserPermission(PortalApplicationPermission permissionToRemove)
        {
            foreach (PortalApplicationPermission p in Permissions)
            {
                if (p.ID == permissionToRemove.ID)
                {
                    ApplicationPermissions.Remove(p);
                    return;
                }

            } // foreach
        }

        /// <summary>
        /// adds a role to the user's role list. Does NOT save this addition to the database.
        /// </summary>
        /// <param name="roles"></param>
        public void AddUserRoles(WebPortalUserRole[] roles)
        {
            foreach (WebPortalUserRole r in roles)
                AddUserRole(r);
        }

        /// <summary>
        /// adds a role to the user's role list. Does NOT save this addition to the database.
        /// </summary>
        /// <param name="role"></param>
        public void AddUserRole(WebPortalUserRole role)
        {
            foreach (WebPortalUserRole r in Roles)
            {
                if (r.RoleID == role.RoleID)
                    return;
            }

            Roles.Add(role);
        }

        /// <summary>
        /// note: does not update the database
        /// </summary>
        public void ClearAllUserRoles()
        {
            Roles.Clear();
        }

        public WebPortalUserRole[] userRoles
        {
            get
            {
                return Roles.ToArray();
            }
        }


        public WebPortalUser()
        {
            uid = -1;
            UserName = "";
            Password = "";
            FullName = "";
            EmailAddress = "";
            Roles = new List<WebPortalUserRole>();
            ApplicationPermissions = new List<PortalApplicationPermission>();
        } // constructor

        public WebPortalUser(int UserId, string userName, string password)
        {
            uid = UserId;
            UserName = userName;
            Password = password;
            FullName = "";
            EmailAddress = "";
            Roles = new List<WebPortalUserRole>();
            ApplicationPermissions = new List<PortalApplicationPermission>();

        } // constructor

        public WebPortalUser(int UserId, string userName, string password, WebPortalUserRole[] roles)
        {
            uid = UserId;
            UserName = userName;
            Password = password;
            FullName = "";
            EmailAddress = "";
            Roles = new List<WebPortalUserRole>(roles);
            ApplicationPermissions = new List<PortalApplicationPermission>();

        } // constructor

        public WebPortalUser(int UserId, string userName, string password, string[] roleNames)
        {
            uid = UserId;
            UserName = userName;
            Password = password;
            FullName = "";
            EmailAddress = "";
            Roles = new List<WebPortalUserRole>();

            foreach (string roleName in roleNames)
            {
                WebPortalUserRole role = WebPortalUserRole.Fetch(roleName);
                if (role != null)
                {
                    AddUserRole(role);
                }
            } // foreach
            ApplicationPermissions = new List<PortalApplicationPermission>();
        } // constructor

        /// <summary>
        /// Saves username, password, roles, permissions - but NOT extendedInfos
        /// </summary>
        /// <returns></returns>
        public bool SaveToDatabase()
        {
            if (uid >= 0)
                return (new WebPortalUserDB()).UpdateUser(this);
            else
                return (new WebPortalUserDB()).InsertUser(this);
        }

        public bool SetLastLoginInDatabaseToNow()
        {
            return (new WebPortalUserDB()).SetLastLoginInDatabaseToNow(this);
        }

        /// <summary>
        /// sets an extended value field for the user in the database. 
        /// This is useful for applications that need to extend the amount of information
        /// gathered for a user. The extendedInfo area provides a common place to store 
        /// that extended information.
        /// </summary>
        /// <param name="key">the case insensitive key for the information. eg: "fullname"</param>
        /// <param name="val">the value to set the extended information to</param>
        public bool saveExtendedInfo(string key, string val)
        {
            if (key.Length > 255)
                //throw new Exception("Extended info key must be less than 255 characters: "+key);
                return false;
            if (val.Length > 1024)
                // throw new Exception("Extended info value must be less than 1024 characters: "+val);
                return false;
            WebPortalUserDB db = new WebPortalUserDB();
            bool b = db.setExtendedInfo(this, key, val);
            if (b)
                ExtendedInfoCached = false;

            return b;
        }

        /// <summary>
        /// gets an Extended value field for the user.
        /// This is useful for applications that need to extend the amount of information
        /// gathered for a user. The extendedInfo area provides a common place to store 
        /// that extended information.
        /// </summary>
        /// <param name="key">the case insensitive key for the information. eg: "FullName"</param>
        /// <param name="notFoundValue">the value to return if the key is not found for the user</param>
        /// <returns>the value associated with the key, or the notFoundValue if the key is not found for the user.</returns>
        public string FetchExtendedInfo(string key, string notFoundValue)
        {
            WebPortalUserDB db = new WebPortalUserDB();
            return db.getExtendedInfo(this, key, notFoundValue);
        }

        protected bool ExtendedInfoCached = false;
        protected NameValueCollection CachedExtendedInfos = null;
        public string FetchCachedExtendedInfo(string key, string notFoundValue)
        {
            NameValueCollection extInfos = null;
            if (ExtendedInfoCached)
                extInfos = CachedExtendedInfos;
            else
                extInfos = FetchAllExtendedInfo();

            try
            {
                if (extInfos[key] != null)
                    return extInfos[key];
            }
            catch
            { }
            return notFoundValue;
        }

        /// <summary>
        /// gets all of the extended info key=>value pairs currently set for the user from the database
        /// </summary>
        /// <returns></returns>
        public NameValueCollection FetchAllExtendedInfo()
        {
            WebPortalUserDB db = new WebPortalUserDB();
            return db.getAllExtendedInfo(this);
        }



        /// <summary>
        /// deletes an Extended Info key=>value pair in the database
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool DeleteExtendedInfo(string key)
        {
            WebPortalUserDB db = new WebPortalUserDB();
            return db.removeExtendedInfo(this, key);
        }

        /// <summary>
        /// Checks if the current user is in a specified role.
        /// If the RoleName is empty (""), always returns TRUE.
        /// </summary>
        /// <param name="RoleName">the case insensitive role name</param>
        /// <returns>true if in the role, false if not</returns>
        public bool inRole(string RoleName)
        {
            if (RoleName == "")
                return true;

            foreach (WebPortalUserRole role in this.Roles)
            {
                if (String.Compare(role.Name, RoleName, true) == 0)
                {
                    return true;
                }
            } // foreach
            return false;
        }

        public bool hasPermissionTo(string actionRequested, PortalApplication portalApp)
        {
            portalApp.EnsurePermissionsInDatabase();

            string currentAppName = portalApp.GetApplicationName();
            return hasPermissionTo(actionRequested, currentAppName);
        }

        public bool hasPermissionTo(PortalApplicationPermission permissionRequested)
        {
            return hasPermissionTo(permissionRequested.Action, permissionRequested.ApplicationName);
        }

        /// <summary>
        /// marked as private as passing a string for the portalAppName doesn't allow as many checks as other types.
        /// </summary>
        /// <param name="actionRequested"></param>
        /// <param name="portalAppName"></param>
        /// <returns></returns>
        private bool hasPermissionTo(string actionRequested, string portalAppName)
        {
            foreach (PortalApplicationPermission p in Permissions)
            {
                if ((String.Compare(p.ApplicationName, portalAppName, true) == 0) &&
                    (String.Compare(p.Action, actionRequested, true) == 0))
                {
                    return true;
                }
            } // foreach
            return false;
        }

        /// <summary>
        /// returns NULL if no user is logged in.
        /// </summary>
        /// <returns></returns>
        public static WebPortalUser GetCurrentWebPortalUser(PortalApplication portalApp)
        {
            try
            {
                // -- we cache the currentWebPortal user so that we don't go to the database
                string cacheKey = "WebPortalUser.currentWebPortalUser";
                if (PerRequestCache.CacheContains(cacheKey))
                    return PerRequestCache.GetFromCache(cacheKey, null) as WebPortalUser;

                if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.User != null
                    && System.Web.HttpContext.Current.User.Identity != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    WebPortalUser u = WebPortalUser.FetchUser(System.Web.HttpContext.Current.User.Identity.Name, portalApp);
                    PerRequestCache.AddToCache(cacheKey, u);
                    return u;
                }
                else
                    return null;
            }
            catch
            { }
            return null;

        }




        public static bool UsernameExists(string username, PortalApplication portalApp)
        {
            WebPortalUser user = (new WebPortalUserDB()).FetchWebPortalUser(username, portalApp);
            if (user == null || user.uid < 0)
                return false;
            return true;
        }

        /// <summary>
        /// Checks to see if the username and password match a user in the system.
        /// Returns true if the username exists, and the password matches, otherwise returns false.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool CheckLogin(string username, string password, PortalApplication portalApp)
        {
            WebPortalUser user = (new WebPortalUserDB()).FetchWebPortalUser(username, portalApp);
            if (user != null && String.Compare(user.Password, password) == 0)
            {
                return true;
            }
            return false;
        } // checkLogin


        public static bool DeleteUser(WebPortalUser user)
        {
            if (user.uid > -1)
            {
                WebPortalUserDB db = new WebPortalUserDB();
                return db.DeleteUser(user);
            }
            return false;
        }

        public enum SortUsersBy { UserId, UserName, UserFullName }


        /// <summary>
        /// gets all the Users in the system, sorted by UserName
        /// </summary>
        /// <returns>an Array of WebPortalUser objects</returns>
        public static WebPortalUser[] FetchAll(PortalApplication portalApp)
        {
            return (new WebPortalUserDB()).FetchAllWebPortalUsers(SortUsersBy.UserName, portalApp);

        } // getAllUsers

        public static WebPortalUser[] FetchAll(SortUsersBy sortBy, PortalApplication portalApp)
        {
            return (new WebPortalUserDB()).FetchAllWebPortalUsers(sortBy, portalApp);
        } // getAllUsers


        /// <summary>
        /// gets an Array of all WebPortalUsers that are in a specified role, sorted by UserName
        /// </summary>
        /// <param name="role">the (case insensitive) role to find all users for</param>
        /// <returns>an ArrayList of WebPortalUser Objects</returns>
        public static WebPortalUser[] FetchAll(WebPortalUserRole inRole, PortalApplication portalApp)
        {
            return (new WebPortalUserDB()).FetchAllWebPortalUsers(inRole, SortUsersBy.UserName, portalApp);

        } // FetchAll

        /// <summary>
        /// gets an Array of all WebPortalUsers that are in a specified role
        /// </summary>
        /// <param name="role">the (case insensitive) role to find all users for</param>
        /// <returns>an ArrayList of WebPortalUser Objects</returns>
        public static WebPortalUser[] FetchAll(WebPortalUserRole inRole, SortUsersBy sortBy, PortalApplication portalApp)
        {
            return (new WebPortalUserDB()).FetchAllWebPortalUsers(inRole, sortBy, portalApp);

        } // FetchAll

        /// <summary>
        /// gets a user from their username. Returns NULL if user was not found.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static WebPortalUser FetchUser(string username, PortalApplication portalApp)
        {
            return (new WebPortalUserDB()).FetchWebPortalUser(username, portalApp);
        }

        /// <summary>
        /// gets a user from their userID. Returns NULL if user was not found.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static WebPortalUser FetchUser(int userId, PortalApplication portalApp)
        {
            return (new WebPortalUserDB()).FetchWebPortalUser(userId, portalApp);
        }


    }
}

