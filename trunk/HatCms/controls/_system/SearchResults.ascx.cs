namespace HatCMS.controls
{
	using System;
    using System.Text;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using Hatfield.Web.Portal.Search.Lucene;
	using Hatfield.Web.Portal;
	using Hatfield.Web.Portal.Collections;
	using System.Collections;
    using System.Collections.Generic;
	using System.Collections.Specialized;
    using HatCMS.Placeholders;    

	/// <summary>
	///		Uses the Lucene Search engine library to search and index the hatCMS based website.
	/// </summary>
	public partial class SearchResults : System.Web.UI.UserControl
	{

        private List<int> indexedNewsDetailsIds = new  List<int>(); // ensures duplicates over multiple pages are not indexed.
        private List<int> indexedJobDetailsIds = new List<int>(); // ensures duplicates over multiple pages are not indexed.
        protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("_system/KeywordIndex"));
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("_system/SpellCheckerIndex"));
            return ret.ToArray();
        }

        private string getPageFilenameForIndex(CmsPage page)
        {
            return page.ID.ToString();
        }

        private CmsPage getPageFromIndex(IndexableFileInfo fi)
        {
            return CmsContext.getPageById(Convert.ToInt32(fi.Filename));
        }

        /// <summary>
        /// if the language is not found, returns the Invalid language (that has .isInvalidLanguage set to TRUE)
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        private CmsLanguage getLanguageFromIndex(IndexableFileInfo fi)
        {
            return CmsLanguage.GetFromHaystack(fi.SectionName, CmsConfig.Languages);
        }
               
        private IndexableFileInfo[] indexStandardPage(CmsPage page)
        {
            List<IndexableFileInfo> ret = new List<IndexableFileInfo>();
            foreach (CmsLanguage lang in CmsConfig.Languages)
            {
                string desc = page.getSearchEngineDescription(lang);
                if (desc.Trim() != "")
                {
                    // -- use the searchEngineDescription
                    IndexableFileInfo fInfo = new IndexableFileInfo(getPageFilenameForIndex(page), "", page.Title, desc, lang.shortCode, page.LastUpdatedDateTime, true);
                    return new IndexableFileInfo[] { fInfo };
                }
                else
                {                    
                    string content = PageUtils.StripTags(page.renderAllPlaceholdersToString(lang));
                    IndexableFileInfo fInfo = new IndexableFileInfo(getPageFilenameForIndex(page), "", page.Title, content, lang.shortCode, page.LastUpdatedDateTime, false);
                    ret.Add(fInfo);
                 
                }
            } // foreach language
            return ret.ToArray();
        }

		protected void ReIndexAllPages()
		{			
			// to extract urls: http://www.dotnetcoders.com/web/Learning/Regex/exHrefExtractor.aspx
			// or http://regexlib.com/REDetails.aspx?regexp_id=1525
            List<IndexableFileInfo> fileInfos = new List<IndexableFileInfo>();
            Dictionary<int, CmsPage> AllPages = CmsContext.HomePage.getLinearizedPages();
			// -- translate from [path] => {CmsPage} to IndexableFileInfo objects
			foreach(int pageId in AllPages.Keys)
			{
                CmsPage page = AllPages[pageId];
                if (page.isVisibleForCurrentUser)
				{
                    if (! page.hasPlaceholder("PageRedirect") && page.ShowInMenu) // do not index redirect pages.
                    {
                        // index the content of placeholders
                        fileInfos.AddRange(indexStandardPage(page));
                    }
				}
			} // foreach page

			IndexableFileInfo[] fInfos = fileInfos.ToArray();

            string SearchEngineIndexDir = CmsConfig.getConfigValue("SearchEngineIndexDir", @"~\_system\KeywordIndex\");
            string SearchEngineSpellingIndexDir = CmsConfig.getConfigValue("SearchEngineSpellingIndexDir", @"~\_system\SpellCheckerIndex\");

            if (SearchEngineIndexDir.StartsWith("~\\") || SearchEngineIndexDir.StartsWith("~"+System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                SearchEngineIndexDir = SearchEngineIndexDir.Substring(2); // remove ~\
                SearchEngineIndexDir = Server.MapPath(CmsContext.ApplicationPath + SearchEngineIndexDir);
            }

            if (SearchEngineSpellingIndexDir.StartsWith("~\\") || SearchEngineSpellingIndexDir.StartsWith("~" + System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                SearchEngineSpellingIndexDir = SearchEngineSpellingIndexDir.Substring(2); // remove ~\
                SearchEngineSpellingIndexDir = Server.MapPath(CmsContext.ApplicationPath + SearchEngineSpellingIndexDir);
            }


            LuceneIndexer.doIndex(SearchEngineIndexDir, SearchEngineSpellingIndexDir, LuceneIndexer.IndexCreationMode.CreateNewIndex, fInfos, new Object());            
		} // ReIndexAllPages


        public static int numItemsPerPage
        {
            get
            {
                return 10;
            }
        }

        private IndexableFileInfo[] getFileInfosForCurrentLanguage(IndexableFileInfo[] searchResults)
        {
            if (searchResults.Length < 1 || CmsConfig.Languages.Length < 2)
                return searchResults;

            List<IndexableFileInfo> ret = new List<IndexableFileInfo>();
            CmsLanguage currLang = CmsContext.currentLanguage;
            foreach (IndexableFileInfo fi in searchResults)
            {
                CmsLanguage fiLang = getLanguageFromIndex(fi);
                if (fiLang == currLang)
                    ret.Add(fi);
            }

            return ret.ToArray();
        }
        
		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
            string SearchEngineSpellingIndexDir = @"~\_system\SpellCheckerIndex\";            
            string spellingIndexDir = CmsConfig.getConfigValue("SearchEngineSpellingIndexDir", SearchEngineSpellingIndexDir);

            if (spellingIndexDir.StartsWith("~\\"))
            {
                spellingIndexDir = spellingIndexDir.Substring(2); // remove ~\
                spellingIndexDir = Server.MapPath(CmsContext.ApplicationPath + spellingIndexDir);
            }

            string keywordIndexDir = CmsConfig.getConfigValue("SearchEngineIndexDir", "");
            if (keywordIndexDir.StartsWith("~\\"))
            {
                keywordIndexDir = keywordIndexDir.Substring(2); // remove ~\
                keywordIndexDir = Server.MapPath(CmsContext.ApplicationPath + keywordIndexDir);
            }

            CmsPage currentPage = CmsContext.currentPage;

            /*
             * Item Output Template String Format:
             * {0} = Page's Title
             * {1} = Page's MenuTitle
             * {2} = Page's relative URL
             * {3} = the search string snippet to display for this page
             * {4} = Page's breadcrumb created using MenuTitle's, concatenated using ' > ' spacers
             * {5} = "odd" or "even"
             * 
             */            
            string defaultItemTemplate = "<div class=\"SearchResultItem {5}\"><a class=\"SearchResultItemLink\" href=\"{2}\">{0}</a><br/><blockquote class=\"SearchResultInfo\"><div class=\"Snippet\">{3}</div><div class=\"SearchResultItemBreadcrumb\">{4}</div></blockquote></div>";
            string itemOutputTemplate = CmsControlUtils.getControlParameterKeyValue(this, "ItemTemplate", defaultItemTemplate);                        

			string action = PageUtils.getFromForm("action","");
			StringBuilder html = new StringBuilder();

            html.Append("<div class=\"SearchResults\">");

			if (action == "re-Index All Pages" || action.ToLower() == "reindexall")
			{
				ReIndexAllPages();
				html.Append("<p><strong>All Pages have been re-indexed</strong><p>");
			}
			
			string noQuery = Guid.NewGuid().ToString();
			string query = PageUtils.getFromForm("q", noQuery);
            
            html.Append("<div class=\"SearchBox\">");
			// -- output search box
            string formId = "SearchResults";
			html.Append(currentPage.getFormStartHtml(formId));
			if (query == noQuery)
			{
				html.Append(PageUtils.getInputTextHtml("q","queryBox", "", 20, 255));
			}
			else
			{
				html.Append(PageUtils.getInputTextHtml("q","queryBox", query, 20, 255));
			}
			html.Append("<input type=\"submit\" value=\"search\">");
            
            // -- output doIndex button
            if (CmsContext.currentUserIsSuperAdmin)
			{								
				html.Append(" Admin:<input type=\"submit\" name=\"action\" value=\"re-Index All Pages\">");			
			}
            html.Append(currentPage.getFormCloseHtml(formId));
            html.Append("</div>");
			html.Append("<p>");

            LuceneKeywordSearch search = new LuceneKeywordSearch(keywordIndexDir, spellingIndexDir);

			if (query != noQuery)
			{                
                
                // -- do the search with keyword highlighting
                IndexableFileInfo[] fileInfos = search.doSearch(query, query);

                fileInfos = getFileInfosForCurrentLanguage(fileInfos);

                // -- output the results
                if (fileInfos.Length < 1)
                {
                    // -- no results
                    html.Append("<strong>Your search for \"" + query + "\" returned no results</strong>");
                    // -- get spelling suggestion
                    string spellingSuggestion = search.getSpellingSuggestion(query);
                    if (spellingSuggestion.Trim() != query.Trim())
                    {
                        NameValueCollection p = new NameValueCollection();
                        p.Add("q", spellingSuggestion);
                        string newSearchUrl = CmsContext.getUrlByPagePath(CmsContext.currentPage.Path, p);
                        html.Append("<p><font color=\"#cc0000\">Did you mean:</font> \"<a href=\"" + newSearchUrl + "\">" + spellingSuggestion + "</a>\" <font color=\"#cc0000\">?</font></p>");
                    }                             
                }
                else
                {
                    html.Append("<strong>Your search for \"" + query + "\" returned " + fileInfos.Length + " results</strong><p>");
                    html.Append("<p>" + getPagerOutput(fileInfos) + "</p>");

                    int startAtItemNumber = PageUtils.getFromForm("num", 0);
                    if (startAtItemNumber >= fileInfos.Length)
                    {
                        startAtItemNumber = fileInfos.Length - 1;
                    }
                    int endAt = Math.Min(startAtItemNumber + numItemsPerPage - 1, fileInfos.Length - 1);

                    if (startAtItemNumber == 0 && endAt == 0 && fileInfos.Length == 1)
                    {
                        html.Append(getItemDisplay(fileInfos[0], itemOutputTemplate));
                    }
                    else
                    {
                        for (int i = startAtItemNumber; i <= endAt; i++)
                        {
                            if (endAt <= 0)
                                break;
                            IndexableFileInfo fileInfo = fileInfos[i];
                            html.Append(getItemDisplay(fileInfo, itemOutputTemplate));
                        } // for

                        html.Append("<p>" + getPagerOutput(fileInfos) + "</p>");
                    } // else			
                }
			} // if query != noQuery

            html.Append("</div>");

			writer.WriteLine(html.ToString());

		} // Render

        private string getPageUrl(int pageNumber)
        {
            int startAt = (pageNumber - 1) * numItemsPerPage;
            string query = PageUtils.getFromForm("q", "");
            NameValueCollection urlParams = new NameValueCollection();
            urlParams.Add("num",startAt.ToString());
            urlParams.Add("q", query);
            string url = CmsContext.getUrlByPagePath(CmsContext.currentPage.Path, urlParams);
            return url;
        }

        protected string getPagerOutput(IndexableFileInfo[] searchResults)
        {
            StringBuilder html = new StringBuilder();

            html.Append("<div class=\"pager\">");
            int numPages = (int)Math.Ceiling((double)searchResults.Length / numItemsPerPage);
            if (numPages <= 0)
                numPages = 1;

            int startAtItemNumber = PageUtils.getFromForm("num", 0);
            if (startAtItemNumber >= searchResults.Length)
            {
                startAtItemNumber = searchResults.Length - 1;
            }


            int currPageNum = (int)Math.Ceiling((double)startAtItemNumber / numItemsPerPage) + 1;

            if (currPageNum > 1 && numPages > 1)
            {
                html.Append("<a href=\"" + getPageUrl(currPageNum - 1) + "\">");
                html.Append("&laquo; prev");
                html.Append("</a> ");
            }

            html.Append("Page " + currPageNum.ToString() + " of " + numPages.ToString());

            if (currPageNum < numPages && numPages > 1)
            {
                html.Append(" <a href=\"" + getPageUrl(currPageNum + 1) + "\">");
                html.Append("next &raquo;");
                html.Append("</a> ");
            }

            html.Append("</div>");

            return(html.ToString());
        } // OutputPager


        private string getItemBreadcrumbOutput(CmsPage page)
        {
            StringBuilder breadcrumb = new StringBuilder();
            if (page.ID != CmsContext.HomePage.ID)
            {
                string[] pathParts = page.Path.Split(new char[] { '/' });
                string prevPath = "/";
                for (int i = 0; i < pathParts.Length; i++)
                {
                    string currPath = prevPath + pathParts[i];
                    currPath = currPath.Replace("//", "/");
                    CmsPage p = CmsContext.getPageByPath(currPath);
                    string displayTitle = p.MenuTitle;
                    if (displayTitle == "")
                        displayTitle = p.Title;

                    if (i < pathParts.Length - 1 && p.ParentPage.ID > -1)
                    {
                        breadcrumb.Append("<a href=\"" + p.Url + "\">" + displayTitle + "</a>");

                        if (i < pathParts.Length - 2)
                            breadcrumb.Append(" &gt; ");
                    }                    
                    // breadcrumb.Append(currPath+" > ");
                    prevPath = currPath + "/";
                } // foreach

                return breadcrumb.ToString();
            }
            else
                return page.Title; // home page

        }

        bool oddItem = true;

        private string getItemDisplay(IndexableFileInfo fileInfo, string defaultOutputTemplate)
        {
            /*
             * Item Output Template String Format:
             * {0} = Page's Title
             * {1} = Page's MenuTitle
             * {2} = Page's relative URL
             * {3} = the search string snippet to display for this page
             * {4} = Page's breadcrumb created using MenuTitle's, concatenated using ' > ' spacers
             * {5} = "odd" or "even"
             * 
             */

            CmsPage targetPage = getPageFromIndex(fileInfo);

            // allow per-template ItemTemplates
            string itemOutputTemplate = CmsControlUtils.getControlParameterKeyValue(this, "ItemTemplate_" + targetPage.TemplateName, defaultOutputTemplate);
            
            string url = targetPage.Url;
            if (fileInfo.FilenameParameters != "")
            {
                if (url.IndexOf("?") > -1)
                    url += "&" + fileInfo.FilenameParameters;
                else
                    url += "?" + fileInfo.FilenameParameters;
            }

            string ItemBreadcrumb = getItemBreadcrumbOutput(targetPage);

            string OddOrEven = "even";
            if (oddItem)
                OddOrEven = "odd";

            oddItem = !oddItem;

            try
            {
                string ret = String.Format(itemOutputTemplate, targetPage.Title, targetPage.MenuTitle, url, StringUtils.StripHTMLTags(fileInfo.Contents), ItemBreadcrumb, OddOrEven);
                return ret;
            }
            catch (Exception fEx)
            {
                throw new Exception("Search Item Display Template error (url: \"" + url + "\"); template: (" + itemOutputTemplate + ")");
            }
            return "";
        }

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}
		#endregion
	}
}
