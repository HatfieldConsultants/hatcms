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
    public class GoogleMapDb : PlaceholderDb
    {
        // ALTER TABLE googlemap ADD COLUMN `KMLOverlayUrl` TEXT NOT NULL AFTER `MapType`;
        public GoogleMapInfo getGoogleMap(CmsPage page, int identifier, bool createNewIfDoesNotExist)
        {
            if (page.Id < 0 || identifier < 0)
                return new GoogleMapInfo();

            string sql = "select APIKey, PopupHtml, KMLOverlayUrl, Latitude, Longitude,intitialZoomLevel, MapType from googlemap c where c.pageid = " + page.Id.ToString() + " and c.identifier = " + identifier.ToString() + " and deleted is null;";
            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                GoogleMapInfo info = new GoogleMapInfo();
                info.APIKey = dr["APIKey"].ToString();
                info.PopupHtml = dr["PopupHtml"].ToString();
                info.Latitude = Convert.ToDouble(dr["Latitude"]);
                info.Longitude = Convert.ToDouble(dr["Longitude"]);
                try
                {
                    info.intitialZoomLevel = Convert.ToInt32(dr["intitialZoomLevel"]);
                }
                catch
                {
                    throw new Exception("you need to add a intitialZoomLevel column to the database!!");
                    // ALTER TABLE `googlemap` ADD COLUMN `intitialZoomLevel` int(10) unsigned NOT NULL default 13 AFTER `Longitude`;
                }

                try
                {
                    info.displayType = (GoogleMapInfo.MapDisplay)Enum.Parse(typeof(GoogleMapInfo.MapDisplay), dr["MapType"].ToString());
                }
                catch
                {
                    throw new Exception("you need to add a MapType column to the database!!");
                    // ALTER TABLE `googlemap` ADD COLUMN `MapType` VARCHAR(60) NOT NULL AFTER `Longitude`;
                }

                try
                {
                    info.KMLOverlayUrl = dr["KMLOverlayUrl"].ToString();
                }
                catch
                {
                    throw new Exception("you need to add a KMLOverlayUrl column to the database!!");
                    // ALTER TABLE `googlemap` ADD COLUMN `KMLOverlayUrl` VARCHAR(255) NOT NULL default '' AFTER `PopupHtml`;
                }
                return info;
            }
            else
            {
                if (createNewIfDoesNotExist)
                {
                    return createNewGoogleMap(page, identifier);
                }
                else
                {
                    throw new Exception("getGoogleMap database error: placeholder does not exist");
                }
            }
            return new GoogleMapInfo();
        } // getHtmlContent

        public GoogleMapInfo createNewGoogleMap(CmsPage page, int identifier)
        {
            GoogleMapInfo info = new GoogleMapInfo();

            string sql = "insert into googlemap (pageid, identifier, APIKey, PopupHtml, KMLOverlayUrl, Latitude, Longitude, MapType) values (";
            sql = sql + page.Id.ToString() + "," + identifier.ToString() + ",";
            sql += "'"+dbEncode(info.APIKey)+"', '"+dbEncode(info.PopupHtml)+"', '"+dbEncode(info.KMLOverlayUrl)+"', "+info.Latitude+", "+info.Longitude+",  ";
            sql += "'" + dbEncode(Enum.GetName(typeof(GoogleMapInfo.MapDisplay), info.displayType)) + "' ";
            sql += "); ";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {
                page.setLastUpdatedDateTimeToNow();                
                return info;
            }
            else
                return new GoogleMapInfo();

        }
        /// <summary>
        
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="mapInfo"></param>
        /// <returns></returns>
        public bool saveUpdatedGoogleMap(CmsPage page, int identifier, GoogleMapInfo mapInfo)
        {
            string sql = "update googlemap set ";
            sql += " APIKey = '"+dbEncode(mapInfo.APIKey)+"', ";
            sql += " KMLOverlayUrl = '" + dbEncode(mapInfo.KMLOverlayUrl) + "', ";            
            sql += " PopupHtml = '" + dbEncode(mapInfo.PopupHtml) + "', ";
            sql += " Latitude = "+mapInfo.Latitude+", ";
            sql += " Longitude = " + mapInfo.Longitude + ", ";
            sql += " MapType = '" + dbEncode(Enum.GetName(typeof(GoogleMapInfo.MapDisplay),mapInfo.displayType)) + "' ";
            sql += " where pageid= " + page.Id.ToString();
            sql += " AND identifier = " + identifier.ToString() + "; ";
            //sql = sql + " SELECT LAST_INSERT_ID() as newId;";

            int numAffected = this.RunUpdateQuery(sql);
            if (numAffected > 0)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;

        }
    }
}
