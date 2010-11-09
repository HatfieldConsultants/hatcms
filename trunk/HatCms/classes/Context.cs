using System;
using System.Security.Principal;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using Hatfield.Web.Portal;
using Hatfield.Web.Portal.Authentication;
using System.Configuration;
using System.Globalization;
using System.Threading;

namespace HatCMS
{
	/// <summary>
	/// This object provides access to the entire page handling process including 
	/// page retrieval, user authentication, system configuration, and mode switching
	/// </summary>
	public class CmsContext
	{
        /// <summary>
        /// gets the HomePage CmsPage object
        /// </summary>
        public static CmsPage HomePage
        {
            get
            {
                string cacheKey = "homePage";

                if (PerRequestCache.CacheContains(cacheKey))
                {
                    return (CmsPage)PerRequestCache.GetFromCache(cacheKey, new CmsPage());
                }

                CmsPageDb db = new CmsPageDb();
                int id = db.getHomePageId();
                CmsPage ret = db.getPage(id);

                PerRequestCache.AddToCache(cacheKey, ret);

                return ret;

            }
        }
        
        /// <summary>
		/// gets the page that the user requested to view
		/// </summary>
		public static CmsPage currentPage
		{
            get
            {
                string cacheKey = "currentPage"+DateTime.Now.Ticks;
                if (PerRequestCache.CacheContains(cacheKey))
                {
                    return (CmsPage)PerRequestCache.GetFromCache(cacheKey, new CmsPage());
                }                
                
                string path = getRequestedPagePathFromForm();
                                
                // -- get page by revision
                if (CmsContext.currentUserCanAuthor && CmsContext.currentEditMode == CmsEditMode.View)
                {
                    int revisionNumberToDisplay = PageUtils.getFromForm("revNum", -1);
                    if (revisionNumberToDisplay >= 0)
                    {
                        CmsPage revToRet = getPageByPath(path, revisionNumberToDisplay);
                        PerRequestCache.AddToCache(cacheKey, revToRet);
                        
                        return revToRet;
                    }
                }
                
                // -- default view by path
                CmsPage ret = getPageByPath(path);
                
                PerRequestCache.AddToCache(cacheKey, ret);
                return ret;
            

            }
		}

        /// <summary>
        /// Gets the requested page path from either the query string or the posted form.
        /// The path returned NEVER includes the currentLanguage's shortCode.
        /// No verification of the page path's existance is done.
        /// </summary>
        /// <returns></returns>
        public static string getRequestedPagePathFromForm()
        {            
            string pParam = PageUtils.getFromForm("p", "");
                
            // -- check if multiple "p" form variables have been specified (which is an error).
            string[] pParamArray = PageUtils.getFromForm("p");
            if (pParamArray.Length > 1)
            {                    
                foreach(string p in pParamArray)
                {
                    if (pParam != pParamArray[0])
                        throw new ArgumentException("Error: there was a form submission error - the 'p' form variable has multiple values, and is reserved for the CMS' page path");
                }
                pParam = pParamArray[0];
            }


            // -- take off the first "/"
			if (pParam.StartsWith("/"))
				pParam = pParam.Substring(1,pParam.Length-1);            
            
            string[] path_parts = pParam.Split(new char[] {'/'});

            // -- remove the language code if configured & specified.
            if (CmsConfig.UseLanguageShortCodeInPageUrls && path_parts.Length >= 1 && String.Compare(path_parts[0], currentLanguage.shortCode, true) == 0)
            {
                return pParam.Substring(currentLanguage.shortCode.Length);
            }
            else
            {
                if (!pParam.StartsWith("/"))
				    pParam = "/"+pParam;
                return pParam;
            }            
        } // getRequestedPagePathFromForm


        

        
        private static CmsPageCache _pageCacheObject = null;
        
        /// <summary>
        /// Gets the current <see cref="PageCache"/>instance. Note: for internal use only. 
        /// </summary>
        /// <returns></returns>
        public static CmsPageCache getPageCache()
        {                 
            string cacheKey = "hatCmsPageCache";

            // System.Web.HttpContext.Current.Items is a per-request cache: http://msdn.microsoft.com/en-us/magazine/cc163854.aspx#S6
            if (System.Web.HttpContext.Current != null)
            {
                if (!System.Web.HttpContext.Current.Items.Contains(cacheKey))
                {
                    System.Web.HttpContext.Current.Items.Add(cacheKey, new CmsPageCache());
                }
                return (CmsPageCache)System.Web.HttpContext.Current.Items[cacheKey];
            }
            else
            {
                if (_pageCacheObject == null)
                    _pageCacheObject = new CmsPageCache();
                return _pageCacheObject;
            }                 
     
        }

        /// <summary>
        /// The language for the currently requested page. If the language can not be determined, the first configured language is used
        /// The system must always have at least one language configured.         
        /// </summary>
        public static CmsLanguage currentLanguage
        {
            get
            {
                // -- note: there's always at least one Language in the CmsConfig.Language array
                //          there's no need to do Per-request caching of .currentLanguage because CmsConfig.Languages does that already.
                if (CmsConfig.Languages.Length == 1)
                    return CmsConfig.Languages[0];
                else
                {
                    string path = PageUtils.getFromForm("p", ""); // do not use getRequestedPagePathFromForm
                    string[] pathParts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (pathParts.Length > 0)
                    {
                        string langCodeToFind = pathParts[0];
                        CmsLanguage l = CmsLanguage.GetFromHaystack(langCodeToFind, CmsConfig.Languages);
                        if (l.isValidLanguage)
                            return l;
                    }

                    return CmsConfig.Languages[0];
                }
            } // get
        }        

        /// <summary>
        /// Obtain the current short date formation (e.g. DateTime.ToString("d"))
        /// </summary>
        /// <returns></returns>
        public static string currentShortDateFormat()
        {
            return Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern.ToUpper();
        }


		/// <summary>
		/// gets the currently logged in user.
		/// </summary>
        private static System.Security.Principal.IPrincipal currentUser
		{
			get
			{
                if (!System.Web.HttpContext.Current.Items.Contains("authProcessed"))
                {
                    WebPortalAuthentication.processAuthenticateRequest(System.Web.HttpContext.Current);
                    System.Web.HttpContext.Current.Items.Add("authProcessed",  true);
                }
                System.Web.HttpContext.Current.Items.Remove("authProcessed");
                return System.Web.HttpContext.Current.User;
			}
		}

        /// <summary>
        /// Returns true if the currently logged in user has author level privledges.
        /// </summary>
        public static bool currentUserCanAuthor
        {
            get
            {
                if (currentUserIsSuperAdmin)
                    return true;
                else if (CmsContext.currentWebPortalUser != null && CmsContext.currentWebPortalUser.inRole(CmsConfig.getConfigValue("AuthorAccessUserRole","Author")))
                    return true;
                return false;
            }
        }

        /// <summary>
        /// returns true if the currently logged in user is logged in.
        /// </summary>
        public static bool currentUserIsLoggedIn
        {
            get
            {
                if (currentUserCanAuthor || currentUserIsSuperAdmin)
                    return true;
                if (CmsContext.currentWebPortalUser != null && CmsContext.currentWebPortalUser.inRole(CmsConfig.getConfigValue("LoginUserRole", Guid.NewGuid().ToString())))
                    return true;
                return false;
            }
        }

        
        /// <summary>
        /// returns true if the currently logged in user has super-admin level privledges.
        /// </summary>
        public static bool currentUserIsSuperAdmin
        {
            get
            {
                if (CmsContext.currentWebPortalUser != null && CmsContext.currentWebPortalUser.inRole(CmsConfig.getConfigValue("AdminUserRole", "Administrator")))
                    return true;
                return false;
            }
        }
        
        /// <summary>
        /// the current Web Portal User. If not authenticated (not logged in), is set to NULL        
        /// </summary>
        public static WebPortalUser currentWebPortalUser
        {
            get
            {
                try
                {
                    // -- we cache the currentWebPortal user so that we don't go to the database
                    if (System.Web.HttpContext.Current.Items.Contains("currentWebPortalUser"))
                        return System.Web.HttpContext.Current.Items["currentWebPortalUser"] as WebPortalUser;

                    if (currentUser != null && currentUser.Identity.IsAuthenticated)
                    {
                        WebPortalUser u = WebPortalUser.FetchUser(currentUser.Identity.Name, CmsPortalApplication.GetInstance());
                        System.Web.HttpContext.Current.Items.Add("currentWebPortalUser", u);
                        return u;
                    }
                    else
                        return null;
                }
                catch
                { }
                return null;
            }
        }
                

        /// <summary>
		/// gets the currently executing ApplicationPath. Returned value always ends in a trailing '/'
		/// </summary>
		public static string ApplicationPath
		{
			get
			{
                return PageUtils.ApplicationPath;                
			}
		}

        
        /// <summary>
        /// the un-validated page version number to display.
        /// returns -1 to view the page's current version
        /// </summary>
        public static int requestedPageVersionNumberToView
        {
            get
            {
                return PageUtils.getFromForm("revNum", -1);
            }
        }

        /// <summary>
        /// Checks if the user is currently viewing a previous version of the current page.
        /// </summary>
        public static bool currentlyViewingPreviousPageVersion
        {
            get
            {
                if (CmsContext.currentUserCanAuthor && CmsContext.currentEditMode == CmsEditMode.View)
                {
                    int revisionNumberToDisplay = requestedPageVersionNumberToView;
                    if (revisionNumberToDisplay >= 0)
                    {
                        CmsPageRevisionData revData = currentPage.getCurrentRevisionData();
                        if (revData != null && revData.RevisionNumber > revisionNumberToDisplay)
                            return true;
                    }
                }
                return false;
            }
        }
       
        /// <summary>
        /// The Form name that tracks the <see cref="currentEditMode"/> currentEditMode
        /// </summary>
        public static string EditModeFormName = "hatCmsEditMode";
        
        /// <summary>
        /// gets the current page's edit mode. The current edit mode is stored in the URL or form with the name EditModeFormName.        
        /// </summary>
        public static CmsEditMode currentEditMode
        {
            get
            {
                string formVal = PageUtils.getFromForm(EditModeFormName, "");
                if (currentUserCanAuthor && formVal == "1")
                {                    
                    return CmsEditMode.Edit;
                }
                return CmsEditMode.View;
            }
        } // currentEditMode

		/// <summary>
		/// sets the currentEditMode and redirects the user to the current page
		/// </summary>
		/// <param name="newEditMode"></param>
		public static void setEditModeAndRedirectToCurrent(CmsEditMode newEditMode)
		{
            setEditModeAndRedirect(newEditMode, currentPage);            
		}

		/// <summary>
		/// sets the currentEditMode and redirects to another CmsPage. if the newEditMode is Edit, the target page is locked for editing.
        /// If the newEditMode is View, the target page is unlocked for editing.
        /// Note: targetCmsPagePath should never contain the language code 
		/// </summary>
		/// <param name="newEditMode"></param>
        /// <param name="targetPage">The page to redirect to</param>
		public static void setEditModeAndRedirect(CmsEditMode newEditMode, CmsPage targetPage)
		{
            setEditModeAndRedirect(newEditMode, targetPage, new NameValueCollection());
		}

        /// <summary>
        /// sets the currentEditMode and redirects to another CmsPage. if the newEditMode is Edit, the target page is locked for editing.
        /// If the newEditMode is View, the target page is unlocked for editing.        
        /// This function ends the current Request, so nothing will execute after it.
        /// </summary>
        /// <param name="newEditMode"></param>
        /// <param name="targetPage">The page to redirect to</param>
        /// <param name="paramList"></param>
        public static void setEditModeAndRedirect(CmsEditMode newEditMode, CmsPage targetPage, NameValueCollection paramList)
        {            
            List<string> paramListKeys = new List<string>();
            foreach (string k in paramList.Keys)
                paramListKeys.Add(k);
            // -- only allow authors to go to CmsEditMode.Edit
            if (newEditMode == CmsEditMode.Edit && CmsContext.currentUserCanAuthor)
            {
                if (targetPage.lockPageForEditing() != null)
                {
                    // only add parameter if it doesn't exist already
                    if (StringUtils.IndexOf(paramListKeys.ToArray(), EditModeFormName, StringComparison.CurrentCultureIgnoreCase) < 0)
                        paramList.Add(EditModeFormName, "1");
                    else
                        paramList[EditModeFormName] = "1";                    
                }

            }
            else // View
            {
                targetPage.clearCurrentPageLock();
                // remove the edit mode parameter (if it exists)
                if (StringUtils.IndexOf(paramListKeys.ToArray(), EditModeFormName, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    paramList.Remove(EditModeFormName);                
            }

            // -- bust through the cache if user is logged on
            if (CmsContext.currentUserIsLoggedIn)
            {
                paramList.Add("nocache", DateTime.Now.Ticks.ToString());
            }


            System.Web.HttpContext context = System.Web.HttpContext.Current;
            // note: do not use targetPage.getUrl(paramList); (it causes an infinite loop)
            string url = getUrlByPagePath(targetPage.Path, paramList);

            context.Response.Redirect(url, true);
        }

        /// <summary>
        /// Adjust the Culture Info and let DateTime.ToString() to provide translated text.
        /// </summary>
        /// <param name="lang">Language Code</param>
        private static void setCurrentCultureInfo(CmsLanguage lang)
        {
            CmsLanguage[] langArray = CmsConfig.Languages;
            CultureInfo[] cultureArray = CmsConfig.CultureInformation;

            int index = CmsLanguage.IndexOf(lang.shortCode, langArray);
            CultureInfo targetCulture = cultureArray[index];

            Thread.CurrentThread.CurrentCulture = targetCulture;
            Thread.CurrentThread.CurrentUICulture = targetCulture;
        }

		/// <summary>
		/// This function is called whenever a new request is started.
		/// </summary>
        public static void StartNewRequest()
		{            
            // -- clear the static page database cache variables on every new request            
            CmsPageCache c = getPageCache();
			c.Clear();     
            
            // -- set the current thread's culture info
            setCurrentCultureInfo(currentLanguage);
		}

        /// <summary>
        /// This function is called by the Global.asax's Application_Start_Session function.
        /// </summary>
        public static void Application_Start_Session()
        {
            
        }
		

        /// <summary>
		/// gets a CmsPage from the database with the given path.
        /// The pagePath should NEVER includes the language shortCode.
		/// Returns an empty CmsPage (with id = -1) if the page was not found.
		/// </summary>
        /// <param name="pagePath">The path of the page to find in the pagePathLanguage</param>
        /// <param name="pagePathLanguage">The language to search to find the pagePath</param>
		/// <returns>the CmsPage with the given path. If not found, an empty CmsPage (with id = -1) is returned</returns>
        public static CmsPage getPageByPath(string pagePath, CmsLanguage pagePathLanguage)
        {
            CmsPageDb db = new CmsPageDb();
            return db.getPage(pagePath, pagePathLanguage);
        }

		/// <summary>
		/// gets a CmsPage from the database with the given path in the currentLanguage.
        /// The pagePath should NEVER includes the language shortCode.
		/// Returns an empty CmsPage (with id = -1) if the page was not found.
		/// </summary>
        /// <param name="pagePath"></param>
		/// <returns>the CmsPage with the given path. If not found, an empty CmsPage (with id = -1) is returned</returns>
		public static CmsPage getPageByPath(string pagePath)
		{
            string cacheKey = "page" + pagePath.Trim().ToLower();
            
            if (PerRequestCache.CacheContains(cacheKey))
            {
                return (CmsPage)PerRequestCache.GetFromCache(cacheKey, new CmsPage());
            }

            
            CmsPageDb db = new CmsPageDb();
            CmsPage ret = db.getPage(pagePath);
            
            PerRequestCache.AddToCache(cacheKey, ret);
            return ret;
		}

        /// <summary>
        /// gets a CmsPage from the database with the given path in the currentLanguage.
        /// The pagePath should NEVER includes the language shortCode.
        /// Returns an empty CmsPage (with id = -1) if the page was not found.
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="pageRevisionNumber">the revision number of the page to get</param>
        /// <returns>the CmsPage with the given path. If not found, an empty CmsPage (with id = -1) is returned</returns>
        public static CmsPage getPageByPath(string pagePath, int pageRevisionNumber)
        {
            CmsPageDb db = new CmsPageDb();
            if (pageRevisionNumber >= 0)
                return db.getPage(pagePath, pageRevisionNumber);
            return
                db.getPage(pagePath);
        }

		/// <summary>
		/// gets a CmsPage from the database with the given ID.
		/// Returns an empty CmsPage (with id = -1) if the page was not found.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>the CmsPage with the given PageId. If not found, an empty CmsPage (with id = -1) is returned</returns>
		public static CmsPage getPageById(int id)
		{
			CmsPageDb db = new CmsPageDb();
			return db.getPage(id);
		}

        /// <summary>
        /// Checks if a page with the given pageId exists in the database.
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public static bool pageExists(int pageId)
        {
            CmsPageDb db = new CmsPageDb();
            CmsPage p = db.getPage(pageId);
            if (p.ID >= 0)
                return true;
            return false;
        }

		/// <summary>
		/// checks if a page with the given path exists in the currentLanguage.
        /// The pagePath should NEVER includes the language shortCode.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
        public static bool pageExists(string pagePath)
		{
            try
            {
                CmsPage p = getPageByPath(pagePath);
                if (p.ID < 0)
                    return false;
                return true;
            }
            catch
            { }
            return false;
		}

		/// <summary>
		/// checks if a child page exists for a parent page in any of the system's languages.
		/// </summary>
		/// <param name="parentPageId"></param>
		/// <param name="childName"></param>
		/// <returns></returns>
		public static bool childPageWithNameExists(int parentPageId, string childName)
		{
			CmsPageDb db = new CmsPageDb();
			CmsPage parentPage = db.getPage(parentPageId);
			if (parentPage.ID < 0)
				return false;

            foreach (CmsPage p in parentPage.ChildPages)
            {
                if (StringUtils.IndexOf(p.Names, childName, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    return true;
            }
			return false;
		}

        /// <summary>
        /// returns true if any of the .name fields in the specified pageLangInfos matches ChildPage.names, returns TRUE
        /// </summary>
        /// <param name="parentPageId"></param>
        /// <param name="pageLangInfos"></param>
        /// <returns></returns>
        public static bool childPageWithNameExists(int parentPageId, CmsPageLanguageInfo[] pageLangInfos)
        {
            CmsPageDb db = new CmsPageDb();
            CmsPage parentPage = db.getPage(parentPageId);
            if (parentPage.ID < 0)
                return false;
            
            foreach (CmsPage p in parentPage.ChildPages)
            {
                foreach (CmsPageLanguageInfo langInfo in pageLangInfos)
                {
                    string nameToFind = langInfo.name;
                    CmsLanguage pageLanguage = CmsLanguage.GetFromHaystack(langInfo.languageShortCode, CmsConfig.Languages);
                    if (String.Compare(p.getName(pageLanguage), nameToFind, true) == 0)
                        return true;
                }
            }
            return false;
        }


        /// <summary>
        /// returns the user accessible URL for a pagePath in the currentLanguage.
        /// The pagePath should NEVER includes the language shortCode.
        /// Note: does not check if the path exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string getUrlByPagePath(string pagePath)
        {
            return getUrlByPagePath(pagePath, CmsUrlFormat.RelativeToRoot, currentLanguage);
        }

        /// <summary>
        /// returns the user accessible URL for a page in the given PageLanguage.
        /// The pagePath should NEVER includes the language shortCode.
        /// Note: does not check if the path exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string getUrlByPagePath(string pagePath, CmsLanguage pageLanguage)
        {
            return getUrlByPagePath(pagePath, CmsUrlFormat.RelativeToRoot, pageLanguage);
        }

        /// <summary>
        /// returns the user accessible URL for a page.
        /// The pagePath should NEVER includes the language shortCode.
        /// Note: does not check if the path exists.
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="urlFormat"></param>
        /// <returns></returns>
        public static string getUrlByPagePath(string pagePath, CmsUrlFormat urlFormat)
        {
            return getUrlByPagePath(pagePath, urlFormat, currentLanguage);
        }

        /// <summary>
        /// returns the user accessible URL for a page
        /// The pagePath should NEVER includes the language shortCode.
        /// Note: does not check if the path exists.
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static string getUrlByPagePath(string pagePath, NameValueCollection paramList)
        {
            return getUrlByPagePath(pagePath, paramList, CmsUrlFormat.RelativeToRoot, currentLanguage);
        }

        /// <summary>
        /// returns the user accessible URL for a page
        /// The pagePath should NEVER includes the language shortCode.
        /// Note: does not check if the path exists.
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static string getUrlByPagePath(string pagePath, NameValueCollection paramList, CmsLanguage pageLanguage)
        {
            return getUrlByPagePath(pagePath, paramList, CmsUrlFormat.RelativeToRoot, pageLanguage);
        }

        /// <summary>
        /// returns the user accessible URL for a page
        /// The pagePath should NEVER includes the language shortCode.
        /// Note: does not check if the path exists.
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>		
        public static string getUrlByPagePath(string pagePath, NameValueCollection paramList, CmsUrlFormat urlFormat)
        {
            return getUrlByPagePath(pagePath, paramList, urlFormat, currentLanguage);
        }

        /// <summary>
        /// returns the user accessible URL for a page.
        /// The pagePath should NEVER includes the language shortCode.
        /// Note: does not check if the path exists.
        /// </summary>
        /// <param name="pagePath"></param>
        /// <returns></returns>
        public static string getUrlByPagePath(string pagePath, CmsUrlFormat urlFormat, CmsLanguage pageLanguage)
        {                        
            

            // -- replace spaces in the path
            string spaceReplacementChar = "+";
            if (spaceReplacementChar != "")
            {
                pagePath = pagePath.Replace(" ", spaceReplacementChar);
            }


            string url = "";

            // use the language path in the URL if configured to do so.
            string langPath = "";
            if (CmsConfig.UseLanguageShortCodeInPageUrls)
                langPath = "/" + pageLanguage.shortCode.ToLower();

            

            if (pagePath == "/" || (CmsConfig.UseLanguageShortCodeInPageUrls && string.Compare("/" + pageLanguage.shortCode, pagePath, true) == 0))
                pagePath += "/default";

            pagePath = langPath + pagePath;
            url = CmsContext.ApplicationPath + pagePath + ".aspx";

            url = url.Replace("///", "/");
            url = url.Replace("//", "/");
        

            switch (urlFormat)
            {
                case CmsUrlFormat.RelativeToRoot:
                    break;
                case CmsUrlFormat.FullIncludingProtocolAndDomainName:
                    if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Server == null)
                        throw new Exception("getUrlByPagePath() requires a running web request!");

                    System.Web.HttpRequest r = System.Web.HttpContext.Current.Request;
                    

                    string rootUrl = r.Url.Scheme + "://" + r.Url.Host;
                    if (!r.Url.IsDefaultPort)
                        rootUrl += ":" + r.Url.Port.ToString();
                    url = rootUrl + url;
                    break;
                default:
                    throw new ArgumentException("unknown CmsUrlFormat specified!!");
            }

            return url;

        }

       

		/// <summary>
		/// returns the user accessible URL for a page
        /// The pagePath should NEVER includes the language shortCode.
		/// Note: does not check if the path exists.
		/// </summary>
        /// <param name="pagePath"></param>
		/// <param name="paramList"></param>
        /// <param name="pageLanguage"></param>
        /// <param name="urlFormat"></param>
		/// <returns></returns>		
        public static string getUrlByPagePath(string pagePath, NameValueCollection paramList, CmsUrlFormat urlFormat, CmsLanguage pageLanguage)
		{
            string url = getUrlByPagePath(pagePath, urlFormat, pageLanguage);

            System.Web.HttpContext context = System.Web.HttpContext.Current;

            string urlQueryStartChar = "?";
            if (url.IndexOf("?") > -1)
                urlQueryStartChar = "&";

            bool first = true;
			foreach(string key in paramList.Keys)
			{
				string val = paramList[key];
                if (first)
                {
                    if (context != null)
                        url = url + urlQueryStartChar + key + "=" + context.Server.UrlEncode(val);
                    else
                        url = url + urlQueryStartChar + key + "=" + val;

                    first = false;
                }
                else
                {
                    url = url + "&" + key + "=" + context.Server.UrlEncode(val);
                }
			}
			return url;
		}

		/// <summary>
		/// gets the names of the currently available templates for the current user.
		/// </summary>
        public static string[] getTemplateNamesForCurrentUser()
        {
            return currentPage.TemplateEngine.getTemplateNamesForCurrentUser();

        }

        public static CmsPage[] getAllPagesWithPlaceholder(string placeholderType)
        {
            List<CmsPage> ret = new List<CmsPage>();
            try
            {
                Dictionary<int, CmsPage> allPages = HomePage.getLinearizedPages();
                foreach (CmsPage page in allPages.Values)
                {
                    if (page.hasPlaceholder(placeholderType))
                        ret.Add(page);
                } // foreach page
            }
            catch
            { }
            return ret.ToArray();
        }
        

        /// <summary>
        /// Handle PageNotFound (404) errors.
        /// <para>If the useInternal404NotFoundErrorHandler config entry is true, will send the user to the page 
        /// specified by Internal404NotFoundErrorHandlerPageUrl config entry.</para>
        /// </summary>
        public static void HandleNotFoundException()
        {

            bool useInternal404NotFoundErrorHandler = CmsConfig.getConfigValue("useInternal404NotFoundErrorHandler", false);

            if (useInternal404NotFoundErrorHandler)
            {
                string fromUrl = PageUtils.getFromForm("aspxerrorpath", System.Web.HttpContext.Current.Request.Url.PathAndQuery);
                fromUrl = System.Web.HttpContext.Current.Server.UrlEncode(fromUrl);

                string Internal404NotFoundErrorHandlerPageUrl = CmsContext.ApplicationPath + "/_internal/error404.aspx?from=" + fromUrl;
                if (CmsConfig.getConfigValue("Internal404NotFoundErrorHandlerPageUrl","") != "" )
                {
                    Internal404NotFoundErrorHandlerPageUrl = String.Format(CmsConfig.getConfigValue("Internal404NotFoundErrorHandlerPageUrl",""), fromUrl);
                }

                // use Server.Transfer (And not Response.Redirect) to hide the CMS URL from the user.                
                System.Web.HttpContext.Current.Server.Transfer(Internal404NotFoundErrorHandlerPageUrl);
                return;
            }
            else
            {
                // <?php header("HTTP/1.1 404 Not Found"); ?>
                // <?php header("Status: 404 Not Found"); ?>

                System.Web.HttpResponse resp = System.Web.HttpContext.Current.Response;
                resp.ClearContent();
                resp.StatusCode = 404;
                resp.AddHeader("Status", "404 Not Found");

                resp.Write("<html><body><strong>The page that you requested does not exist.</strong><p><em>Visit our <a href=\"" + CmsContext.ApplicationPath + "\">home page here</a></em></p></body></html>");
                resp.End();
                return;
                 
                // throw new System.Web.HttpException(404, "File Not Found"); //http://forums.asp.net/t/762031.aspx
            }
        }

        /// <summary>
        /// Handles a fatal server exception.
        /// </summary>
        public static void HandleFatalServerException()
        {
            System.Web.HttpResponse resp = System.Web.HttpContext.Current.Response;
            resp.ClearContent();
            resp.StatusCode = 400;
            resp.AddHeader("Status", "400 Bad Request");

            // resp.StatusDescription = "Not Found";
            // resp.Write("404 Not Found");
            //resp.Flush();
            resp.Write("<html><body><strong>The page that you requested caused a server error.</strong><p><em>Please try back later, or try visiting our <a href=\"" + CmsContext.ApplicationPath + "\">home page</a>.</em></p></body></html>");
            
            resp.End();
            return;

        }


		
		/// <summary>
		/// NEVER construct a CmsContext object. All functions provided in this class are static.
		/// </summary>
		public CmsContext()
        { throw new ArgumentException("never contstruct a CmsContext object. All functions in this class are static."); }

	}
}
