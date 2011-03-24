using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using Hatfield.Web.Portal;
using Hatfield.Web.Portal.Data;

namespace HatCMS
{
    /// <summary>
    /// DB object for `ZoneUserRole`
    /// </summary>
    public class CmsZoneUserRoleDb : MySqlDbObject 
    {
        protected static string TABLE_NAME = "zoneuserrole";

        public CmsZoneUserRoleDb()
            : base(ConfigurationManager.AppSettings["ConnectionString"])
        { }

        /// <summary>
        /// Select all the aurhority definitions by providing a zone (zone ID)
        /// </summary>
        /// <param name="z"></param>
        /// <returns></returns>
        public List<CmsZoneUserRole> fetchAllByZone(CmsPageSecurityZone z)
        {
            StringBuilder sql = new StringBuilder("SELECT ZoneId,UserRoleId,ReadAccess,WriteAccess FROM ");
            sql.Append(TABLE_NAME);
            sql.Append(" WHERE ZoneId=" + z.ZoneId.ToString());
            sql.Append(" ORDER by ZoneId, UserRoleId;");

            DataSet ds = this.RunSelectQuery(sql.ToString());

            List<CmsZoneUserRole> list = new List<CmsZoneUserRole>();
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                    list.Add(fromDataRow(dr));
            }
            return list;
        }

        /// <summary>
        /// Count the number of roles ID defined.  If the count is zero, it indicates
        /// the WebPortalUser does not belong to the roles which are required to access
        /// the zone.
        /// </summary>
        /// <param name="z"></param>
        /// <param name="roleArray"></param>
        /// <returns></returns>
        public int fetchRoleMatchingCountForRead(CmsPageSecurityZone z, WebPortalUserRole[] roleArray)
        {
            StringBuilder sql = new StringBuilder("SELECT Count(ReadAccess) AS MatchingCount FROM ");
            sql.Append(TABLE_NAME);
            sql.Append(" WHERE ZoneId=" + z.ZoneId.ToString());
            sql.Append(" AND ReadAccess=1");
            sql.Append(" AND UserRoleId in (");
            foreach (WebPortalUserRole r in roleArray)
                sql.Append(r.RoleID + ",");
            sql.Remove(sql.Length - 1, 1);
            sql.Append(");");

            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds) == false)
                return 0;

            DataRow dr = ds.Tables[0].Rows[0];
            return Convert.ToInt32(dr["MatchingCount"]);
        }

        /// <summary>
        /// Count the number of roles ID defined.  If the count is zero, it indicates
        /// the WebPortalUser does not belong to the roles which are required to access
        /// the zone.
        /// </summary>
        /// <param name="z"></param>
        /// <param name="roleArray"></param>
        /// <returns></returns>
        public int fetchRoleMatchingCountForWrite(CmsPageSecurityZone z, WebPortalUserRole[] roleArray)
        {
            StringBuilder sql = new StringBuilder("SELECT Count(WriteAccess) AS MatchingCount FROM ");
            sql.Append(TABLE_NAME);
            sql.Append(" WHERE ZoneId=" + z.ZoneId.ToString());
            sql.Append(" AND WriteAccess=1");
            sql.Append(" AND UserRoleId in (");
            foreach (WebPortalUserRole r in roleArray)
                sql.Append(r.RoleID + ",");
            sql.Remove(sql.Length - 1, 1);
            sql.Append(");");

            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds) == false)
                return 0;

            DataRow dr = ds.Tables[0].Rows[0];
            return Convert.ToInt32(dr["MatchingCount"]);
        }

        /// <summary>
        /// Insert into `ZoneUserRole`
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool insert(CmsZoneUserRole entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(TABLE_NAME);
            sql.Append(" (ZoneId,UserRoleId,ReadAccess,WriteAccess) VALUES (");
            sql.Append(entity.ZoneId.ToString() + ",");
            sql.Append(entity.UserRoleId.ToString() + ",");
            sql.Append(entity.ReadAccessAsInt.ToString() + ",");
            sql.Append(entity.WriteAccessAsInt.ToString() + ");");

            int affected = this.RunUpdateQuery(sql.ToString());
            if (affected > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Bulk insert into `ZoneUserRole`
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        public bool insert(List<CmsZoneUserRole> entityList)
        {
            if (entityList.Count == 0)
                return true;

            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(TABLE_NAME);
            sql.Append(" (ZoneId,UserRoleId,ReadAccess,WriteAccess) VALUES ");

            for (int x = 0; x < entityList.Count; x++)
            {
                CmsZoneUserRole g = entityList[x];
                sql.Append("(" + g.ZoneId.ToString() + "," + g.UserRoleId.ToString() + "," + g.ReadAccessAsInt.ToString() + "," + g.WriteAccessAsInt.ToString() + ")");
                if (x + 1 < entityList.Count)
                    sql.Append(",");
                else
                    sql.Append(";");
            }

            int affected = this.RunUpdateQuery(sql.ToString());
            if (affected == entityList.Count)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Delete records according to zone id
        /// </summary>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool deleteByZone(CmsPageSecurityZone z)
        {
            StringBuilder sql = new StringBuilder("DELETE FROM ");
            sql.Append(TABLE_NAME);
            sql.Append(" WHERE ZoneId=" + z.ZoneId.ToString() + ";");

            int affected = this.RunUpdateQuery(sql.ToString());
            if (affected > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Put datarow raw values to entity object
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        protected CmsZoneUserRole fromDataRow(DataRow dr)
        {
            CmsZoneUserRole entity = new CmsZoneUserRole();
            entity.ZoneId = Convert.ToInt32(dr["ZoneId"]);
            entity.UserRoleId = Convert.ToInt32(dr["UserRoleId"]);
            entity.ReadAccess = Convert.ToBoolean(dr["ReadAccess"]);
            entity.WriteAccess = Convert.ToBoolean(dr["WriteAccess"]);
            return entity;
        }
    }
}
