using System;
using System.Web.UI;
using HatCMS.Placeholders;
using System.Collections.Generic;
using System.Text;
using Hatfield.Web.Portal;
using System.Collections.Specialized;

namespace HatCMS.placeholders.Procurement
{
    public class ProcurementAggregator : BaseCmsPlaceholder
    {
        /// <summary>
        /// If the CmsPage contains a "ProcurementAggregator", then it is a ProcurementAggregator page
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static bool isProcurementAggregator(CmsPage page)
        {
            return (StringUtils.IndexOf(page.getAllPlaceholderNames(), "ProcurementAggregator", StringComparison.CurrentCultureIgnoreCase) > -1);
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
            ret.Add(new CmsDatabaseTableDependency("ProcurementAggregator"));
            ret.Add(new CmsDatabaseTableDependency("ProcurementDetails"));

            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("Procurement.ReadArticleText"));
            ret.Add(new CmsConfigItemDependency("Procurement.NoProcurementText"));
            ret.Add(new CmsConfigItemDependency("Procurement.NoProcurementForText"));

            return ret.ToArray();
        }

        /// <summary>
        /// Render the "Add a Procurement opportunity"
        /// </summary>
        /// <param name="action"></param>
        /// <param name="pageToRenderFor"></param>
        /// <param name="langToRenderFor"></param>
        /// <returns></returns>
        protected static string AddProcurementEditMenuRender(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
        {
            NameValueCollection createPageParams = action.CreateNewPageOptions.GetCreatePagePopupParams();
            if (action.CreateNewPageOptions.RequiresUserInput())
                return CmsPageEditMenu.DefaultStandardActionRenderers.RenderPopupLink("CreateNewPagePath", "/_admin/createPage", createPageParams, pageToRenderFor, langToRenderFor, "<strong>Add</strong> a Procurement opportunity", 500, 400);
            else
                return CmsPageEditMenu.DefaultStandardActionRenderers.RenderLink("CreateNewPagePath", "/_admin/createPage", createPageParams, pageToRenderFor, langToRenderFor, "<strong>Add</strong> a Procurement opportunity");
        }

        /// <summary>
        /// Adds the "Add a Procurement opportunity" menu item to the Edit Menu.
        /// </summary>
        /// <param name="pageToAddCommandTo"></param>
        /// <param name="ProcurementAggregatorPage"></param>
        public static void AddProcurementCommandToEditMenu(CmsPage pageToAddCommandTo, CmsPage ProcurementAggregatorPage)
        {
            // -- only add the command if the user can author
            if (!CmsContext.currentUserCanAuthor)
                return;

            // -- base the command off the existing "create new sub-page" command
            CmsPageEditMenuAction createNewSubPage = pageToAddCommandTo.EditMenu.getActionItem(CmsEditMenuActionItem.CreateNewPage);

            if (createNewSubPage == null)
                throw new Exception("Fatal Error in in ProcurementAggregator placeholder - could not get the existing CreateNewPage action");

            CmsPageEditMenuAction newAction = createNewSubPage.Copy(); // copy everything from the CreateNewPage entry            

            // -- configure this command to not prompt authors for any information.
            //    the minimum information needed to create a page is the new page's filename (page.name)
            //      -- get the next unique filename            
            string newPageName = "";
            for (int ProcurementNum = 1; ProcurementNum < int.MaxValue; ProcurementNum++)
            {
                string pageNameToTest = "Procurement opportunity " + ProcurementNum.ToString();
                if (!CmsContext.childPageWithNameExists(ProcurementAggregatorPage.ID, pageNameToTest))
                {
                    newPageName = pageNameToTest;
                    break;
                }
            }

            string newPageTitle = "";
            string newPageMenuTitle = "";
            string newPageSearchEngineDescription = "";
            bool newPageShowInMenu = false;
            string newPageTemplate = CmsConfig.getConfigValue("Procurement.DetailsTemplateName", "ProcurementDetails");

            newAction.CreateNewPageOptions = CmsCreateNewPageOptions.GetInstanceWithNoUserPrompts(newPageName, newPageTitle, newPageMenuTitle, newPageSearchEngineDescription, newPageShowInMenu, newPageTemplate, ProcurementAggregatorPage.ID);

            newAction.CreateNewPageOptions.ParentPageId = ProcurementAggregatorPage.ID;
            newAction.SortOrdinal = createNewSubPage.SortOrdinal + 1;
            newAction.doRenderToString = AddProcurementEditMenuRender;

            pageToAddCommandTo.EditMenu.addCustomActionItem(newAction);
        }

        public override bool revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return true; // this placeholder doesn't implement revisions
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] param)
        {
            // CmsContext.setCurrentCultureInfo(langToRenderFor);
            ProcurementDb db = new ProcurementDb();
            ProcurementDb.ProcurementAggregatorData entity = db.fetchProcurementAggregator(page, identifier, langToRenderFor, true);

            string ProjectSummaryId = "ProjectSummary_" + page.ID.ToString() + "_" + identifier.ToString() + "_" + langToRenderFor.shortCode;

            // ------- CHECK THE FORM FOR ACTIONS
            string action = PageUtils.getFromForm(ProjectSummaryId + "_Action", "");
            if (action.Trim().ToLower() == "update")
            {
                // save the data to the database
                int id = PageUtils.getFromForm(ProjectSummaryId + "_ProjectSummaryId", -1);
                int newDefaultYear = PageUtils.getFromForm("defaultYear_" + ProjectSummaryId, -1);

                entity.YearToDisplay = newDefaultYear;

                bool b = db.updateProcurementAggregator(page, identifier, langToRenderFor, entity);
                if (!b)
                    writer.Write("Error saving updates to database!");
            }

            // ------- START RENDERING
            // note: no need to put in the <form></form> tags.

            StringBuilder html = new StringBuilder();
            html.Append("<p><strong>Procurement Aggregator Display Settings:</strong></p>");

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
            AddProcurementCommandToEditMenu(page, page);
            // CmsContext.setCurrentCultureInfo(langToRenderFor);
            CmsPage currentPage = CmsContext.currentPage;
            StringBuilder html = new StringBuilder();

            ProcurementDb db = new ProcurementDb();
            ProcurementDb.ProcurementAggregatorData ProcurementAggregator = db.fetchProcurementAggregator(page, identifier, langToRenderFor ,true);

            int currYear = PageUtils.getFromForm("yr", Int32.MinValue);
            if (currYear == Int32.MinValue)
                currYear = ProcurementAggregator.YearToDisplay;

            List<ProcurementDb.ProcurementDetailsData> articleList = new List<ProcurementDb.ProcurementDetailsData>();
            Dictionary<CmsPage, CmsPlaceholderDefinition[]> childPages = page.getPlaceholderDefinitionsForChildPages("ProcurementDetails");
            foreach (CmsPage childPage in childPages.Keys)
            {
                CmsPlaceholderDefinition[] def = childPages[childPage];
                ProcurementDb.ProcurementDetailsData entity = db.fetchProcurementDetails(childPage, def[0].Identifier, langToRenderFor, true);
                articleList.Add(entity);
            }

            // -- display results
            html.Append(getHtmlForSummaryView(articleList.ToArray(), currYear, langToRenderFor));
            writer.Write(html.ToString());
        }

        private string getReadArticleText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("Procurement.ReadArticleText", "read article", lang);
        }

        private string getHtmlForSummaryView(ProcurementDb.ProcurementDetailsData[] ProcurementDetails, int displayYear, CmsLanguage lang)
        {
            sortByDateOfProcurement(ProcurementDetails);
            StringBuilder html = new StringBuilder();

            if (ProcurementDetails.Length == 0)
            {
                if (displayYear == -1)
                    html.Append("<p><strong>" + getNoProcurementText(lang) + "</strong>");
                else
                    html.Append("<p><strong>" + getNoProcurementForYearText(lang) + displayYear.ToString() + ".</strong>");
            }
            else
            {
                bool showYearTitles = false;
                if (displayYear == -1)
                    showYearTitles = true;

                int previousYearTitle = -1;
                int previousMonthTitle = -1;
                bool monthULStarted = false;

                foreach (ProcurementDb.ProcurementDetailsData Procurement in ProcurementDetails)
                {
                    if (showYearTitles && (previousYearTitle == -1 || Procurement.DateOfProcurement.Year != previousYearTitle))
                    {
                        if (monthULStarted)
                            html.Append("</ul>");

                        html.Append("<h2>");
                        html.Append(Procurement.DateOfProcurement.Year);
                        html.Append("</h2>");
                        previousYearTitle = Procurement.DateOfProcurement.Year;
                    }

                    if (previousMonthTitle == -1 || Procurement.DateOfProcurement.Month != previousMonthTitle)
                    {
                        if (monthULStarted)
                            html.Append("</ul>");
                        if (showYearTitles)
                            html.Append("<strong>");
                        else
                            html.Append("<h2>");
                        html.Append(Procurement.DateOfProcurement.ToString("MMMM"));
                        if (showYearTitles)
                            html.Append("</strong>");
                        else
                            html.Append("</h2>");
                        html.Append("<ul>");
                        monthULStarted = true;
                        previousMonthTitle = Procurement.DateOfProcurement.Month;
                    }

                    // -- create the details url
                    NameValueCollection paramList = new NameValueCollection();

                    CmsPage childPage = new CmsPageDb().getPage(Procurement.PageId);
                    string detailsUrl = CmsContext.getUrlByPagePath(childPage.Path, paramList);
                    string ProcurementTitle = childPage.getTitle(Procurement.Lang);
                    string readArticle = getReadArticleText(Procurement.Lang);

                    string FormattedDisplay = String.Format(ProcurementDb.ProcurementAggregatorData.DisplayFormat, Procurement.DateOfProcurement.ToString("MMM d yyyy"), ProcurementTitle, detailsUrl, readArticle);
                    html.Append(FormattedDisplay);
                } // foreach
                if (monthULStarted)
                    html.Append("</ul>");
            }
            return html.ToString();
        }

        private string getNoProcurementText(CmsLanguage lang)
        {
            string defaultTxt = "No Procurement opportunities are currently in the system.";
            return CmsConfig.getConfigValue("Procurement.NoProcurementText", defaultTxt, lang);
        }

        private string getNoProcurementForYearText(CmsLanguage lang)
        {
            string defaultTxt = "There is no pocurement item for the year ";
            return CmsConfig.getConfigValue("Procurement.NoProcurementTextForText", defaultTxt, lang);
        }

        /// <summary>
        /// Sort the Procurement opportunity array by the Date Of Procurement in descending order.
        /// </summary>
        /// <param name="ProcurementArray"></param>
        protected void sortByDateOfProcurement(ProcurementDb.ProcurementDetailsData[] ProcurementArray)
        {
            ProcurementDb.ProcurementDetailsDataComparer comparer = new ProcurementDb.ProcurementDetailsDataComparer();
            comparer.Field = ProcurementDb.ProcurementDetailsDataComparer.CompareType.DateOfProcurement;
            Array.Sort(ProcurementArray, comparer);
            Array.Reverse(ProcurementArray);
        }
    }
}
