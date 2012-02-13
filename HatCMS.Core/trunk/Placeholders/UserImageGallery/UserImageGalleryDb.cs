using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Placeholders
{
    public class UserImageGalleryDb : PlaceholderDb
    {
        public UserImageGalleryPlaceholderData getUserImageGalleryPlaceholderData(CmsPage page, int identifier, CmsLanguage lang, bool createNewIfDoesNotExist)
        {
            if (page.Id < 0 || identifier < 0)
                return new UserImageGalleryPlaceholderData();

            string sql = "select PageId, Identifier, LangCode, NumThumbsPerPage, NumThumbsPerRow, ThumbnailDisplayBoxWidth, ThumbnailDisplayBoxHeight, FullSizeDisplayBoxWidth, FullSizeDisplayBoxHeight, FullSizeLinkMode, CaptionDisplayLocation ";
            sql += " FROM userimagegallery ";
            sql += " WHERE PageId = " + page.Id.ToString() + " AND Identifier = " + identifier.ToString() + " AND LangCode='" + dbEncode(lang.shortCode) + "';";

            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                
                DataRow dr = ds.Tables[0].Rows[0];
                

                return fromDataRow(dr);
            }
            else if (createNewIfDoesNotExist)
            {
                return createNewUserImageGalleryPlaceholderData(page, identifier, lang);
            }
            else
                throw new ArgumentException("Error: no User Image Gallery placeholder exists");
        }

        private UserImageGalleryPlaceholderData fromDataRow(DataRow dr)
        {
            UserImageGalleryPlaceholderData ret = new UserImageGalleryPlaceholderData();
//            ret.UserImageGalleryPlaceholderId = Convert.ToInt32(dr["UserImageGalleryId"]);
            ret.PageId = Convert.ToInt32(dr["PageId"]);
            ret.Identifier = Convert.ToInt32(dr["Identifier"]);
            ret.Lang = new CmsLanguage(dr["LangCode"].ToString());
            ret.NumThumbsPerPage = Convert.ToInt32(dr["NumThumbsPerPage"]);
            ret.NumThumbsPerRow = Convert.ToInt32(dr["NumThumbsPerRow"]);
            ret.ThumbnailDisplayBoxWidth = Convert.ToInt32(dr["ThumbnailDisplayBoxWidth"]);
            ret.ThumbnailDisplayBoxHeight = Convert.ToInt32(dr["ThumbnailDisplayBoxHeight"]);
            ret.FullSizeDisplayBoxWidth = Convert.ToInt32(dr["FullSizeDisplayBoxWidth"]);
            ret.FullSizeDisplayBoxHeight = Convert.ToInt32(dr["FullSizeDisplayBoxHeight"]);
            ret.FullSizeLinkMode = (UserImageGalleryPlaceholderData.FullSizeImageDisplayMode)Enum.Parse(typeof(UserImageGalleryPlaceholderData.FullSizeImageDisplayMode), dr["FullSizeLinkMode"].ToString());
            ret.CaptionLocation = (UserImageGalleryPlaceholderData.CaptionDisplayLocation)Enum.Parse(typeof(UserImageGalleryPlaceholderData.CaptionDisplayLocation), dr["CaptionDisplayLocation"].ToString());
            return ret;
        }

        public UserImageGalleryPlaceholderData[] getAllUserImageGalleryPlaceholderDatas()
        {
            string sql = "select u.pageid, u.identifier, u.langcode, u.NumThumbsPerPage, u.NumThumbsPerRow, u.ThumbnailDisplayBoxWidth, u.ThumbnailDisplayBoxHeight, u.FullSizeDisplayBoxWidth, u.FullSizeDisplayBoxHeight, u.FullSizeLinkMode, u.CaptionDisplayLocation ";
            sql += " FROM userimagegallery u left join pages p on (p.pageid = u.pageid) ";
            sql += " where p.deleted is null ";

            DataSet ds = RunSelectQuery(sql);
            List<UserImageGalleryPlaceholderData> ret = new List<UserImageGalleryPlaceholderData>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                ret.Add(fromDataRow(dr));
            } // foreach
            return ret.ToArray();
        }

        public UserImageGalleryPlaceholderData createNewUserImageGalleryPlaceholderData(CmsPage page, int identifier, CmsLanguage lang)
        {
            UserImageGalleryPlaceholderData item = UserImageGalleryPlaceholderData.CreateWithDefaults();

            string sql = "INSERT INTO userimagegallery ";
            sql += "(PageId, Identifier, LangCode, NumThumbsPerPage, NumThumbsPerRow, ThumbnailDisplayBoxWidth, ThumbnailDisplayBoxHeight, FullSizeDisplayBoxWidth, FullSizeDisplayBoxHeight, FullSizeLinkMode, CaptionDisplayLocation)";
            sql += " VALUES ( ";
            sql += page.Id.ToString() + ", ";
            sql += identifier.ToString() + ", '";
            sql += dbEncode(lang.shortCode) + "', ";
            sql += item.NumThumbsPerPage.ToString() + ", ";
            sql += item.NumThumbsPerRow.ToString() + ", ";
            sql += item.ThumbnailDisplayBoxWidth.ToString() + ", ";
            sql += item.ThumbnailDisplayBoxHeight.ToString() + ", ";
            sql += item.FullSizeDisplayBoxWidth.ToString() + ", ";
            sql += item.FullSizeDisplayBoxHeight.ToString() + ", ";
            sql += "'" + dbEncode(Enum.GetName(typeof(UserImageGalleryPlaceholderData.FullSizeImageDisplayMode), item.FullSizeLinkMode)) + "'" + ", ";
            sql += "'" + dbEncode(Enum.GetName(typeof(UserImageGalleryPlaceholderData.CaptionDisplayLocation), item.CaptionLocation)) + "'" + " ";
            sql += " ); ";

            int newId = this.RunInsertQuery(sql);
            if (newId >= 0)
            {
//                item.UserImageGalleryPlaceholderId = newId;
                page.setLastUpdatedDateTimeToNow();
                return item;
            }
            else
                return new UserImageGalleryPlaceholderData();
        }

        public bool saveUpdatedUserImageGalleryPlaceholderData(CmsPage page, int identifier, CmsLanguage lang, UserImageGalleryPlaceholderData item)
        {
            string sql = "UPDATE userimagegallery SET ";

            sql += "NumThumbsPerPage = " + item.NumThumbsPerPage.ToString() + ", ";
            sql += "NumThumbsPerRow = " + item.NumThumbsPerRow.ToString() + ", ";
            sql += "ThumbnailDisplayBoxWidth = " + item.ThumbnailDisplayBoxWidth.ToString() + ", ";
            sql += "ThumbnailDisplayBoxHeight = " + item.ThumbnailDisplayBoxHeight.ToString() + ", ";
            sql += "FullSizeDisplayBoxWidth = " + item.FullSizeDisplayBoxWidth.ToString() + ", ";
            sql += "FullSizeDisplayBoxHeight = " + item.FullSizeDisplayBoxHeight.ToString() + ", ";
            sql += "FullSizeLinkMode = " + "'" + dbEncode(Enum.GetName(typeof(UserImageGalleryPlaceholderData.FullSizeImageDisplayMode), item.FullSizeLinkMode)) + "'" + ", ";
            sql += "CaptionDisplayLocation = " + "'" + dbEncode(Enum.GetName(typeof(UserImageGalleryPlaceholderData.CaptionDisplayLocation), item.CaptionLocation)) + "'" + " ";
            sql += " WHERE ";
            sql += "PageId = " + page.Id.ToString() + " AND ";
            sql += "Identifier = " + identifier.ToString() + " AND ";
            sql += "LangCode = '" + dbEncode(lang.shortCode) + "' ";
            sql += " ; ";

            int numAffected = this.RunUpdateQuery(sql);
            if (numAffected < 0)
            {
                return false;
            }
            return true;

        } // saveUpdatedUserImageGalleryPlaceholderData


    }
}
