using System;
using System.Text;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Hatfield.Web.Portal;

namespace HatCMS
{
	/// <summary>
	/// The class that handles all database calls for <see cref="CmsPage"/> objects.
	/// </summary>
	public class CmsPageDb: Hatfield.Web.Portal.Data.MySqlDbObject
	{
		private CmsPageCache pageCache;
		
		/// <summary>
		/// provides database functions for the CmsPage object
		/// </summary>
		public CmsPageDb(): base(ConfigurationManager.AppSettings["ConnectionString"])
		{
            pageCache = CmsContext.getPageCache();
            if (!this.pageCache.AllPagesHaveBeenCached)
                loadAllPagesIntoCache();
		}

        private void loadAllPagesIntoCache()
        {
            try
            {
                string sql = "select p.pageId, l.langCode, l.name, l.title, l.menuTitle, l.searchEngineDescription, p.showInMenu, p.template, p.parentPageId, p.SortOrdinal, p.CreatedDateTime, p.LastUpdatedDateTime, p.LastModifiedBy, p.RevisionNumber ";
                sql += " from pages p left join pagelanginfo l on (p.pageid = l.pageid) where p.Deleted Is Null order by p.sortordinal, p.LastUpdatedDateTime ";
                DataSet ds = this.RunSelectQuery(sql);

                if (hasRows(ds))
                {
                    CmsPage[] pages = pagesFromRows(ds.Tables[0].Rows);
                    if (pages.Length == 0)
                        throw new CmsNoPagesFoundException("No pages could be loaded from the database - there could be a database connection issue!");

                    pageCache.AddAllPagesToCache(pages);
                    // -- cache the home page id
                    foreach (CmsPage p in pages)
                    {
                        if (p.Name == "" && p.ParentID == 0)
                            pageCache.SetHomePageId(p.ID);
                    }

                    if (pageCache.GetHomePageId() < 0)
                    {
                        throw new CmsNoPagesFoundException("The home page could not be loaded from the database");
                    }
                } // if has data
            }
            catch (Exception ex)
            {
                throw new CmsNoPagesFoundException("No pages could be loaded from the database - there could be a database connection issue!");
            }
            

        }       

        private CmsPage[] pagesFromRows(DataRowCollection rows)
        {
            Dictionary<string, CmsPage> pages = new Dictionary<string, CmsPage>();
            foreach (DataRow dr in rows)
            {
                int id = Convert.ToInt32(dr["pageId"]);
                
                string key = id.ToString();
                CmsPage pageToModify;
                if (pages.ContainsKey(key))
                {
                    pageToModify = pages[key];
                }
                else
                {
                    pageToModify = new CmsPage();
                    pageToModify.ID = id;
                    // p.showInMenu, p.template, p.parentPageId, p.SortOrdinal, p.CreatedDateTime, p.LastUpdatedDateTime, p.LastModifiedBy, p.RevisionNumber
                    pageToModify.ShowInMenu = Convert.ToBoolean(dr["showInMenu"]);
                    pageToModify.TemplateName = dr["template"].ToString();
                    pageToModify.ParentID = Convert.ToInt32(dr["parentPageId"]);
                    pageToModify.SortOrdinal = Convert.ToInt32(dr["SortOrdinal"]);
                    pageToModify.CreatedDateTime = Convert.ToDateTime(dr["CreatedDateTime"]);
                    pageToModify.LastUpdatedDateTime = Convert.ToDateTime(dr["LastUpdatedDateTime"]);
                    pageToModify.LastModifiedBy = dr["LastModifiedBy"].ToString();
                    pageToModify.RevisionNumber = Convert.ToInt32(dr["RevisionNumber"]);
                }

                // l.langCode, l.name, l.title, l.menuTitle, l.searchEngineDescription
                CmsPageLanguageInfo langInfo = new CmsPageLanguageInfo();
                langInfo.languageShortCode = dr["langCode"].ToString();
                langInfo.name = getPossiblyNullValue(dr, "name", "");
                langInfo.title = dr["title"].ToString();
                langInfo.menuTitle = dr["menuTitle"].ToString();
                langInfo.searchEngineDescription = dr["searchEngineDescription"].ToString();
                
                List<CmsPageLanguageInfo> infos = new List<CmsPageLanguageInfo>(pageToModify.LanguageInfo);
                infos.Add(langInfo);
                pageToModify.LanguageInfo = infos.ToArray();
                
                if (!pages.ContainsKey(key))
                    pages.Add(key, pageToModify);
            }
            
            List<CmsPage> ret = new List<CmsPage>(pages.Values);
            return ensurePageLanguageInfoConsistency(ret.ToArray());
        }

        private CmsPage[] ensurePageLanguageInfoConsistency(CmsPage[] pages)
        {
            List<CmsPage> ret = new List<CmsPage>(pages);
            foreach (CmsPage page in pages)
            {                
                foreach (CmsLanguage l in CmsConfig.Languages)
                {
                    CmsPageLanguageInfo lang = CmsPageLanguageInfo.GetFromHaystack(l, page.LanguageInfo);
                    if (lang.languageShortCode == "")
                    {
                        // -- useful debugging information
                        string sql = "INSERT INTO pagelanginfo (pageId, langCode, name, title, menuTitle, searchEngineDescription) select pageId, '" + lang.languageShortCode.ToLower() + "', name,title, menuTitle, searchEngineDescription from pages;";
#if !DEBUG 
                        sql = "";
#endif
                        throw new Exception("Page " + page.ID + " needs to have language " + lang.languageShortCode + " information added: " + sql); ;
                    }
                } // foreach system language
            } // foreach page
            return ret.ToArray();
        }        

		/// <summary>
		/// gets a page. Returns a newly created page (with ID &lt; 0) if the pageId was not found.
		/// </summary>
		/// <param name="pageId"></param>
		/// <returns></returns>
		public CmsPage getPage(int pageId)
		{
			if (pageId >= 0)
			{
				CmsPage cached = pageCache.Get(pageId);
				if (cached != null)
					return cached;
			}
			
			// -- page not found
			CmsPage blankPage = new CmsPage();
			// blankPage.ID = -1;
            pageCache.Add(pageId,blankPage);
			return blankPage;
			
		} // getPage

		/// <summary>
		/// gets the child pages for a parent page
		/// </summary>
		/// <param name="parentPageId"></param>
		/// <returns></returns>
        public CmsPage[] getChildPages(int parentPageId)
        {
            
            if (parentPageId >= 0)
            {
                List<CmsPage> subPages = new List<CmsPage>();
                CmsPage[] allPages = pageCache.getAllPagesFromCache();
                foreach (CmsPage p in allPages)
                {
                    if (p.ParentID == parentPageId)
                        subPages.Add(p);
                }                
                return CmsPage.SortPagesBySortOrdinal( subPages.ToArray());
            }
            CmsPage[] ret2 = new CmsPage[0];
            return ret2;

        } // getChildPages

		/// <summary>
		/// returns the PageId for the Home Page (the home page has it's name set to NULL, and it's parentId set to 0).
		/// If the home page is not found, or multiple pages are found, returns -1.
		/// </summary>
		/// <returns></returns>
		public int getHomePageId()
		{
			// -- try getting the ID from the cache
			int cachedId = pageCache.GetHomePageId();
			if (cachedId != Int32.MinValue)
				return cachedId;

			return -1;
		} // getHomePageId

		/// <summary>
		/// gets a child page with a given name. If not found, returns an empty CmsPage object.
		/// </summary>
		/// <param name="parentPageId"></param>
		/// <param name="childName"></param>
		/// <returns></returns>
		public CmsPage getPage(int parentPageId, string childName, CmsLanguage childNameLanguage)
		{            
            CmsPage cachedParentPage = pageCache.Get(parentPageId);
            if (cachedParentPage == null)
            {
                // -- force the parent page to be in the cache
                cachedParentPage = getPage(parentPageId);
            }

            CmsPage cachedChild = pageCache.Get(parentPageId, childName, childNameLanguage);
            if (cachedChild != null)
                return cachedChild;
            
			return new CmsPage();
		}


        /// <summary>
        /// gets a CmsPage object that has the specified path. 
        /// The pagePath should NEVER includes the language shortCode.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public CmsPage getPage(string pagePath, CmsLanguage pagePathLanguage)
        {
            return getPage(pagePath, Int32.MinValue, pagePathLanguage);
        }

        /// <summary>
        /// gets a CmsPage object that has the specified path. 
        /// The pagePath should NEVER includes the language shortCode.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public CmsPage getPage(string pagePath)
        {
            return getPage(pagePath, Int32.MinValue, CmsContext.currentLanguage);
        }

        /// <summary>
        /// gets a CmsPage object that has the specified path. 
        /// The pagePath should NEVER includes the language shortCode.
        /// If the path is not found, throws a CmsPageNotFoundException error.
        /// If the revion number is not found, a RevisionNotFoundException error is thrown.
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="pageRevisionNumber"></param>
        /// <returns></returns>
        public CmsPage getPage(string pagePath, int pageRevisionNumber)
        {
            return getPage(pagePath, pageRevisionNumber, CmsContext.currentLanguage);
        }

		/// <summary>
		/// gets a CmsPage object that has the specified path. 
        /// The pagePath should NEVER includes the language shortCode.
        /// If the path is not found, throws a CmsPageNotFoundException error.
        /// If the revion number is not found, a RevisionNotFoundException error is thrown.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public CmsPage getPage(string pagePath, int pageRevisionNumber, CmsLanguage pageLanguage)
		{						
            // -- put spaces back in the path
            string replaceSpaceWith = "+";
            if (replaceSpaceWith != "")
			{
                pagePath = pagePath.Replace(replaceSpaceWith, " ");
			}
			
			
			// -- take off the first "/"
            if (pagePath.StartsWith("/"))
                pagePath = pagePath.Substring(1, pagePath.Length - 1);


            pagePath = pagePath.ToLower();

            // -- check if it's the home page that the path is for
            bool isHomePagePath = ((pagePath == "") || (String.Compare(pagePath, "default", true) == 0));            

            if (isHomePagePath)
			{
                CmsPage homePage = this.getPage(getHomePageId());
                if (pageRevisionNumber >= 0)
                {                    
                    CmsPageRevisionData revData = homePage.getRevisionData(pageRevisionNumber);
                    if (revData != null)
                    {
                        homePage.RevisionNumber = revData.RevisionNumber;
                    }
                    else
                        throw new RevisionNotFoundException();
                }
                return homePage;
			}

            string[] path_parts = pagePath.Split(new char[] { '/' });
            int parentId = getHomePageId();
			if (parentId < 0 )
				throw new Exception("no home page found!");

			CmsPage currentPage = new CmsPage();
            
                        
            // -- go through all the parts of the page path (from first to last), making sure that all pages exist.
            for (int i = 0; i < path_parts.Length; i++)
			{
                currentPage = getPage(parentId, path_parts[i], pageLanguage);
				if (currentPage.ID < 0)
				{
                    throw new CmsPageNotFoundException();
				}
				else
				{
					parentId = currentPage.ID;	
				}
			} // for

            if (currentPage.ID < 0)
            {
                throw new CmsPageNotFoundException();
            }

            if (pageRevisionNumber >= 0)
            {
                CmsPageRevisionData revData = currentPage.getRevisionData(pageRevisionNumber);
                if (revData != null)
                {
                    currentPage.RevisionNumber = revData.RevisionNumber;
                }
                else
                    throw new RevisionNotFoundException();
            }
                
			return currentPage;

		} // getPage

		/// <summary>
		/// inserts a new page in the database
		/// </summary>
		/// <param name="newPage"></param>
		/// <returns>true if inserted successfully, false if not.</returns>
		public bool createNewPage(CmsPage newPage)
		{
            if (newPage.LanguageInfo.Length != CmsConfig.Languages.Length)
                throw new Exception("Error: when creating a new page, the page must have at least one LanguageInfo specified!");

            string parentId = newPage.ParentID.ToString();
            if (newPage.ParentID < 0)
                parentId = "NULL";

            string sql = "INSERT into pages ( showInMenu, template, parentPageId, sortOrdinal, LastUpdatedDateTime, CreatedDateTime, LastModifiedBy, RevisionNumber) VALUES ";
            sql = sql + "(";             
            sql = sql + Convert.ToInt32(newPage.ShowInMenu) + ", ";
            sql = sql + "'" + this.dbEncode(newPage.TemplateName) + "', ";
            sql = sql + parentId + ", ";
            sql = sql + newPage.SortOrdinal.ToString() + ", ";
            sql = sql + "NOW(), NOW(), ";
            sql = sql + "'"+ dbEncode(newPage.LastModifiedBy)+"', ";
            sql = sql + newPage.RevisionNumber+" ";
            sql = sql + "); ";

            int newId = this.RunInsertQuery(sql);
			if (newId > -1)
			{
                newPage.ID = newId;

                StringBuilder langSql = new StringBuilder();

                langSql.Append("INSERT INTO pagelanginfo ");
                langSql.Append("(pageId, langCode, name, title, menuTitle, searchEngineDescription)");
                langSql.Append(" VALUES ");
                foreach (CmsPageLanguageInfo item in newPage.LanguageInfo)
                {
                    langSql.Append(" ( ");
                    langSql.Append(" " + newId + ", ");
                    langSql.Append(" '" + dbEncode(item.languageShortCode) + "', ");
                    if (item.name == "")
                        langSql.Append("NULL, ");
                    else
                        langSql.Append("'" + dbEncode(item.name) + "'" + ", ");

                    langSql.Append("'" + dbEncode(item.title) + "'" + ", ");
                    langSql.Append("'" + dbEncode(item.menuTitle) + "'" + ", ");
                    langSql.Append("'" + dbEncode(item.searchEngineDescription) + "'" + " ");
                    langSql.Append(" ),");
                } // foreach

                // remove trailing comma
                string s = langSql.ToString().Substring(0, langSql.ToString().Length - 1);
                int numInserted = this.RunUpdateQuery(s); // do not use RunInsertQuery                
                if (numInserted != newPage.LanguageInfo.Length)
                    return false;

				newPage.LastUpdatedDateTime = DateTime.Now;
				newPage.CreatedDateTime = DateTime.Now;
                pageCache.Add(newPage.ID, newPage);
				return true;
			}
			return false;
		}

        

		/// <summary>
		/// sets a page as being deleted in the database.        
		/// </summary>
        /// <param name="pageToDelete">The page to delete</param>
		/// <returns>true if updated successfully, false if not.</returns>
		public bool deletePage(CmsPage pageToDelete)
		{
            if (pageToDelete.ID < 0)
				return false; // nothing to delete            

            // Mark this page as being deleted.
            string sql = "UPDATE pages set DELETED = now() WHERE pageid = " + pageToDelete.ID.ToString() + " ; ";			
			
			int numAffected = this.RunUpdateQuery(sql);
			if (numAffected > 0)
			{
                pageCache.Remove(pageToDelete);
                return true;
			}
			return false;

		} // deletePage

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns>the new revision number for the page, or -1 on failure</returns>
        public int createNewPageRevision(CmsPage page)
        {
            if (page.ID < 0)
                return -1;

            int[] allRevNums = page.getAllRevisionNumbers();
            int newRevisionNumber = page.RevisionNumber + 1;
            if (allRevNums.Length > 0)
                newRevisionNumber = allRevNums[allRevNums.Length - 1] + 1;            
            else if (page.RevisionNumber < 1)
                newRevisionNumber = 1;
            
            string lastModifiedBy = "";
            if (CmsContext.currentUserIsLoggedIn)
                lastModifiedBy = CmsContext.currentWebPortalUser.UserName;


            // insert into PageRevisionData
            string revSql = "INSERT into pagerevisiondata (PageId, RevisionNumber, ModificationDate, ModifiedBy) VALUES ( ";
            revSql += page.ID.ToString()+", ";
            revSql += newRevisionNumber.ToString()+", ";
            revSql += " NOW(), ";
            revSql += "'"+dbEncode(lastModifiedBy)+"'";
            revSql += "); ";

            // use RunUpdateQuery because there's no AutoInc column for PageRevisionData
            int numAffected = RunUpdateQuery(revSql);
            if (numAffected <= 0)
                return -1;

            // -- update the pages table to have the most up-to-date data
            string pageUpdateSql = "update pages set RevisionNumber = "+newRevisionNumber.ToString()+" where pageid = "+page.ID.ToString()+" ;";
            numAffected = this.RunUpdateQuery(pageUpdateSql);
            if (numAffected > 0)
            {
                // important: do not update page.RevisionNumber!!
                pageCache.Update(page);
                return newRevisionNumber;
            }
            return -1;
        }

        public CmsPageRevisionData[] getAllRevisionData(CmsPage page)
        {
            string sql = "select * from pagerevisiondata where PageId = " + page.ID.ToString() + " ORDER BY ModificationDate ASC; ";
            DataSet ds = this.RunSelectQuery(sql);
            List<CmsPageRevisionData> ret = new List<CmsPageRevisionData>();
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    CmsPageRevisionData rev = new CmsPageRevisionData();
                    rev.PageId = page.ID;
                    rev.RevisionNumber = Convert.ToInt32(dr["RevisionNumber"]);
                    rev.RevisionSavedAt = Convert.ToDateTime(dr["ModificationDate"]);
                    rev.RevisionSavedByUsername = dr["ModifiedBy"].ToString();
                    ret.Add(rev);
                } // foreach
            }
            return ret.ToArray();
        }

		/// <summary>
		/// sets the lastUpdated timestamp and the LastModifiedBy name for an existing page.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="dt"></param>
		/// <returns>true if updated successfully, false if not.</returns>
		public bool setLastUpdatedTime(CmsPage page, DateTime dt)
		{
			if (page.ID < 0)
				return false; // nothing to delete

            string lastModifiedBy = "";
            if (CmsContext.currentUserIsLoggedIn)
                lastModifiedBy = CmsContext.currentWebPortalUser.UserName;

            string sql = "UPDATE pages set LastUpdatedDateTime = " + dbEncode(dt) + " ";
            sql += ", LastModifiedBy = '" + dbEncode(lastModifiedBy) + "' ";
            sql += " WHERE pageid = " + page.ID.ToString() + " ; ";			
			
			int numAffected = this.RunUpdateQuery(sql);
			if (numAffected > 0)
			{
                page.LastUpdatedDateTime = dt;
                page.LastModifiedBy = lastModifiedBy;
                pageCache.Update(page);
                return true;
			}
			return false;			
		}

		/// <summary>
		/// sets the sortOrdinal for an existing page.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="newOrdinalValue"></param>
		/// <returns>true if updated successfully, false if not.</returns>
		public bool updateSortOrdinal(CmsPage page, int newOrdinalValue)
		{
			if (page.ID == -1)
				return false; // nothing to delete

            string sql = "UPDATE pages set SortOrdinal = " + newOrdinalValue.ToString() + " WHERE pageid = " + page.ID.ToString() + " ; ";

			// sql = sql + " SELECT LAST_INSERT_ID() as newId;";
			
			int numAffected = this.RunUpdateQuery(sql);
			if (numAffected > 0 && page.setLastUpdatedDateTimeToNow())
			{
				// int newId = Convert.ToInt32(ds.Tables[0].Rows[0]["newId"]);
                page.SortOrdinal = newOrdinalValue;
                pageCache.Update(page);
				return true;
			}
			return false;			
		}

		/// <summary>
		/// sets the title for an existing page
		/// </summary>
		/// <param name="page"></param>
		/// <param name="newTitle"></param>
		/// <returns>true if updated successfully, false if not.</returns>
		public bool updateTitle(CmsPage page, string newTitle, CmsLanguage pageLanguage)
		{
			if (page.ID == -1)
				return false; // nothing to delete

            string sql = "UPDATE pagelanginfo set Title = '" + this.dbEncode(newTitle) + "' WHERE pageid = " + page.ID.ToString() + " AND langCode like '" + pageLanguage.shortCode + "' ; ";

			// sql = sql + " SELECT LAST_INSERT_ID() as newId;";
			
			int numAffected = this.RunUpdateQuery(sql);
			if (numAffected > 0 && page.setLastUpdatedDateTimeToNow())
			{
				// -- update the tracking object
                CmsPageLanguageInfo.GetFromHaystack(pageLanguage, page.LanguageInfo).title = newTitle;
                
                pageCache.Update(page);
				return true;
			}
			return false;			
		}


		/// <summary>
		/// sets the menuTitle for an existing page
		/// </summary>
		/// <param name="page"></param>
		/// <param name="newTitle"></param>
		/// <returns>true if updated successfully, false if not.</returns>
        public bool updateMenuTitle(CmsPage page, string newMenuTitle, CmsLanguage pageLanguage)
		{
			if (page.ID == -1)
				return false; // nothing to update

            string sql = "UPDATE pagelanginfo set menuTitle = '" + this.dbEncode(newMenuTitle) + "' WHERE pageid = " + page.ID.ToString() + " AND langCode like '" + pageLanguage.shortCode + "' ; ";

			// sql = sql + " SELECT LAST_INSERT_ID() as newId;";
			
			int numAffected = this.RunUpdateQuery(sql);
			if (numAffected > 0 && page.setLastUpdatedDateTimeToNow())
			{
                // page.MenuTitle = newMenuTitle;
                CmsPageLanguageInfo.GetFromHaystack(pageLanguage, page.LanguageInfo).menuTitle = newMenuTitle;
                pageCache.Update(page);
				return true;
			}
			return false;			
		}

        /// <summary>
        /// sets the searchEngineDescription for an existing page.
        /// ALTER TABLE `pages` ADD COLUMN `SearchEngineDescription` TEXT NOT NULL AFTER `menuTitle`;
		/// </summary>
		/// <param name="page">the existing page to update</param>
        /// <param name="newSearchEngineDescription">the new search engine description</param>
		/// <returns>true if updated successfully, false if not.</returns>
        public bool updateSearchEngineDescription(CmsPage page, string newSearchEngineDescription, CmsLanguage pageLanguage)
		{
			if (page.ID == -1)
				return false; // nothing to update

            string sql = "UPDATE pagelanginfo set SearchEngineDescription = '" + this.dbEncode(newSearchEngineDescription) + "' WHERE pageid = " + page.ID.ToString() + " AND langCode like '" + pageLanguage.shortCode + "' ; ";
			
			int numAffected = this.RunUpdateQuery(sql);
			if (numAffected > 0 && page.setLastUpdatedDateTimeToNow())
			{
                // page.SearchEngineDescription = newSearchEngineDescription;
                CmsPageLanguageInfo.GetFromHaystack(pageLanguage, page.LanguageInfo).searchEngineDescription = newSearchEngineDescription;
                pageCache.Update(page);
				return true;
			}
			return false;			
		}

        public bool updateParentPage(CmsPage page, CmsPage newParentPage)
		{
			if (page.ID == -1)
				return false; // nothing to update

            string sql = "UPDATE pages set parentPageId = " + newParentPage.ID + " WHERE pageid = " + page.ID.ToString() + " ; ";


			int numAffected = this.RunUpdateQuery(sql);
            if (numAffected > 0 && page.setLastUpdatedDateTimeToNow())
            {
                page.ParentID = newParentPage.ID;
                // -- clear the memory cached pages and sub-pages
                pageCache.Clear();
                
                
                pageCache.Update(page);
                return true;
            }
			return false;			
		}

        public bool updateName(CmsPage page, string newName, CmsLanguage pageLanguage)
		{
			if (page.ID == -1)
				return false; // nothing to update

            string sql = "UPDATE pagelanginfo set name = '" + dbEncode(newName.ToLower()) + "' WHERE pageid = " + page.ID.ToString() + " AND  langCode like '" + pageLanguage.shortCode + "'  ; ";


			int numAffected = this.RunUpdateQuery(sql);
            if (numAffected > 0 && page.setLastUpdatedDateTimeToNow())
            {                                
                // page.Name = newName.ToLower();
                CmsPageLanguageInfo.GetFromHaystack(pageLanguage, page.LanguageInfo).name = newName.ToLower();
                
                // -- clear the memory cached pages and sub-pages
                pageCache.Clear();
                
                
                pageCache.Update(page);
                return true;
            }
			return false;			
		}

        public bool updateTemplateName(CmsPage page, string newTemplateName)
        {
            if (page.ID == -1)
                return false; // nothing to update

            string sql = "UPDATE pages set template = '" + dbEncode(newTemplateName) + "' WHERE pageid = " + page.ID.ToString() + " ; ";

            int numAffected = this.RunUpdateQuery(sql);
            if (numAffected > 0 && page.setLastUpdatedDateTimeToNow())
            {
                page.TemplateName = newTemplateName;
                // -- clear the memory cached pages and sub-pages
                pageCache.Clear();
                                
                pageCache.Update(page);
                return true;
            }
            return false;
        }        

        private void clearAllExpiredPageLocks()
        {
            string sql = "DELETE from pagelocks where LockExpiresAt < NOW();";
            int numDeleted = this.RunUpdateQuery(sql);
            Console.Write(numDeleted.ToString());
        }

        public CmsPageLockData getPageLockData(CmsPage targetPage)
        {
            clearAllExpiredPageLocks();
            
            string sql = "select * from pagelocks where pageid = " + targetPage.ID.ToString() + " ;";
            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                CmsPageLockData ret = new CmsPageLockData();
                DataRow dr = ds.Tables[0].Rows[0];
                ret.PageId = Convert.ToInt32(dr["pageid"]);
                ret.LockedByUsername = dr["LockedByUsername"].ToString();
                ret.LockExpiresAt = Convert.ToDateTime(dr["LockExpiresAt"]);
                return ret;
            }
            return null;
        }

        public void clearCurrentPageLock(CmsPage pageToUnlock)
        {
            // delete both expired locks and the lock for the current page
            string sql = "DELETE from pagelocks where LockExpiresAt < NOW() OR PageId = " + pageToUnlock.ID.ToString() + ";";
            int numDeleted = this.RunUpdateQuery(sql);
            Console.Write(numDeleted.ToString());
        }

        private static object PageEditingLock = new object();


        public static int DefaultLockTimeMinutes = 30;
        
        public CmsPageLockData lockPageForEditing(CmsPage pageToLock)
        {
            return lockPageForEditing(pageToLock, DefaultLockTimeMinutes);
        }

        public CmsPageLockData lockPageForEditing(CmsPage pageToLock, int minutesToLockFor)
        {
            lock (PageEditingLock) // this should probably be done in the database
            {
                if (getPageLockData(pageToLock) != null)
                    return null; // page is already locked
                
                string lastModifiedBy = "";
                if (CmsContext.currentUserIsLoggedIn)
                    lastModifiedBy = CmsContext.currentWebPortalUser.UserName;

                CmsPageLockData ret = new CmsPageLockData();
                ret.PageId = pageToLock.ID;
                ret.LockedByUsername = lastModifiedBy;
                ret.LockExpiresAt = new DateTime(DateTime.Now.Ticks + TimeSpan.FromMinutes(minutesToLockFor).Ticks);

                string sql = "";
                sql += "INSERT into pagelocks (pageid, LockedByUsername, LockExpiresAt) VALUES (";
                sql += ret.PageId.ToString() + ", ";
                sql += "'" + dbEncode(ret.LockedByUsername) + "', ";
                sql += " " + dbEncode(ret.LockExpiresAt) + " ";
                sql += ");" + Environment.NewLine;

                int numUpdated = this.RunUpdateQuery(sql);
                if (numUpdated != 1)
                    return null;
                else
                    return ret;
            } // lock

        } // lockPageForEditing                
	}
}
