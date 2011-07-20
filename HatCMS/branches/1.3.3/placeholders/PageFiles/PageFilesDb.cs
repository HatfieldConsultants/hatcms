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
    public class PageFilesDb : PlaceholderDb
    {

        public PageFilesPlaceholderData getPageFilesData(CmsPage page, int identifier,CmsLanguage langToRenderFor, bool createNewIfDoesNotExist)
        {
            if (page.ID < 0 || identifier < 0)
                return new PageFilesPlaceholderData();

            string sql = "select tabularDisplayLinkMode, sortDirection, sortColumn, numFilesToShowPerPage, accessLevelToAddFiles, accessLevelToEditFiles ";
            sql += " from pagefiles c ";
            sql += " where c.pageid = " + page.ID.ToString() + " and c.identifier = " + identifier.ToString() + " and langShortCode like '" + dbEncode(langToRenderFor.shortCode) + "' and deleted is null;";
            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                PageFilesPlaceholderData data = new PageFilesPlaceholderData();
                data.tabularDisplayLinkMode = (PageFilesPlaceholderData.TabularDisplayLinkMode)Enum.Parse(typeof(PageFilesPlaceholderData.TabularDisplayLinkMode), dr["tabularDisplayLinkMode"].ToString());
                data.sortDirection = (PageFilesPlaceholderData.SortDirection)Enum.Parse(typeof(PageFilesPlaceholderData.SortDirection), dr["sortDirection"].ToString());
                data.sortColumn = (PageFilesPlaceholderData.SortColumn)Enum.Parse(typeof(PageFilesPlaceholderData.SortColumn), dr["sortColumn"].ToString());
                data.accessLevelToAddFiles = (BaseCmsPlaceholder.AccessLevel)Enum.Parse(typeof(BaseCmsPlaceholder.AccessLevel), dr["accessLevelToAddFiles"].ToString());
                data.accessLevelToEditFiles = (BaseCmsPlaceholder.AccessLevel)Enum.Parse(typeof(BaseCmsPlaceholder.AccessLevel), dr["accessLevelToEditFiles"].ToString());
                data.numFilesToShowPerPage = Convert.ToInt32(dr["numFilesToShowPerPage"]);
                return data;
            }
            else
            {
                if (createNewIfDoesNotExist)
                {
                    return createNewPageFilesData(page, identifier, langToRenderFor);
                }
                else
                {
                    throw new Exception("getPageFilesData database error: placeholder does not exist");
                }
            }
            return new PageFilesPlaceholderData();
        } // getFlashObject

        public PageFilesPlaceholderData createNewPageFilesData(CmsPage page, int identifier, CmsLanguage lang)
        {
            PageFilesPlaceholderData data = new PageFilesPlaceholderData();

            string sql = "insert into pagefiles (pageid, identifier, langShortCode, tabularDisplayLinkMode, sortDirection, sortColumn, numFilesToShowPerPage, accessLevelToAddFiles, accessLevelToEditFiles) values (";
            sql = sql + page.ID.ToString() + "," + identifier.ToString() + ", '" + dbEncode(lang.shortCode) + "', ";
            sql = sql + "'" + Enum.GetName(typeof(PageFilesPlaceholderData.TabularDisplayLinkMode), data.tabularDisplayLinkMode) + "', ";
            sql = sql + "'" + Enum.GetName(typeof(PageFilesPlaceholderData.SortDirection), data.sortDirection) + "', ";
            sql = sql + "'" + Enum.GetName(typeof(PageFilesPlaceholderData.SortColumn), data.sortColumn) + "', ";
            sql = sql + data.numFilesToShowPerPage.ToString() + ", ";
            sql = sql + "'" + Enum.GetName(typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToAddFiles) + "', ";
            sql = sql + "'" + Enum.GetName(typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToEditFiles) + "' ";
            sql += "); ";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {
                page.setLastUpdatedDateTimeToNow();                                
            }

            return data;

        } // createNewPageFilesData

        public bool saveUpdatedPageFilesData(CmsPage page, int identifier, CmsLanguage lang, PageFilesPlaceholderData data)
        {
            string sql = "update pagefiles set ";
            sql += " tabularDisplayLinkMode = '" + Enum.GetName(typeof(PageFilesPlaceholderData.TabularDisplayLinkMode), data.tabularDisplayLinkMode) + "', ";
            sql += " sortDirection = '" + Enum.GetName(typeof(PageFilesPlaceholderData.SortDirection), data.sortDirection) + "', ";
            sql += " sortColumn = '" + Enum.GetName(typeof(PageFilesPlaceholderData.SortColumn), data.sortColumn) + "', ";
            sql += " numFilesToShowPerPage = " + data.numFilesToShowPerPage.ToString() + ", ";
            sql += " accessLevelToAddFiles = '" + Enum.GetName(typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToAddFiles) + "', ";
            sql += " accessLevelToEditFiles = '" + Enum.GetName(typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToEditFiles) + "' ";
            sql += " where pageid = " + page.ID.ToString();
            sql += " AND langShortCode like '" + dbEncode(lang.shortCode) + "' ";
            sql += " AND identifier = " + identifier.ToString() + "; ";

            int numAffected = this.RunUpdateQuery(sql);
            if (numAffected > 0)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;

        } // saveUpdatedPageFilesData


        private PageFilesItemData getItemDataFromDataRow(DataRow dr)
        {
            PageFilesItemData item = new PageFilesItemData();
            item.Id = Convert.ToInt32(dr["PageFileItemId"]);
            item.Filename = dr["Filename"].ToString();
            item.Title = dr["Title"].ToString();
            item.Author = dr["Author"].ToString();
            item.Abstract = dr["Abstract"].ToString();
            item.FileSize = Convert.ToInt64(dr["FileSize"]);
            item.lastModified = Convert.ToDateTime(dr["LastModified"]);
            item.CreatorUsername = dr["CreatorUsername"].ToString();

            item.DetailsPageId = Convert.ToInt32(dr["PageId"]);
            item.Identifier = Convert.ToInt32(dr["Identifier"]);
            string langCode = dr["langShortCode"].ToString();
            item.Lang = CmsLanguage.GetFromHaystack(langCode, CmsConfig.Languages);

            return item;

        } // ItemDataFromDataRow


        /// <summary>
        /// deletes the pageFileItemToDelete. Note: the calling pageFileItemToDelete.Filename is modified with this function.
        /// </summary>
        /// <param name="pageFileItemToDelete"></param>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <returns>TRUE if successful, false if not</returns>
        public bool deletePageFilesItemData(PageFilesItemData pageFileItemToDelete, CmsPage page, int identifier, CmsLanguage language)
        {
            if (pageFileItemToDelete.Id >= 0)
            {
                string renamedFilename = "Deleted." + DateTime.Now.ToString("yyyyMMdd.HH.mm.ss.")+pageFileItemToDelete.Filename;

                string currentFilenameOnDisk = pageFileItemToDelete.getFilenameOnDisk(page, identifier, language);
                pageFileItemToDelete.Filename = renamedFilename;
                string renamedFilenameOnDisk = pageFileItemToDelete.getFilenameOnDisk(page, identifier, language);

                if (System.IO.File.Exists(currentFilenameOnDisk))
                {
                    try
                    {
                        System.IO.File.Move(currentFilenameOnDisk, renamedFilenameOnDisk);
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                        return false;
                    }
                }

                string sql = "update pagefileitem set Deleted = NOW(), Filename = '" + dbEncode(renamedFilename) + "' where PageFileItemId = " + pageFileItemToDelete.Id.ToString();

                int numAffected = this.RunUpdateQuery(sql);
                if (numAffected > 0)
                {
                    
                    return page.setLastUpdatedDateTimeToNow();
                }
   
            } // if

            return false;
        }

        /// <summary>
        /// returns PageFilesItemData.Id = -1 if not found
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public PageFilesItemData getPageFilesItemData(int fileId)
        {

            if (fileId < 1)
                return new PageFilesItemData();

            string sql = "select PageFileItemId, PageId, Identifier, langShortCode, Filename, Title, Author, Abstract, FileSize, LastModified, CreatorUsername from pagefileitem ";
            sql += " where PageFileItemId = " + fileId.ToString() + " and deleted is null ";

            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];

                return getItemDataFromDataRow(dr);
            } // if has data
            return new PageFilesItemData();
        }

        public PageFilesItemData[] getPageFilesItemDatas(CmsPage[] pages, CmsLanguage pageLanguage)
        {
            if (pages.Length == 0)
                return new PageFilesItemData[0];

            List<string> pageIds = new List<string>();
            foreach (CmsPage p in pages)
            {
                pageIds.Add(p.ID.ToString());
            }

            string sql = "select PageFileItemId, PageId, Identifier, langShortCode, Filename, Title, Abstract, Author, FileSize, LastModified, CreatorUsername from pagefileitem ";
            sql += " where PageId in (" + string.Join(",", pageIds.ToArray()) + ") and langShortCode like '" + pageLanguage.shortCode + "' and deleted is null ";

            // -- run the query
            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasRows(ds))
            {
                List<PageFilesItemData> ret = new List<PageFilesItemData>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ret.Add(getItemDataFromDataRow(dr));
                } // foreach
                return ret.ToArray();

            } // if has data
            return new PageFilesItemData[0];

        }

        public PageFilesItemData[] getPageFilesItemDatas(CmsPage page, int identifier, CmsLanguage pageLanguage, PageFilesPlaceholderData data)
        {
            string sql = "select PageFileItemId, PageId, Identifier, langShortCode, Filename, Title, Abstract, Author, FileSize, LastModified, CreatorUsername from pagefileitem ";
            sql += " where PageId = " + page.ID.ToString() + " and Identifier=" + identifier.ToString() + " and langShortCode like '"+pageLanguage.shortCode+"' and deleted is null ";

            if (data.sortColumn != PageFilesPlaceholderData.SortColumn.NoSorting)
            {
                switch (data.sortColumn)
                {
                    case PageFilesPlaceholderData.SortColumn.Filename:
                        sql += " order by Filename ";
                        break;
                    case PageFilesPlaceholderData.SortColumn.Title:
                        sql += " order by Title ";
                        break;
                    case PageFilesPlaceholderData.SortColumn.FileSize:
                        sql += " order by FileSize ";
                        break;
                    case PageFilesPlaceholderData.SortColumn.DateLastModified:
                        sql += " order by LastModified ";
                        break;
                    default:
                        throw new ArgumentException("Invalid PageFilesData.SortColumn specified");
                        break;
                } // switch
                switch (data.sortDirection)
                {
                    case PageFilesPlaceholderData.SortDirection.Ascending:
                        sql += " ASC ";
                        break;
                    case PageFilesPlaceholderData.SortDirection.Descending:
                        sql += " DESC ";
                        break;
                    default:
                        throw new ArgumentException("invalid PageFilesData.SortDirection specified");
                        break;
                } // switch
            } // if sorting is enabled

            // -- run the query
            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasRows(ds))
            {
                List<PageFilesItemData> ret = new List<PageFilesItemData>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ret.Add(getItemDataFromDataRow(dr));
                } // foreach
                return ret.ToArray();
                
            } // if has data
            return new PageFilesItemData[0];
        } // getPageFilesItemDatas

        public bool saveNewPageFilesItemData(PageFilesItemData newItem, CmsPage page, int indentifier, CmsLanguage pageLanguage)
        {
            string sql = "insert into pagefileitem (PageId, Identifier, langShortCode, Filename, Title, Author, Abstract, FileSize, LastModified, CreatorUsername) VALUES (";
            sql += "" + page.ID.ToString() + ", ";
            sql += "" + indentifier.ToString() + ", ";
            sql += "'" + dbEncode(pageLanguage.shortCode) + "', ";
            sql += "'" + dbEncode(newItem.Filename) + "', ";
            sql += "'" + dbEncode(newItem.Title) + "', ";
            sql += "'" + dbEncode(newItem.Author) + "', ";
            sql += "'" + dbEncode(newItem.Abstract) + "', ";
            sql += "" + newItem.FileSize.ToString() + ", ";
            sql += "" + dbEncode(newItem.lastModified) + ", ";
            sql += "'" + dbEncode(newItem.CreatorUsername) + "' ";
            sql += ")";

            int newId = this.RunInsertQuery(sql);
            if (newId >= 0)
            {
                newItem.Id = newId;
                return true;
            }
            return false;
        }

        public bool saveUpdatedPageFilesItemData(PageFilesItemData updatedFileItem)
        {
            if (updatedFileItem.Id < 0)
                return false;

            string sql = "update pagefileitem set ";

            sql += "Filename = '" + dbEncode(updatedFileItem.Filename) + "', ";
            sql += "Title = '" + dbEncode(updatedFileItem.Title) + "', ";
            sql += "Author = '" + dbEncode(updatedFileItem.Author) + "', ";
            sql += "Abstract = '" + dbEncode(updatedFileItem.Abstract) + "', ";
            sql += "FileSize = " + updatedFileItem.FileSize.ToString() + ", ";
            sql += "LastModified = " + dbEncode(updatedFileItem.lastModified) + ", ";
            sql += "CreatorUsername = '" + dbEncode(updatedFileItem.CreatorUsername) + "' ";

            sql += " where PageFileItemId = " + updatedFileItem.Id.ToString() + "; ";

            int NumModified = this.RunUpdateQuery(sql);
            if (NumModified >= 0)
            {
                return true;
            }
            return false;
        } //  saveUpdatedPageFilesItemData


    } // class PageFilesDb
}
