using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Hatfield.Web.Portal.Data;

namespace HatCMS.Placeholders
{
    public class FileLibraryDb : PlaceholderDb
    {
        #region aggregator
        protected static string AGGREGATOR_TABLE = "FileLibraryAggregator";

        public bool deleteAggregatorData(CmsPage page)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(AGGREGATOR_TABLE);
            sql.Append(" SET Deleted=NOW()");
            sql.Append(" WHERE PageId=" + page.ID.ToString() + ";");

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        /// <summary>
        /// Insert file library aggregator data
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool insertFileLibraryAggregator(CmsPage page, int identifier, CmsLanguage lang, FileLibraryAggregatorData entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(AGGREGATOR_TABLE);
            sql.Append(" (PageId,Identifier,LangCode,NumFilesOverview,NumFilesPerPage) VALUES (");
            sql.Append(page.ID.ToString() + ",");
            sql.Append(identifier.ToString() + ",'");
            sql.Append(dbEncode(lang.shortCode) + "',");
            sql.Append(entity.NumFilesForOverview.ToString() + ",");
            sql.Append(entity.NumFilesPerPage.ToString() + ");");

            int affected = this.RunUpdateQuery(sql.ToString());
            if (affected > 0)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;
        }

        /// <summary>
        /// select file library aggregator data
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="createIfNotExist"></param>
        /// <returns></returns>
        public FileLibraryAggregatorData fetchAggregatorData(CmsPage page, int identifier, CmsLanguage lang, bool createIfNotExist)
        {
            if (page.ID < 0 || identifier < 0)
                return new FileLibraryAggregatorData();

            StringBuilder sql = new StringBuilder("SELECT NumFilesOverview,NumFilesPerPage FROM ");
            sql.Append(AGGREGATOR_TABLE);
            sql.Append(" WHERE PageId=" + page.ID.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "'");
            sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND Deleted is null;");

            FileLibraryAggregatorData entity = new FileLibraryAggregatorData();
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                rowToData(dr, entity);
            }
            else
            {
                if (createIfNotExist)
                {
                    if (insertFileLibraryAggregator(page, identifier, lang, entity) == false)
                        throw new Exception("fetchFileLibraryAggregator() database error: Error creating new placeholder");
                }
                else
                    throw new Exception("fetchFileLibraryAggregator() database error: placeholder does not exist");
            }
            return entity;
        }

        /// <summary>
        /// Update file library aggregator data
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool updateAggregatorData(CmsPage page, int identifier, CmsLanguage lang, FileLibraryAggregatorData entity)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(AGGREGATOR_TABLE);
            sql.Append(" SET NumFilesOverview=" + entity.NumFilesForOverview.ToString() + ",");
            sql.Append(" NumFilesPerPage=" + entity.NumFilesPerPage.ToString());
            sql.Append(" WHERE PageId=" + page.ID.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "'");
            sql.Append(" AND Identifier=" + identifier.ToString());

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        /// <summary>
        /// Put datarow raw values to entity object
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="entity"></param>
        protected void rowToData(DataRow dr, FileLibraryAggregatorData entity)
        {
            entity.NumFilesForOverview = Convert.ToInt32(dr["NumFilesOverview"]);
            entity.NumFilesPerPage = Convert.ToInt32(dr["NumFilesPerPage"]);
        }
        #endregion

        #region details
        protected static string DETAILS_TABLE = "filelibrarydetails";

        /// <summary>
        /// Delete file library details data (set DELETED flag with NOW())
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public bool deleteDetailsData(CmsPage page)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(DETAILS_TABLE);
            sql.Append(" SET Deleted=NOW()");
            sql.Append(" WHERE PageId=" + page.ID.ToString() + ";");

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        /// <summary>
        /// Insert file library details data
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool insertDetailsData(CmsPage detailsPage, int identifier, CmsLanguage lang, FileLibraryDetailsData entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(DETAILS_TABLE);
            sql.Append(" (PageId,Identifier,LangCode,FileName,CategoryId,Author,Description,LastModified,CreatedBy,EventPageId) VALUES (");
            sql.Append(detailsPage.ID.ToString() + ",");
            sql.Append(identifier.ToString() + ",'");
            sql.Append(dbEncode(lang.shortCode) + "','");
            sql.Append(dbEncode(entity.FileName) + "',");
            sql.Append(entity.CategoryId.ToString() + ",'");
            sql.Append(dbEncode(entity.Author) + "','");
            sql.Append(dbEncode(entity.Description) + "',");
            sql.Append(dbEncode(DateTime.Now) + ",'");
            sql.Append(dbEncode(entity.CreatedBy) + "',");
            sql.Append(entity.EventPageId.ToString() + ");");

            int affected = this.RunUpdateQuery(sql.ToString());
            if (affected > 0) {
                entity.DetailsPageId = detailsPage.ID;
                entity.Identifier = identifier;
                entity.Lang = lang;
                return detailsPage.setLastUpdatedDateTimeToNow();
            }
            else
                return false;
        }

        /// <summary>
        /// Update `FileLibraryDetails` (FileName and CreatedBy will not be changed)
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool updateDetailsData(CmsPage detailsPage, int identifier, CmsLanguage lang, FileLibraryDetailsData entity)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(DETAILS_TABLE);
            sql.Append(" SET CategoryId=" + entity.CategoryId.ToString() + ",");
            sql.Append(" Author='" + dbEncode(entity.Author) + "',");
            sql.Append(" Description='" + dbEncode(entity.Description) + "',");
            sql.Append(" LastModified=" + dbEncode(DateTime.Now) + ",");
            sql.Append(" EventPageId=" + entity.EventPageId.ToString());
            sql.Append(" WHERE PageId=" + detailsPage.ID.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "'");
            sql.Append(" AND Identifier=" + identifier.ToString());

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        /// <summary>
        /// Select the record count according to category
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        public int fetchCountByCategory(FileLibraryCategoryData cat)
        {
            StringBuilder sql = new StringBuilder("SELECT Count(PageId) AS RecordCount FROM ");
            sql.Append(DETAILS_TABLE);
            sql.Append(" WHERE CategoryId=" + cat.CategoryId.ToString());
            sql.Append(" AND Deleted is null;");

            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                return Convert.ToInt32(dr["RecordCount"]);
            }
            return 0;
        }

        /// <summary>
        /// Select the record count according to category (overload)
        /// </summary>
        /// <param name="cat"></param>
        /// <param name="pageArray"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public int fetchCountByCategory(FileLibraryCategoryData cat, CmsPage[] pageArray, int identifier, CmsLanguage lang)
        {
            if (pageArray.Length == 0)
                return 0;

            StringBuilder pageId = new StringBuilder();
            foreach (CmsPage p in pageArray)
                pageId.Append(p.ID.ToString() + ",");
            pageId.Remove(pageId.Length - 1, 1);

            StringBuilder sql = new StringBuilder("SELECT Count(PageId) AS RecordCount FROM ");
            sql.Append(DETAILS_TABLE);
            sql.Append(" WHERE Deleted IS NULL");
            sql.Append(" AND CategoryId=" + cat.CategoryId.ToString());
            sql.Append(" AND PageId IN(" + pageId.ToString() + ")");
            sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND LangCode='" + dbEncode(lang.shortCode) + "'");

            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                return Convert.ToInt32(dr["RecordCount"]);
            }
            return 0;
        }

        public FileLibraryDetailsData fetchDetailsData(CmsPage page, int identifier, CmsLanguage lang, bool createIfNotExist)
        {
            if (page.ID < 0 || identifier < 0)
                return new FileLibraryDetailsData();

            StringBuilder sql = new StringBuilder("SELECT PageId,Identifier,LangCode,FileName,CategoryId,Author,Description,LastModified,CreatedBy,EventPageId FROM ");
            sql.Append(DETAILS_TABLE);
            sql.Append(" WHERE PageId=" + page.ID.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "'");
            sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND Deleted is null;");

            FileLibraryDetailsData entity = new FileLibraryDetailsData();
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                rowToData(dr, entity);
            }
            else
            {
                if (createIfNotExist)
                {
                    if (insertDetailsData(page, identifier, lang, entity) == false)
                        throw new Exception("fetchDetailsData() database error: Error creating new placeholder");
                }
                else
                    throw new Exception("fetchDetailsData() database error: placeholder does not exist");
            }
            return entity;
        }

        public List<FileLibraryDetailsData> fetchDetailsData(CmsPage page)
        {
            StringBuilder sql = new StringBuilder("SELECT PageId,Identifier,LangCode,FileName,CategoryId,Author,Description,LastModified,CreatedBy,EventPageId FROM ");
            sql.Append(DETAILS_TABLE);
            sql.Append(" WHERE PageId=" + page.ID.ToString());
            sql.Append(" AND Deleted IS NULL");
            sql.Append(" ORDER BY FileName, LangCode;");

            List<FileLibraryDetailsData> list = new List<FileLibraryDetailsData>();
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    FileLibraryDetailsData entity = new FileLibraryDetailsData();
                    rowToData(dr, entity);
                    list.Add(entity);
                }
            }
            return list;
        }

        public List<FileLibraryDetailsData> fetchDetailsData(CmsPage[] pageArray, CmsLanguage lang)
        {
            return fetchDetailsData(pageArray, Int32.MinValue, lang, null, Int32.MinValue, Int32.MinValue);
        }

        public List<FileLibraryDetailsData> fetchDetailsData(CmsPage[] pageArray, int identifier, CmsLanguage lang)
        {
            return fetchDetailsData(pageArray, identifier, lang, null, Int32.MinValue, Int32.MinValue);
        }

        public List<FileLibraryDetailsData> fetchDetailsData(CmsPage[] pageArray, int identifier, CmsLanguage lang, FileLibraryCategoryData category, int offset, int count)
        {
            List<FileLibraryDetailsData> list = new List<FileLibraryDetailsData>();
            if (pageArray.Length == 0)
                return list;

            List<string> pageIds = new List<string>();
            foreach (CmsPage p in pageArray)
                pageIds.Add(p.ID.ToString());            

            StringBuilder sql = new StringBuilder("SELECT PageId,Identifier,LangCode,FileName,CategoryId,Author,Description,LastModified,CreatedBy,EventPageId FROM ");
            sql.Append(DETAILS_TABLE);
            sql.Append(" WHERE ");
            sql.Append(" PageId IN(" + string.Join(",", pageIds.ToArray()) + ")");
            if (identifier >= 0 )
                sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND LangCode='" + dbEncode(lang.shortCode) + "'");
            if (category != null)
            {
                sql.Append(" AND CategoryId=" + category.CategoryId);
            }
            sql.Append(" AND Deleted IS NULL");
            
            if (category != null)
                sql.Append(" ORDER BY PageId DESC");
            else
                sql.Append(" ORDER BY CategoryId DESC");

            if (count > -1)
                sql.Append(" LIMIT " + offset + "," + count + "");
            else
                sql.Append(";");

            DataSet ds = this.RunSelectQuery(sql.ToString());

            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    FileLibraryDetailsData entity = new FileLibraryDetailsData();
                    rowToData(dr, entity);
                    list.Add(entity);
                }
            }
            return list;
        }

        public List<FileLibraryDetailsData> fetchDetailsData(CmsLanguage lang, CmsPage eventPage)
        {
            StringBuilder sql = new StringBuilder("SELECT PageId,Identifier,LangCode,FileName,CategoryId,Author,Description,LastModified,CreatedBy,EventPageId FROM ");
            sql.Append(DETAILS_TABLE);
            sql.Append(" WHERE EventPageId=" + eventPage.ID.ToString());
            sql.Append(" AND LangCode='" + dbEncode(lang.shortCode) + "'");
            sql.Append(" AND Deleted IS NULL");
            sql.Append(" ORDER BY PageId;");

            DataSet ds = this.RunSelectQuery(sql.ToString());

            List<FileLibraryDetailsData> list = new List<FileLibraryDetailsData>();
            datasetToList(ds, list);
            return list;
        }

        /// <summary>
        /// Select the latest uploaded files to show on overview tab
        /// </summary>
        /// <param name="pageArray"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<FileLibraryDetailsData> fetchLatestUpload(CmsPage[] pageArray, int identifier, CmsLanguage lang, int count)
        {
            List<FileLibraryDetailsData> list = new List<FileLibraryDetailsData>();
            if (pageArray.Length == 0)
                return list;

            StringBuilder pageId = new StringBuilder();
            foreach (CmsPage p in pageArray)
                pageId.Append(p.ID.ToString() + ",");
            pageId.Remove(pageId.Length - 1, 1);

            StringBuilder sql = new StringBuilder("SELECT PageId,Identifier,LangCode,FileName,CategoryId,Author,Description,LastModified,CreatedBy,EventPageId FROM ");
            sql.Append(DETAILS_TABLE);
            sql.Append(" WHERE PageId IN (" + pageId.ToString() + ")");
            sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND LangCode='" + dbEncode(lang.shortCode) + "'");
            sql.Append(" ORDER BY PageId DESC");
            sql.Append(" LIMIT 0, " + count + ";");
            DataSet ds = this.RunSelectQuery(sql.ToString());

            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    FileLibraryDetailsData entity = new FileLibraryDetailsData();
                    rowToData(dr, entity);
                    list.Add(entity);
                }
            }
            return list;
        }

        protected void datasetToList(DataSet ds, List<FileLibraryDetailsData> list)
        {
            if (this.hasRows(ds) == false)
                return;

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                FileLibraryDetailsData entity = new FileLibraryDetailsData();
                rowToData(dr, entity);
                list.Add(entity);
            }
        }

        protected void rowToData(DataRow dr, FileLibraryDetailsData entity)
        {
            entity.DetailsPageId = Convert.ToInt32(dr["PageId"]);
            entity.Identifier = Convert.ToInt32(dr["Identifier"]);
            entity.Lang = new CmsLanguage(dr["LangCode"].ToString());
            entity.FileName = dr["FileName"].ToString();
            entity.CategoryId = Convert.ToInt32(dr["CategoryId"]);
            entity.Author = dr["Author"].ToString();
            entity.Description = dr["Description"].ToString();            
            entity.CreatedBy = dr["CreatedBy"].ToString();
            entity.EventPageId = Convert.ToInt32(dr["EventPageId"]);
        }
        #endregion

        #region category
        protected static string CATEGORY_TABLE = "filelibrarycategory";

        /// <summary>
        /// Insert into `FileLibraryCategory`
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool insertCategoryData(FileLibraryCategoryData entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(CATEGORY_TABLE);
            sql.Append(" (CategoryId,LangCode,EventRequired,CategoryName,SortOrdinal) VALUES (");
            sql.Append(entity.CategoryId.ToString() + ",'");
            sql.Append(dbEncode(entity.Lang.shortCode) + "',");
            sql.Append(entity.EventRequiredAsInt + ",'");
            sql.Append(dbEncode(entity.CategoryName) + "',");
            sql.Append(entity.SortOrdinal.ToString() + ");");

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        /// <summary>
        /// Delete from `FileLibraryCategory`
        /// (can't just set DELETED timestamp because foreign key still exists under this case)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool deleteCategoryData(FileLibraryCategoryData entity)
        {
            StringBuilder sql = new StringBuilder("DELETE FROM ");
            sql.Append(CATEGORY_TABLE);
            sql.Append(" WHERE CategoryId=" + entity.CategoryId.ToString() + ";");

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        /// <summary>
        /// Update `FileLibraryCategory`
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool updateCategoryData(FileLibraryCategoryData entity)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(CATEGORY_TABLE);
            sql.Append(" SET EventRequired=" + entity.EventRequiredAsInt + ",");
            sql.Append(" CategoryName='" + dbEncode(entity.CategoryName) + "',");
            sql.Append(" SortOrdinal=" + entity.SortOrdinal.ToString());
            sql.Append(" WHERE CategoryId=" + entity.CategoryId.ToString());
            sql.Append(" AND LangCode='" + dbEncode(entity.Lang.shortCode) + "';");

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        /// <summary>
        /// Select all records from `FileLibraryCategory`
        /// </summary>
        /// <returns></returns>
        public List<FileLibraryCategoryData> fetchCategoryList()
        {
            StringBuilder sql = new StringBuilder("SELECT CategoryId,LangCode,EventRequired,CategoryName,SortOrdinal FROM ");
            sql.Append(CATEGORY_TABLE);
            sql.Append(" ORDER BY SortOrdinal, CategoryId, LangCode;");

            DataSet ds = this.RunSelectQuery(sql.ToString());

            List<FileLibraryCategoryData> list = new List<FileLibraryCategoryData>();
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    FileLibraryCategoryData entity = new FileLibraryCategoryData();
                    rowToData(dr, entity);
                    list.Add(entity);
                }
            }
            return list;
        }

        /// <summary>
        /// Select all records from `FileLibraryCategory` according to language code
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public List<FileLibraryCategoryData> fetchCategoryList(CmsLanguage lang)
        {
            StringBuilder sql = new StringBuilder("SELECT CategoryId,LangCode,EventRequired,CategoryName,SortOrdinal FROM ");
            sql.Append(CATEGORY_TABLE);
            sql.Append(" WHERE LangCode='" + dbEncode(lang.shortCode) + "'");
            sql.Append(" ORDER BY SortOrdinal, CategoryId, LangCode;");

            DataSet ds = this.RunSelectQuery(sql.ToString());

            List<FileLibraryCategoryData> list = new List<FileLibraryCategoryData>();
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    FileLibraryCategoryData entity = new FileLibraryCategoryData();
                    rowToData(dr, entity);
                    list.Add(entity);
                }
            }
            return list;
        }

        /// <summary>
        /// Select the MAX() of category id of existing record, and then add 1.
        /// </summary>
        /// <returns></returns>
        public int fetchNextCategoryId()
        {
            StringBuilder sql = new StringBuilder("SELECT MAX(CategoryId) AS maxCategoryId FROM ");
            sql.Append(CATEGORY_TABLE);
            sql.Append(";");

            int returnVal = 1;
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds) == false)
                return returnVal;

            DataRow dr = ds.Tables[0].Rows[0];
            object obj = dr["maxCategoryId"];
            try
            {
                return Convert.ToInt32(obj) + 1;
            }
            catch
            {
                return returnVal;
            }
        }

        /// <summary>
        /// Put datarow raw values to entity object
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="entity"></param>
        protected void rowToData(DataRow dr, FileLibraryCategoryData entity)
        {
            entity.CategoryId = Convert.ToInt32(dr["CategoryId"]);
            entity.Lang = new CmsLanguage(dr["LangCode"].ToString());
            entity.EventRequired = Convert.ToBoolean(dr["EventRequired"]);
            entity.CategoryName = dr["CategoryName"].ToString();
            entity.SortOrdinal = Convert.ToInt32(dr["SortOrdinal"]);
        }
        #endregion
    }
}
