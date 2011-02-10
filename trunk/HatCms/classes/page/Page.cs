using System;
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
				return (new CmsPageDb()).getPage(ParentID);
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
        /// Derive the Zone by the page ID
        /// </summary>
        public CmsZone Zone
        {
            get { return new CmsZoneDb().fetchByPage(this); }
        }

        /// <summary>
        /// Check if this page is located at the CmsZone boundary
        /// (i.e. an exact record in `zone` table)
        /// </summary>
        public bool isZoneBoundary
        {
            get
            {
                CmsZone z = new CmsZoneDb().fetchByPage(this, false);
                return (z != null) ? true : false;
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
                if (! CmsContext.currentUserCanAuthor && this.hasPlaceholder("PageRedirect"))
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
        /// This only checks for visibility, not access restrictions.
        /// As such, if isVisibleForCurrentUser is false, the user may still be able to successfully navigate to the page.
        /// </summary>
        public bool isVisibleForCurrentUser
        {
            get
            {
                if (CmsContext.currentUserIsSuperAdmin)
                    return true;
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
        /// Returns a dictionary of placeholder identifiers (an integer array) for child pages that have a particular placeholder.
        /// Only pages that have the placeholderType defined are returned. (If a child page does not have the placeholderType, it is not included in the returned dictionary.)
        /// <br />WARNING!!! if the template file for the child page doesn't exist, this function will throw an Exception!!!
        /// </summary>
        /// <param name="placeholderType">The placeholder type to find in child pages.</param>
        /// <returns></returns>
        public Dictionary<CmsPage, CmsPlaceholderDefinition[]> getPlaceholderDefinitionsForChildPages(string placeholderType)
        {
            Dictionary<CmsPage, CmsPlaceholderDefinition[]> ret = new Dictionary<CmsPage, CmsPlaceholderDefinition[]>();
            foreach (CmsPage childPage in this.ChildPages)
            {
                CmsPlaceholderDefinition[] phDefs = childPage.getPlaceholderDefinitions(placeholderType);
                if (phDefs.Length > 0)
                {
                    ret[childPage] = phDefs;
                }
            } // foreach
            return ret;
        }

        /// <summary>
        /// Returns a dictionary of placeholder identifiers (an integer array) for child pages that have a particular placeholder.
        /// Only pages that have the placeholderType defined are returned. (If a child page does not have the placeholderType, it is not included in the returned dictionary.)
        /// <br />WARNING!!! if the template file for the child page doesn't exist, this function will throw an Exception!!!
        /// </summary>
        /// <param name="placeholderType">The placeholder type to find in child pages.</param>
        /// <returns></returns>
        public Dictionary<CmsPage, CmsPlaceholderDefinition[]> getChildPagePlaceholderDefinitions(string placeholderType)
        {
            return getPlaceholderDefinitionsForChildPages(placeholderType);
        }

        /// <summary>
        /// Returns a list of Child Pages that contain the specified placeholderType.
        /// </summary>
        /// <param name="placeholderType"></param>
        /// <returns></returns>
        public CmsPage[] getChildPagesWithPlaceholder(string placeholderType)
        {
            Dictionary<CmsPage, CmsPlaceholderDefinition[]> dict = getPlaceholderDefinitionsForChildPages(placeholderType);
            List<CmsPage> ret = new List<CmsPage>(dict.Keys);
            return ret.ToArray();
        }

        /// <summary>
        /// returns an array of CmsPlaceholderDefinitions that are definied in the template for this page.        
        /// <br />WARNING!!! if the template file for this page doesn't exist, this function will throw an Exception!!!
        /// </summary>
        /// <returns></returns>
        public CmsPlaceholderDefinition[] getAllPlaceholderDefinitions()
        {
            // -- this function is used all over the place, so let's cache it's results
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
            CmsPlaceholderDefinition[] all = TemplateEngine.getAllPlaceholderDefinitions();
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
                        bool b = PlaceholderUtils.revertToRevision(phType, oldPage, this, phInfo[phType].ToArray(), lang);
                        if (!b)
                            ret = false;
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
        /// in the format obj[pageId] => CmsPage. Only returns pages that the current user can access.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, CmsPage> getLinearizedPages()
        {
            Dictionary<int, CmsPage> ret = new Dictionary<int, CmsPage>();

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
        public bool deleteThisPage()
        {
            CmsPageDb db = new CmsPageDb();
            return db.deletePage(this);
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
            
        } // SortPagesByLastModifiedDate 

        public static bool ArrayContainsPage(CmsPage[] haystack, CmsPage needle)
        {
            foreach (CmsPage p in haystack)
            {
                if (p.ID == needle.ID)
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

	}
}
