using Hatfield.Web.Portal.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System;

namespace HatCMS
{
    /// <summary>
    /// DB object for `Zone`
    /// </summary>
    public class CmsPageSecurityZoneDb : MySqlDbObject
    {
        protected static string TABLE_NAME = "zone";

        public CmsPageSecurityZoneDb() : base(ConfigurationManager.AppSettings["ConnectionString"])
        { }

        public CmsPageSecurityZone fetch(int zoneId)
        {
            StringBuilder sql = new StringBuilder("SELECT ZoneId, StartingPageId, ZoneName FROM ");
            sql.Append(TABLE_NAME);
            sql.Append(" WHERE Deleted IS NULL ");
            sql.Append(" AND ZoneId=" + zoneId.ToString() + ";");

            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds) == false)
                return new CmsPageSecurityZone();

            DataRow dr = ds.Tables[0].Rows[0];
            return fromDataRow(dr);
        }

        /// <summary>
        /// Find out the zone where the current page is located.  If the current page is
        /// not defined in ZoneManagement, search the parent page.  Repeat until a record
        /// is defined in ZoneManagement.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public CmsPageSecurityZone fetchByPage(CmsPage page)
        {
            StringBuilder sql = new StringBuilder("SELECT p.ParentPageId, p.PageId, z.ZoneId, z.StartingPageId, z.ZoneName FROM pages p LEFT JOIN ");
            sql.Append("(SELECT * FROM " + TABLE_NAME + " WHERE Deleted IS NULL) z on p.PageId=z.StartingPageId");
            sql.Append(" WHERE p.PageId={0};");

            int id = page.ID;
            CmsPageSecurityZone z = null;
            while (z == null)
            {
                string formattedSQL = String.Format(sql.ToString(), new string[] { id.ToString() });
                DataSet ds = this.RunSelectQuery(formattedSQL);
                if (hasSingleRow(ds))
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    try
                    {
                        z = fromDataRow(dr);
                        return z;
                    }
                    catch
                    {
                        try
                        {
                            id = Convert.ToInt32(dr["ParentPageId"]);
                        }
                        catch
                        {
                            break;
                        }
                    } // catch
                } // if hasRows
                else
                {
                    throw new Exception("Error: can not execute Zone SQL: " + formattedSQL);
                }
            }
            return z;
        }

        /// <summary>
        /// Recursive is T: see what CmsZone a page is.
        /// Recursive is F: select the exact zone record given a cms page (i.e. boundary page).
        /// </summary>
        /// <param name="page"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public CmsPageSecurityZone fetchByPage(CmsPage page, bool recursive)
        {
            if (recursive)
                return fetchByPage(page);

            StringBuilder sql = new StringBuilder("SELECT p.ParentPageId, p.PageId, z.ZoneId, z.StartingPageId, z.ZoneName FROM pages p LEFT JOIN ");
            sql.Append("(SELECT * FROM " + TABLE_NAME + " WHERE Deleted IS NULL) z on p.PageId=z.StartingPageId");
            sql.Append(" WHERE p.PageId={0};");

            int id = page.ID;
            CmsPageSecurityZone z = null;
            string formattedSQL = String.Format(sql.ToString(), new string[] { id.ToString() });
            DataSet ds = this.RunSelectQuery(formattedSQL);
            DataRow dr = ds.Tables[0].Rows[0];
            try
            {
                z = fromDataRow(dr);
            }
            catch { }
            return z;
        }

        /// <summary>
        /// Select all zone records order by their names
        /// </summary>
        /// <returns></returns>
        public List<CmsPageSecurityZone> fetchAll()
        {
            StringBuilder sql = new StringBuilder("SELECT ZoneId, StartingPageId, ZoneName FROM ");
            sql.Append(TABLE_NAME);
            sql.Append(" WHERE Deleted IS NULL");
            sql.Append(" ORDER BY ZoneName;");

            DataSet ds = this.RunSelectQuery(sql.ToString());

            List<CmsPageSecurityZone> list = new List<CmsPageSecurityZone>();
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                    list.Add(fromDataRow(dr));
            }
            return list;
        }

        /// <summary>
        /// Insert into `zone`
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool insert(CmsPageSecurityZone entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(TABLE_NAME);
            sql.Append(" (StartingPageId,ZoneName) VALUES (");
            sql.Append(entity.StartingPageId.ToString() + ",'");
            sql.Append(dbEncode(entity.ZoneName) + "');");

            int newId = this.RunInsertQuery(sql.ToString());
            if (newId > 0)
            {
                entity.ZoneId = newId;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Update `Zone`
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool update(CmsPageSecurityZone entity)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(TABLE_NAME);
            sql.Append(" SET StartingPageId=" + entity.StartingPageId.ToString() + ",");
            sql.Append(" ZoneName='" + dbEncode(entity.ZoneName) + "'");
            sql.Append(" WHERE ZoneId=" + entity.ZoneId.ToString() + ";");

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        /// <summary>
        /// Delete from `Zone`
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool delete(CmsPageSecurityZone entity)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(TABLE_NAME);
            sql.Append(" SET Deleted=Now()");
            sql.Append(" WHERE ZoneId=" + entity.ZoneId.ToString() + ";");

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        /// <summary>
        /// Put raw datarow values to entity object
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        protected CmsPageSecurityZone fromDataRow(DataRow dr)
        {
            CmsPageSecurityZone entity = new CmsPageSecurityZone();
            entity.ZoneId = Convert.ToInt32(dr["ZoneId"]);
            entity.StartingPageId = Convert.ToInt32(dr["StartingPageId"]);
            entity.ZoneName = dr["ZoneName"].ToString();
            return entity;
        }
    }
}
