using System;
using System.Text;
using System.Web;
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
                
                int id = CmsPage.FetchHomePageId();
                CmsPage ret = CmsPage.FetchPageById(id);

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
                string cacheKey = "currentPage" + DateTime.Now.Ticks;
                if (PerRequestCache.CacheContains(cacheKey))
                {
                    return (CmsPage)PerRequestCache.GetFromCache(cacheKey, new CmsPage());
                }                
                
                string path = getRequestedPagePathFromForm();                                                                
                
                // -- default view by path (ignore the revision param)
                CmsPage ret = getPageByPath(path);
                if (ret.Id >= 0 && ret.currentUserCanWrite) // note: don't check against the current ViewMode because a stack overflow (circular references) will occur.
                {
                    // -- get page by revision
                    int revisionNumberToDisplay = PageUtils.getFromForm("revNum", -1);
                    if (revisionNumberToDisplay >= 0)
                    {
                        CmsPage revToRet = getPageByPath(path, revisionNumberToDisplay);
                        if (revToRet.currentUserCanWrite)
                        {
                            PerRequestCache.AddToCache(cacheKey, revToRet);

                            return revToRet;
                        }
                    }

                }
                
                PerRequestCache.AddToCache(cacheKey, ret);
                return ret;
            

            }
		}

        /// <summary>
        /// Get the current security zone of the page
        /// </summary>
        public static CmsPageSecurityZone currentZone
        {
            get { return CmsContext.currentPage.SecurityZone; }
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
            
            if (PerRequestCache.CacheContains(cacheKey))
            {
                return (CmsPageCache)PerRequestCache.GetFromCache(cacheKey, new CmsPageCache());
            }
            else
            {
                if (_pageCacheObject == null)
                    _pageCacheObject = new CmsPageCache();

                PerRequestCache.AddToCache(cacheKey, _pageCacheObject);
                
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
		/// gets the currently logged in user.
		/// </summary>
        private static System.Security.Principal.IPrincipal currentUser
		{
			get
			{
                string cacheKey = "authProcessed";
                if (!PerRequestCache.CacheContains(cacheKey))
                {
                    WebPortalAuthentication.processAuthenticateRequest(System.Web.HttpContext.Current);
                    PerRequestCache.AddToCache("authProcessed",  true);
                }
                
                return System.Web.HttpContext.Current.User;
			}
		}

        /// <summary>
        /// returns true if the currently logged in user is logged in.
        /// </summary>
        public static bool currentUserIsLoggedIn
        {
            get
            {
                if (currentUserIsSuperAdmin)
                    return true;
                if (CmsContext.currentWebPortalUser != null && CmsContext.currentWebPortalUser.inRole(CmsConfig.getConfigValue("LoginUserRole", Guid.NewGuid().ToString())))
                    return true;
                if (CmsContext.currentWebPortalUser != null && CmsContext.currentWebPortalUser.inRole(CmsConfig.getConfigValue("AuthorAccessUserRole", Guid.NewGuid().ToString())))
                    return true;
                if (CmsContext.currentWebPortalUser != null && CmsContext.currentWebPortalUser.inRole(CmsConfig.getConfigValue("AdminUserRole", Guid.NewGuid().ToString())))
                    return true;
                return false;
            }
        }

        /// <summary>
        /// returns true if the current user can write files to the ~/UserFiles/ directory on disk.
        /// Note that all users (including anonymous users) can read files in this directory.
        /// </summary>
        public static bool currentUserCanWriteToUserFilesOnDisk
        {
            get
            {
                return currentUserIsLoggedIn;
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
                    // -- we cache the currentWebPortal user so that we don't go to the database all the time
                    string cacheKey = "currentWebPortalUser";
                    if (PerRequestCache.CacheContains(cacheKey))
                        return PerRequestCache.GetFromCache(cacheKey, null) as WebPortalUser;

                    if (currentUser != null && currentUser.Identity.IsAuthenticated)
                    {
                        WebPortalUser u = WebPortalUser.FetchUser(currentUser.Identity.Name, CmsPortalApplication.GetInstance());
                        PerRequestCache.AddToCache(cacheKey, u);
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
                if (CmsContext.currentUserIsLoggedIn && CmsContext.currentEditMode == CmsEditMode.View && currentPage.currentUserCanWrite)
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
        /// Checks to see if the current user is making an offline (CD/DVD/memory stick) version
        /// </summary>
        public static bool currentUserIsMakingOfflineVersion
        {
            get
            {
                return Hatfield.Web.Portal.PageUtils.ClientIsMakingOfflineVersion;
            }
        }

        /// <summary>
        /// The Form name that tracks the <see cref="currentUserIsRequestingPrintFriendlyVersion"/> currentUserIsRequestingPrintFriendlyVersion
        /// </summary>
        public static string PrintFriendlyVersionFormName = "print";

        /// <summary>
        /// Checks if the current user is requesting a print-friendly version of the page.
        /// </summary>
        public static bool currentUserIsRequestingPrintFriendlyVersion
        {
            get
            {
                int formVal = PageUtils.getFromForm(PrintFriendlyVersionFormName, 0);
                if (formVal == 1)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// gets the current version of HatCMS.Core that is in-use.
        /// </summary>
        public static System.Version currentHatCMSCoreVersion
        {
            get
            {
                return typeof(CmsPage).Assembly.GetName().Version;
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
                if (formVal == "")
                    return CmsEditMode.View;

                if (formVal == "1" && currentPage.currentUserCanWrite) // note: the currentPage.currentUserCanWrite potentially goes to the database                
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
            // -- only allow authorized people to go to CmsEditMode.Edit
            if (newEditMode == CmsEditMode.Edit && targetPage.currentUserCanWrite)
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
                paramList.Add("hatCmsNocache", DateTime.Now.Ticks.ToString());
            }


            
            // note: do not use targetPage.getUrl(paramList); (it causes an infinite loop)
            string url = getUrlByPagePath(targetPage.Path, paramList);

            System.Web.HttpContext context = System.Web.HttpContext.Current;
            if (context != null)
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

        private static CmsUserInterface _UserInterface;
        public static CmsUserInterface UserInterface
        {
            get
            {
                if (_UserInterface != null)
                    return _UserInterface;
                
                throw new NotImplementedException("Error: You must pass call CmsContext.Application_Start from your global.asax file with a valid CmsUserInterface object");
            }
        }


        #region global.asax functions
        private static System.Threading.Timer _backgroundTaskTimer = null;

        /// <summary>
        /// time-interval (milliseconds) to wait between calls to RunBackgroundPeriodicTasks(). 
        /// default value is every hour (3600000 ms)
        /// </summary>
        private const int _backgroundTimerPeriod_ms = 60 * 60 * 1000; 
        /// <summary>
        /// This function is called by the Global.asax's Application_Start_Session function.
        /// </summary>
        public static void Application_Start_Session()
        {
            
        }

        /// <summary>
        /// This function is called by the global.asax's Application_start function
        /// </summary>
        public static void Application_Start(CmsUserInterface UIToRegister )
        {
            _UserInterface = UIToRegister;

            // -- Initialize all active modules (plugins)
            // CmsModuleUtils.LoadAllActiveModules();

            // -- run all OnApplicationStart background tasks
            CmsBackgroundTaskUtils.RunAllApplicationStartBackgroundTasks();

            // -- initialize Period Background Task timer
            int dueTime_ms = 60 * 1000; // milliseconds to wait before calling RunBackgroundPeriodicTasks for first time.            

            _backgroundTaskTimer = new System.Threading.Timer(RunBackgroundPeriodicTasks, null, dueTime_ms, _backgroundTimerPeriod_ms);

        }

        private static void RunBackgroundPeriodicTasks(object state)
        {
            // -- ensure we don't call this function again until RunBackgroundTasks has returned
            _backgroundTaskTimer.Change(Timeout.Infinite, Timeout.Infinite);
            try
            {
                CmsBackgroundTaskUtils.RunAllApplicablePeriodicTasks();
            }
            catch (Exception ex)
            {
                Console.Write("Background Task Error: Something went wrong. Reason: {0} Stack: {1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                _backgroundTaskTimer.Change(_backgroundTimerPeriod_ms, _backgroundTimerPeriod_ms);
            }
        }


        public static void Application_End()
        {
            // -- run all OnApplicationStart background tasks
            CmsBackgroundTaskUtils.RunAllApplicationEndBackgroundTasks();

            // -- get rid of the periodic background events timer.
            if (_backgroundTaskTimer != null)
                _backgroundTaskTimer.Dispose();

        }

        /// <summary>
        /// This function is called by the global.asax's Application_BeginRequest function.
        /// This is where all the URL re-writing magic happens.
        /// </summary>
        public static void Application_BeginRequest(System.Web.HttpContext context)
        {
            ProcessSwfUploadFixes(context); // this needs to be before the UrlsToNotRemap is checked.

            // get the requested page path
            string baseProcessingPage = CmsContext.ApplicationPath + "default.aspx";
            string pagePath = context.Request.Url.AbsolutePath.Substring(CmsContext.ApplicationPath.Length);

            // do not process the urls in "URLsToNotRemap" configuration entry.
            if (StringUtils.IndexOf(CmsConfig.URLsToNotRemap, pagePath, StringComparison.CurrentCultureIgnoreCase) != -1)
            {
                return;
            }

            // only re-map ASPX files
            string extension = System.IO.Path.GetExtension(pagePath);
            if (String.Compare(extension, ".aspx", true) != 0)
                return;

            CmsContext.StartNewRequest();

            // remove extension
            pagePath = pagePath.Substring(0, pagePath.Length - ".aspx".Length);

            if (String.Compare(pagePath, "default", true) == 0)
                pagePath = "/";


            if (!pagePath.StartsWith("/"))
                pagePath = "/" + pagePath;

            string queryString = "p=" + pagePath; // note: no starting "?"
            if (context.Request.Url.Query != "")
            {
                string query = context.Request.Url.Query;
                if (query.StartsWith("?"))
                {
                    // replace beginning "?" with "&"
                    query = query.Substring(1);
                    query = "&" + query;
                }
                queryString += query;
            }

            // send the request to the default.aspx page with the page path in the querystring.            
            context.RewritePath(baseProcessingPage, null, queryString);
        }

        private static void ProcessSwfUploadFixes(System.Web.HttpContext context)
        {
            if (context == null || context.Request == null)
                return;

            try
            {
                string session_param_name = "ASPSESSID";
                string session_cookie_name = "ASP.NET_SESSIONID";

                if (context.Request.Form[session_param_name] != null)
                {
                    UpdateCookie(session_cookie_name, HttpContext.Current.Request.Form[session_param_name], context);
                }
                else if (context.Request.QueryString[session_param_name] != null)
                {
                    UpdateCookie(session_cookie_name, context.Request.QueryString[session_param_name], context);
                }
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("Error Initializing Session");
            }

            try
            {
                string auth_param_name = "AUTHID";
                string auth_cookie_name = System.Web.Security.FormsAuthentication.FormsCookieName;

                if (context.Request.Form[auth_param_name] != null)
                {
                    UpdateCookie(auth_cookie_name, context.Request.Form[auth_param_name], context);
                }
                else if (context.Request.QueryString[auth_param_name] != null)
                {
                    UpdateCookie(auth_cookie_name, context.Request.QueryString[auth_param_name], context);
                }

            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("Error Initializing Forms Authentication");
            }
        }

        private static void UpdateCookie(string cookie_name, string cookie_value, HttpContext context)
        {
            HttpCookie cookie = context.Request.Cookies.Get(cookie_name);
            if (cookie == null)
            {
                cookie = new HttpCookie(cookie_name);
                HttpContext.Current.Request.Cookies.Add(cookie);
            }
            cookie.Value = cookie_value;
            context.Request.Cookies.Set(cookie);
        }


        /// <summary>
        /// This function is called by the global.asax's Application_Error function
        /// </summary>
        public static void Application_Error(HttpContext context)
        {
            string techEmail = CmsConfig.getConfigValue("TechnicalAdministratorEmail", "");
            string smtpServer = CmsConfig.getConfigValue("smtpServer", "");
            if (techEmail.IndexOf("@") < 1 || smtpServer.Trim() == "")
            {
                Hatfield.Web.Portal.ApplicationUtils.Application_Error_StandardEmailSender(context, techEmail, new string[] { techEmail }, smtpServer);
            }

        }

        #endregion

        #region getPageByPath function
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
            return CmsPage.FetchPageByPath(pagePath, pagePathLanguage);
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

            
            CmsPage ret = CmsPage.FetchPageByPath(pagePath);
            
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
            
            if (pageRevisionNumber >= 0)
                return CmsPage.FetchPageByPath(pagePath, pageRevisionNumber);
            return
                CmsPage.FetchPageByPath(pagePath);
        }

        #endregion

        /// <summary>
		/// gets a CmsPage from the database with the given ID.
		/// Returns an empty CmsPage (with id = -1) if the page was not found.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>the CmsPage with the given PageId. If not found, an empty CmsPage (with id = -1) is returned</returns>
		public static CmsPage getPageById(int id)
		{			
			return CmsPage.FetchPageById(id);
		}

        /// <summary>
        /// Checks if a page with the given pageId exists in the database.
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public static bool pageExists(int pageId)
        {            
            CmsPage p = CmsPage.FetchPageById(pageId);
            if (p.Id >= 0)
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
                if (p.Id < 0)
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
			CmsPage parentPage = CmsPage.FetchPageById(parentPageId);
			if (parentPage.Id <= 0)
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
        public static bool childPageWithNameExists(int parentPageId, IList<CmsPageLanguageInfo> pageLangInfos)
        {
            
            CmsPage parentPage = CmsPage.FetchPageById(parentPageId);
            if (parentPage.Id < 0)
                return false;
            
            foreach (CmsPage p in parentPage.ChildPages)
            {
                foreach (CmsPageLanguageInfo langInfo in pageLangInfos)
                {
                    string nameToFind = langInfo.Name;
                    CmsLanguage pageLanguage = CmsLanguage.GetFromHaystack(langInfo.LanguageShortCode, CmsConfig.Languages);
                    if (String.Compare(p.getName(pageLanguage), nameToFind, true) == 0)
                        return true;
                }
            }
            return false;
        }

        #region getUrlByPagePath functions

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
                    // there is a way to get the bindings (domain names) for the current website from IIS using Microsoft.Web.Administration.dll (found in c:\windows\system32\inetsrv\)
                    // info and code is here: http://blogs.msdn.com/b/carlosag/archive/2011/01/21/get-iis-bindings-at-runtime-without-being-an-administrator.aspx
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

            string urlQueryStartChar = "?";
            if (url.IndexOf("?") > -1)
                urlQueryStartChar = "&";

            bool first = true;
			foreach(string key in paramList.Keys)
			{
				string val = paramList[key];
                if (first)
                {
                    url = url + urlQueryStartChar + key + "=" + System.Web.HttpUtility.UrlEncode(val);

                    first = false;
                }
                else
                {
                    url = url + "&" + key + "=" + System.Web.HttpUtility.UrlEncode(val);
                }
			}
			return url;
        }

        #endregion

        /// <summary>
		/// gets the names of the currently available templates for the current user.
		/// </summary>
        public static string[] getTemplateNamesForCurrentUser()
        {
            return currentPage.TemplateEngine.getTemplateNamesForCurrentUser();
        }

        #region getAllPagesWithPlaceholder functions

        public enum PageGatheringMode { ChildPagesOnly, FullRecursion }

        /// <summary>
        /// get all pages (from rootPageToGatherFrom and below) that implement placeholderType.
        /// </summary>
        /// <param name="placeholderType"></param>
        /// <param name="rootPageToGatherFrom"></param>
        /// <param name="gatheringMode"></param>
        /// <returns></returns>
        public static CmsPage[] getAllPagesWithPlaceholder(string placeholderType, CmsPage rootPageToGatherFrom, PageGatheringMode gatheringMode)
        {
            return getAllPagesWithPlaceholders(new string[] { placeholderType }, rootPageToGatherFrom, gatheringMode);
        }

        /// <summary>
        /// get all pages (from rootPageToGatherFrom and below) that implement one or more placeholderTypes.
        /// </summary>
        /// <param name="placeholderType"></param>
        /// <param name="rootPageToGatherFrom"></param>
        /// <param name="gatheringMode"></param>
        /// <returns></returns>
        public static CmsPage[] getAllPagesWithPlaceholders(string[] placeholderTypes, CmsPage rootPageToGatherFrom, PageGatheringMode gatheringMode)
        {
            List<CmsPage> ret = new List<CmsPage>();
            try
            {
                if (gatheringMode == PageGatheringMode.FullRecursion)
                {
                    Dictionary<int, CmsPage> allPages = rootPageToGatherFrom.getLinearizedPages();
                    foreach (CmsPage page in allPages.Values)
                    {
                        foreach (string placeholderType in placeholderTypes)
                        {
                            if (page.hasPlaceholder(placeholderType))
                            {
                                ret.Add(page);
                                break; // only add the page once if it implements more than one placeholder
                            }
                        }
                    } // foreach page
                }
                else if (gatheringMode == PageGatheringMode.ChildPagesOnly)
                {
                    foreach (CmsPage page in rootPageToGatherFrom.ChildPages)
                    {
                        foreach (string placeholderType in placeholderTypes)
                        {
                            if (page.hasPlaceholder(placeholderType))
                            {
                                ret.Add(page);
                                break; // only add the page once if it implements more than one placeholder
                            }
                        } // foreach placeholderType
                    } // foreach page
                }
            }
            catch
            { }
            return ret.ToArray();
        }

        /// <summary>
        /// gets all pages on the entire site that implement the placeholderType specified.        
        /// </summary>
        /// <param name="placeholderType"></param>
        /// <returns></returns>
        public static CmsPage[] getAllPagesWithPlaceholder(string placeholderType)
        {
            return getAllPagesWithPlaceholder(placeholderType, HomePage, PageGatheringMode.FullRecursion);
        }

        #endregion

        /// <summary>
        /// NOTE: returns ALL matching pages in the system, regardless of the user's security level.
        /// </summary>
        /// <param name="placeholderType"></param>
        /// <param name="rootPageToGatherFrom"></param>
        /// <param name="gatheringMode"></param>
        /// <returns></returns>
        public static Dictionary<CmsPage, CmsPlaceholderDefinition[]> getAllPlaceholderDefinitions(string placeholderType, CmsPage rootPageToGatherFrom, PageGatheringMode gatheringMode)
        {
            Dictionary<CmsPage, CmsPlaceholderDefinition[]> ret = new Dictionary<CmsPage, CmsPlaceholderDefinition[]>();
            try
            {
                if (gatheringMode == PageGatheringMode.FullRecursion)
                {
                    Dictionary<int, CmsPage> allPages = rootPageToGatherFrom.getLinearizedPages();
                    foreach (CmsPage page in allPages.Values)
                    {
                        CmsPlaceholderDefinition[] defs = page.getPlaceholderDefinitions(placeholderType);
                        if (defs.Length > 0)
                            ret.Add(page, defs);

                    } // foreach page
                }
                else if (gatheringMode == PageGatheringMode.ChildPagesOnly)
                {
                    foreach (CmsPage page in rootPageToGatherFrom.ChildPages)
                    {
                        CmsPlaceholderDefinition[] defs = page.getPlaceholderDefinitions(placeholderType);
                        if (defs.Length > 0)
                            ret.Add(page, defs);
                    } // foreach page
                }
            }
            catch
            { }
            return ret;
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
                string defaultUrl = "";
                if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null)
                    defaultUrl = System.Web.HttpContext.Current.Request.Url.PathAndQuery;

                string fromUrl = PageUtils.getFromForm("aspxerrorpath", defaultUrl);
                fromUrl = System.Web.HttpUtility.UrlEncode(fromUrl);

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
                System.Web.HttpResponse resp = System.Web.HttpContext.Current.Response;
                resp.ClearContent();
                resp.StatusCode = 404;
                resp.AddHeader("Status", "404 Not Found");

                string EOL = Environment.NewLine;
                StringBuilder html404 = new StringBuilder();

                // the default 404 page is based on the HTML5 Boilerplate
                // https://raw.github.com/h5bp/html5-boilerplate/master/404.html

                html404.Append("<!doctype html>" + EOL);
                html404.Append("<html>" + EOL);
                html404.Append("<head>" + EOL);
                html404.Append("  <meta charset=\"utf-8\">" + EOL);
                html404.Append("  <title>Page Not Found</title>" + EOL);
                html404.Append("  <style>" + EOL);
                html404.Append("    html { font-size: 100%; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }" + EOL);
                html404.Append("    html, input { font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; }" + EOL);
                html404.Append("    body { margin: 0 10px; font-size: 20px; line-height: 1.4; background: #f0f0f0; color: #737373; }" + EOL);
                html404.Append("    ::-moz-selection { background: #fe57a1; color: #fff; text-shadow: none; }" + EOL);
                html404.Append("    ::selection { background: #fe57a1; color: #fff; text-shadow: none; }" + EOL);
                html404.Append("    a { color: rgb(36, 109, 56); text-decoration: underline; }" + EOL);
                html404.Append("    a:hover, a:focus, a:active { color: rgb(96, 73, 141); text-shadow: 2px 2px 2px rgba(36, 109, 56, 0.5); }" + EOL);
                html404.Append("    h1 { font-size: 50px; text-align: center; margin: 0 10px; }" + EOL);
                html404.Append("    h1 span { color: #bbb; }" + EOL);
                html404.Append("    h3 { margin: 1.5em 0 0.5em; }" + EOL);
                html404.Append("    p { margin: 1em 0; }" + EOL);
                html404.Append("    ul { margin: 1em 0; padding: 0 0 0 40px; }" + EOL);
                html404.Append("    .container { max-width: 500px; _width: 500px; padding: 30px 20px 50px; border: 1px solid #b3b3b3; margin: 40px auto 0; border-radius: 4px; box-shadow: 0 1px 10px #a7a7a7, inset 0 1px 0 #fff; background: #fcfcfc; }" + EOL);
                html404.Append("    .container div { max-width: 380px; margin: 0 auto; }" + EOL);
                html404.Append("    /* google search */" + EOL);
                html404.Append("    #goog-fixurl ul { list-style: none; padding: 0; margin: 0; }" + EOL);
                html404.Append("    #goog-fixurl form { margin: 0; }" + EOL);
                html404.Append("    #goog-fixurl input { font-size: 16px; line-height: normal; vertical-align: top; }" + EOL);
                html404.Append("    #goog-wm-qt { width: 220px; height: 20px; padding: 5px; border: 1px solid #bbb; margin: 5px 10px 0 0; }" + EOL);
                html404.Append("    #goog-wm-sb { display: inline-block; height: 32px; margin: 5px 0 0; white-space: nowrap; cursor: pointer; padding: 0 10px; border: 1px solid #bbb; border-radius: 2px; color: #444; background-color: #f5f5f5; background-image: -webkit-linear-gradient(#f5f5f5, #f1f1f1); background-image: -moz-linear-gradient(#f5f5f5, #f1f1f1); background-image: -ms-linear-gradient(#f5f5f5, #f1f1f1); background-image: -o-linear-gradient(#f5f5f5, #f1f1f1); *overflow: visible; *display: inline; *zoom: 1; -webkit-appearance: none; -moz-appearance: none; appearance: none; }" + EOL);
                html404.Append("    #goog-wm-sb:hover, #goog-wm-sb:focus { border-color: #aaa; color: #222; box-shadow: 0 1px 1px rgba(0, 0, 0, 0.1); background-color: #f8f8f8; background-image: -webkit-linear-gradient(#f8f8f8, #f1f1f1); background-image: -moz-linear-gradient(#f8f8f8, #f1f1f1); background-image: -ms-linear-gradient(#f8f8f8, #f1f1f1); background-image: -o-linear-gradient(#f8f8f8, #f1f1f1); }" + EOL);
                html404.Append("    #goog-wm-sb:focus { border-color: #105cb6; outline: 0; }" + EOL);
                html404.Append("  </style>" + EOL);
                html404.Append("</head>" + EOL);
                html404.Append("<body>" + EOL);
                html404.Append("  <div class=\"container\">" + EOL);
                html404.Append("    <h1>Page Not Found</h1>" + EOL);
                html404.Append("    <div>" + EOL);
                html404.Append("      <p>Sorry, but the page you were trying to view does not exist.</p>" + EOL);
                html404.Append("      <p>It looks like this was the result of either:</p>" + EOL);
                html404.Append("      <ul>" + EOL);
                html404.Append("        <li>a mistyped address</li>" + EOL);
                html404.Append("        <li>an out-of-date link</li>" + EOL);
                html404.Append("      </ul>" + EOL);
                html404.Append("    <p><em>Visit our <a href=\"" + CmsContext.ApplicationPath + "\">home page here</a></em></p>" + EOL);
                html404.Append("    </div>" + EOL);
                html404.Append("    <script>" + EOL);
                html404.Append("      var GOOG_FIXURL_LANG = (navigator.language || '').slice(0,2)," + EOL);
                html404.Append("      GOOG_FIXURL_SITE = location.host;" + EOL);
                html404.Append("    </script>" + EOL);
                html404.Append("    <script src=\"http://linkhelp.clients.google.com/tbproxy/lh/wm/fixurl.js\"></script>" + EOL);
                html404.Append("  </div>" + EOL);
                html404.Append("</body>" + EOL);
                html404.Append("</html>" + EOL);

                resp.Write(html404.ToString());
                resp.End();
                return;                                
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
