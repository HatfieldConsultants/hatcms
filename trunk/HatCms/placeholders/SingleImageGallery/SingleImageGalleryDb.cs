using System;
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
    public class SingleImageGalleryDb : PlaceholderDb
    {
        public SingleImageGalleryPlaceholderData getSingleImageGallery(CmsPage page, int identifier, CmsLanguage forLanguage, bool createNewIfDoesNotExist)
        {
            if (page.ID < 0 || identifier < 0)
                return new SingleImageGalleryPlaceholderData();

            string sql = "select * from singleimagegallery c where c.pageid = " + page.ID.ToString() + " and c.identifier = " + identifier.ToString() + " and langShortCode like '" + dbEncode(forLanguage.shortCode) + "' and deleted is null;";
            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];

                SingleImageGalleryPlaceholderData ret = new SingleImageGalleryPlaceholderData();
                
                ret.PageIdToGatherImagesFrom = getPossiblyNullValue(dr, "PageIdToGatherImagesFrom", CmsContext.HomePage.ID);

                ret.RecursiveGatherImages = Convert.ToBoolean(dr["RecursiveGatherImages"]);

                ret.ThumbImageDisplayBoxWidth = Convert.ToInt32(dr["ThumbnailDisplayBoxWidth"]);
                ret.ThumbImageDisplayBoxHeight = Convert.ToInt32(dr["ThumbnailDisplayBoxHeight"]);

                ret.OverrideFullDisplayBoxSize = Convert.ToBoolean(dr["OverrideFullDisplayBoxSize"]);

                

                ret.FullSizeDisplayBoxWidth = Convert.ToInt32(dr["FullSizeDisplayBoxWidth"]);

                ret.FullSizeDisplayBoxHeight = Convert.ToInt32(dr["FullSizeDisplayBoxHeight"]);

                ret.NumThumbsPerRow = Convert.ToInt32(dr["NumThumbsPerRow"]);

                ret.NumThumbsPerPage = Convert.ToInt32(dr["NumThumbsPerPage"]);
                
                ret.TagsImagesMustHave = dr["ShowOnlyTags"].ToString().Split(new string[] { SingleImageData.TagStorageSeperator }, StringSplitOptions.RemoveEmptyEntries);

                return ret;
            }
            else
            {
                if (createNewIfDoesNotExist)
                {
                    return createNewSingleImageGallery(page, identifier, forLanguage, new SingleImageGalleryPlaceholderData());
                }
                else
                {
                    throw new Exception("getSingleImageGallery database error: placeholder does not exist");
                }
            }
            return new SingleImageGalleryPlaceholderData();
        } // getPlainTextContent

        public SingleImageGalleryPlaceholderData createNewSingleImageGallery(CmsPage page, int identifier, CmsLanguage forLanguage, SingleImageGalleryPlaceholderData data)
        {
            string sql = "insert into singleimagegallery (pageid, identifier, langShortCode, PageIdToGatherImagesFrom,RecursiveGatherImages,ThumbnailDisplayBoxWidth,ThumbnailDisplayBoxHeight, OverrideFullDisplayBoxSize,FullSizeDisplayBoxWidth,FullSizeDisplayBoxHeight, NumThumbsPerRow, NumThumbsPerPage, ShowOnlyTags ) values (";
            sql += page.ID.ToString() + ",";
            sql += identifier.ToString() + ", ";
            sql += "'" + dbEncode(forLanguage.shortCode) + "', ";
            sql += "" + data.PageIdToGatherImagesFrom + ", ";
            sql += "" + Convert.ToInt32(data.RecursiveGatherImages).ToString() + ", ";            
            sql += "" + data.ThumbImageDisplayBoxWidth + ", ";
            sql += "" + data.ThumbImageDisplayBoxHeight + ", ";
            
            sql += "" + Convert.ToInt32(data.OverrideFullDisplayBoxSize).ToString() + ", ";
            sql += "" + (data.FullSizeDisplayBoxWidth).ToString() + ", ";
            sql += "" + (data.FullSizeDisplayBoxHeight).ToString() + ", ";
            sql += "" + (data.NumThumbsPerRow).ToString() + ", ";
            sql += "" + (data.NumThumbsPerPage).ToString() + ", ";            
            
            sql += "'" + dbEncode(String.Join(SingleImageData.TagStorageSeperator, data.TagsImagesMustHave)) + "' ";
            sql += "); ";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {

                if (page.setLastUpdatedDateTimeToNow())
                    return data;
            }
            return new SingleImageGalleryPlaceholderData();

        } // createNewSingleImageGallery

        public bool saveUpdatedSingleImageGallery(CmsPage page, int identifier, CmsLanguage forLanguage, SingleImageGalleryPlaceholderData data)
        {            
            string sql = "update singleimagegallery set ";
            sql += "PageIdToGatherImagesFrom = " + data.PageIdToGatherImagesFrom + ", ";
            sql += "RecursiveGatherImages = " + Convert.ToInt32(data.RecursiveGatherImages).ToString() + ", ";
            sql += "ThumbnailDisplayBoxHeight = " + data.ThumbImageDisplayBoxHeight + ", ";
            sql += "ThumbnailDisplayBoxWidth = " + data.ThumbImageDisplayBoxWidth + ", ";
            sql += "OverrideFullDisplayBoxSize = " + Convert.ToInt32(data.OverrideFullDisplayBoxSize).ToString() + ", ";
            sql += "FullSizeDisplayBoxWidth = " + (data.FullSizeDisplayBoxWidth).ToString() + ", ";
            sql += "FullSizeDisplayBoxHeight = " + (data.FullSizeDisplayBoxHeight).ToString() + ", ";
            sql += "NumThumbsPerRow = " + (data.NumThumbsPerRow).ToString() + ", ";
            sql += "NumThumbsPerPage = " + (data.NumThumbsPerPage).ToString() + ", ";            
            
            sql += "ShowOnlyTags = '" + dbEncode(String.Join(SingleImageData.TagStorageSeperator, data.TagsImagesMustHave)) + "' ";

            sql += " where pageid= " + page.ID.ToString();
            sql += " AND langShortCode like '" + dbEncode(forLanguage.shortCode) + "' ";
            sql += " AND identifier = " + identifier.ToString() + "; ";

            int numAffected = this.RunUpdateQuery(sql);
            if (numAffected > 0)
            {                
                return page.setLastUpdatedDateTimeToNow();
            }
            else
                return false;

        } // saveUpdatedSingleImageGallery
    }
}
