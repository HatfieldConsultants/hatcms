using System;
using System.Web.UI;
using HatCMS.Placeholders;
using System.Collections.Generic;
using System.Text;
using Hatfield.Web.Portal;
using System.Collections.Specialized;

namespace HatCMS.Placeholders.NewsDatabase
{
    public class NewsArticleAggregator : BaseCmsPlaceholder
    {        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            // -- CKEditor dependencies
            ret.AddRange(CKEditorHelpers.CKEditorDependencies);

            // -- database tables
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE `NewsArticleAggregator` (
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `LangCode` varchar(2) NOT NULL,
                  `DefaultYearToDisplay` int(11) NOT NULL,
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;"));

            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE `NewsArticleDetails` (
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `LangCode` varchar(2) NOT NULL,
                  `DateOfNews` datetime DEFAULT NULL,
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;"));

            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("NewsArticle.ReadArticleText"));
            ret.Add(new CmsConfigItemDependency("NewsArticle.NoNewsText"));
            ret.Add(new CmsConfigItemDependency("NewsArticle.NoNewsTextForText"));

            // -- template dependency
            ret.Add(new CmsTemplateDependency(CmsConfig.getConfigValue("NewsArticle.DetailsTemplateName", "_NewsArticleDetails")));

            ret.Add(CmsFileDependency.UnderAppPath("images/_system/calendar/arrowRight.jpg", new DateTime(2011, 3, 1)));

            return ret.ToArray();
        }

        /// <summary>
        /// Render the "Add a news article"
        /// </summary>
        /// <param name="action"></param>
        /// <param name="pageToRenderFor"></param>
        /// <param name="langToRenderFor"></param>
        /// <returns></returns>
        protected static string AddNewsArticleEditMenuRender(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
        {
            NameValueCollection createPageParams = action.CreateNewPageOptions.GetCreatePagePopupParams();
            if (action.CreateNewPageOptions.RequiresUserInput())
                return CmsPageEditMenu.DefaultStandardActionRenderers.RenderPopupLink("CreateNewPagePath", "/_admin/createPage", createPageParams, pageToRenderFor, langToRenderFor, "<strong>Add</strong> a news article", 500, 400);
            else
                return CmsPageEditMenu.DefaultStandardActionRenderers.RenderLink("CreateNewPagePath", "/_admin/createPage", createPageParams, pageToRenderFor, langToRenderFor, "<strong>Add</strong> a news article");
        }

        /// <summary>
        /// Adds the "Add a news article" menu item to the Edit Menu.
        /// </summary>
        /// <param name="pageToAddCommandTo"></param>
        /// <param name="newsArticleAggregatorPage"></param>
        public static void AddNewsArticleCommandToEditMenu(CmsPage pageToAddCommandTo, CmsPage newsArticleAggregatorPage)
        {
            // -- only add the command if the user has write-access to the page
            if (!pageToAddCommandTo.currentUserCanWrite)
                return;

            // -- base the command off the existing "create new sub-page" command
            CmsPageEditMenuAction createNewSubPage = pageToAddCommandTo.EditMenu.getActionItem(CmsEditMenuActionItem.CreateNewPage);

            if (createNewSubPage == null)
                throw new Exception("Fatal Error in in NewsArticleAggregator placeholder - could not get the existing CreateNewPage action");

            CmsPageEditMenuAction newAction = createNewSubPage.Copy(); // copy everything from the CreateNewPage entry            

            // -- configure this command to not prompt authors for any information.
            //    the minimum information needed to create a page is the new page's filename (page.name)
            //      -- get the next unique filename            
            string newPageName = "";
            for (int newsArticleNum = 1; newsArticleNum < int.MaxValue; newsArticleNum++)
            {
                string pageNameToTest = "News Article " + newsArticleNum.ToString();
                if (!CmsContext.childPageWithNameExists(newsArticleAggregatorPage.ID, pageNameToTest))
                {
                    newPageName = pageNameToTest;
                    break;
                }
            }

            string newPageTitle = "";
            string newPageMenuTitle = "";
            string newPageSearchEngineDescription = "";
            bool newPageShowInMenu = false;
            string newPageTemplate = CmsConfig.getConfigValue("NewsArticle.DetailsTemplateName", "_NewsArticleDetails");

            newAction.CreateNewPageOptions = CmsCreateNewPageOptions.GetInstanceWithNoUserPrompts(newPageName, newPageTitle, newPageMenuTitle, newPageSearchEngineDescription, newPageShowInMenu, newPageTemplate, newsArticleAggregatorPage.ID);

            newAction.CreateNewPageOptions.ParentPageId = newsArticleAggregatorPage.ID;
            newAction.SortOrdinal = createNewSubPage.SortOrdinal + 1;
            newAction.doRenderToString = AddNewsArticleEditMenuRender;

            pageToAddCommandTo.EditMenu.addCustomActionItem(newAction);
        }

        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] param)
        {            
            NewsArticleDb db = new NewsArticleDb();
            NewsArticleDb.NewsArticleAggregatorData entity = db.fetchNewsAggregator(page, identifier, langToRenderFor, true);

            string ProjectSummaryId = "ProjectSummary_" + page.ID.ToString() + "_" + identifier.ToString() + "_" + langToRenderFor.shortCode;

            // ------- CHECK THE FORM FOR ACTIONS
            string action = PageUtils.getFromForm(ProjectSummaryId + "_Action", "");
            if (action.Trim().ToLower() == "update")
            {
                // save the data to the database
                int id = PageUtils.getFromForm(ProjectSummaryId + "_ProjectSummaryId", -1);
                int newDefaultYear = PageUtils.getFromForm("defaultYear_" + ProjectSummaryId, -1);

                entity.YearToDisplay = newDefaultYear;

                bool b = db.updateNewsAggregator(page, identifier, langToRenderFor, entity);
                if (!b)
                    writer.Write("Error saving updates to database!");
            }

            // ------- START RENDERING
            // note: no need to put in the <form></form> tags.

            StringBuilder html = new StringBuilder();
            html.Append("<p><strong>News Aggregator Display Settings:</strong></p>");

            html.Append("<table>");
            string dropDownHtml = PageUtils.getInputTextHtml("defaultYear_" + ProjectSummaryId, "defaultYear_" + ProjectSummaryId, entity.YearToDisplay.ToString(), 7, 4);

            html.Append("<tr><td>Default Year to display summaries for: (-1 = all years)</td>");
            html.Append("<td>" + dropDownHtml + "</td></tr>");

            html.Append("</table>");

            html.Append("<input type=\"hidden\" name=\"" + ProjectSummaryId + "_Action\" value=\"update\">");
            html.Append("<input type=\"hidden\" name=\"" + ProjectSummaryId + "_ProjectSummaryId\" value=\"" + page.ID.ToString() + "\">");

            writer.WriteLine(html.ToString());
        }

        public class RenderParameters
        {
            public static RenderParameters fromParamList(string[] paramList, NewsArticleDb.NewsArticleAggregatorData aggData)
            {
                return new RenderParameters(paramList, aggData);
            }

            /// <summary>
            /// {0} = News Date in format "MMM d, yyyy"
            /// {1} = News Title
            /// {2} = Full news article view URL
            /// {3} = Article Snippet
            /// </summary>
            public string ArticleDisplayFormat = "<strong>{1}</strong> ({0}) &#160;<a class=\"readNewsArticle\" href=\"{2}\">{3}</a><br /><br />";

            public int NumCharactersToShowInSnippet = 100;

            public NewsArticleDb.NewsArticleAggregatorData AggregatorData;
                        

            /// <summary>
            /// Set to Int32.MinValue for the current page.
            /// </summary>
            public int PageIdToGatherNewsFrom = Int32.MinValue;

            /// <summary>
            /// if set to false, only gathers files from child pages. If true, gathers from all 
            /// </summary>
            public bool RecursiveGatherNews = false;                          

            public RenderParameters(string[] paramList, NewsArticleDb.NewsArticleAggregatorData aggData)
            {
                if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
                {
                    ArticleDisplayFormat = PlaceholderUtils.getParameterValue("displayformat", ArticleDisplayFormat, paramList);
                    NumCharactersToShowInSnippet = PlaceholderUtils.getParameterValue("snippetchars", NumCharactersToShowInSnippet, paramList);
                    AggregatorData = aggData;
                    PageIdToGatherNewsFrom = PlaceholderUtils.getParameterValue("gatherfrompageid", Int32.MinValue, paramList);
                    RecursiveGatherNews = PlaceholderUtils.getParameterValue("gatherrecusive", RecursiveGatherNews, paramList);                    
                }
                else
                    throw new ArgumentException("Invalid CmsTemplateEngineVersion");

            } // constructor

        } // RenderParameters 

        private class NewsAggItem
        {
            public DateTime NewsDate;
            public string PageDisplayURL;            
            public string Title;
            public string NewsArticleHtml;

            public NewsAggItem(DateTime newsdate, string pageDisplayURL, string title, string newsarticlehtml)
            {
                NewsDate = newsdate;
                PageDisplayURL = pageDisplayURL;                
                Title = title;
                NewsArticleHtml = newsarticlehtml;                
            } // constructor

            public string GetContentHash()
            {
                StringBuilder ret = new StringBuilder();
                ret.Append(NewsDate.Ticks.ToString());
                ret.Append(PageDisplayURL);                
                ret.Append(Title);
                ret.Append(NewsArticleHtml);                
                return ret.ToString();
            }

            private string getReadArticleText(CmsLanguage lang)
            {
                return CmsConfig.getConfigValue("NewsArticle.ReadArticleText", "read article", lang);
            }


            public string getHtmlDisplay(RenderParameters renderParams, CmsLanguage lang)
            {
                StringBuilder html = new StringBuilder();

                string dateDisplay = this.NewsDate.ToString("MMM d yyyy");
                string detailsUrl = this.PageDisplayURL;
                string newsTitle = this.Title;
                string readArticle = getReadArticleText(lang);
                string snippet = StringUtils.StripHTMLTags(this.NewsArticleHtml);
                snippet = snippet.Trim();
                int snippetLen = Math.Min(renderParams.NumCharactersToShowInSnippet, snippet.Length);
                snippet = snippet.Substring(0, snippetLen);

                string FormattedDisplay = String.Format(renderParams.ArticleDisplayFormat, dateDisplay, newsTitle, detailsUrl, readArticle, snippet);

                return FormattedDisplay;
            }

            public static NewsAggItem[] SortByNewsDate(NewsAggItem[] toSort)
            {
                List<NewsAggItem> ret = new List<NewsAggItem>(toSort);
                ret.Sort(CompareNewsByDate);
                ret.Reverse(); // the most recent at the top
                return ret.ToArray();
            }

            private static int CompareNewsByDate(NewsAggItem x, NewsAggItem y)
            {
                return DateTime.Compare(x.NewsDate, y.NewsDate);
            }

            public static bool ArrayContainsNews(NewsAggItem[] haystack, NewsAggItem newsToFind)
            {
                Dictionary<string, NewsAggItem> hash = new Dictionary<string, NewsAggItem>();
                foreach (NewsAggItem f in haystack)
                {
                    hash.Add(f.GetContentHash(), f);
                }

                return hash.ContainsKey(newsToFind.GetContentHash());
            }

            public static NewsAggItem FromNewsArticleDetailsData(NewsArticleDb.NewsArticleDetailsData sourceDetails)
            {                 
                CmsPage detailsPage = CmsContext.getPageById(sourceDetails.DetailsPageId);
                DateTime dateOfNews = sourceDetails.DateOfNews;
                string PageDisplayURL = detailsPage.getUrl(sourceDetails.Lang);                
                string Title = detailsPage.getTitle(sourceDetails.Lang);
                string NewsArticleHtml = detailsPage.renderPlaceholdersToString("HtmlContent", sourceDetails.Lang, CmsPage.RenderPlaceholderFilterAction.RunAllPageAndPlaceholderFilters);

                return new NewsAggItem(dateOfNews, PageDisplayURL, Title, NewsArticleHtml);
            }

            public static NewsAggItem[] FromNewsArticleDetailsData(NewsArticleDb.NewsArticleDetailsData[] sourceDetails)
            {
                List<NewsAggItem> ret = new List<NewsAggItem>();
                foreach (NewsArticleDb.NewsArticleDetailsData news in sourceDetails)
                {
                    ret.Add(FromNewsArticleDetailsData(news));
                }
                return ret.ToArray();
            }

            public static NewsAggItem[] RemoveDuplicates(List<NewsAggItem> list)
            {
                Dictionary<string, NewsAggItem> ret = new Dictionary<string, NewsAggItem>();
                foreach (NewsAggItem item in list)
                {
                    string key = item.GetContentHash();
                    if (!ret.ContainsKey(key))
                        ret.Add(key, item);
                } // foreach

                return new List<NewsAggItem>(ret.Values).ToArray();

            } // RemoveDuplicates


        } // NewsAggItem class

        private NewsAggItem[] FetchAutoAggregatedNewsArticleDetails(CmsPage aggregatorPage, int aggIdentifier, CmsLanguage aggLang, RenderParameters renderParams)
        {
            CmsPage rootPageToGatherFrom = aggregatorPage;
            if (renderParams.PageIdToGatherNewsFrom >= 0)
                rootPageToGatherFrom = CmsContext.getPageById(renderParams.PageIdToGatherNewsFrom);

            CmsContext.PageGatheringMode gatherMode = CmsContext.PageGatheringMode.ChildPagesOnly;
            if (renderParams.RecursiveGatherNews)
                gatherMode = CmsContext.PageGatheringMode.FullRecursion;
            
            CmsPage[] newsDetailsPages = CmsContext.getAllPagesWithPlaceholder("NewsArticleDetails", rootPageToGatherFrom, gatherMode);
            NewsArticleDb.NewsArticleDetailsData[] newsToShow = new NewsArticleDb().getNewsDetailsByYear(newsDetailsPages, renderParams.AggregatorData.YearToDisplay, aggLang);

            return NewsAggItem.FromNewsArticleDetailsData(newsToShow);
        }

        private NewsAggItem[] FetchAllNewsAggItems(CmsPage aggregatorPage, int aggIdentifier, CmsLanguage aggLang, RenderParameters renderParams)
        {
            List<NewsAggItem> ret = new List<NewsAggItem>();
            ret.AddRange(FetchAutoAggregatedNewsArticleDetails(aggregatorPage, aggIdentifier, aggLang, renderParams));            

            // -- fetch all manually added news items here

            return ret.ToArray();
        }

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] param)
        {
            AddNewsArticleCommandToEditMenu(page, page);
            
            
            CmsPage currentPage = CmsContext.currentPage;
            StringBuilder html = new StringBuilder();

            NewsArticleDb db = new NewsArticleDb();
            NewsArticleDb.NewsArticleAggregatorData newsAggregator = db.fetchNewsAggregator(page, identifier, langToRenderFor ,true);            

            RenderParameters renderParams = RenderParameters.fromParamList(param, newsAggregator);

            NewsAggItem[] newsToShow = FetchAllNewsAggItems(page, identifier, langToRenderFor, renderParams);            

            // -- display results
            html.Append(getHtmlForSummaryView(newsToShow, renderParams, langToRenderFor));
            writer.Write(html.ToString());
        }


        private string getHtmlForSummaryView(NewsAggItem[] newsDetails, RenderParameters renderParams, CmsLanguage lang)
        {            
            StringBuilder html = new StringBuilder();

            int displayYear = renderParams.AggregatorData.YearToDisplay;

            if (newsDetails.Length == 0)
            {
                if (displayYear < 0)
                    html.Append("<p><strong>" + getNoNewsText(lang) + "</strong>");
                else
                    html.Append("<p><strong>" + getNoNewsForYearText(lang) + displayYear.ToString() + ".</strong>");
            }
            else
            {
                newsDetails = NewsAggItem.SortByNewsDate(newsDetails);
                
                bool showYearTitles = false;
                if (displayYear == -1)
                    showYearTitles = true;

                int previousYearTitle = -1;
                int previousMonthTitle = -1;
                bool monthULStarted = false;

                foreach (NewsAggItem news in newsDetails)
                {
                    if (displayYear >= 0 && news.NewsDate.Year != displayYear)
                        continue; // skip this item.

                    if (showYearTitles && (previousYearTitle == -1 || news.NewsDate.Year != previousYearTitle))
                    {
                        if (monthULStarted)
                            html.Append("</ul>");

                        html.Append("<h2>");
                        html.Append(news.NewsDate.Year);
                        html.Append("</h2>");
                        previousYearTitle = news.NewsDate.Year;
                    }

                    if (previousMonthTitle == -1 || news.NewsDate.Month != previousMonthTitle)
                    {
                        if (monthULStarted)
                            html.Append("</ul>");
                        if (showYearTitles)
                            html.Append("<strong>");
                        else
                            html.Append("<h2>");
                        html.Append(news.NewsDate.ToString("MMMM"));
                        if (showYearTitles)
                            html.Append("</strong>");
                        else
                            html.Append("</h2>");
                        html.Append("<ul>");
                        monthULStarted = true;
                        previousMonthTitle = news.NewsDate.Month;
                    }

                    string FormattedDisplay = news.getHtmlDisplay(renderParams, lang);
                    html.Append(FormattedDisplay);
                } // foreach
                if (monthULStarted)
                    html.Append("</ul>");
            }
            return html.ToString();
        }

        private string getNoNewsText(CmsLanguage lang)
        {
            string defaultTxt = "No news postings are currently available.";
            return CmsConfig.getConfigValue("NewsArticle.NoNewsText", defaultTxt, lang);
        }

        private string getNoNewsForYearText(CmsLanguage lang)
        {
            string defaultTxt = "No news postings are currently available.";
            return CmsConfig.getConfigValue("NewsArticle.NoNewsTextForText", defaultTxt, lang);
        }
        

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            List<Rss.RssItem> ret = new List<Rss.RssItem>();

            // -- get the news
            NewsArticleDb.NewsArticleAggregatorData aggData = (new NewsArticleDb()).fetchNewsAggregator(page, placeholderDefinition.Identifier, langToRenderFor, true);
            RenderParameters renderParams = RenderParameters.fromParamList(placeholderDefinition.ParamList, aggData);
            NewsAggItem[] newsItems = FetchAllNewsAggItems(page, placeholderDefinition.Identifier, langToRenderFor, renderParams);

            int currYear = renderParams.AggregatorData.YearToDisplay;

            foreach (NewsAggItem newsItem in newsItems)
            {
                if (currYear < 0 || newsItem.NewsDate.Year == currYear)
                {                    
                    Rss.RssItem rssItem = new Rss.RssItem();
                    rssItem = InitRssItem(rssItem, page, langToRenderFor);

                    rssItem.Title = newsItem.Title;

                    rssItem.Link = new Uri(newsItem.PageDisplayURL, UriKind.RelativeOrAbsolute);
                    rssItem.PubDate_GMT = newsItem.NewsDate.ToUniversalTime();

                    rssItem.Description = newsItem.NewsArticleHtml;

                    ret.Add(rssItem);
                }
            }


            return ret.ToArray();
        } // GetRssFeedItems

    }
}
