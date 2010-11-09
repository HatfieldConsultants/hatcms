using System;
using System.Text;
using System.Web.Configuration;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Hatfield.Web.Portal.Data;

namespace Hatfield.Web.Portal
{
	/// <summary>
	/// provides the database access for the WebPortalUser Class
	/// </summary>
	public class WebPortalUserDB: Hatfield.Web.Portal.Data.MySqlDbObject
	{
		public WebPortalUserDB()
		{
            if (WebConfigurationManager.AppSettings["hatWebPortalConnectionString"] == null || WebConfigurationManager.AppSettings["hatWebPortalConnectionString"] == "")
			{
				throw new Exception("You MUST set the \"hatWebPortalConnectionString\" value in the web.config file!");
			}

            doConnection(WebConfigurationManager.AppSettings["hatWebPortalConnectionString"]);
			// "server=localhost;uid=hatportal;pwd=hatportal;database=hatportal;"					
		}

        private string getStandardUserSQL(string whereClause, WebPortalUser.SortUsersBy sortBy, PortalApplication portalApp)
		{
            if (whereClause.Trim() != "")
                whereClause = " AND " + whereClause;

            string permissionsWhere = "";
            string appName = portalApp.GetApplicationName();
            if (appName != "" && portalApp.GetAllPermissionsForApplication().Length > 0)
            {
                permissionsWhere = " AND ("+DBDialect.isNull("p.PermissionsId")+" or p.ApplicationName = '"+dbEncode(appName)+"' )  ";
            }

			string sql = @"
				select  appuser.appuserid, appuser.username, appuser.password, appuser.FullName, appuser.EmailAddress, appuser.LastLoginDateTime, 
                        roles.Name as RoleName, roles.roleid, roles.Description as RoleDesc,
                        x.`key` as exKey, x.`value` as exVal,
                        p.PermissionsId, p.ApplicationName, p.Action, p.Description
				from appuser 
				LEFT JOIN appuserroles on (appuser.appuserid = appuserroles.appuserid)  
				LEFT JOIN roles ON (appuserroles.roleid = roles.roleid) 
                LEFT JOIN appuserextendedinfo x on (x.userId = appuser.AppUserId)
                LEFT JOIN appuserpermissions up on (appuser.AppUserId = up.AppUserId)
                LEFT JOIN permissions p ON (p.PermissionsId = up.PermissionsId) 
				WHERE " + DBDialect.isNull("appuser.deleted") + " AND " + DBDialect.isNull("roles.deleted") + " and " + DBDialect.isNull("x.deleted") + " and " + DBDialect.isNull("p.Deleted") + "  ";
            
            sql = sql + permissionsWhere;
			sql = sql + whereClause;

            switch (sortBy)
            {
                case WebPortalUser.SortUsersBy.UserId:
                    sql = sql + @" ORDER BY appuserid ";
                    break;
                case WebPortalUser.SortUsersBy.UserName:
                    sql = sql + @" ORDER BY username ";
                    break;
                case WebPortalUser.SortUsersBy.UserFullName:
                    sql = sql + @" ORDER BY FullName ";
                    break;
                default: 
                    throw new ArgumentException("Invalid SortBy given");
            }


            

			return sql;
		} // getStandardUserSQL
               

		/// <summary>
		/// gets an active (not deleted) WebPortalUser object from the database, 
		/// or NULL if one does not exist
		/// </summary>
		/// <param name="username">the case insensitive username to get the user data for</param>
		/// <returns></returns>
		public WebPortalUser FetchWebPortalUser(string username, PortalApplication portalApp)
		{
            username = username.Trim();
			
            string sql = getStandardUserSQL("UserName like '" + dbEncode(username) + "'", WebPortalUser.SortUsersBy.UserFullName, portalApp);

			DataSet ds = RunSelectQuery(sql);
            WebPortalUser[] users = getWebPortalUsersFromStandardDataSet(ds);
			if (users.Length == 1)
			{
				return users[0];
			}
			return null;

        } // FetchWebPortalUser


		/// <summary>
		/// gets an active (not deleted) WebPortalUser object from the database, or NULL if one does not exist
		/// </summary>
		/// <param name="userID">the usedId to get the user data for</param>
		/// <returns></returns>
        public WebPortalUser FetchWebPortalUser(int userID, PortalApplication portalApp)
		{
			if (userID > -1)
			{
								
				string sql = getStandardUserSQL("appuser.AppUserId = "+userID.ToString()+"", WebPortalUser.SortUsersBy.UserId, portalApp);
				DataSet ds = RunSelectQuery(sql);
                WebPortalUser[] users = getWebPortalUsersFromStandardDataSet(ds);
				if (users.Length == 1)
				{
					return users[0] as WebPortalUser;
				}
			}
			return null;

        } // FetchWebPortalUser


		/// <summary>
		/// gets an Array of all active (not deleted) WebPortalUser objects in the system
		/// </summary>
		/// <returns>an arraylist of WebPortalUser Objects</returns>
        public WebPortalUser[] FetchAllWebPortalUsers(WebPortalUser.SortUsersBy sortBy, PortalApplication portalApp)
		{	
			string sql = getStandardUserSQL("", sortBy, portalApp);
			DataSet ds = RunSelectQuery(sql);
			return getWebPortalUsersFromStandardDataSet(ds);
		} // getAllWebPortalUsers


		/// <summary>
		/// gets an ArrayList of all (not deleted) WebPortalUsers that are in a specified role
		/// </summary>
		/// <param name="role">the (case insensitive) role to find all users for</param>
		/// <returns>an ArrayList of  Objects</returns>
        public WebPortalUser[] FetchAllWebPortalUsers(WebPortalUserRole role, WebPortalUser.SortUsersBy sortBy, PortalApplication portalApp)
		{									
            string sql = getStandardUserSQL("roles.roleId = " + role.RoleID + "", sortBy, portalApp);
            DataSet ds = RunSelectQuery(sql);
			return getWebPortalUsersFromStandardDataSet(ds);
		} // getAllWebPortalUsers

		/// <summary>
		/// returns an ArrayList of WebPortalUser objects. The passed in DataSet MUST have been created
		/// using getStandardUserSQL()
		/// </summary>
		/// <param name="ds">The DataSet returned from the query</param>
		/// <returns>an ArrayList of WebPortalUser objects</returns>
        private WebPortalUser[] getWebPortalUsersFromStandardDataSet(DataSet ds)
		{

            Dictionary<string, WebPortalUser> storage = new Dictionary<string, WebPortalUser>();
            if (hasRows(ds))            
			{
                
				foreach(DataRow dr in ds.Tables[0].Rows)
				{
					string key = dr["AppUserId"].ToString();

                    // -- get the user
                    WebPortalUser user;
                    if (!storage.ContainsKey(key))
                    {
                         user = new WebPortalUser(Convert.ToInt32(dr["AppUserId"]), dr["username"].ToString(), dr["password"].ToString());
                         user.FullName = dr["FullName"].ToString();
                         user.EmailAddress = dr["EmailAddress"].ToString();
                         user.LastLogin = getPossiblyNullValue(dr, "LastLoginDateTime", DateTime.MinValue);
                    }
                    else
                    {
                        user = storage[key];                        
                    }

                    // -- roles
                    if (dr["roleid"] != System.DBNull.Value)
                    {
                        WebPortalUserRole role = new WebPortalUserRole(Convert.ToInt32(dr["roleid"]), dr["RoleName"].ToString(), dr["RoleDesc"].ToString());
                        user.AddUserRole(role);
                    }

                    // x.`key` as exKey, x.`value` as exVal,
                    
                        
                    // -- Permissions
                    //      p.PermissionsId, p.ApplicationName, p.Action, p.Description
                    if (dr["PermissionsId"] != System.DBNull.Value)
                    {
                        PortalApplicationPermission p = new PortalApplicationPermission();
                        p.ID = Convert.ToInt32(dr["PermissionsId"]);
                        p.ApplicationName = dr["ApplicationName"].ToString().Trim();
                        p.Action = dr["Action"].ToString().Trim();
                        p.Description = dr["Description"].ToString();
                        
                        user.AddUserPermission(p);
                    }


                    if (!storage.ContainsKey(key))
                        storage.Add(key, user);

				} // foreach row
				// ---- copy the storage NameValueCollection to the ArrayList				
				
				
			} // if there is data

            List<WebPortalUser> ret = new List<WebPortalUser>();
            foreach (string k in storage.Keys)
                ret.Add(storage[k]);

            return ret.ToArray();
		} // getWebPortalUsersFromStandardDataSet


		/// <summary>
		/// sets a user's Extended Information for a given key to the val
        /// you must have a valid user id before setting extended infos!
		/// </summary>
		/// <param name="user">the user to set the info for</param>
		/// <param name="key">the data key to set the data for (case insensitive)</param>
		/// <param name="val">the value to associate with the user's key</param>
		public bool setExtendedInfo(WebPortalUser user, string key, string val)
		{
            val = this.dbEncode(val);
			if (user.uid > -1)
			{
				key = key.ToLower();
				int eid = ExtendedInfoExists(user, key);
				if (eid != -1)
				{ // update
					string sql = "UPDATE appuserextendedinfo ";
					sql = sql + "SET appuserextendedinfo.val = '"+dbEncode(val)+"' ";
					sql = sql + "WHERE appuserextendedinfo.UserId = "+user.uid.ToString()+" ";
					sql = sql + "AND appuserextendedinfo.key = '"+dbEncode(key)+"';";
					int numAffected = this.RunUpdateQuery(sql);
					return (numAffected > 0);
				}
				else
				{ // insert
					string sql = "INSERT INTO appuserextendedinfo ";
					sql = sql + "(appuserextendedinfo.UserId, appuserextendedinfo.Key, appuserextendedinfo.Value) ";
					sql = sql + " VALUES (";
					sql = sql + user.uid.ToString()+" , ";
					sql = sql + "'"+dbEncode(key)+"', ";
					sql = sql + "'"+dbEncode(val)+"') ";
					this.RunInsertQuery(sql);
					return true;
				}
			}	
			return false;
		} // setExtendedInfo

		public bool removeExtendedInfo(WebPortalUser user, string key)
		{
			key = key.Trim().ToLower();
			int keyId = ExtendedInfoExists(user,key);
			if (user.uid > -1 && key != "" && keyId > -1)
			{
				string sql = "UPDATE appuserextendedinfo a set a.deleted = "+DBDialect.currentDateTime+" ";
				sql += "WHERE a.ExtendedInfoId = "+keyId.ToString()+"; ";
				try
				{
					int numAffected = RunUpdateQuery(sql);
					return (numAffected > 0);
				} 
				catch
				{
					return false;
				}
				// return true;
			}
			return false;
		} // removeExtendedInfo

		private int ExtendedInfoExists(WebPortalUser user, string key)
		{
			if (user.uid > -1)
			{
				key = key.ToLower();
				string sql = "SELECT ExtendedInfoId from appuserextendedinfo a where ";
				sql = sql + " a.userid = "+user.uid.ToString();
				sql = sql + " AND a.key = '"+dbEncode(key)+"' ";
				sql = sql + " AND "+DBDialect.isNull("a.Deleted")+" ";
				DataSet ds = this.RunSelectQuery(sql);
				if (ds.Tables[0] != null & ds.Tables[0].Rows.Count == 1)
				{
					return Convert.ToInt32(ds.Tables[0].Rows[0]["ExtendedInfoId"].ToString());
				}
			}
			return -1;
		}

		public string getExtendedInfo(WebPortalUser user, string key, string notFoundValue)
		{
			if (user.uid > -1)
			{
				key = key.ToLower();
				string sql = "SELECT value from appuserextendedinfo a WHERE ";
				sql = sql + " a.userid = "+user.uid.ToString();
				sql = sql + " AND a.key = '"+key+"' ";
				sql = sql + " AND "+DBDialect.isNull("a.Deleted")+" ";
				DataSet ds = this.RunSelectQuery(sql);
				if (ds.Tables[0] != null & ds.Tables[0].Rows.Count == 1)
				{
					return (ds.Tables[0].Rows[0]["value"].ToString());
				}
			}
			return notFoundValue;
		}

		public NameValueCollection getAllExtendedInfo(WebPortalUser user)
		{
			NameValueCollection ret = new NameValueCollection();

			string sql = "select a.key, a.value from appuserextendedinfo a WHERE ";
			sql += " a.userid = "+user.uid.ToString();
			sql +=" AND "+DBDialect.isNull("a.Deleted")+" ";

			DataSet ds = this.RunSelectQuery(sql);
			if (ds.Tables[0] != null & ds.Tables[0].Rows.Count >= 1)
			{
				foreach(DataRow dr in ds.Tables[0].Rows)
				{
					ret.Add((dr["key"].ToString()),(dr["value"].ToString()));
				} // foreach row
			}

			return ret;
		}

		/// <summary>
		/// Delete a user with a given user ID from the system
		/// </summary>
		/// <param name="userID"></param>
		/// <returns>true if deleted successfully, false if not</returns>
		public bool DeleteUser(WebPortalUser userToDelete)
		{
            if (userToDelete.uid > -1)
			{
                string sql = "UPDATE appuser set DELETED = " + DBDialect.currentDateTime + " WHERE appuserid = " + userToDelete.uid.ToString() + " ";
				int numAffected = RunUpdateQuery(sql);
				return (numAffected > 0);
			}
			return false;
			
		} // DeleteUser

        public bool InsertUser(WebPortalUser user)
        {
            string sql = "INSERT INTO appuser (UserName, Password, FullName, EmailAddress, LastLoginDateTime) VALUES ('" + dbEncode(user.UserName) + "', '" + dbEncode(user.Password) + "', '" + dbEncode(user.FullName) + "', '" + dbEncode(user.EmailAddress) + "', " + DBDialect.currentDateTime + ") ";
            int newId = this.RunInsertQuery(sql);
            if (newId >= 0)
            {
                user.uid = newId;
                bool b = AddRolesToExistingUser(user, user.userRoles);
                if (b)
                    return AddPermissionsToExistingUser(user, user.Permissions);
            }
            return false;
        }

		/// <summary>
		/// Updates all fields for a user in the database.
		/// This will create the user and roles if they do not already exist.
		/// </summary>
		/// <param name="user">the user to save</param>
		/// <returns></returns>
		public bool UpdateUser(WebPortalUser user)
		{
            string sql = "UPDATE appuser SET ";
            sql += "UserName = " + "'" + dbEncode(user.UserName) + "'" + ", ";
            sql += "Password = " + "'" + dbEncode(user.Password) + "'" + ", ";
            sql += "FullName = " + "'" + dbEncode(user.FullName) + "'" + ", ";
            sql += "EmailAddress = " + "'" + dbEncode(user.EmailAddress) + "'" + ", ";
            sql += "LastLoginDateTime = " + DBDialect.currentDateTime + " ";
            sql += " WHERE AppUserId = " + user.uid.ToString();

            int numUpdated = this.RunUpdateQuery(sql);
            if (numUpdated >= 0)
            {
                // -- user has been updated, let's update role associations
                RemoveUserFromAllRoles(user);

                bool b = AddRolesToExistingUser(user, user.userRoles);
                if (b)
                {
                    RemoveUserFromAllPermissions(user);
                    return AddPermissionsToExistingUser(user, user.Permissions);
                }
            }
            return false;

            
		} // UpdateUser

        public bool SetLastLoginInDatabaseToNow(WebPortalUser user)
        {
            string sql = "UPDATE appuser SET ";
            sql += "LastLoginDateTime = " + DBDialect.currentDateTime + " ";
            sql += " WHERE AppUserId = " + user.uid.ToString();
            int numUpdated = this.RunUpdateQuery(sql);
            if (numUpdated >= 0)
                return true;
            return false;
        }



		/// <summary>
		/// checks to see if a User's username is active and is already in the system
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		private int UserNameExists(string userName)
		{
            string sql = "SELECT AppUserId from appuser where UserName like '" + dbEncode(userName.Trim()) + "' and " + DBDialect.isNull("Deleted") + " ";
			DataSet ds = this.RunSelectQuery(sql);
			if (hasSingleRow(ds))
			{
				return Convert.ToInt32(ds.Tables[0].Rows[0]["AppUserId"]);
			}
			return -1;
		} // userExists

		/// <summary>
		/// checks to see if a role exists in the system. Returns the Role's RoleID if successful, -1 if not.
		/// </summary>
		/// <param name="role"></param>
		/// <returns>the Role's RoleID if successful, -1 if not</returns>
		private int RoleNameExists(string roleName)
		{
            if (roleName != "")
			{
                roleName = roleName.Trim();
				string sql = "SELECT r.roleId from roles r where r.name like '"+dbEncode(roleName)+"' AND "+DBDialect.isNull("r.Deleted")+" ";
				DataSet ds = this.RunSelectQuery(sql);
                if (hasSingleRow(ds))
				{
					return Convert.ToInt32(ds.Tables[0].Rows[0]["roleId"]);
				}
			}
			return -1;
		} // userExists

		/// <summary>
		/// adds a role to the system.
		/// </summary>
		/// <param name="role"></param>
		/// <returns>the role that was inserted or updated.</returns>
        public bool InsertRole(WebPortalUserRole role)
        {
            string sql = "INSERT INTO roles (name, description) VALUES ('" + dbEncode(role.Name) + "', '" + dbEncode(role.Description) + "') ";
            
            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {
                role.RoleID = newId;
                return true;
            }
            else
                return false;

        } // AddRole

		

		public bool AddRolesToExistingUser(WebPortalUser user, WebPortalUserRole[] roles)
		{
            if (roles.Length > 0 && user.uid > -1)
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO appuserroles ");
                sql.Append("(AppUserId, RoleId)");
                sql.Append(" VALUES ");
                foreach (WebPortalUserRole role in roles)
                {
                    sql.Append(" ( ");
                    if (user.uid < 0)
                        sql.Append("NULL, ");
                    else
                        sql.Append(user.uid.ToString() + ", ");

                    if (role.RoleID < 0)
                        sql.Append("NULL, ");
                    else
                        sql.Append(role.RoleID.ToString() + " ");

                    sql.Append(" ),");
                } // foreach

                // remove trailing comma
                string s = sql.ToString().Substring(0, sql.ToString().Length - 1);
                int numInserted = this.RunUpdateQuery(s); // do not use RunInsertQuery
                if (numInserted == roles.Length)
                    return true;

            }
            else if (roles.Length == 0)
                return true;

			return false;
		}



        public bool AddPermissionsToExistingUser(WebPortalUser user, PortalApplicationPermission[] permissions)
        {
            if (permissions.Length > 0 && user.uid > -1)
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO appuserpermissions ");
                sql.Append("(AppUserId, PermissionsId)");
                sql.Append(" VALUES ");
                foreach (PortalApplicationPermission perm in permissions)
                {
                    sql.Append(" ( ");
                    if (user.uid < 0)
                        sql.Append("NULL, ");
                    else
                        sql.Append(user.uid.ToString() + ", ");

                    if (perm.ID < 0)
                        sql.Append("NULL ");
                    else
                        sql.Append(perm.ID.ToString() + " ");

                    sql.Append(" ),");
                } // foreach

                // remove trailing comma
                string s = sql.ToString().Substring(0, sql.ToString().Length - 1);
                int numInserted = this.RunUpdateQuery(s); // do not use RunInsertQuery
                if (numInserted == permissions.Length)
                    return true;

            }
            else if (permissions.Length == 0)
                return true;

            return false;
        }


        /// <summary>
        /// removes all the permissions for the given user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool RemoveUserFromAllPermissions(WebPortalUser user)
        {
            if (user.uid > -1)
            {
                string sql = "DELETE from appuserpermissions where appuserid = " + user.uid.ToString() + "; ";
                int numAffected = RunUpdateQuery(sql);
                return (numAffected >= 0);
            }
            return false;
        }

		/// <summary>
		/// removes all the roles for the given user
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public bool RemoveUserFromAllRoles(WebPortalUser user)
		{
			if (user.uid > -1)
			{				
                string sql = "DELETE from appuserroles where appuserid = " + user.uid.ToString() + "; ";
				int numAffected = RunUpdateQuery(sql);
				return (numAffected >= 0);
			}
			return false;
		}

        private WebPortalUserRole roleFromRow(DataRow dr)
        {
            int id = Convert.ToInt32(dr["roleid"]);
            string name = dr["Name"].ToString();
            string desc = dr["Description"].ToString();

            WebPortalUserRole role = new WebPortalUserRole(id, name, desc);
            return role;
        }

		public WebPortalUserRole[] FetchAllUserRoles()
		{
            List<WebPortalUserRole> ret = new List<WebPortalUserRole>();
			string sql = "select * from roles where "+DBDialect.isNull("Deleted")+" ";
            sql += " ORDER BY Name ";

			DataSet ds = this.RunSelectQuery(sql);
			if (hasRows(ds))
			{
				foreach(DataRow dr in ds.Tables[0].Rows)
				{
                    ret.Add(roleFromRow(dr));
				} // foreach
			}
			return ret.ToArray();
		} // getAllUserRoles
        
        public WebPortalUserRole[] FetchAllUserRoles(int[] matchingRoleIds)
		{
            if (matchingRoleIds.Length == 0)
                return new WebPortalUserRole[0];

            List<WebPortalUserRole> ret = new List<WebPortalUserRole>();
			string sql = "select * from roles where "+DBDialect.isNull("Deleted")+" ";
            sql += " AND roleId in (" + StringUtils.Join(",", matchingRoleIds) + ") ";
            sql += " ORDER BY Name ";

			DataSet ds = this.RunSelectQuery(sql);
			if (hasRows(ds))
			{
				foreach(DataRow dr in ds.Tables[0].Rows)
				{
                    ret.Add(roleFromRow(dr));
				} // foreach
			}
			return ret.ToArray();
		} // getAllUserRoles

        

		/// <summary>
		/// returns null if role name not found
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
        public WebPortalUserRole FetchUserRole(string name)
		{
            string sql = "select * from roles where " + DBDialect.isNull("Deleted") + " and name like '" + dbEncode(name) + "' ";
            DataSet ds = this.RunSelectQuery(sql);
            if (hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                return roleFromRow(dr);
            }

			return null;
		} // getUserRole

		/// <summary>
		/// returns NULL if the role was not found
		/// </summary>
		/// <param name="roleId"></param>
		/// <returns></returns>
		public WebPortalUserRole FetchUserRole(int roleId)
		{
			if (roleId > -1)
			{
				string sql = "select * from roles WHERE roleId = "+roleId+" ";
				DataSet ds = RunSelectQuery(sql);
				if (hasSingleRow(ds))
				{
					DataRow dr = ds.Tables[0].Rows[0];
					
					return roleFromRow(dr);
				}
			}
			return null;
		} // getUserRole

		public bool UpdateRole(WebPortalUserRole role)
		{
			if (role.RoleID > -1)
			{
				string sql = "UPDATE roles SET ";
				sql += "name = '"+dbEncode(role.Name)+"', ";
                sql += "description = '" + dbEncode(role.Description) + "' ";
                sql += "WHERE roleId = " + role.RoleID.ToString() + " ";

				try
				{
					int numAffected = RunUpdateQuery(sql);
					return numAffected > 0;
				}
				catch
				{
					return false;
				}
			}
			return false;
		} // setNameAndDescriptionAndSave


		public bool DeleteRole(WebPortalUserRole role)
		{
			if (role.RoleID > -1)
			{
				string sql = "UPDATE roles r SET ";
				sql += "r.Deleted = "+DBDialect.currentDateTime+" ";			
				sql += "WHERE r.roleId = "+role.RoleID.ToString()+" ";

				try
				{
					int numAffected = RunUpdateQuery(sql);
					return numAffected > 0;
				}
				catch
				{
					return false;
				}
			}
			return false;
		} // setNameAndDescriptionAndSave
	}
}
