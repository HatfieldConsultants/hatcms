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
using Hatfield.Web.Portal.Data;

namespace HatCMS.Placeholders
{
    public class SingleImageDb : PlaceholderDb
    {
        // ALTER TABLE `singleimage` ADD COLUMN `Tags` VARCHAR(255) NOT NULL DEFAULT '' AFTER `Credits`;
        private SingleImageData fromDataRow(DataRow dr)
        {            
            SingleImageData info = new SingleImageData();
            info.SingleImageId = Convert.ToInt32(dr["SingleImageId"]);
            info.PageId = Convert.ToInt32(dr["PageId"]);
            info.PageIdentifier = Convert.ToInt32(dr["Identifier"]);
            info.ImagePath = dr["ImagePath"].ToString();
            /*
            info.ThumbnailDisplayBoxWidth = Convert.ToInt32(dr["ThumbnailDisplayBoxWidth"]);
            info.ThumbnailDisplayBoxHeight = Convert.ToInt32(dr["ThumbnailDisplayBoxHeight"]);
            info.FullSizeDisplayBoxWidth = Convert.ToInt32(dr["FullSizeDisplayBoxWidth"]);
            info.FullSizeDisplayBoxHeight = Convert.ToInt32(dr["FullSizeDisplayBoxHeight"]);
             */
            info.Caption = (dr["Caption"]).ToString();
            info.Credits = (dr["Credits"]).ToString();

            string sTags = (dr["Tags"]).ToString();
            string[] arrTags = sTags.Split(new string[] { SingleImageData.TagStorageSeperator }, StringSplitOptions.RemoveEmptyEntries);
            info.Tags = arrTags;
            
            return info;
        }

        /// <summary>
        /// returns NULL if not found
        /// </summary>
        /// <param name="SingleImageId"></param>
        /// <returns></returns>
        public SingleImageData getSingleImage(int SingleImageId)
        {
            string sql = "select PageId, Identifier, SingleImageId, ImagePath, Caption, Credits, Tags from singleimage c ";
            sql += " where c.SingleImageId = " + SingleImageId.ToString() + "; "; 

            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];

                SingleImageData info = fromDataRow(dr);

                return info;
            }

            return null;

        }
        
        public SingleImageData getSingleImage(CmsPage page, int identifier, CmsLanguage forLanguage, bool createNewIfDoesNotExist)
        {
            if (page.ID < 0 || identifier < 0)
                return new SingleImageData();

            string sql = "select PageId,Identifier, SingleImageId, ImagePath, Caption, Credits, Tags from singleimage c ";
            sql += " where c.pageid = " + page.ID.ToString() + " and c.identifier = " + identifier.ToString() + " and RevisionNumber = " + page.RevisionNumber.ToString() + " and langShortCode like '" + dbEncode(forLanguage.shortCode) + "' and deleted is null;";
            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasRows(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                
                SingleImageData info = fromDataRow(dr);

                return info;
            }
            else
            {
                if (createNewIfDoesNotExist)
                {
                    return createNewSingleImage(page, identifier, forLanguage, new SingleImageData());
                }
                else
                {
                    throw new Exception("getSingleImage database error: placeholder does not exist");
                }
            }
            return new SingleImageData();
        } // getSingleImage
        
        public SingleImageData createNewSingleImage(CmsPage page, int identifier, CmsLanguage forLanguage, SingleImageData imgData)
        {
            string sql = "insert into singleimage (pageid, identifier,RevisionNumber, ImagePath, ThumbnailDisplayBoxWidth, ThumbnailDisplayBoxHeight, FullSizeDisplayBoxWidth, FullSizeDisplayBoxHeight, Caption, Credits, Tags, langShortCode) values (";
            sql = sql + page.ID.ToString() + "," + identifier.ToString() + "," + page.RevisionNumber.ToString() + ",";
            sql += "'" + imgData.ImagePath + "', -1, ";
            sql += "-1, ";
            sql += "-1, ";
            sql += "-1, ";
            sql += "'" + dbEncode(imgData.Caption) + "', ";
            sql += "'"+ dbEncode(imgData.Credits) + "', ";
            sql += "'" + dbEncode(String.Join(SingleImageData.TagStorageSeperator, imgData.Tags)) + "', ";
            sql += "'" + dbEncode(forLanguage.shortCode) + "' ";
            sql += "); ";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {
                page.setLastUpdatedDateTimeToNow();
                SingleImageData info = new SingleImageData();
                info.PageId = page.ID;
                info.SingleImageId = newId;
                return info;
            }
            else
                return new SingleImageData();

        }
        

        public bool saveUpdatedSingleImage(CmsPage page, int identifier, CmsLanguage forLanguage, SingleImageData image)
        {
            // with revisions we insert, not update the database
            int newRevisionNumber = page.createNewRevision();
            if (newRevisionNumber < 0)
                return false;

            // insert values for un-used columns because default values may not be present.

            string sql = "insert into singleimage (pageid, identifier,RevisionNumber, ImagePath, ThumbnailDisplayBoxWidth, ThumbnailDisplayBoxHeight, FullSizeDisplayBoxWidth, FullSizeDisplayBoxHeight, Caption, Credits, Tags, langShortCode) values (";
            sql = sql + page.ID.ToString() + "," + identifier.ToString() + "," + newRevisionNumber.ToString() + ",";
            sql += "'" + dbEncode(image.ImagePath) + "', ";
            sql += "-1, ";
            sql += "-1, ";
            sql += "-1, ";
            sql += "-1, ";
            sql += "'" + dbEncode(image.Caption) + "',";
            sql += "'" + dbEncode(image.Credits) + "', ";
            sql += "'" + dbEncode(String.Join(SingleImageData.TagStorageSeperator, image.Tags)) + "', ";
            sql += "'" + dbEncode(forLanguage.shortCode) + "' ";
            sql += "); ";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {
                page.setLastUpdatedDateTimeToNow();
                SingleImageData info = new SingleImageData();
                info.PageId = page.ID;
                info.SingleImageId = newId;
                return true;
            }
            else
                return false;

        }

        public SingleImageData[] getSingleImages(CmsPage[] pages, CmsLanguage specifiedLanguage)
        {
            if (pages.Length < 0)
                return new SingleImageData[0];

            List<string> whereOrStatements = new List<string>();
            foreach (CmsPage p in pages)
            {
                whereOrStatements.Add("( c.pageid = " + p.ID + " and c.RevisionNumber = "+p.RevisionNumber+"   )");                
            } // foreach

            string sql = "select PageId, Identifier, SingleImageId, ImagePath, Caption, Credits, Tags from singleimage c ";
            sql += " where (" + String.Join(" OR ", whereOrStatements.ToArray()) + ") ";
            if (CmsConfig.Languages.Length > 1)
            {
                sql += " and langShortCode like '" + dbEncode(specifiedLanguage.shortCode) + "' ";
            }
            sql += " and ImagePath <> '' and deleted is null;";
            DataSet ds = this.RunSelectQuery(sql);
            List<SingleImageData> ret = new List<SingleImageData>();
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    SingleImageData d = fromDataRow(dr);
                    ret.Add(d);
                } // foreach
            }

            return ret.ToArray();

        } // getSingleImages


        public string[] getAllPossibleTags()
        {
            string configEntry = CmsConfig.getConfigValue("SingleImage.Tags", "");
            string[] tags = configEntry.Split(new string[] { SingleImageData.TagStorageSeperator }, StringSplitOptions.RemoveEmptyEntries);
            List<string> ret = new List<string>();
            foreach (string t in tags)
            {
                if (ret.IndexOf(t) < 0)
                    ret.Add(t);
            } // foreach
            return ret.ToArray();
        }

        public string[] getAllTagsUsedByActiveImages()
        {
            string sql = "SELECT distinct s.tags FROM singleimage s left join pages p on (p.pageid = s.pageid) where tags != '' and s.revisionnumber = p.revisionnumber and p.deleted is null and s.deleted is null;";
            DataSet ds = this.RunSelectQuery(sql);
            List<string> ret = new List<string>();
            if (this.hasRows(ds))
            {
                foreach(DataRow dr in ds.Tables[0].Rows)
                {
                    string[] tags = dr["tags"].ToString().Split(new string[] { SingleImageData.TagStorageSeperator }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string t in tags)
                    {
                        if (ret.IndexOf(t) < 0)
                            ret.Add(t);
                    } // foreach
                } // foreach
            }
            return ret.ToArray();
        }
    }
}
