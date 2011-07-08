using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Hatfield.Web.Portal.Data;

namespace Hatfield.Web.Portal
{
    public abstract class PortalApplication
    {
        public abstract string GetApplicationName();
        public abstract PortalApplicationPermission[] GetAllPermissionsForApplication();

        private bool ensuredInDatabase = false;
        public bool EnsurePermissionsInDatabase()
        {
            bool ret = true;
            if (!ensuredInDatabase)
            {
                ret = PortalApplicationPermission.EnsurePermissionsInDatabase(this);
                ensuredInDatabase = true;
            }
            return ret;
        }

        public static PortalApplicationWithNoPermissions GetInstanceWithNoPermissions()
        {
            return new PortalApplicationWithNoPermissions();
        }

    }

    public class PortalApplicationWithNoPermissions : PortalApplication
    {
        public override string GetApplicationName()
        {
            return "";
        }

        public override PortalApplicationPermission[] GetAllPermissionsForApplication()
        {
            return new PortalApplicationPermission[0];
        }
    }

    public class PortalApplicationPermission
    {
        // ApplicationName + PermissionAction makes up a unique entry

        public int ID;
        public string ApplicationName;
        public string Action;
        public string Description;

        public PortalApplicationPermission()
        {
            ID = Int32.MinValue;
            ApplicationName = "";
            Action = "";
            Description = "";
        }

        public PortalApplicationPermission(string applicationName, string action, string description)
        {
            ID = Int32.MinValue;
            ApplicationName = applicationName;
            Action = action;
            Description = description;
        }

        public bool SaveToDatabase()
        {
            if (ID < 0)
            {
                return (new permissionsDB()).Insert(this);
            }
            else
            {
                return (new permissionsDB()).Update(this);
            }
        } // SaveToDatabase

        public static PortalApplicationPermission Fetch(int PermissionsId)
        {
            return (new permissionsDB()).Fetch(PermissionsId);
        } // Fetch

        public static PortalApplicationPermission[] FetchAll()
        {
            return (new permissionsDB()).FetchAll();
        } // FetchAll

        public static PortalApplicationPermission[] FetchAll(string applicationName)
        {
            return (new permissionsDB()).FetchAll(applicationName);
        } // FetchAll

        public static string[] FetchAllApplicationNames()
        {
            return (new permissionsDB()).FetchAllApplicationNames();
        } // FetchAll


        public static bool BulkInsert(PortalApplicationPermission[] items)
        {
            return (new permissionsDB()).BulkInsert(items);
        } // BulkInsert

        /// <summary>
        /// ensures that all permissions for the portal app are reflected in the database.
        /// Inserts new items into the database, and deletes items that don't.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static bool EnsurePermissionsInDatabase(PortalApplication portalApp)
        {
            return (new permissionsDB()).EnsurePermissionsInDatabase(portalApp);
        }


        public static bool Delete(PortalApplicationPermission toDelete)
        {
            return (new permissionsDB()).Delete(toDelete.ID);
        } // Delete
        #region permissionsDB
        private class permissionsDB : Hatfield.Web.Portal.Data.MySqlDbObject
        {
            public permissionsDB()
                : base(ConfigUtils.getConfigValue("hatWebPortalConnectionString", ""))
            { }

            public bool Insert(PortalApplicationPermission item)
            {
                string sql = "INSERT INTO permissions ";
                sql += "(ApplicationName, Action, Description)";
                sql += " VALUES ( ";
                sql += "'" + dbEncode(item.ApplicationName) + "'" + ", ";
                sql += "'" + dbEncode(item.Action) + "'" + ", ";
                sql += "'" + dbEncode(item.Description) + "'" + ", ";

                sql += " ); ";

                int newId = this.RunInsertQuery(sql);
                if (newId > -1)
                {
                    item.ID = newId;
                    return true;
                }
                return false;

            } // Insert


            public bool BulkInsert(PortalApplicationPermission[] items)
            {
                if (items.Length == 0) return true;
                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO permissions ");
                sql.Append("(ApplicationName, Action, Description)");
                sql.Append(" VALUES ");
                foreach (PortalApplicationPermission item in items)
                {
                    sql.Append(" ( ");
                    sql.Append("'" + dbEncode(item.ApplicationName) + "'" + ", ");
                    sql.Append("'" + dbEncode(item.Action) + "'" + ", ");
                    sql.Append("'" + dbEncode(item.Description) + "'" + " ");

                    sql.Append(" ),");
                } // foreach

                // remove trailing comma
                string s = sql.ToString().Substring(0, sql.ToString().Length - 1);
                int numInserted = this.RunUpdateQuery(s); // do not use RunInsertQuery
                if (numInserted == items.Length)
                    return true;

                return false;

            } // Insert

            public bool Update(PortalApplicationPermission item)
            {
                string sql = "UPDATE permissions SET ";
                sql += "ApplicationName = " + "'" + dbEncode(item.ApplicationName) + "'" + ", ";
                sql += "Action = " + "'" + dbEncode(item.Action) + "'" + ", ";
                sql += "Description = " + "'" + dbEncode(item.Description) + "'" + ", ";

                sql += " WHERE PermissionsId = " + item.ID.ToString();
                sql += " ; ";

                int numAffected = this.RunUpdateQuery(sql);
                if (numAffected < 0)
                {
                    return false;
                }
                return true;

            } // Update

            private bool arrayContains(PortalApplicationPermission needle, PortalApplicationPermission[] haystack)
            {
                foreach (PortalApplicationPermission p in haystack)
                {
                    if ((String.Compare(needle.ApplicationName, p.ApplicationName, true) == 0) &&
                        (String.Compare(needle.Action, p.Action, true) == 0))
                        return true;
                }
                return false;
            }

            public bool EnsurePermissionsInDatabase(PortalApplication portalApp)
            {
                string appName = portalApp.GetApplicationName();
                PortalApplicationPermission[] appPermissionsInDB = FetchAll(appName);
                PortalApplicationPermission[] appPermissionsDefined = portalApp.GetAllPermissionsForApplication();

                List<PortalApplicationPermission> toInsert = new List<PortalApplicationPermission>();
                foreach (PortalApplicationPermission appPermission in appPermissionsDefined)
                {
                    if (!arrayContains(appPermission, appPermissionsInDB))
                        toInsert.Add(appPermission);
                } // foreach

                List<PortalApplicationPermission> toDelete = new List<PortalApplicationPermission>();
                foreach (PortalApplicationPermission dbPermission in appPermissionsInDB)
                {
                    if (!arrayContains(dbPermission, appPermissionsDefined))
                        toDelete.Add(dbPermission);
                } // foreach

                return (BulkInsert(toInsert.ToArray()) && BulkDelete(toDelete.ToArray()));

            }


            public bool BulkDelete(PortalApplicationPermission[] toDelete)
            {
                if (toDelete.Length == 0)
                    return true;

                List<string> ids = new List<string>();
                foreach (PortalApplicationPermission p in toDelete)
                {
                    ids.Add(p.ID.ToString());
                }

                string sql = "UPDATE permissions ";
                sql += " set deleted = " + dbEncode(DateTime.Now) + " ";
                sql += " WHERE PermissionsId in (" + String.Join(",", ids.ToArray()) + ") ";
                int numAffected = this.RunUpdateQuery(sql);
                if (numAffected != toDelete.Length)
                {
                    return false;
                }
                return true;
            }

            public bool Delete(int PermissionsId)
            {
                string sql = "UPDATE permissions ";
                sql += " set deleted = " + dbEncode(DateTime.Now) + " ";
                sql += " WHERE PermissionsId = " + PermissionsId.ToString();
                int numAffected = this.RunUpdateQuery(sql);
                if (numAffected < 0)
                {
                    return false;
                }
                return true;
            }

            private PortalApplicationPermission GetFromRow(DataRow dr)
            {
                PortalApplicationPermission item = new PortalApplicationPermission();
                item.ID = Convert.ToInt32(dr["PermissionsId"]);

                item.ApplicationName = (dr["ApplicationName"]).ToString();

                item.Action = (dr["Action"]).ToString();

                item.Description = (dr["Description"]).ToString();

                return item;
            } // GetFromRow

            public PortalApplicationPermission Fetch(int PermissionsId)
            {
                if (PermissionsId < 0)
                    return new PortalApplicationPermission();
                string sql = "SELECT PermissionsId, ApplicationName, Action, Description from permissions ";
                sql += " WHERE PermissionsId = " + PermissionsId.ToString();
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasSingleRow(ds))
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    return GetFromRow(dr);
                }
                return new PortalApplicationPermission();
            } // Fetch

            public PortalApplicationPermission[] FetchAll(string applicationName)
            {
                string sql = "SELECT PermissionsId, ApplicationName, Action, Description from permissions ";
                sql += " WHERE " + DBDialect.isNull("Deleted") + " ";
                sql += " AND ApplicationName = '" + dbEncode(applicationName) + "' ";
                sql += " order by ApplicationName, Action ";

                List<PortalApplicationPermission> arrayList = new List<PortalApplicationPermission>();
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        arrayList.Add(GetFromRow(dr));
                    } // foreach row
                } // if there is data

                return arrayList.ToArray();
            } // FetchAll

            public PortalApplicationPermission[] FetchAll()
            {
                string sql = "SELECT PermissionsId, ApplicationName, Action, Description from permissions ";
                sql += " WHERE " + DBDialect.isNull("Deleted") + " ";
                sql += " order by ApplicationName, Action ";

                List<PortalApplicationPermission> arrayList = new List<PortalApplicationPermission>();
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        arrayList.Add(GetFromRow(dr));
                    } // foreach row
                } // if there is data

                return arrayList.ToArray();
            } // FetchAll

            public string[] FetchAllApplicationNames()
            {
                string sql = "select distinct ApplicationName from permissions   ";
                sql += " WHERE " + DBDialect.isNull("Deleted") + " ";
                sql += " order by ApplicationName, Action ";
                List<string> ret = new List<string>();
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ret.Add(dr["ApplicationName"].ToString());
                    } // foreach row
                } // if there is data

                return ret.ToArray();
            } // FetchAllApplicationNames

        } // class permissionsDB
        #endregion
    } // PortalApplicationPermission
}
