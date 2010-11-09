using System;
using System.Collections;
using System.Collections.Generic;

namespace Hatfield.Web.Portal
{
    /// <summary>
    /// Summary description for WebPortalUserRole.
    /// </summary>
    public class WebPortalUserRole
    {
        public int RoleID;
        public string Name;
        public string Description;

        /// <summary>
        /// creates a new user role
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public WebPortalUserRole(int roleID, string name, string description)
        {
            RoleID = roleID;
            Name = name;
            Description = description;
        }

        public bool SaveToDatabase()
        {
            if (RoleID < 0)
                return (new WebPortalUserDB()).InsertRole(this);
            else
                return (new WebPortalUserDB()).UpdateRole(this);
        }

        /// <summary>
        /// gets an Array of WebPortalUserRole objects of all the User Roles in the system
        /// </summary>
        /// <returns></returns>
        public static WebPortalUserRole[] FetchAll()
        {
            return (new WebPortalUserDB()).FetchAllUserRoles();
        }

        /// <summary>
        /// gets an Array of WebPortalUserRole objects of all the User Roles in the system
        /// </summary>
        /// <returns></returns>
        public static WebPortalUserRole[] FetchAll(int[] matchingRoleIds)
        {
            return (new WebPortalUserDB()).FetchAllUserRoles(matchingRoleIds);
        }

        /// <summary>
        /// gets a complete list of all user role names in the system
        /// </summary>
        /// <returns>an array of user role names</returns>
        public static string[] FetchAllUserRoleNames()
        {
            List<string> ret = new List<string>();
            WebPortalUserRole[] roles = FetchAll();
            foreach (WebPortalUserRole role in roles)
            {
                ret.Add(role.Name);
            }

            return ret.ToArray();
        }

        /// <summary>
        /// gets a specified user role for a given role's name. Returns null if none found.
        /// </summary>
        /// <param name="name">the case insensitive role name</param>
        /// <returns>the WebPortalUserRole object found, or NULL if not found</returns>
        public static WebPortalUserRole Fetch(string name)
        {
            WebPortalUserDB db = new WebPortalUserDB();
            return db.FetchUserRole(name);
        }

        public static WebPortalUserRole Fetch(int roleId)
        {
            WebPortalUserDB db = new WebPortalUserDB();
            return db.FetchUserRole(roleId);
        }



        public static bool Delete(WebPortalUserRole roleToDelete)
        {
            if (roleToDelete.RoleID > -1)
            {
                return (new WebPortalUserDB()).DeleteRole(roleToDelete);
            }
            return false;
        }


    } // class
}
