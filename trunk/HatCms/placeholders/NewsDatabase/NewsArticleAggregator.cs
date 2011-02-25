using System;
using System.Web.UI;
using HatCMS.Placeholders;
using System.Collections.Generic;
using System.Text;
using Hatfield.Web.Portal;
using System.Collections.Specialized;

namespace HatCMS.placeholders.NewsDatabase
{
    public class NewsArticleAggregator : BaseCmsPlaceholder
    {
        /// <summary>
        /// If the CmsPage contains a "NewsArticleAggregator", then it is a NewsArticleAggregator page
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static bool isNewsArticleAggregator(CmsPage page)
        {
            return (StringUtils.IndexOf(page.getAllPlaceholderNames(), "NewsArticleAggregator", StringComparison.CurrentCultureIgnoreCase) > -1);
        }

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
            ret.Add(new CmsDatabaseTableDependency("NewsArticleAggregator"));
            ret.Add(new CmsDatabaseTableDependency("NewsArticleDetails"));

            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("NewsArticle.ReadArticleText"));
            ret.Add(new CmsConfigItemDependency("NewsArticle.NoNewsText"));
            ret.Add(new CmsConfigItemDependency("NewsArticle.NoNewsTextForText"));

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
            string newPageTemplate = CmsConfig.getConfigValue("NewsArticle.DetailsTemplateName", "NewsArticleDetails");

            newAction.CreateNewPageOptions = CmsCreateNewPageOptions.GetInstanceWithNoUserPrompts(newPageName, newPageTitle, newPageMenuTitle, newPageSearchEngineDescription, newPageShowInMenu, newPageTemplate, newsArticleAggregatorPage.ID);

            newAction.CreateNewPageOptions.ParentPageId = newsArticleAggregatorPage.ID;
            newAction.SortOrdinal = createNewSubPage.SortOrdinal + 1;
            newAction.doRenderToString = AddNewsArticleEditMenuRender;

            pageToAddCommandTo.EditMenu.addCustomActionItem(newAction);
        }

        public override RevertToRevisionResult revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] param)
        {
            // CmsContext.setCurrentCultureInfo(langToRenderFor);
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
            string s = PageUtils.getInputTextHtml("defaultYear_" + ProjectSummaryId, "defaultYear_" + ProjectSummaryId, entity.YearToDisplay.ToString(), 7, 4);

            html.Append("<tr><td>Default Year to display summaries for: (-1 = all years)</td>");
            html.Append("<td>" + s + "</td></tr>");

            html.Append("</table>");

            html.Append("<input type=\"hidden\" name=\"" + ProjectSummaryId + "_Action\" value=\"update\">");
            html.Append("<input type=\"hidden\" name=\"" + ProjectSummaryId + "_ProjectSummaryId\" value=\"" + page.ID.ToString() + "\">");

            writer.WriteLine(html.ToString());
        }

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] param)
        {
            AddNewsArticleCommandToEditMenu(page, page);
            // CmsContext.setCurrentCultureInfo(langToRenderFor);
            CmsPage currentPage = CmsContext.currentPage;
            StringBuilder html = new StringBuilder();

            NewsArticleDb db = new NewsArticleDb();
            NewsArticleDb.NewsArticleAggregatorData newsAggregator = db.fetchNewsAggregator(page, identifier, langToRenderFor ,true);

            int currYear = PageUtils.getFromForm("yr", Int32.MinValue);
            if (currYear == Int32.MinValue)
                currYear = newsAggregator.YearToDisplay;

            List<NewsArticleDb.NewsArticleDetailsData> articleList = new List<NewsArticleDb.NewsArticleDetailsData>();
            Dictionary<CmsPage, CmsPlaceholderDefinition[]> childPages = page.getPlaceholderDefinitionsForChildPages("NewsArticleDetails");
            foreach (CmsPage childPage in childPages.Keys)
            {
                CmsPlaceholderDefinition[] def = childPages[childPage];
                NewsArticleDb.NewsArticleDetailsData entity = db.fetchNewsDetails(childPage, def[0].Identifier, langToRenderFor, true);
                articleList.Add(entity);
            }

            // -- display results
            html.Append(getHtmlForSummaryView(articleList.ToArray(), currYear, langToRenderFor));
            writer.Write(html.ToString());
        }

        private string getReadArticleText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("NewsArticle.ReadArticleText", "read article", lang);
        }

        private string getHtmlForSummaryView(NewsArticleDb.NewsArticleDetailsData[] newsDetails, int displayYear, CmsLanguage lang)
        {
            sortByDateOfNews(newsDetails);
            StringBuilder html = new StringBuilder();

            if (newsDetails.Length == 0)
            {
                if (displayYear == -1)
                    html.Append("<p><strong>" + getNoNewsText(lang) + "</strong>");
                else
                    html.Append("<p><strong>" + getNoNewsForYearText(lang) + displayYear.ToString() + ".</strong>");
            }
            else
            {
                bool showYearTitles = false;
                if (displayYear == -1)
                    showYearTitles = true;

                int previousYearTitle = -1;
                int previousMonthTitle = -1;
                bool monthULStarted = false;

                foreach (NewsArticleDb.NewsArticleDetailsData news in newsDetails)
                {
                    if (showYearTitles && (previousYearTitle == -1 || news.DateOfNews.Year != previousYearTitle))
                    {
                        if (monthULStarted)
                            html.Append("</ul>");

                        html.Append("<h2>");
                        html.Append(news.DateOfNews.Year);
                        html.Append("</h2>");
                        previousYearTitle = news.DateOfNews.Year;
                    }

                    if (previousMonthTitle == -1 || news.DateOfNews.Month != previousMonthTitle)
                    {
                        if (monthULStarted)
                            html.Append("</ul>");
                        if (showYearTitles)
                            html.Append("<strong>");
                        else
                            html.Append("<h2>");
                        html.Append(news.DateOfNews.ToString("MMMM"));
                        if (showYearTitles)
                            html.Append("</strong>");
                        else
                            html.Append("</h2>");
                        html.Append("<ul>");
                        monthULStarted = true;
                        previousMonthTitle = news.DateOfNews.Month;
                    }

                    // -- create the details url
                    NameValueCollection paramList = new NameValueCollection();
                    //paramList.Add(NewsDetails.CurrentNewsIdFormName, news.PageId.ToString());

                    CmsPage childPage = new CmsPageDb().getPage(news.PageId);
                    string detailsUrl = CmsContext.getUrlByPagePath(childPage.Path, paramList);
                    string newsTitle = childPage.getTitle(news.Lang);
                    string readArticle = getReadArticleText(news.Lang);

                    string FormattedDisplay = String.Format(NewsArticleDb.NewsArticleAggregatorData.DisplayFormat, news.DateOfNews.ToString("MMM d yyyy"), newsTitle, detailsUrl, readArticle);
                    html.Append(FormattedDisplay);
                } // foreach
                if (monthULStarted)
                    html.Append("</ul>");
            }
            return html.ToString();
        }

        private string getNoNewsText(CmsLanguage lang)
        {
            string defaultTxt = "No news postings are currently in the system.";
            return CmsConfig.getConfigValue("NewsArticle.NoNewsText", defaultTxt, lang);
        }

        private string getNoNewsForYearText(CmsLanguage lang)
        {
            string defaultTxt = "There is no news for the year ";
            return CmsConfig.getConfigValue("NewsArticle.NoNewsTextForText", defaultTxt, lang);
        }

        /// <summary>
        /// Sort the news article array by the Date Of News in descending order.
        /// </summary>
        /// <param name="newsArray"></param>
        protected void sortByDateOfNews(NewsArticleDb.NewsArticleDetailsData[] newsArray)
        {
            NewsArticleDb.NewsArticleDetailsDataComparer comparer = new NewsArticleDb.NewsArticleDetailsDataComparer();
            comparer.Field = NewsArticleDb.NewsArticleDetailsDataComparer.CompareType.DateOfNews;
            Array.Sort(newsArray, comparer);
            Array.Reverse(newsArray);
        }
    }
}
