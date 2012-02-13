using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal.Data;

namespace HatCMS.Placeholders
{
    public class FlashObjectDb : PlaceholderDb
    {
        public FlashObjectData getFlashObject(CmsPage page, int identifier, bool createNewIfDoesNotExist)
        {
            if (page.Id < 0 || identifier < 0)
                return new FlashObjectData();

            string sql = "select SWFPath, DisplayWidth, DisplayHeight from flashobject c where c.pageid = " + page.Id.ToString() + " and c.identifier = " + identifier.ToString() + " and deleted is null;";
            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                FlashObjectData info = new FlashObjectData();
                info.SWFPath = dr["SWFPath"].ToString();
                info.DisplayWidth = Convert.ToInt32(dr["DisplayWidth"]);
                info.DisplayHeight = Convert.ToInt32(dr["DisplayHeight"]);
                return info;
            }
            else
            {
                if (createNewIfDoesNotExist)
                {
                    return createNewFlashObject(page, identifier);
                }
                else
                {
                    throw new Exception("getFlashObject database error: placeholder does not exist");
                }
            }
            return new FlashObjectData();
        } // getFlashObject

        public FlashObjectData createNewFlashObject(CmsPage page, int identifier)
        {
            string sql = "insert into flashobject (pageid, identifier, SWFPath, DisplayWidth, DisplayHeight) values (";
            sql = sql + page.Id.ToString() + "," + identifier.ToString() + ",";
            sql += "'', " + FlashObject.DefaultDisplayWidth.ToString() + ", " + FlashObject.DefaultDisplayHeight.ToString() + " ";
            sql += "); ";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {
                page.setLastUpdatedDateTimeToNow();
                FlashObjectData info = new FlashObjectData();                
                return info;
            }
            else
                return new FlashObjectData();

        } // createNewFlashObject

        public bool saveUpdatedFlashObject(CmsPage page, int identifier, FlashObjectData flashObject)
        {
            string sql = "update flashobject set ";
            sql += " SWFPath = '" + dbEncode(flashObject.SWFPath) + "', ";
            sql += " DisplayWidth = " + flashObject.DisplayWidth + ", ";
            sql += " DisplayHeight = " + flashObject.DisplayHeight + " ";
            sql += " where pageid= " + page.Id.ToString();
            sql += " AND identifier = " + identifier.ToString() + "; ";            

            int numAffected = this.RunUpdateQuery(sql);
            if (numAffected > 0)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;

        } // saveUpdatedFlashObject
    }
}
