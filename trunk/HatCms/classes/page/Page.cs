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
	public class CmsPage: System.Web.UI.UserControl
	{
		/// <summary>
		/// the unique identifier for this page. 
		/// Set to -1 if not initialized from the database.
		/// </summary>
		public new int ID;
        
        /// <summary>
        /// The CmsPageLanguageInfo instances track all language specific parameters (such as title, menuTitle, name, etc) for this page.
        /// </summary>
        public CmsPageLanguageInfo[] LanguageInfo;
        
        /// <summary>
		/// the user-friendly title of this page.
        /// Note: changes based on CmsContext.currentLanguage.
		/// Set this value using setTitle().
		/// </summary>
        public string Title
        {
            get
            {
                return CmsPageLanguageInfo.GetFromHaystack(CmsContext.currentLanguage, LanguageInfo).title;
            }
        }

        /// <summary>
        /// Gets the user-friendly title of this page for the specified language.
        /// </summary>
        /// <param name="forLanguage"></param>
        /// <returns></returns>
        public string getTitle(CmsLanguage forLanguage)
        {
            return CmsPageLanguageInfo.GetFromHaystack(forLanguage, LanguageInfo).title;
        }

        /// <summary>
        /// All Titles for the current page. Each language can give the page a different Title.
        /// </summary>
        public string[] Titles
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    ret.Add(CmsPageLanguageInfo.GetFromHaystack(lang, LanguageInfo).title);
                } // foreach
                return ret.ToArray();
            }
        }
		
		/// <summary>
		/// the user-friendly menu-title of this page.
        /// Note: changes based on CmsContext.currentLanguage.
		/// </summary>
        public string MenuTitle
        {
            get
            {
                return CmsPageLanguageInfo.GetFromHaystack(CmsContext.currentLanguage, LanguageInfo).menuTitle;
            }
        }

        public string getMenuTitle(CmsLanguage forLanguage)
        {
            return CmsPageLanguageInfo.GetFromHaystack(forLanguage, LanguageInfo).menuTitle;
        }

        /// <summary>
        /// All MenuTitles for the current page. Each language can give the page a different MenuTitles.
        /// </summary>
        public string[] MenuTitles
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    ret.Add(CmsPageLanguageInfo.GetFromHaystack(lang, LanguageInfo).menuTitle);
                } // foreach
                return ret.ToArray();
            }
        }

        /// <summary>
        /// the search engine description of this page.
        /// Note: changes based on CmsContext.currentLanguage.
        /// </summary>
        public string SearchEngineDescription
        {
            get
            {
                return CmsPageLanguageInfo.GetFromHaystack(CmsContext.currentLanguage, LanguageInfo).searchEngineDescription;
            }
        }

        public string getSearchEngineDescription(CmsLanguage forLanguage)
        {
            return CmsPageLanguageInfo.GetFromHaystack(forLanguage, LanguageInfo).searchEngineDescription;
        }

        /// <summary>
        /// All SearchEngineDescriptions for the current page. Each language can give the page a different SearchEngineDescription.
        /// </summary>
        public string[] SearchEngineDescriptions
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    ret.Add(CmsPageLanguageInfo.GetFromHaystack(lang, LanguageInfo).searchEngineDescription);
                } // foreach
                return ret.ToArray();
            }
        }


		/// <summary>
		/// show this page in a menu?
		/// </summary>
		public bool ShowInMenu;

        
        /// <summary>
		/// The filename of this page. Forms this page's part of the URL. For the home page, the Name is String.Empty
        /// Note: changes based on CmsContext.currentLanguage.
		/// </summary>
        public string Name
        {
            get
            {
                return CmsPageLanguageInfo.GetFromHaystack(CmsContext.currentLanguage, LanguageInfo).name;
            }
        }

        /// <summary>
        /// All names (filenames) for the current page. Each language can give the page a different name.
        /// </summary>
        public string[] Names
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    ret.Add(CmsPageLanguageInfo.GetFromHaystack(lang, LanguageInfo).name);
                } // foreach
                return ret.ToArray();                
            }
        }

        public string getName(CmsLanguage forLanguage)
        {
            return CmsPageLanguageInfo.GetFromHaystack(forLanguage, LanguageInfo).name;
        }

        

        private int _parentId;
        /// <summary>
		/// the ID for the parent page. Set to -1 if no parent exists, or if not yet initialized.
		/// </summary>
        public int ParentID
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
		/// the name of the template that is used to render this page.
		/// this name must refer directly to a file in the "~/templates/" directory.
		/// files in the "~/templates/" directory must have the ".htm" extension, while
		/// this property must NOT contain the filename extension.
		/// </summary>
		public string TemplateName;

		/// <summary>
		/// the timestamp for when this page was created.
		/// </summary>
		public DateTime CreatedDateTime;

		/// <summary>
		/// the timestamp for when this page was last updated.
		/// update this value using setLastUpdatedDateTimeToNow().
		/// </summary>
		public DateTime LastUpdatedDateTime;


        /// <summary>
        /// the username that last modified the page.
        /// update this value using setLastUpdatedDateTimeToNow().
        /// </summary>
        public string LastModifiedBy;

        /// <summary>
        /// the Date and time to remove anonymous access at.
        /// set to DateTime.MinValue if Anonymous access should never be removed.
        /// </summary>
        // public DateTime removeAnonymousAccessAt;
		
		/// <summary>
		/// the sort placement of this page.
		/// set this value using setSortOrdinal().
		/// </summary>
		public int SortOrdinal;

        /// <summary>
        /// The revision number for this page
        /// </summary>
        public int RevisionNumber;

        /// <summary>
        /// gets the most recent PageRevisionData .
        /// returns NULL if not found        
        /// </summary>
        /// <returns></returns>
        public CmsPageRevisionData getCurrentRevisionData()
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
        public CmsPageRevisionData getRevisionData(int revisionNumber)
        {
            CmsPageRevisionData[] allRevs;
            string cacheKey = "allRevs" + this.Path;
            // System.Web.HttpContext.Current.Items is a per-request cache: http://msdn.microsoft.com/en-us/magazine/cc163854.aspx#S6
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Items.Contains(cacheKey))
            {
                allRevs = (CmsPageRevisionData[])System.Web.HttpContext.Current.Items[cacheKey];
            }
            else
            {
                allRevs = getAllRevisionData();
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
        public int createNewRevision()
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

        public bool isPageIsLockedForEditing()
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
        public CmsPageLockData getCurrentPageLockData()
        {            
            return (new CmsPageDb()).getPageLockData(this);
        }


        /// <summary>
        /// Locks the page for editing by the current user. When locked, other users can not edit the page. 
        /// Returns NULL if the page could not be locked
        /// </summary>
        /// <returns></returns>
        public CmsPageLockData lockPageForEditing()
        {            
            return (new CmsPageDb()).lockPageForEditing(this);
        }

        /// <summary>
        /// Removes the current page's edit lock.
        /// </summary>
        public void clearCurrentPageLock()
        {            
            (new CmsPageDb()).clearCurrentPageLock(this);
        }

		
		/// <summary>
		/// gets the CmsPage object for the parent page.
		/// If no parent exists, a valid CmsPage is returned, with it's ID set to -1.		
		/// </summary>
		public CmsPage ParentPage
		{
			get
			{				
				return (new CmsPageDb()).FetchPageById(ParentID);
			}
		}

        /// <summary>
        /// All child pages of the current page. Does not take page security or visibility into consideration.
        /// </summary>
        public CmsPage[] AllChildPages
        {
            get
            {
                lock (_childPagesLock)
                {
                    CmsPageDb db = new CmsPageDb();
                    return db.getChildPages(this.ID);
                }
            }
        }

		/// <summary>
		/// The child pages that the current user can access. This method takes security and visibility into consideration.
        /// If visibility and security should not be considered, use <see cref="AllChildPages">. <seealso cref="AllChildPages"/>
		/// </summary>
		public CmsPage[] ChildPages
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
        public string Path
        {
            get { return getPath(CmsContext.currentLanguage); }
        }

        /// <summary>
        /// All Paths for the current page. Each language can give the page a different Path.
        /// </summary>
        public string[] Paths
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
        public string getPath(CmsLanguage forLanguage)
        {

            string cacheKey = "pagePath_" + this.ID + forLanguage.shortCode;

            if (PerRequestCache.CacheContains(cacheKey))
            {
                return (string)PerRequestCache.GetFromCache(cacheKey, "");
            }

            string path = "";
            CmsPage page = this;
            bool first = true;
            while (page.ID != -1)
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
        public int Level
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
        public string[] Urls
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
        public CmsPageSecurityZone Zone
        {
            get 
            {
                if (zoneCached)
                    return cachedZone;

                cachedZone = new CmsZoneDb().fetchByPage(this);
                zoneCached = true;
                return cachedZone;
            }
        }

        /// <summary>
        /// Check if this page is located at the CmsZone boundary
        /// (i.e. an exact record in `zone` table)
        /// </summary>
        public bool isZoneBoundary
        {
            get
            {
                CmsPageSecurityZone z = new CmsZoneDb().fetchByPage(this, false);
                return (z != null) ? true : false;
            }
        }

        /// <summary>
        /// Checks if the current user has write (author) access to this page.
        /// </summary>
        public bool currentUserCanWrite
        {
            get
            {                
                if (CmsContext.currentUserIsSuperAdmin)
                    return true;

                return Zone.canWrite(CmsContext.currentWebPortalUser);
            }
        }

        /// <summary>
        /// Checks if the current user has read access to this page.
        /// </summary>
        public bool currentUserCanRead
        {
            get
            {                
                if (CmsContext.currentUserIsSuperAdmin)
                    return true;

                return Zone.canRead(CmsContext.currentWebPortalUser);
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
		public string Url
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

        public string getUrl(CmsLanguage pageLanguage)
        {
            return CmsContext.getUrlByPagePath(this.getPath(pageLanguage), pageLanguage);
        }

        public string getUrl(CmsUrlFormat urlFormat)
        {
            return CmsContext.getUrlByPagePath(this.Path, urlFormat);
        }

        public string getUrl(CmsUrlFormat urlFormat, CmsLanguage pageLanguage)
        {
            return CmsContext.getUrlByPagePath(this.getPath(pageLanguage), urlFormat, pageLanguage);
        }

        public string getUrl(NameValueCollection pageParams)
        {
            return CmsContext.getUrlByPagePath(this.Path, pageParams);
        }

        public string getUrl(NameValueCollection pageParams, CmsLanguage pageLanguage)
        {
            return CmsContext.getUrlByPagePath(this.getPath(pageLanguage), pageParams, pageLanguage);
        }

        public string getUrl(NameValueCollection pageParams, CmsUrlFormat urlFormat)
        {
            return CmsContext.getUrlByPagePath(this.Path, pageParams, urlFormat);
        }


        public string getUrl(Dictionary<string, string> pageParams)
        {
            return getUrl(pageParams, CmsUrlFormat.RelativeToRoot);
        }

        public string getUrl(Dictionary<string, string> pageParams, CmsLanguage pageLanguage)
        {
            NameValueCollection paramList = new NameValueCollection();
            foreach (string key in pageParams.Keys)
            {
                paramList.Add(key, pageParams[key]);
            }
            return CmsContext.getUrlByPagePath(this.getPath(pageLanguage), paramList, pageLanguage);            
        }

        public string getUrl(Dictionary<string, string> pageParams, CmsUrlFormat urlFormat)
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
        public CmsPageHeadSection HeadSection;

        /// <summary>
        /// The CMS edit menu that allows pages to be edited, deleted, saved, etc.
        /// </summary>
        public CmsPageEditMenu EditMenu;

        /// <summary>
        /// Checks to see if this page is visible for the current user.
        /// This checks for both visibility AND access restrictions (based on Zone security rules).
        /// As such, if isVisibleForCurrentUser is false, the user will not be able to successfully navigate to the page.
        /// </summary>
        public bool isVisibleForCurrentUser
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
			this.ID = -1;
            LanguageInfo = new CmsPageLanguageInfo[0];
			
			CreatedDateTime = DateTime.MinValue;
			LastUpdatedDateTime = DateTime.MinValue;
            LastModifiedBy = "";
            
			_parentId = -1;
			SortOrdinal = -1;
            RevisionNumber = -1;
						
            _childPagesLock = new object();
            HeadSection = new CmsPageHeadSection(this);
            EditMenu = new CmsPageEditMenu(this);
						
		} // constructor
        

        /// <summary>
        /// a function that sees if this page is the currentPage.
        /// </summary>
        /// <returns></returns>
        public bool isSelfSelected()
        {
            if (this.ID == CmsContext.currentPage.ID)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// a recursive function that sees if this page, or a child page of this page is the currentPage.
        /// </summary>
        /// <returns></returns>
        public bool isChildOrSelfSelected()
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
        public bool isChildSelected()
        {

            foreach (CmsPage childPage in this.ChildPages)
            {
                if (childPage.ID == CmsContext.currentPage.ID)
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
        public bool isChildOf(CmsPage possibleParentPage)
        {
            CmsPage currPage = this;
            while (currPage.ParentPage.ID != -1)
            {
                if (currPage.ID == possibleParentPage.ID)
                    return true;
                currPage = currPage.ParentPage;
            }
            return false;
        }

        public bool isParentOrSelfSelected()
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
        public bool isParentSelected()
        {
            CmsPage currPage = this;
            while (currPage.ParentPage.ID != -1)
            {
                if (currPage.ID == CmsContext.currentPage.ID)
                    return true;
                currPage = currPage.ParentPage;
            }
            return false;
        } // childIsSelected

        public bool isSiblingSelected()
        {
            if (ParentPage.ID >= 0)
            {
                foreach (CmsPage sibling in ParentPage.ChildPages)
                {
                    if (sibling.isSelfSelected())
                        return true;
                }
            }
            return false;
        }

        public bool isSiblingOrSiblingChildSelected()
        {
            if (ParentPage.ID >= 0)
            {
                foreach (CmsPage sibling in ParentPage.ChildPages)
                {
                    if (sibling.isChildOrSelfSelected())
                        return true;
                }
            }
            return false;
        }
		
        /// <summary>
        /// Renders the page through the creation of child controls. The TemplateEngine creates and adds these controls.
        /// </summary>
		protected override void CreateChildControls()
		{
            if (this.ID < 0)
                throw new Exception("this page could not be rendered because it has not been initialized from the database.");

            // -- checks if the current user can read the current page. If not authorized, redirect to the Login page.
            bool canRead = this.Zone.canRead(CmsContext.currentWebPortalUser);
            if ( canRead == false && this.Path != CmsConfig.getConfigValue("LoginPath","/_admin/login"))
            {
                NameValueCollection loginParams = new NameValueCollection();
                loginParams.Add("target", this.ID.ToString());
                CmsContext.setEditModeAndRedirect(CmsEditMode.View, CmsContext.getPageByPath(CmsConfig.getConfigValue("LoginPath", "/_admin/login")), loginParams);
            }	
			
            // -- create all placeholders and controls based on the page's template.
			TemplateEngine.CreateChildControls();

            // -- Run the page output filters
            this.Response.Filter = new CmsOutputFilterUtils.PageResponseOutputFilter(Response.Filter, this);
            
		}

        

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
        public string getFormStartHtml(string formId, string onSubmit, string style, string actionUrl, string method)
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

        public string getFormStartHtml(string formId, string onSubmit)
        {
            return getFormStartHtml(formId, onSubmit, "", "", "post");
        }

        public string getFormStartHtml(string formId)
        {
            return getFormStartHtml(formId, "", "", "", "post");            
        }
        

		/// <summary>
		/// gets the Html to close the form started with getFormStartHtml(). 
        /// Note: IE has an issue that you can not have nested form tags (&ltform&gt &ltform&gt &lt/form&gt  &lt/form&gt is invalid and won't work!!!)
		/// </summary>
		/// <returns></returns>
		public string getFormCloseHtml(string formId)
		{
            if (String.Compare(formId, _startedFormId) != 0)
                return "";
            
            string html = "</form>";
            _startedFormId = "";
			return html;						
		}
                

		/// <summary>
		/// Gets the value stored in a placeholder on this page.
		/// This function is mostly used for aggregator controls. Placeholders should
		/// retrieve their values directly (without using this method).
		/// </summary>
		/// <exception cref="System.Exception">thrown when placeholderType is invalid</exception>
		/// <param name="placeholderType">the placeholder type to get values for</param>
		/// <param name="identifier">the placeholder identifier to get the value for. Use <see cref="getPlaceholderIdentifiers">getPlaceholderIdentifiers()</see> to get the valid identifiers.</param>
		/// <returns>the value found for the given placeholderType and identifier. Returns an empty string (string.empty) if the identifier was not found in the system.</returns>
        public string renderPlaceholderToString(CmsPlaceholderDefinition phDef, CmsLanguage language)
		{			
            return PlaceholderUtils.renderPlaceholderToString(this, language, phDef);
						
		}

        public string renderAllPlaceholdersToString(CmsLanguage forLanguage)
        {
            CmsPlaceholderDefinition[] phDefs = getAllPlaceholderDefinitions();
            StringBuilder ret = new StringBuilder();

            foreach (CmsPlaceholderDefinition phDef in phDefs)
            {
                ret.Append(renderPlaceholderToString(phDef, forLanguage));
            }

            return ret.ToString();
        }

        public string renderPlaceholdersToString(string placeholderTypeToRender, CmsLanguage forLanguage)
        {
            CmsPlaceholderDefinition[] phDefs = getPlaceholderDefinitions(placeholderTypeToRender);
            StringBuilder ret = new StringBuilder();

            foreach (CmsPlaceholderDefinition phDef in phDefs)
            {
                ret.Append(renderPlaceholderToString(phDef, forLanguage));
            }

            return ret.ToString();
        }

        

        /// <summary>
        /// returns an array of CmsPlaceholderDefinitions that are definied in the template for this page.        
        /// <br />WARNING!!! if the template file for this page doesn't exist, this function will throw an Exception!!!
        /// </summary>
        /// <returns></returns>
        public CmsPlaceholderDefinition[] getAllPlaceholderDefinitions()
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
        public CmsPlaceholderDefinition[] getPlaceholderDefinitions(string placeholderType)
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
        public string[] getAllPlaceholderNames()
        {
            try
            {
                CmsPlaceholderDefinition[] defs = this.getAllPlaceholderDefinitions();
                List<string> ret = new List<string>();
                foreach (CmsPlaceholderDefinition phDef in defs)
                {
                    if (ret.IndexOf(phDef.PlaceholderType) < 0)
                        ret.Add(phDef.PlaceholderType);
                } // foreach
                return ret.ToArray();
            }
            catch { }
            return new string[0];
        }

        public bool hasPlaceholder(string placeholderType)
        {
            string[] allPhNames = getAllPlaceholderNames();
            return (StringUtils.IndexOf(allPhNames, placeholderType, StringComparison.CurrentCultureIgnoreCase) > -1);
        }

        /// <summary>
        /// gets a list of control names used in the page's template.
        /// if a control is referenced more than once, it will be found multiple times in the return array.
        /// <br />WARNING!!! if the template file for this page doesn't exist, this function will throw an Exception!!!
        /// </summary>
        /// <returns></returns>
        public string[] getAllControlPaths()
        {
            return TemplateEngine.getAllControlPaths();            
        }
       

        /// <summary>
        /// get a list of all valid revision numbers for this page, sorted so that the oldest Rev# is first ([0])
        /// </summary>
        /// <returns></returns>
        public int[] getAllRevisionNumbers()
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
        public CmsPageRevisionData[] getAllRevisionData()
        {
            //@@TODO: getAllRevisionData() should be cached in memory
            CmsPageDb db = new CmsPageDb();
            return db.getAllRevisionData(this);
        }

        public bool revertToRevision(int oldRevisionNumberToMakeLive)
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
                if (oldPage.ID < 0)
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
        		

        public CmsTemplateEngine TemplateEngine
        {
            get
            {
                CmsTemplateEngineVersion templateVersion = CmsConfig.TemplateEngineVersion;
                string cacheKey = "templateEngine_" + Enum.GetName(typeof(CmsTemplateEngineVersion), templateVersion) + this.TemplateName + this.ID.ToString();

                if (PerRequestCache.CacheContains(cacheKey))
                    return (CmsTemplateEngine)PerRequestCache.GetFromCache(cacheKey, null);

                CmsTemplateEngine engine;
                switch (templateVersion)
                {
                    case CmsTemplateEngineVersion.v1: engine = new TemplateEngine.TemplateEngineV1(TemplateName, this); break;
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
        public Dictionary<int, CmsPage> getLinearizedPages()
        {
            Dictionary<int, CmsPage> ret = new Dictionary<int, CmsPage>();

            if (!this.isVisibleForCurrentUser)
                return ret;

            ret.Add(this.ID, this);            

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
			ret.Add(parentPage.ID, parentPage);
            
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
        public bool setLastUpdatedDateTimeToNow()
        {
            CmsPageDb db = new CmsPageDb();
            return db.setLastUpdatedTime(this, DateTime.Now);

        }

        /// <summary>
        /// saves the new page.sortOrdinal to the database
        /// </summary>
        /// <param name="newOrdinalValue"></param>
        /// <returns></returns>
        public bool setSortOrdinal(int newOrdinalValue)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateSortOrdinal(this, newOrdinalValue);
        }

        /// <summary>
        /// saves the new page.Title to the database.
        /// </summary>
        /// <param name="newTitle"></param>
        /// <returns></returns>
        public bool setTitle(string newTitle, CmsLanguage forLanguage)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateTitle(this, newTitle, forLanguage);
        }

        /// <summary>
        /// saves the new page.MenuTitle to the database.
        /// </summary>
        /// <param name="newTitle"></param>
        /// <returns></returns>
        public bool setMenuTitle(string newMenuTitle, CmsLanguage forLanguage)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateMenuTitle(this, newMenuTitle, forLanguage);
        }

        /// <summary>
        /// saves the new page.SearchEngineDescription to the database.
        /// </summary>
        /// <param name="newTitle"></param>
        /// <returns></returns>
        public bool setSearchEngineDescription(string newSearchEngineDescription, CmsLanguage forLanguage)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateSearchEngineDescription(this, newSearchEngineDescription, forLanguage);
        }

        /// <summary>
        /// saves the new page.ParentPageId to the database.
        /// </summary>
        /// <param name="newTitle"></param>
        /// <returns></returns>
        public bool setParentPage(CmsPage newParentPage)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateParentPage(this, newParentPage);
        }

        /// <summary>
        /// saves the new page.Name to the database
        /// </summary>
        /// <param name="newName"></param>
        /// <returns></returns>
        public bool setName(string newName, CmsLanguage forLanguage)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateName(this, newName, forLanguage);
        }

        /// <summary>
        /// saves the page.TemplateName to the database
        /// </summary>
        /// <param name="newTemplateName"></param>
        /// <returns></returns>
        public bool setTemplateName(string newTemplateName)
        {
            CmsPageDb db = new CmsPageDb();
            return db.updateTemplateName(this, newTemplateName);
        }

        /// <summary>
        /// Mark this page as being deleted in the database.
        /// If this page is a master page for any placeholders, the master pages are updated accordingly.
        /// </summary>
        /// <returns></returns>
        public bool DeleteThisPage()
        {
            CmsPageDb db = new CmsPageDb();
            return db.deletePage(this);
        }

        /// <summary>
        /// Update the page's .ShowInMenu property in the database.
        /// </summary>
        /// <param name="newIndicator"></param>
        /// <returns></returns>
        public bool UpdateShowInMenuFlag(bool newIndicator)
        {            
            return new CmsPageDb().updateShowInMenuIndicator(this, newIndicator);
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
            return ArrayContainsPageId(haystack, needle.ID);
        }

        public static bool ArrayContainsPageId(CmsPage[] haystack, int needlePageId)
        {
            foreach (CmsPage p in haystack)
            {
                if (p.ID == needlePageId)
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

            /// <summary>
            /// provides database functions for the CmsPage object
            /// </summary>
            public CmsPageDb()
                : base(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"])
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
            /// Get cms pages by template name (case not sensitive)
            /// </summary>
            /// <param name="templateName"></param>
            /// <returns></returns>
            public CmsPage[] FetchPagesByTemplateName(string templateName)
            {
                StringBuilder sql = new StringBuilder("select p.pageId, l.langCode, l.name, l.title, l.menuTitle, l.searchEngineDescription, p.showInMenu, p.template, p.parentPageId, p.SortOrdinal, p.CreatedDateTime, p.LastUpdatedDateTime, p.LastModifiedBy, p.RevisionNumber");
                sql.Append(" from pages p left join pagelanginfo l on (p.pageid = l.pageid)");
                sql.Append(" where p.Deleted Is Null");
                sql.Append(" and p.template like '" + dbEncode(templateName) + "'");
                sql.Append(" order by p.pageId;");
                DataSet ds = this.RunSelectQuery(sql.ToString());

                if (hasRows(ds) == false)
                    return new CmsPage[0];

                CmsPage[] pages = pagesFromRows(ds.Tables[0].Rows);
                if (pages.Length == 0)
                    return new CmsPage[0];

                return pages;
            }

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

                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT into pages ( showInMenu, template, parentPageId, sortOrdinal, LastUpdatedDateTime, CreatedDateTime, LastModifiedBy, RevisionNumber) VALUES ");
                sql.Append("(");
                sql.Append(Convert.ToInt32(newPage.ShowInMenu) + ", ");
                sql.Append("'" + this.dbEncode(newPage.TemplateName) + "', ");
                sql.Append(parentId + ", ");
                sql.Append(newPage.SortOrdinal.ToString() + ", ");
                sql.Append("NOW(), NOW(), ");
                sql.Append("'" + dbEncode(newPage.LastModifiedBy) + "', ");
                sql.Append(newPage.RevisionNumber + " ");
                sql.Append( "); ");

                int newId = this.RunInsertQuery(sql.ToString());
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
                revSql += page.ID.ToString() + ", ";
                revSql += newRevisionNumber.ToString() + ", ";
                revSql += " NOW(), ";
                revSql += "'" + dbEncode(lastModifiedBy) + "'";
                revSql += "); ";

                // use RunUpdateQuery because there's no AutoInc column for PageRevisionData
                int numAffected = RunUpdateQuery(revSql);
                if (numAffected <= 0)
                    return -1;

                // -- update the pages table to have the most up-to-date data
                string pageUpdateSql = "update pages set RevisionNumber = " + newRevisionNumber.ToString() + " where pageid = " + page.ID.ToString() + " ;";
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

            public bool updateShowInMenuIndicator(CmsPage page, bool newIndicator)
            {
                if (page.ID == -1)
                    return false; // nothing to update

                string sql = "UPDATE pages SET showInMenu=" + newIndicator.ToString() + " WHERE pageid=" + page.ID.ToString() + ";";

                int numAffected = this.RunUpdateQuery(sql);
                if (numAffected > 0 && page.setLastUpdatedDateTimeToNow())
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
        #endregion

    }
}
