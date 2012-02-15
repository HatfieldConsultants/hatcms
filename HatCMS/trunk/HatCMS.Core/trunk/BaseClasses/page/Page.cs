using System;
using System.Data;
using System.Web.UI;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using HatCMS.Placeholders;
using Hatfield.Web.Portal;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using SharpArch.Core.DomainModel;
using NHibernate.Validator.Constraints;
using HatCMS.Core.DataRepository;
using HatCMS.Core.DataPersitentHelper;
using SharpArch.Core.PersistenceSupport;
using NHibernate;
using SharpArch.Data.NHibernate;

namespace HatCMS
{
	/// <summary>
	/// Represents a page in the system. Pages can be rendered and queried.
	/// Pages are organized in a tree structure: each page can have one parent,
	/// and multiple children. 
    /// <para>
    /// If the website has more than one language, each page automatically has URLs and content in that language.
    /// As such, single-language pages are not allowed when more than one language is used.
    /// </para>
	/// </summary>
	public class CmsPage : Entity
    {
        #region page modle attribute

        /// <summary>
        /// show this page in a menu?
        /// </summary>
        /// 
        private bool showinmenu;
        public virtual bool ShowInMenu
        {
            
            get { return showinmenu; }
            set { showinmenu = value; }

        }

        /// <summary>
        /// the name of the template that is used to render this page.
        /// this name must refer directly to a file in the "~/templates/" directory.
        /// files in the "~/templates/" directory must have the ".htm" extension, while
        /// this property must NOT contain the filename extension.
        /// </summary>
        /// 
        private string template;
        public virtual string TemplateName
        {
            get
            {
                return template;
            }
            set
            {
                template = value;
            }
        }

        private int _parentId;
        /// <summary>
        /// the ID for the parent page. Set to -1 if no parent exists, or if not yet initialized.
        /// </summary>
        public virtual int ParentID
        {
            get
            {
                return _parentId;
            }

            set
            {
                if (ParentID != value)
                {
                    urlCached = false;
                    _parentId = value;
                }
            }
        }

        /// <summary>
        /// the timestamp for when this page was created.
        /// </summary>
        private DateTime createdatetime;
        public virtual DateTime CreatedDateTime
        {
            get { return createdatetime; }
            set { createdatetime = value; }
        }

        /// <summary>
        /// the timestamp for when this page was last updated.
        /// update this value using setLastUpdatedDateTimeToNow().
        /// </summary>
        /// 
        private DateTime lastupdateDateTime;
        public virtual DateTime LastUpdatedDateTime
        {
            get { return lastupdateDateTime; }
            set { lastupdateDateTime = value; }
        }


        /// <summary>
        /// the username that last modified the page.
        /// update this value using setLastUpdatedDateTimeToNow().
        /// </summary>
        /// 
        private string lastmodifiedby;
        public virtual string LastModifiedBy
        {
            get { return lastmodifiedby; }
            set { lastmodifiedby = value; }
        }

        /// <summary>
        /// the Date and time to remove anonymous access at.
        /// set to DateTime.MinValue if Anonymous access should never be removed.
        /// </summary>
        // public DateTime removeAnonymousAccessAt;

        /// <summary>
        /// the sort placement of this page.
        /// set this value using setSortOrdinal().
        /// </summary>
        private int sortordinal;
        public virtual int SortOrdinal
        {
            get { return sortordinal; }
            set { sortordinal = value; }
        }



        /// <summary>
        /// The revision number for this page
        /// </summary>
        /// 
        private int revisionnumber;
        public virtual int RevisionNumber
        {
            get { return revisionnumber; }
            set { revisionnumber = value; }
        }

        private DateTime? deleted;
        public virtual DateTime? Deleted
        {
            get { return deleted; }
            set { deleted = value; }
        }

        ///
        ///
        /// <summary>
        /// The CmsPageLanguageInfo instances track all language specific parameters (such as title, menuTitle, name, etc) for this page.
        /// </summary>
        public virtual IList<CmsPageLanguageInfo> LanguageInfo
        {
            get { return languageinfo; }
            set { languageinfo = value; }
        }
        private IList<CmsPageLanguageInfo> languageinfo;

        #endregion page modle attribute

        
        
        /// <summary>
		/// the user-friendly title of this page.
        /// Note: changes based on CmsContext.currentLanguage.
		/// Set this value using setTitle().
		/// </summary>
        public virtual string Title
        {
            get
            {
                return CmsPageLanguageInfo.GetFromHaystack(CmsContext.currentLanguage, LanguageInfo).Title;
            }
        }

        /// <summary>
        /// Gets the user-friendly title of this page for the specified language.
        /// </summary>
        /// <param name="forLanguage"></param>
        /// <returns></returns>
        public virtual string getTitle(CmsLanguage forLanguage)
        {
            return CmsPageLanguageInfo.GetFromHaystack(forLanguage, LanguageInfo).Title;
        }

        /// <summary>
        /// All Titles for the current page. Each language can give the page a different Title.
        /// </summary>
        public virtual string[] Titles
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    ret.Add(CmsPageLanguageInfo.GetFromHaystack(lang, LanguageInfo).Title);
                } // foreach
                return ret.ToArray();
            }
        }
		
		/// <summary>
		/// the user-friendly menu-title of this page.
        /// Note: changes based on CmsContext.currentLanguage.
		/// </summary>
        public virtual string MenuTitle
        {
            get
            {
                return CmsPageLanguageInfo.GetFromHaystack(CmsContext.currentLanguage, LanguageInfo).MenuTitle;
            }
        }

        public virtual string getMenuTitle(CmsLanguage forLanguage)
        {
            return CmsPageLanguageInfo.GetFromHaystack(forLanguage, LanguageInfo).MenuTitle;
        }

        /// <summary>
        /// All MenuTitles for the current page. Each language can give the page a different MenuTitles.
        /// </summary>
        public virtual string[] MenuTitles
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    ret.Add(CmsPageLanguageInfo.GetFromHaystack(lang, LanguageInfo).MenuTitle);
                } // foreach
                return ret.ToArray();
            }
        }

        /// <summary>
        /// the search engine description of this page.
        /// Note: changes based on CmsContext.currentLanguage.
        /// </summary>
        public virtual string SearchEngineDescription
        {
            get
            {
                return CmsPageLanguageInfo.GetFromHaystack(CmsContext.currentLanguage, LanguageInfo).SearchEngineDescription;
            }
        }

        public virtual string getSearchEngineDescription(CmsLanguage forLanguage)
        {
            return CmsPageLanguageInfo.GetFromHaystack(forLanguage, LanguageInfo).SearchEngineDescription;
        }

        /// <summary>
        /// All SearchEngineDescriptions for the current page. Each language can give the page a different SearchEngineDescription.
        /// </summary>
        public virtual string[] SearchEngineDescriptions
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    ret.Add(CmsPageLanguageInfo.GetFromHaystack(lang, LanguageInfo).SearchEngineDescription);
                } // foreach
                return ret.ToArray();
            }
        }


		



        /// <summary>
        /// Characters that must not be found in a page's name (ie a page's filename that is used to construct the page's url).
        /// Refer to "Reserved Characters" in RFC3986: http://www.ietf.org/rfc/rfc3986.txt
        /// </summary>
        public static string[] InvalidPageNameChars = new string[] { "\\", ":", "/", "?", "#", "[", "]", "@", "!", "$", "&", "'", "(", ")", "*", "+", ",", ";", "=", "~", "%", ".", "|" };


        /// <summary>
		/// The filename of this page. Forms this page's part of the URL. For the home page, the Name is String.Empty
        /// Note: changes based on CmsContext.currentLanguage.
		/// </summary>
        public virtual string Name
        {
            get
            {
                return CmsPageLanguageInfo.GetFromHaystack(CmsContext.currentLanguage, LanguageInfo).Name;
            }
        }

        /// <summary>
        /// All names (filenames) for the current page. Each language can give the page a different name.
        /// </summary>
        public virtual string[] Names
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    ret.Add(CmsPageLanguageInfo.GetFromHaystack(lang, LanguageInfo).Name);
                } // foreach
                return ret.ToArray();                
            }
        }

        public virtual string getName(CmsLanguage forLanguage)
        {
            return CmsPageLanguageInfo.GetFromHaystack(forLanguage, LanguageInfo).Name;
        }	

        /// <summary>
        /// gets the most recent PageRevisionData .
        /// returns NULL if not found        
        /// </summary>
        /// <returns></returns>
        public virtual CmsPageRevisionData getCurrentRevisionData()
        {
            CmsPageRevisionData[] allRevs = getAllRevisionData();
            if (allRevs.Length >= 1)
                return allRevs[allRevs.Length - 1];

            return null;
        }
        /// <summary>
        /// gets a Revision data for this page based on the provided revisionNumber. 
        /// returns NULL if not found
        /// </summary>
        /// <param name="revisionNumber"></param>
        /// <returns></returns>
        public virtual CmsPageRevisionData getRevisionData(int revisionNumber)
        {
            CmsPageRevisionData[] allRevs;
            string cacheKey = "allRevs" + this.Id;
            
            if (PerRequestCache.CacheContains(cacheKey))
            {
                allRevs = (CmsPageRevisionData[])PerRequestCache.GetFromCache(cacheKey, new CmsPageRevisionData[0]);
            }
            else
            {
                allRevs = getAllRevisionData();
                PerRequestCache.AddToCache(cacheKey, allRevs);
            }
                                   
            foreach (CmsPageRevisionData rev in allRevs)
            {
                if (rev.RevisionNumber == revisionNumber)
                    return rev;
            } // foreach
            return null;
        }


        private bool _newRevisionHasBeenCreated = false;
        private int _newRevisionNumber = Int32.MinValue;
        /// <summary>
        /// creates a new revision of the page and returns the new revision number to use.
        /// Note: this does not update the RevisionNumber property in memory
        /// It is safe to call this function multiple times (ie multiple placeholders can call this function in the same request)
        /// </summary>
        /// <returns>The new revision number to use</returns>
        public virtual int createNewRevision()
        {
            if (_newRevisionHasBeenCreated)
            {
                return _newRevisionNumber;
            }
            else
            {                
                _newRevisionNumber = (new CmsPageDb()).createNewPageRevision(this);
                if (_newRevisionNumber >= 0)                
                    _newRevisionHasBeenCreated = true;                    
                
                return _newRevisionNumber;
            }
        }

        public virtual bool isPageIsLockedForEditing()
        {
            CmsPageLockData d = getCurrentPageLockData();
            if (d == null)
                return false;
            
            return true;
        }

        /// <summary>
        /// returns NULL if there's no lock
        /// </summary>
        /// <returns></returns>
        public virtual CmsPageLockData getCurrentPageLockData()
        {            
            return (new CmsPageDb()).getPageLockData(this);
        }


        /// <summary>
        /// Locks the page for editing by the current user. When locked, other users can not edit the page. 
        /// Returns NULL if the page could not be locked
        /// </summary>
        /// <returns></returns>
        public virtual CmsPageLockData lockPageForEditing()
        {            
            return (new CmsPageDb()).lockPageForEditing(this);
        }

        /// <summary>
        /// Removes the current page's edit lock.
        /// </summary>
        public virtual void clearCurrentPageLock()
        {            
            (new CmsPageDb()).clearCurrentPageLock(this);
        }

		
		/// <summary>
		/// gets the CmsPage object for the parent page.
		/// If no parent exists, a valid CmsPage is returned, with it's ID set to -1.		
		/// </summary>
        public virtual CmsPage ParentPage
		{
			get
			{				
				return (new CmsPageDb()).FetchPageById(ParentID);
			}
		}

        /// <summary>
        /// All child pages of the current page. Does not take page security or visibility into consideration.
        /// </summary>
        public virtual CmsPage[] AllChildPages
        {
            get
            {
                lock (_childPagesLock)
                {
                    CmsPageDb db = new CmsPageDb();
                    return db.getChildPages(this.Id);
                }
            }
        }

		/// <summary>
		/// The child pages that the current user can access. This method takes security and visibility into consideration.
        /// If visibility and security should not be considered, use <see cref="AllChildPages">. <seealso cref="AllChildPages"/>
		/// </summary>
        public virtual CmsPage[] ChildPages
		{
			get
			{
                lock (_childPagesLock)
                {
                    List<CmsPage> ret = new List<CmsPage>();
                    foreach (CmsPage childPage in AllChildPages)
                    {
                        if (childPage.isVisibleForCurrentUser)
                            ret.Add(childPage);
                    }
                    return ret.ToArray();
                }
			}
		}
        

        /// <summary>
        /// get's the page's path based on the current language.
        /// The path returned NEVER includes the currentLanguage's shortCode.
        /// Note: changes based on CmsContext.currentLanguage.
        /// </summary>
        public virtual string Path
        {
            get { return getPath(CmsContext.currentLanguage); }
        }

        /// <summary>
        /// All Paths for the current page. Each language can give the page a different Path.
        /// </summary>
        public virtual string[] Paths
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    ret.Add(getPath(lang));
                } // foreach
                return ret.ToArray();
            }
        }

		/// <summary>
		/// gets the full file path to this page. 
        /// The path returned NEVER includes the currentLanguage's shortCode.
		/// </summary>
        public virtual string getPath(CmsLanguage forLanguage)
        {

            string cacheKey = "pagePath_" + this.Id + forLanguage.shortCode;

            if (PerRequestCache.CacheContains(cacheKey))
            {
                return (string)PerRequestCache.GetFromCache(cacheKey, "");
            }

            string path = "";
            CmsPage page = this;
            bool first = true;
            //change page.Id to be 1
            while (page.Id != 0)
            {
                if (first)
                {
                    path = page.getName(forLanguage) + path;
                    first = false;
                }
                else
                    path = page.getName(forLanguage) + "/" + path;

                page = page.ParentPage;
            } // while

            if (!path.StartsWith("/"))
                path = "/" + path;

            if (path.StartsWith("//"))
                path = path.Substring(1);

            PerRequestCache.AddToCache(cacheKey, path);
            return path;

        }

		/// <summary>
		/// the level of the page. The home page is 0, under the home page is 1, etc...
		/// </summary>
        public virtual int Level
		{
			get
			{
                string thePath = this.Path;
                if (thePath == "/")
					return 0; // home page
                string[] pathParts = thePath.Split(new char[] { '/' });
				return pathParts.Length-1;
			}
		}

        /// <summary>
        /// All Urls for the current page. Each language gives the page a different url.
        /// </summary>
        public virtual string[] Urls
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    ret.Add(getUrl(lang));
                } // foreach
                return ret.ToArray();
            }
        }

        /// <summary>
        /// cache the URL parameter because redirect pages go to the database all the time!
        /// </summary>
        private bool zoneCached = false;
        private CmsPageSecurityZone cachedZone = null;

        /// <summary>
        /// Derive the Zone by the page ID (note: the result is cached in-memory)
        /// </summary>
        public virtual CmsPageSecurityZone SecurityZone
        {
            get 
            {
                if (zoneCached)
                    return cachedZone;

                cachedZone = new CmsPageSecurityZoneDb().fetchByPage(this);
                zoneCached = true;
                return cachedZone;
            }
        }

        /// <summary>
        /// Check if this page is located at the CmsZone boundary
        /// (i.e. an exact record in `zone` table)
        /// </summary>
        public virtual bool isSecurityZoneBoundary
        {
            get
            {
                CmsPageSecurityZone z = new CmsPageSecurityZoneDb().fetchByPage(this, false);
                return (z != null) ? true : false;
            }
        }

        /// <summary>
        /// Checks if the current user has write (author) access to this page.
        /// </summary>
        public virtual bool currentUserCanWrite
        {
            get
            {                
                if (CmsContext.currentUserIsSuperAdmin)
                    return true;

                return SecurityZone.canWrite(CmsContext.currentWebPortalUser);
            }
        }

        /// <summary>
        /// Checks if the current user has read access to this page.
        /// </summary>
        public virtual bool currentUserCanRead
        {
            get
            {                
                if (CmsContext.currentUserIsSuperAdmin)
                    return true;

                return SecurityZone.canRead(CmsContext.currentWebPortalUser);
            }
        }

        /// <summary>
        /// cache the URL parameter because redirect pages go to the database all the time!
        /// </summary>
        private bool urlCached = false;
        private string cachedUrl = "";
        /// <summary>
		/// gets the navigatable URL of this page. Does not include the hostname or protocol.
		/// </summary>
        public virtual string Url
		{
			get
			{
                if (urlCached)
                    return cachedUrl;

                string url = "";
                // -- for redirect pages, if the current user can NOT author, give the page's absolute page url
                if (! currentUserCanWrite && this.hasPlaceholder("PageRedirect"))
                {
                    PageRedirectDb db = new PageRedirectDb();
                    url = db.getPageRedirectUrl(this, 1, CmsContext.currentLanguage.shortCode, true);

                    url = PageRedirect.resolveRedirectUrl(url);
                    cachedUrl = url;
                }
                else
                {
                    cachedUrl = CmsContext.getUrlByPagePath(Path);
                }
                
                urlCached = true;
                return cachedUrl;

			}
		}

        public virtual string getUrl(CmsLanguage pageLanguage, CmsUrlFormat urlFormat)
        {
            return CmsContext.getUrlByPagePath(this.getPath(pageLanguage), urlFormat, pageLanguage);
        }

        public virtual string getUrl(CmsLanguage pageLanguage)
        {
            return CmsContext.getUrlByPagePath(this.getPath(pageLanguage), pageLanguage);
        }

        public virtual string getUrl(CmsUrlFormat urlFormat)
        {
            return CmsContext.getUrlByPagePath(this.Path, urlFormat);
        }

        public virtual string getUrl(CmsUrlFormat urlFormat, CmsLanguage pageLanguage)
        {
            return CmsContext.getUrlByPagePath(this.getPath(pageLanguage), urlFormat, pageLanguage);
        }

        public virtual string getUrl(NameValueCollection pageParams)
        {
            return CmsContext.getUrlByPagePath(this.Path, pageParams);
        }

        public virtual string getUrl(NameValueCollection pageParams, CmsLanguage pageLanguage)
        {
            return CmsContext.getUrlByPagePath(this.getPath(pageLanguage), pageParams, pageLanguage);
        }

        public virtual string getUrl(NameValueCollection pageParams, CmsUrlFormat urlFormat)
        {
            return CmsContext.getUrlByPagePath(this.Path, pageParams, urlFormat);
        }


        public virtual string getUrl(Dictionary<string, string> pageParams)
        {
            return getUrl(pageParams, CmsUrlFormat.RelativeToRoot);
        }

        public virtual string getUrl(Dictionary<string, string> pageParams, CmsLanguage pageLanguage)
        {
            NameValueCollection paramList = new NameValueCollection();
            foreach (string key in pageParams.Keys)
            {
                paramList.Add(key, pageParams[key]);
            }
            return CmsContext.getUrlByPagePath(this.getPath(pageLanguage), paramList, pageLanguage);            
        }

        public virtual string getUrl(Dictionary<string, string> pageParams, CmsLanguage pageLanguage, CmsUrlFormat urlFormat)
        {
            NameValueCollection paramList = new NameValueCollection();
            foreach (string key in pageParams.Keys)
            {
                paramList.Add(key, pageParams[key]);
            }
            return CmsContext.getUrlByPagePath(this.getPath(pageLanguage), paramList, urlFormat, pageLanguage);
        }

        public virtual string getUrl(Dictionary<string, string> pageParams, CmsUrlFormat urlFormat)
        {
            NameValueCollection paramList = new NameValueCollection();
            foreach (string key in pageParams.Keys)
            {
                paramList.Add(key, pageParams[key]);
            }

            return CmsContext.getUrlByPagePath(this.Path, paramList, urlFormat);
        }

				
        private object _childPagesLock;        

        /// <summary>
        /// the Head section - ie the section of the HTML page between &gt;head&lt and &gt;/head&lt tags.
        /// </summary>
        /// 
        private CmsPageHeadSection headsection;
        public virtual CmsPageHeadSection HeadSection
        {
            get { return headsection; }
            set { headsection = value; }
        }

        /// <summary>
        /// The CMS edit menu that allows pages to be edited, deleted, saved, etc.
        /// </summary>
        /// 
        private CmsPageEditMenu editmenu;
        public virtual CmsPageEditMenu EditMenu
        {
            get { return editmenu; }
            set { editmenu = value; }
        }
        

        /// <summary>
        /// Checks to see if this page is visible for the current user.
        /// This checks for both visibility AND access restrictions (based on Zone security rules).
        /// As such, if isVisibleForCurrentUser is false, the user will not be able to successfully navigate to the page.
        /// </summary>
        public virtual bool isVisibleForCurrentUser
        {
            get
            {
                if (CmsContext.currentUserIsSuperAdmin)
                    return true;

                if (!currentUserCanRead)
                    return false;

                foreach (string path in Paths)
                {
                    if (path.IndexOf("/_") >= 0)
                        return false;
                } // foreach path

                return true;
            }
        }

		/// <summary>
		/// CmsPage constructor
		/// </summary>
        public CmsPage()
		{
            LanguageInfo = new CmsPageLanguageInfo[0];
			
			CreatedDateTime = DateTime.MinValue;
			LastUpdatedDateTime = DateTime.MinValue;
            LastModifiedBy = "";
            
			_parentId = 0;
			SortOrdinal = -1;
            RevisionNumber = -1;
						
            _childPagesLock = new object();
            HeadSection = new CmsPageHeadSection(this);
            EditMenu = new CmsPageEditMenu(this);
						
		} // constructor

        //public CmsPage(int pageid)
        //{
        //    this.ID = pageid;
        //    LanguageInfo = new CmsPageLanguageInfo[0];

        //    CreatedDateTime = DateTime.MinValue;
        //    LastUpdatedDateTime = DateTime.MinValue;
        //    LastModifiedBy = "";

        //    _parentId = -1;
        //    SortOrdinal = -1;
        //    RevisionNumber = -1;

        //    _childPagesLock = new object();
        //    HeadSection = new CmsPageHeadSection(this);
        //    EditMenu = new CmsPageEditMenu(this);

        //} // constructor
        

        /// <summary>
        /// a function that sees if this page is the currentPage.
        /// </summary>
        /// <returns></returns>
        public virtual bool isSelfSelected()
        {
            if (this.Id == CmsContext.currentPage.Id)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// a recursive function that sees if this page, or a child page of this page is the currentPage.
        /// </summary>
        /// <returns></returns>
        public virtual bool isChildOrSelfSelected()
        {
            if (this.isSelfSelected())
            {
                return true;
            }
            else return isChildSelected();
        }
        
        /// <summary>
        /// a recursive function that sees if a child page is the currentPage.
        /// </summary>
        /// <returns></returns>
        public virtual bool isChildSelected()
        {

            foreach (CmsPage childPage in this.ChildPages)
            {
                if (childPage.Id == CmsContext.currentPage.Id)
                {
                    return true;
                }
                bool b = childPage.isChildSelected();
                if (b)
                    return true;
            } // foreach
            return false;

        } // childIsSelected

        /// <summary>
        /// a tree-climbing function that sees if this page is a child page of the possibleParentPage.
        /// </summary>
        /// <returns></returns>
        public virtual bool isChildOf(CmsPage possibleParentPage)
        {
            CmsPage currPage = this;
            while (currPage.ParentPage.Id != -1)
            {
                if (currPage.Id == possibleParentPage.Id)
                    return true;
                currPage = currPage.ParentPage;
            }
            return false;
        }

        public virtual bool isParentOrSelfSelected()
        {
            if (this.isSelfSelected())
            {
                return true;
            }
            else return isParentSelected();
        }

        /// <summary>
        /// sees if a parent page is the currentPage.
        /// </summary>
        /// <returns></returns>
        public virtual bool isParentSelected()
        {
            CmsPage currPage = this;
            while (currPage.ParentPage.Id != -1)
            {
                if (currPage.Id == CmsContext.currentPage.Id)
                    return true;
                currPage = currPage.ParentPage;
            }
            return false;
        } // childIsSelected

        public virtual bool isSiblingSelected()
        {
            if (ParentPage.Id >= 0)
            {
                foreach (CmsPage sibling in ParentPage.ChildPages)
                {
                    if (sibling.isSelfSelected())
                        return true;
                }
            }
            return false;
        }

        public virtual bool isSiblingOrSiblingChildSelected()
        {
            if (ParentPage.Id >= 0)
            {
                foreach (CmsPage sibling in ParentPage.ChildPages)
                {
                    if (sibling.isChildOrSelfSelected())
                        return true;
                }
            }
            return false;
        }

        public virtual System.Web.UI.UserControl ToWebControl()
        {
            return new CmsPageWebControl(this);
        }

        // -- use a private class so that all the functions in System.Web.UI are hidden from the user.
        private class CmsPageWebControl : System.Web.UI.UserControl
        {
            private CmsPage OwningPage;
            public CmsPageWebControl(CmsPage owningPage): base()
            {
                
                OwningPage = owningPage;
            }

            /// <summary>
            /// Renders the page through the creation of child controls. The TemplateEngine creates and adds these controls.
            /// </summary>
            protected override void CreateChildControls()
            {                
                if (OwningPage.Id < 0)
                {
                    if (new CmsPageCache().GetHomePageId() < 0)
                    {
                        throw new CmsNoPagesFoundException("There could be a database connection problem: The home page could be loaded from the database.");
                    }
                    else
                    {
                        throw new Exception("This page could not be rendered because it has not been initialized from the database.");
                    }
                }

                // -- checks if the current user can read the current page. If not authorized, redirect to the Login page.
                //      but only redirect if the current page isn't the login page.
                bool canRead = OwningPage.SecurityZone.canRead(CmsContext.currentWebPortalUser);
                if (canRead == false && String.Compare(OwningPage.Path,CmsConfig.getConfigValue("LoginPath", "/_login"), true) != 0)
                {
                    NameValueCollection loginParams = new NameValueCollection();
                    loginParams.Add("target", OwningPage.Id.ToString());
                    CmsContext.setEditModeAndRedirect(CmsEditMode.View, CmsContext.getPageByPath(CmsConfig.getConfigValue("LoginPath", "/_login")), loginParams);
                    return;
                }

                DataPersister persister = new DataPersister();
                IDbContext dbcontext = persister.getDBContext();
                dbcontext.BeginTransaction();
                try
                {
                    // -- create all placeholders and controls based on the page's template.
                    OwningPage.TemplateEngine.CreateChildControls(this);
                    dbcontext.CommitTransaction();
                }
                catch
                {
                    dbcontext.RollbackTransaction();
                    throw;
                }

                // -- Run the page output filters
                this.Response.Filter = new CmsOutputFilterUtils.PageResponseOutputFilter(Response.Filter, OwningPage);

            }

        } // WebUIPage                

        private string _startedFormId = "";
			
		/// <summary>
		/// gets the HTML code to start an HTML form.
		/// This will output the fields required for proper system functioning.
		/// (if this is not used, form submission will always be directed to the home page).
		/// Note: contains code to ensure this is called only once per page.
        /// Note: IE has an issue that you can not have nested form tags (&ltform&gt &ltform&gt &lt/form&gt  &lt/form&gt is invalid and won't work!!!)
		/// </summary>
		/// <param name="onSubmit">the javascript code to run for the onSubmit event</param>
		/// <returns>the html to start the form</returns>
        public virtual string getFormStartHtml(string formId, string onSubmit, string style, string actionUrl, string method)
        {
            if (_startedFormId != "")
                return "";

            StringBuilder html = new StringBuilder();

            html.Append("<form");
            if (formId != "")
                html.Append(" id=\"" + formId + "\"");
            html.Append(" target=\"_self\"");
            if (actionUrl != "")
                html.Append(" action=\"" + actionUrl + "\"");
            else
                html.Append(" action=\"" + this.Url + "\"");            

            if (onSubmit != "")
                html.Append(" onsubmit=\"" + onSubmit + "\"");

            if (method != "")
                html.Append(" method=\"" + method + "\"");
            
            html.Append(" enctype=\"multipart/form-data\">");

            if (actionUrl == "")
            {
                string pFormVariable = Path;
                if (CmsConfig.UseLanguageShortCodeInPageUrls)
                {
                    pFormVariable = "/" + CmsContext.currentLanguage.shortCode + pFormVariable;
                }
                html.Append("<input type=\"hidden\" name=\"p\" value=\"" + pFormVariable + "\" />");
            }
            
            _startedFormId = formId;

            return html.ToString();
        }

        /// <summary>
        /// gets the Html to start a form. 
        /// Note: IE has an issue that you can not have nested form tags (&ltform&gt &ltform&gt &lt/form&gt  &lt/form&gt is invalid and won't work!!!)
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="onSubmit"></param>
        /// <returns></returns>
        public virtual string getFormStartHtml(string formId, string onSubmit)
        {
            return getFormStartHtml(formId, onSubmit, "", "", "post");
        }

        /// <summary>
        /// gets the Html to start a form. 
        /// Note: IE has an issue that you can not have nested form tags (&ltform&gt &ltform&gt &lt/form&gt  &lt/form&gt is invalid and won't work!!!)
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public virtual string getFormStartHtml(string formId)
        {
            return getFormStartHtml(formId, "", "", "", "post");            
        }
        

		/// <summary>
		/// gets the Html to close the form started with getFormStartHtml(). 
        /// Note: IE has an issue that you can not have nested form tags (&ltform&gt &ltform&gt &lt/form&gt  &lt/form&gt is invalid and won't work!!!)
		/// </summary>
		/// <returns></returns>
        public virtual string getFormCloseHtml(string formId)
		{
            if (String.Compare(formId, _startedFormId) != 0)
                return "";
            
            string html = "</form>";
            _startedFormId = "";
			return html;						
		}

        /// <summary>
        /// This enum specifies whether filters should be run for 
        /// CmsPage.render* functions (including renderPlaceholderToString(), renderAllPlaceholdersToString() and renderPlaceholdersToString())
        /// </summary>
        public enum RenderPlaceholderFilterAction { 
            /// <summary>
            /// Do not run any filters on the returned value
            /// </summary>
            ReturnUnfiltered, 
            /// <summary>
            /// Run both Page and Placeholder filters on the returned value.
            /// This should only be specified if the return value is being directly displayed to the client.
            /// Otherwise, filters will be run multiple times resulting in slower performance.
            /// Note: Placeholder filters are run first, then page placeholders.
            /// </summary>
            RunAllPageAndPlaceholderFilters };

		/// <summary>
		/// Gets the value stored in a placeholder on this page.
		/// This function is mostly used for aggregator controls. Placeholders should
		/// retrieve their values directly (without using this method).
		/// </summary>
		/// <exception cref="System.Exception">thrown when placeholderType is invalid</exception>
		/// <param name="placeholderType">the placeholder type to get values for</param>
		/// <param name="identifier">the placeholder identifier to get the value for. Use <see cref="getPlaceholderIdentifiers">getPlaceholderIdentifiers()</see> to get the valid identifiers.</param>
		/// <returns>the value found for the given placeholderType and identifier. Returns an empty string (string.empty) if the identifier was not found in the system.</returns>
        public virtual string renderPlaceholderToString(CmsPlaceholderDefinition phDef, CmsLanguage language, RenderPlaceholderFilterAction filterAction)
		{			
            string ret = PlaceholderUtils.renderPlaceholderToString(this, language, phDef);
            switch (filterAction)
            {
                case RenderPlaceholderFilterAction.ReturnUnfiltered:
                    return ret;
                    break;
                case RenderPlaceholderFilterAction.RunAllPageAndPlaceholderFilters:
                    // -- 1: placeholder Filters
                    ret = CmsOutputFilterUtils.RunPlaceholderFilters(phDef.PlaceholderType, this, ret);
                    // -- 2: page filters
                    ret = CmsOutputFilterUtils.RunPageOutputFilters(this, ret);
                    return ret;
                    break;
                default:
                    throw new ArgumentException("Error: invalid RenderPlaceholderFilterAction");
            }
						
		}

        public virtual string renderAllPlaceholdersToString(CmsLanguage forLanguage, RenderPlaceholderFilterAction filterAction)
        {
            CmsPlaceholderDefinition[] phDefs = getAllPlaceholderDefinitions();
            StringBuilder ret = new StringBuilder();

            foreach (CmsPlaceholderDefinition phDef in phDefs)
            {
                ret.Append(renderPlaceholderToString(phDef, forLanguage, filterAction));
            }

            return ret.ToString();
        }

        public virtual string renderPlaceholdersToString(string placeholderTypeToRender, CmsLanguage forLanguage, RenderPlaceholderFilterAction filterAction)
        {
            CmsPlaceholderDefinition[] phDefs = getPlaceholderDefinitions(placeholderTypeToRender);
            StringBuilder ret = new StringBuilder();

            foreach (CmsPlaceholderDefinition phDef in phDefs)
            {
                ret.Append(renderPlaceholderToString(phDef, forLanguage, filterAction));
            }

            return ret.ToString();
        }

        

        /// <summary>
        /// returns an array of CmsPlaceholderDefinitions that are definied in the template for this page.        
        /// <br />WARNING!!! if the template file for this page doesn't exist, this function will throw an Exception!!!
        /// </summary>
        /// <returns></returns>
        public virtual CmsPlaceholderDefinition[] getAllPlaceholderDefinitions()
        {
            // -- this function is used all over the place, so let's cache it's results (note: base the cache on the template name, not the page).
            string cacheKey = "getAllPlaceholderDefinitions" + TemplateName;
            if (PerRequestCache.CacheContains(cacheKey))
                return (CmsPlaceholderDefinition[]) PerRequestCache.GetFromCache(cacheKey, new CmsPlaceholderDefinition[0]);

            CmsPlaceholderDefinition[] arr = TemplateEngine.getAllPlaceholderDefinitions();
            PerRequestCache.AddToCache(cacheKey, arr);
            return arr;
        }

        /// <summary>
        /// returns an array of CmsPlaceholderDefinitions that are definied in the template for this page.        
        /// <br />WARNING!!! if the template file for this page doesn't exist, this function will throw an Exception!!!
        /// </summary>
        /// <returns></returns>
        public virtual CmsPlaceholderDefinition[] getPlaceholderDefinitions(string placeholderType)
        {
            CmsPlaceholderDefinition[] all = TemplateEngine.getAllPlaceholderDefinitions(); // use "All.." because it is cached.
            return CmsPlaceholderDefinition.GetByPlaceholderType(all, placeholderType);
        }

        /// <summary>
        /// gets a list of unique placeholder names used in the page's template.
        /// note: all placeholder names are lower case.
        /// If there is a problem with the template, returns an empty array
        /// </summary>
        /// <returns></returns>
        public virtual string[] getAllPlaceholderNames()
        {
            try
            {
                CmsPlaceholderDefinition[] defs = this.getAllPlaceholderDefinitions(); // use "All.." because it is cached.
                List<string> ret = new List<string>();
                foreach (CmsPlaceholderDefinition phDef in defs)
                {
                    if (ret.IndexOf(phDef.PlaceholderType) < 0)
                        ret.Add(phDef.PlaceholderType);
                } // foreach
                return ret.ToArray();
            }
            catch (Exception e) {
                throw e;
            }
            return new string[0];
        }

        public virtual bool hasPlaceholder(string placeholderType)
        {
            string[] allPhNames = getAllPlaceholderNames(); // use "All.." because it is cached.
            return (StringUtils.IndexOf(allPhNames, placeholderType, StringComparison.CurrentCultureIgnoreCase) > -1);
        }

        /// <summary>
        /// gets a list of control path used in the page's template.
        /// if a control is referenced more than once, it will be found multiple times in the return array.
        /// <br/>Note: Control paths never have a filename extension (ie "_system/Login" is a valid control path, while "_system/Login.ascx" is invalid).
        /// <br />WARNING!!! if the template file for this page doesn't exist, this function will throw an Exception!!!
        /// </summary>
        /// <returns></returns>
        public virtual string[] getAllControlPaths()
        {
            List<string> ret = new List<string>();
            CmsControlDefinition[] controlDefs = getAllControlDefinitions(); // use getAllControlDefinitions because it's cached.
            foreach (CmsControlDefinition controlDef in controlDefs)
            {
                ret.Add(controlDef.ControlNameOrPath);
            } // foreach
            return ret.ToArray();
        }

        /// <summary>
        /// gets a list of CmsControlDefinitions used in the page's template.
        /// if a control is referenced more than once, it will be found multiple times in the return array.
        /// <br/>Note: Control paths never have a filename extension (ie "_system/Login" is a valid control path, while "_system/Login.ascx" is invalid).
        /// <br />WARNING!!! if the template file for this page doesn't exist, this function will throw an Exception!!!
        /// </summary>
        /// <returns></returns>
        public virtual CmsControlDefinition[] getAllControlDefinitions()
        {
            // -- this function is used all over the place, so let's cache it's results (note: base the cache on the template name, not the page).
            string cacheKey = "getAllControlDefinitions" + TemplateName;
            if (PerRequestCache.CacheContains(cacheKey))
                return (CmsControlDefinition[])PerRequestCache.GetFromCache(cacheKey, new CmsControlDefinition[0]);

            CmsControlDefinition[] arr = TemplateEngine.getAllControlDefinitions();
            PerRequestCache.AddToCache(cacheKey, arr);
            return arr;
                        
        }
       

        /// <summary>
        /// get a list of all valid revision numbers for this page, sorted so that the oldest Rev# is first ([0])
        /// </summary>
        /// <returns></returns>
        public virtual int[] getAllRevisionNumbers()
        {
            CmsPageRevisionData[] revs = getAllRevisionData();
            List<int> revNums = new List<int>();
            foreach (CmsPageRevisionData rev in revs)
            {
                revNums.Add(rev.RevisionNumber);
            }
            revNums.Sort();
            return revNums.ToArray();
        }

        /// <summary>
        /// gets the pages revision history, sorted in oldest to latest
        /// </summary>
        /// <returns></returns>
        public virtual CmsPageRevisionData[] getAllRevisionData()
        {
            //@@TODO: getAllRevisionData() should be cached in memory
            CmsPageDb db = new CmsPageDb();
            return db.getAllRevisionData(this);
        }

        public virtual bool revertToRevision(int oldRevisionNumberToMakeLive)
        {
            try
            {
                int[] allRevIds = this.getAllRevisionNumbers();
                // validate the parameter
                if (Array.IndexOf(allRevIds, oldRevisionNumberToMakeLive) < 0)
                    return false;
                
                // do not allow reversion of the live version. Note: this.RevisionNumber isn't necessarily the live revision number!                
                if (allRevIds.Length > 0 && allRevIds[allRevIds.Length - 1] == oldRevisionNumberToMakeLive)
                    return false;
                
                CmsPage oldPage = CmsContext.getPageByPath(this.Path, oldRevisionNumberToMakeLive);
                if (oldPage.Id < 0)
                    return false;

                bool ret = true;
                CmsPlaceholderDefinition[] defs = getAllPlaceholderDefinitions();

                Dictionary<string, List<int>> phInfo = CmsPlaceholderDefinition.ToNameIdentifierDictionary(defs);
                foreach (string phType in phInfo.Keys)
                {
                    foreach (CmsLanguage lang in CmsConfig.Languages)
                    {
                        BaseCmsPlaceholder.RevertToRevisionResult b = PlaceholderUtils.revertToRevision(phType, oldPage, this, phInfo[phType].ToArray(), lang);
                        if (b == BaseCmsPlaceholder.RevertToRevisionResult.Failure)
                            ret = false; // keep going, even if one placeholder failed.
                    } // foreach language
                } // foreach

                return ret;
                
            }
            catch
            {
                return false;
            }            
        }
        		
        /// <summary>
        /// The current page's template engine. This parameter is cached in-memory so that it can be called multiple times.
        /// </summary>
        public virtual CmsTemplateEngine TemplateEngine
        {
            get
            {
                CmsTemplateEngineVersion templateVersion = CmsConfig.TemplateEngineVersion;
                string cacheKey = "templateEngine_" + templateVersion.ToString() + this.TemplateName + this.Id.ToString(); // Template engine uses the name and page as constructors, so use those for the cache as well.

                if (PerRequestCache.CacheContains(cacheKey))
                    return (CmsTemplateEngine)PerRequestCache.GetFromCache(cacheKey, null);

                CmsTemplateEngine engine;
                switch (templateVersion)
                {                    
                    case CmsTemplateEngineVersion.v2: engine = new TemplateEngine.TemplateEngineV2(TemplateName, this); break;
                    default: throw new ArgumentException("Invalid Template Engine Version");
                }

                PerRequestCache.AddToCache(cacheKey, engine);
                return engine;
            }

        } // GetTemplateEngine

        /// <summary>
        /// returns a list of all sub-pages (and this current page) in a linearized Dictionary&lt;int, CmsPage&gt;
        /// in the format obj[pageId] => CmsPage. Only returns pages that the current user can access (based on Zonal security restrictions).
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<int, CmsPage> getLinearizedPages()
        {
            Dictionary<int, CmsPage> ret = new Dictionary<int, CmsPage>();

            if (!this.isVisibleForCurrentUser)
                return ret;

            ret.Add(this.Id, this);            

            foreach (CmsPage subPage in this.ChildPages)
            {
                if (subPage.isVisibleForCurrentUser)
                {
                    _RecursiveLinearizePage(subPage, ret);
                }
            }
            return ret;
        }



        private void _RecursiveLinearizePage(CmsPage parentPage, Dictionary<int, CmsPage> ret)
		{
			ret.Add(parentPage.Id, parentPage);
            
            foreach (CmsPage subPage in parentPage.ChildPages)
			{
                if (subPage.isVisibleForCurrentUser)
                {
                    _RecursiveLinearizePage(subPage, ret);
                }                
			}
		}

        /// <summary>
        /// sets the page.LastUpdatedDateTime in the database
        /// </summary>
        /// <returns></returns>
        public virtual bool setLastUpdatedDateTimeToNow()
        {
            CmsPageDb db = new CmsPageDb();
            return db.setLastUpdatedTime(this, DateTime.Now);

        }

        /// <summary>
        /// saves the new page.sortOrdinal to the database
        /// </summary>
        /// <param name="newOrdinalValue"></param>
        /// <returns></returns>
        public virtual bool setSortOrdinal(int newOrdinalValue)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateSortOrdinal(this, newOrdinalValue);
        }

        /// <summary>
        /// saves the new page.Title to the database.
        /// </summary>
        /// <param name="newTitle"></param>
        /// <returns></returns>
        public virtual bool setTitle(string newTitle, CmsLanguage forLanguage)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateTitle(this, newTitle, forLanguage);
        }

        /// <summary>
        /// saves the new page.MenuTitle to the database.
        /// </summary>
        /// <param name="newTitle"></param>
        /// <returns></returns>
        public virtual bool setMenuTitle(string newMenuTitle, CmsLanguage forLanguage)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateMenuTitle(this, newMenuTitle, forLanguage);
        }

        /// <summary>
        /// saves the new page.SearchEngineDescription to the database.
        /// </summary>
        /// <param name="newTitle"></param>
        /// <returns></returns>
        public virtual bool setSearchEngineDescription(string newSearchEngineDescription, CmsLanguage forLanguage)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateSearchEngineDescription(this, newSearchEngineDescription, forLanguage);
        }

        /// <summary>
        /// saves the new page.ParentPageId to the database.
        /// </summary>
        /// <param name="newTitle"></param>
        /// <returns></returns>
        public virtual bool setParentPage(CmsPage newParentPage)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateParentPage(this, newParentPage);
        }

        /// <summary>
        /// saves the new page.Name to the database
        /// </summary>
        /// <param name="newName"></param>
        /// <returns></returns>
        public virtual bool setName(string newName, CmsLanguage forLanguage)
        {
            if (StringUtils.IndexOf(InvalidPageNameChars, newName, StringComparison.CurrentCulture) >= 0)
            {
                throw new ArgumentException("Error: the newName of the page contains invalid characters!");
            }
            
            CmsPageDb db = new CmsPageDb();
            return db.updateName(this, newName, forLanguage);
        }

        /// <summary>
        /// saves the page.TemplateName to the database
        /// </summary>
        /// <param name="newTemplateName"></param>
        /// <returns></returns>
        public virtual bool setTemplateName(string newTemplateName)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateTemplateName(this, newTemplateName);
        }

        /// <summary>
        /// Mark this page as being deleted in the database.
        /// If this page is a master page for any placeholders, the master pages are updated accordingly.
        /// </summary>
        /// <returns></returns>
        public virtual bool DeleteThisPage()
        {
            CmsPageDb db = new CmsPageDb();
            return db.deletePage(this);
        }

        /// <summary>
        /// Update the page's .ShowInMenu property in the database.
        /// </summary>
        /// <param name="newIndicator"></param>
        /// <returns></returns>
        public virtual bool UpdateShowInMenuFlag(bool newIndicator)
        {
            DataPersister persister = new DataPersister();
            IDbContext dbcontext = persister.getDBContext();
            dbcontext.BeginTransaction();
            try
            {
                bool updateresult = new CmsPageDb().updateShowInMenuIndicator(this, newIndicator);
                dbcontext.CommitTransaction();
                return updateresult;
            }
            catch
            {
                dbcontext.RollbackTransaction();
                throw;
            }
        }

		private class LastUpdatedDateComparer: System.Collections.IComparer
		{
			int IComparer.Compare( Object x, Object y )  
			{
				return DateTime.Compare((x as CmsPage).LastUpdatedDateTime, (y as CmsPage).LastUpdatedDateTime);
			}
		} // LastUpdatedDateComparer

		/// <summary>
		/// Returns an array of CmsPage objects, with the first ([0]) having the oldest page, and the last [.Length-1] having the newest
		/// </summary>
		/// <param name="pages"></param>
		/// <returns></returns>
        public static CmsPage[] SortPagesByLastModifiedDate(CmsPage[] pages)
		{
			ArrayList a = new ArrayList(pages);
			LastUpdatedDateComparer comparer = new LastUpdatedDateComparer();
			a.Sort(comparer);
            return (CmsPage[])a.ToArray(typeof(CmsPage));
		} // SortPagesByLastModifiedDate   

        private class SortOrdinalComparer : System.Collections.IComparer
        {
            int IComparer.Compare(Object x, Object y)
            {
                // Less than zero : x is less than y. 
                // Zero : x equals y. 
                // Greater than zero : x is greater than y. 

                if ((x as CmsPage).SortOrdinal < (y as CmsPage).SortOrdinal) return -1;
                if ((x as CmsPage).SortOrdinal == (y as CmsPage).SortOrdinal) return 0;
                return 1;
            }
        } // SortOrdinalComparer

        public static CmsPage[] SortPagesBySortOrdinal(CmsPage[] pages)
        {
            ArrayList a = new ArrayList(pages);
            SortOrdinalComparer comparer = new SortOrdinalComparer();
            a.Sort(comparer);
            return (CmsPage[])a.ToArray(typeof(CmsPage));

        } // SortPagesBySortOrdinal 

        private class SortByTitleComparer : System.Collections.IComparer
        {
            CmsLanguage lang;
            public SortByTitleComparer(CmsLanguage langToUse)
            {
                lang = langToUse;
            }

            int IComparer.Compare(Object x, Object y)
            {
                return String.Compare((x as CmsPage).getTitle(lang), (y as CmsPage).getTitle(lang));
            }
        } // SortByTitleComparer

        public static CmsPage[] SortPagesByTitle(CmsPage[] pages, CmsLanguage langForTitles)
        {
            ArrayList a = new ArrayList(pages);
            SortByTitleComparer comparer = new SortByTitleComparer(langForTitles);
            a.Sort(comparer);
            return (CmsPage[])a.ToArray(typeof(CmsPage));

        } // SortPagesByTitle


        public static bool ArrayContainsPage(CmsPage[] haystack, CmsPage needle)
        {
            return ArrayContainsPageId(haystack, needle.Id);
        }

        public static bool ArrayContainsPageId(CmsPage[] haystack, int needlePageId)
        {
            foreach (CmsPage p in haystack)
            {
                if (p.Id == needlePageId)
                    return true;
            } // foreach
            return false;
        }

        public static CmsPage[] RemovePageFromHaystack(CmsPage[] haystack, CmsPage toRemove)
        {
            List<CmsPage> ret = new List<CmsPage>(haystack);
            ret.Remove(toRemove);
            return ret.ToArray();
        }        

        /// <summary>
        /// Inserts a new page in the database. Note: the pageToInsert must have all properties and LangInfos properly setup.
        /// </summary>
        /// <param name="pageToInsert"></param>
        /// <returns></returns>
        public static bool InsertNewPage(CmsPage pageToInsert)
        {
            return new CmsPageDb().createNewPage(pageToInsert);
        }

        /// <summary>
        /// Fetch pages given a template name
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public static CmsPage[] FetchPageByTemplate(string template)
        {
            return new CmsPageDb().FetchPagesByTemplateName(template);
        }

        /// <summary>
        /// returns the PageId for the Home Page (the home page has it's name set to NULL, and it's parentId set to 0).
        /// If the home page is not found, or multiple pages are found, returns -1.
        /// </summary>
        /// <returns></returns>
        public static int FetchHomePageId()
        {
            return new CmsPageDb().FetchHomePageId();
        }

        /// <summary>
        /// Returns a newly created page object (with ID &lt; 0) if the pageId was not found.
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public static CmsPage FetchPageById(int pageId)
        {
            return new CmsPageDb().FetchPageById(pageId);
        }

        /// <summary>
        /// gets a CmsPage object that has the specified path.       
        /// If the path is not found, throws a CmsPageNotFoundException error.
        /// </summary>
        /// <param name="pagePath"></param>
        /// <returns></returns>
        public static CmsPage FetchPageByPath(string pagePath)
        {
            return new CmsPageDb().FetchPageByPath(pagePath);
        }

        /// <summary>
        /// Gets a CmsPage object that has the specified path.       
        /// If the path is not found, throws a CmsPageNotFoundException error.
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="pagePathLanguage"></param>
        /// <returns></returns>
        public static CmsPage FetchPageByPath(string pagePath, CmsLanguage pagePathLanguage)
        {
            return new CmsPageDb().FetchPageByPath(pagePath, pagePathLanguage);
        }

        /// <summary>
        /// gets a CmsPage object that has the specified path.       
        /// If the path is not found, throws a CmsPageNotFoundException error.
        /// If the revion number is not found, a RevisionNotFoundException error is thrown.
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="pageRevisionNumber"></param>
        /// <returns></returns>
        public static CmsPage FetchPageByPath(string pagePath, int pageRevisionNumber)
        {
            return new CmsPageDb().FetchPageByPath(pagePath, pageRevisionNumber);
        }

        

        #region CmsPageDb class
        /// <summary>
        /// The class that handles all database calls for <see cref="CmsPage">CmsPage</see> objects.
        /// </summary>
        private class CmsPageDb : Hatfield.Web.Portal.Data.MySqlDbObject
        {
            private CmsPageCache pageCache; // private, not static cache of pages in memory
            private PageRepository repository;
            /// <summary>
            /// provides database functions for the CmsPage object
            /// </summary>
            public CmsPageDb()                
            {
                repository = new PageRepository();
                pageCache = CmsContext.getPageCache();
                if (!this.pageCache.AllPagesHaveBeenCached)
                    loadAllPagesIntoCache();
            }

            private void loadAllPagesIntoCache()
            {
                try
                {
                    IList<CmsPage> pagelist = repository.fetallpage();
                    if (pagelist.Count > 0)
                    {
                        CmsPage[] pages = (pagelist as List<CmsPage>).ToArray();
                        if (pages.Length == 0)
                            throw new CmsNoPagesFoundException("No pages could be loaded from the database - there could be a database connection issue!");

                        pageCache.AddAllPagesToCache(pages);
                        // -- cache the home page id
                        foreach (CmsPage p in pages)
                        {
                            if (p.Name == "" && p.ParentID <= 0)
                                pageCache.SetHomePageId(p.Id);
                        }

                        if (pageCache.GetHomePageId() < 0)
                        {
                            throw new CmsNoPagesFoundException("The home page could not be loaded from the database");
                        }
                    } // if has data
                }
                catch (Exception ex)
                {
                    throw ex;
                    //throw new CmsNoPagesFoundException("No pages could be loaded from the database - there could be a database connection issue!");
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
                        pageToModify.Id = id;
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
                    langInfo.LanguageShortCode = dr["langCode"].ToString();
                    langInfo.Name = getPossiblyNullValue(dr, "name", "");
                    langInfo.Title = dr["title"].ToString();
                    langInfo.MenuTitle = dr["menuTitle"].ToString();
                    langInfo.SearchEngineDescription = dr["searchEngineDescription"].ToString();

                    List<CmsPageLanguageInfo> infos = new List<CmsPageLanguageInfo>(pageToModify.LanguageInfo);
                    infos.Add(langInfo);
                    pageToModify.LanguageInfo = infos.ToArray();

                    if (!pages.ContainsKey(key))
                        pages.Add(key, pageToModify);
                }

                List<CmsPage> ret = new List<CmsPage>(pages.Values);
                return ensurePageLanguageInfoConsistency(ret.ToArray());
            }

            //This function check if all the page has page language info
            //does not involve any database operation
            //Not change in the refactor
            private CmsPage[] ensurePageLanguageInfoConsistency(CmsPage[] pages)
            {
                List<CmsPage> ret = new List<CmsPage>(pages);
                foreach (CmsPage page in pages)
                {
                    foreach (CmsLanguage l in CmsConfig.Languages)
                    {
                        CmsPageLanguageInfo lang = CmsPageLanguageInfo.GetFromHaystack(l, page.LanguageInfo);
                        if (lang.LanguageShortCode == "")
                        {
                            // -- useful debugging information
                            string sql = "INSERT INTO pagelanginfo (pageId, langCode, name, title, menuTitle, searchEngineDescription) select pageId, '" + lang.LanguageShortCode.ToLower() + "', name,title, menuTitle, searchEngineDescription from pages;";
#if !DEBUG 
                        sql = "";
#endif
                            throw new Exception("Page " + page.Id + " needs to have language " + lang.LanguageShortCode + " information added: " + sql); ;
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
            public CmsPage FetchPageById(int pageId)
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
                pageCache.Add(pageId, blankPage);
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
                    return CmsPage.SortPagesBySortOrdinal(subPages.ToArray());
                }
                CmsPage[] ret2 = new CmsPage[0];
                return ret2;

            } // getChildPages

            /// <summary>
            /// returns the PageId for the Home Page (the home page has it's name set to NULL, and it's parentId set to 0).
            /// If the home page is not found, or multiple pages are found, returns -1.
            /// </summary>
            /// <returns></returns>
            public int FetchHomePageId()
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
            public CmsPage FetchPage(int parentPageId, string childName, CmsLanguage childNameLanguage)
            {
                CmsPage cachedParentPage = pageCache.Get(parentPageId);
                if (cachedParentPage == null)
                {
                    // -- force the parent page to be in the cache
                    cachedParentPage = FetchPageById(parentPageId);
                }

                CmsPage cachedChild = pageCache.Get(parentPageId, childName, childNameLanguage);
                if (cachedChild != null)
                    return cachedChild;

                return new CmsPage();
            }


            /// <summary>
            /// gets a CmsPage object that has the specified path. 
            /// If the path is not found, throws a CmsPageNotFoundException error.
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public CmsPage FetchPageByPath(string pagePath, CmsLanguage pagePathLanguage)
            {
                return FetchPageByPath(pagePath, Int32.MinValue, pagePathLanguage);
            }

            /// <summary>
            /// gets a CmsPage object that has the specified path. 
            /// If the path is not found, throws a CmsPageNotFoundException error.            
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public CmsPage FetchPageByPath(string pagePath)
            {
                return FetchPageByPath(pagePath, Int32.MinValue, CmsContext.currentLanguage);
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
            public CmsPage FetchPageByPath(string pagePath, int pageRevisionNumber)
            {
                return FetchPageByPath(pagePath, pageRevisionNumber, CmsContext.currentLanguage);
            }

            /// <summary>
            /// gets a CmsPage object that has the specified path. 
            /// The pagePath should NEVER includes the language shortCode.
            /// If the path is not found, throws a CmsPageNotFoundException error.
            /// If the revion number is not found, a RevisionNotFoundException error is thrown.
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public CmsPage FetchPageByPath(string pagePath, int pageRevisionNumber, CmsLanguage pageLanguage)
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
                    CmsPage homePage = this.FetchPageById(FetchHomePageId());
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
                int parentId = FetchHomePageId();
                if (parentId < 0)
                    throw new Exception("no home page found!");

                CmsPage currentPage = new CmsPage();


                // -- go through all the parts of the page path (from first to last), making sure that all pages exist.
                for (int i = 0; i < path_parts.Length; i++)
                {
                    currentPage = FetchPage(parentId, path_parts[i], pageLanguage);
                    if (currentPage.Id < 0)
                    {
                        throw new CmsPageNotFoundException();
                    }
                    else
                    {
                        parentId = currentPage.Id;
                    }
                } // for

                if (currentPage.Id < 0)
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
            /// Get cms pages by template name (case not sensitive)
            /// </summary>
            /// <param name="templateName"></param>
            /// <returns></returns>
            public CmsPage[] FetchPagesByTemplateName(string templateName)
            {
                IList<CmsPage> pagelist = repository.FetchPagesByTemplateName(templateName);

                if(pagelist.Count > 0)
                    return (pagelist as List<CmsPage>).ToArray();
                else
                    return new CmsPage[0];
                
            }

            /// <summary>
            /// inserts a new page in the database
            /// </summary>
            /// <param name="newPage"></param>
            /// <returns>true if inserted successfully, false if not.</returns>
            public bool createNewPage(CmsPage newPage)
            {
                if (newPage.LanguageInfo.Count != CmsConfig.Languages.Length)
                    throw new Exception("Error: when creating a new page, the page must have at least one LanguageInfo specified!");

                if (newPage.ParentID < 0)
                    newPage.ParentID = 0;
                newPage.createdatetime = DateTime.Now;
                newPage.lastupdateDateTime = DateTime.Now;
                try
                {
                    newPage.lastmodifiedby = CmsContext.currentWebPortalUser.UserName;
                }
                catch
                {
                    newPage.lastmodifiedby = "";
                }
                
                CmsPage createdPage = repository.SaveOrUpdate(newPage);
               
                if (createdPage.Id > 0)
                {
                    pageCache.Add(createdPage.Id, createdPage);
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
                if (pageToDelete.Id < 0)
                    return false; // nothing to delete            

                // Mark this page as being deleted.
                pageToDelete.deleted = DateTime.Now;
                CmsPage deletedpage = repository.Update(pageToDelete);
                if (deletedpage.Id > 0)
                    return true;
                else
                    return false;

            } // deletePage

            /// <summary>
            /// 
            /// </summary>
            /// <param name="page"></param>
            /// <returns>the new revision number for the page, or -1 on failure</returns>
            public int createNewPageRevision(CmsPage page)
            {
                if (page.Id < 0)
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

                CmsPageRevisionData newrevision = new CmsPageRevisionData(page.Id, newRevisionNumber, DateTime.Now, lastModifiedBy );
                if (repository.CreateNewRepository(ref page, newrevision) < 0)
                    return -1;
                else
                {
                    // important: do not update page.RevisionNumber!!
                    pageCache.Update(page);
                    return newRevisionNumber;
                }                

            }

            public CmsPageRevisionData[] getAllRevisionData(CmsPage page)
            {
                PageRevisionDataRepository revisiondatarepository = new PageRevisionDataRepository();
                IList<CmsPageRevisionData> revisiondatalist = revisiondatarepository.FetchAllRevisionDataofPage(page);
                return (revisiondatalist as List<CmsPageRevisionData>).ToArray();
            }

            /// <summary>
            /// sets the lastUpdated timestamp and the LastModifiedBy name for an existing page.
            /// </summary>
            /// <param name="page"></param>
            /// <param name="dt"></param>
            /// <returns>true if updated successfully, false if not.</returns>
            public bool setLastUpdatedTime(CmsPage page, DateTime dt)
            {
                if (page.Id < 0)
                    return false; // nothing to delete

                string lastModifiedBy = "";
                if (CmsContext.currentUserIsLoggedIn)
                    lastModifiedBy = CmsContext.currentWebPortalUser.UserName;
                page.lastupdateDateTime = dt;
                page.lastmodifiedby = lastModifiedBy;
                if (repository.SaveOrUpdate(page).Id < 0)
                    return false;
                else
                    return true;
            }

            /// <summary>
            /// sets the sortOrdinal for an existing page.
            /// </summary>
            /// <param name="page"></param>
            /// <param name="newOrdinalValue"></param>
            /// <returns>true if updated successfully, false if not.</returns>
            public bool updateSortOrdinal(CmsPage page, int newOrdinalValue)
            {
                if (page.Id == -1)
                    return false; // nothing to delete
                page.sortordinal = newOrdinalValue;
                if (repository.SaveOrUpdate(page).Id > 0 && page.setLastUpdatedDateTimeToNow())
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
                if (page.Id == -1)
                    return false; // nothing to delete
                if (repository.UpdatePageLanguageTitle(page, newTitle, pageLanguage).Id > 0 && page.setLastUpdatedDateTimeToNow())
                {
                    // -- update the tracking object
                    CmsPageLanguageInfo.GetFromHaystack(pageLanguage, page.LanguageInfo).Title = newTitle;

                    pageCache.Update(page);
                    return true;
                }
                return false;
            }

            public bool updateShowInMenuIndicator(CmsPage page, bool newIndicator)
            {
                if (page.Id == -1)
                    return false; // nothing to update

                page.showinmenu = newIndicator;

                if (repository.SaveOrUpdate(page).Id > 0 && page.setLastUpdatedDateTimeToNow())
                {
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
                if (page.Id == -1)
                    return false; // nothing to update

                if (repository.UpdatePageLanguageMenuTitle(page, newMenuTitle, pageLanguage).Id > 0 && page.setLastUpdatedDateTimeToNow())
                {
                    // page.MenuTitle = newMenuTitle;
                    CmsPageLanguageInfo.GetFromHaystack(pageLanguage, page.LanguageInfo).MenuTitle = newMenuTitle;
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
                if (page.Id == -1)
                    return false; // nothing to update

                if (repository.UpdatePageLanguageSearchEngineDescription(page, newSearchEngineDescription, pageLanguage).Id > 0 && page.setLastUpdatedDateTimeToNow())
                {
                    // page.SearchEngineDescription = newSearchEngineDescription;
                    CmsPageLanguageInfo.GetFromHaystack(pageLanguage, page.LanguageInfo).SearchEngineDescription = newSearchEngineDescription;
                    pageCache.Update(page);
                    return true;
                }
                return false;
            }

            public bool updateParentPage(CmsPage page, CmsPage newParentPage)
            {
                if (page.Id == -1)
                    return false; // nothing to update
                page.ParentID = newParentPage.Id;

                if (repository.SaveOrUpdate(page).Id > 0 && page.setLastUpdatedDateTimeToNow())
                {
                    // -- clear the memory cached pages and sub-pages
                    pageCache.Clear();
                    pageCache.Update(page);
                    return true;
                }
                return false;
            }

            public bool updateName(CmsPage page, string newName, CmsLanguage pageLanguage)
            {
                if (page.Id == -1)
                    return false; // nothing to update
                if (repository.UpdatePageLanguageName(page, newName, pageLanguage).Id > 0 && page.setLastUpdatedDateTimeToNow())
                {
                    // page.Name = newName.ToLower();
                    CmsPageLanguageInfo.GetFromHaystack(pageLanguage, page.LanguageInfo).Name = newName.ToLower();

                    // -- clear the memory cached pages and sub-pages
                    pageCache.Clear();


                    pageCache.Update(page);
                    return true;
                }
                return false;
            }

            public bool updateTemplateName(CmsPage page, string newTemplateName)
            {
                if (page.Id == -1)
                    return false; // nothing to update
                page.TemplateName = newTemplateName;

                if (repository.SaveOrUpdate(page).Id > 0 && page.setLastUpdatedDateTimeToNow())
                {
                    // -- clear the memory cached pages and sub-pages
                    pageCache.Clear();
                    pageCache.Update(page);
                    return true;
                }
                return false;
            }

            private void clearAllExpiredPageLocks()
            {
                repository.clearAllExpiredPageLocks();
            }

            public CmsPageLockData getPageLockData(CmsPage targetPage)
            {
                return repository.getPageLockData(targetPage);
            }

            public void clearCurrentPageLock(CmsPage pageToUnlock)
            {
                // delete both expired locks and the lock for the current page
                repository.clearCurrentPageLock(pageToUnlock);
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
                    ret.PageId = pageToLock.Id;
                    ret.LockedByUsername = lastModifiedBy;
                    ret.LockExpiresAt = new DateTime(DateTime.Now.Ticks + TimeSpan.FromMinutes(minutesToLockFor).Ticks);

                    return repository.lockPageForEditing(ret);
                } // lock

            } // lockPageForEditing                
        }
        #endregion

    }
}
