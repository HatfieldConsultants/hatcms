using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
    public class JobPostingAggregatorData
    {
        /// <summary>
        /// The primary key for this entry.
        /// </summary>
        public int JobSummaryId = -1;
        /// <summary>
        /// the location to display - links to the JobLocation class/table
        /// </summary>
        public int LocationId = -1;
    }
    
    public class JobPostingAggregator: BaseCmsPlaceholder
    {
        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            string[] requiredCols = new string[] {"JobSummaryId", "PageId", "Identifier", "langShortCode", "locationId", "Deleted"};
            string[] removedCols = new string[] {"LocationToDisplay", "HtmlHeader"};
            ret.Add(new CmsDatabaseTableDependency("jobsummary",requiredCols, removedCols));

            ret.Add(new CmsConfigItemDependency("JobPosting.DetailsTemplateName", CmsDependency.ExistsMode.MustExist));
            ret.Add(new CmsTemplateDependency(CmsConfig.getConfigValue("JobPosting.DetailsTemplateName", "JobPosting")));

            return ret.ToArray();
        }


        public override bool revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return true; // no revisions are implemented in this placeholder.
        }


        private static string AddJobPostingEditMenuRender(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
        {                        
            NameValueCollection createPageParams = action.CreateNewPageOptions.GetCreatePagePopupParams();
            if (action.CreateNewPageOptions.RequiresUserInput())
            {
                return CmsPageEditMenu.DefaultStandardActionRenderers.RenderPopupLink("CreateNewPagePath", "/_admin/createPage", createPageParams, pageToRenderFor, langToRenderFor, "<strong>Add</strong> a new job", 500, 400);
            }
            else
            {
                return CmsPageEditMenu.DefaultStandardActionRenderers.RenderLink("CreateNewPagePath", "/_admin/createPage", createPageParams, pageToRenderFor, langToRenderFor, "<strong>Add</strong> a new job");
            }                        
        }

        /// <summary>
        /// Adds the "Add a new job" menu item to the Edit Menu.
        /// </summary>
        /// <param name="pageToAddCommandTo"></param>
        /// <param name="jobAggregatorPage"></param>
        public static void AddJobPostingCommandToEditMenu(CmsPage pageToAddCommandTo, CmsPage jobAggregatorPage)
        {
            // -- only add the command if the user can author
            if (!CmsContext.currentUserCanAuthor)
                return;

            // -- base the command off the existing "create new sub-page" command
            CmsPageEditMenuAction createNewSubPage = pageToAddCommandTo.EditMenu.getActionItem(CmsEditMenuActionItem.CreateNewPage);

            if (createNewSubPage == null)
                throw new Exception("Fatal Error in in JobPostingAggregator placeholder - could not get the existing CreateNewPage action");

            CmsPageEditMenuAction CreateNewJobMenuAction = createNewSubPage.Copy(); // copy everything from the CreateNewPage entry            

            // -- configure this command to not prompt authors for any information.
            //    the minimum information needed to create a page is the new page's filename (page.name)
            //      -- get the next unique filename            
            string newPageName = "";
            for (int jobNum = 1; jobNum < int.MaxValue; jobNum++)
            {
                string pageNameToTest = "Job " + jobNum.ToString();
                if (!CmsContext.childPageWithNameExists(jobAggregatorPage.ID, pageNameToTest))
                {
                    newPageName = pageNameToTest;
                    break;
                }
            }


            string newPageTitle = "";
            string newPageMenuTitle = "";
            string newPageSearchEngineDescription = "";
            bool newPageShowInMenu = false;
            string newPageTemplate = CmsConfig.getConfigValue("JobPosting.DetailsTemplateName", "JobPosting");

            CreateNewJobMenuAction.CreateNewPageOptions = CmsCreateNewPageOptions.GetInstanceWithNoUserPrompts(newPageName, newPageTitle, newPageMenuTitle, newPageSearchEngineDescription, newPageShowInMenu, newPageTemplate, jobAggregatorPage.ID);            

            CreateNewJobMenuAction.CreateNewPageOptions.ParentPageId = jobAggregatorPage.ID;
            CreateNewJobMenuAction.SortOrdinal = createNewSubPage.SortOrdinal + 1;
            CreateNewJobMenuAction.doRenderToString = AddJobPostingEditMenuRender;                

            pageToAddCommandTo.EditMenu.addCustomActionItem(CreateNewJobMenuAction);
        }

        /// <summary>
        /// Renders the placeholder in ViewMode
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="langToRenderFor"></param>
        /// <param name="paramList"></param>
        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            AddJobPostingCommandToEditMenu(page, page);

            Dictionary<CmsPage, CmsPlaceholderDefinition[]> childJobPages = page.getPlaceholderDefinitionsForChildPages("JobPostingDetails");
            JobPostingLocation[] allLocations = JobPostingLocation.FetchAll();
            JobPostingLocation theAllLocationsLocation = JobPostingLocation.getAllLocations(allLocations);

            JobPostingDb db = new JobPostingDb();
            JobPostingAggregatorData aggregatorData = db.getJobPostingAggregatorData(page, identifier, langToRenderFor, true);

            // -- grab all the details for all child job pages.
            
            Dictionary<CmsPage, List<JobPostingDetailsData>> childJobDetails = new Dictionary<CmsPage, List<JobPostingDetailsData>>();
            foreach(CmsPage childPage in childJobPages.Keys)
            {
                childJobDetails[childPage] = new List<JobPostingDetailsData>();
                foreach(CmsPlaceholderDefinition phDef in childJobPages[childPage])
                {
                    JobPostingDetailsData dataObj = db.getJobPostingDetailsData(childPage, phDef.Identifier, langToRenderFor, true);
                    childJobDetails[childPage].Add(dataObj);
                }
            } // foreach child page

            // -- do HTML output only for the location
            StringBuilder html = new StringBuilder();
            foreach (JobPostingLocation location in allLocations)
            {
                if (aggregatorData.LocationId < 0 || aggregatorData.LocationId == theAllLocationsLocation.JobLocationId || location.JobLocationId == aggregatorData.LocationId)
                {
                    html.Append(getHtmlForJobsInLocation(childJobDetails, location, langToRenderFor));
                }
            } // foreach
            writer.Write(html.ToString());
            
        }

        private string getHtmlForJobsInLocation(Dictionary<CmsPage, List<JobPostingDetailsData>> haystack, JobPostingLocation location, CmsLanguage langToDisplay)
        {
            int numJobsToDisplay = 0;
            StringBuilder html = new StringBuilder();
            html.Append("<h2 class=\"JobLocation\">" + location.getLocationText(langToDisplay) + "</h2>" + Environment.NewLine);
            html.Append("<ul>" + Environment.NewLine);

            foreach (CmsPage jobPage in haystack.Keys)
            {
                foreach (JobPostingDetailsData job in haystack[jobPage])
                {
                    if (job.LocationId == location.JobLocationId)
                    {
                        if (job.IsExpired)
                        {
                            html.Append("<span class=\"jobExpiredNotice\" style=\"background-color: yellow; font-weight: bold\">EXPIRED:</span> ");
                        }
                        html.Append("<strong>");
                        html.Append(jobPage.Title);
                        html.Append("</strong>");
                        html.Append(" <a href=\"" + jobPage.Url + "\">full job description &raquo;</a>" + Environment.NewLine);
                        html.Append("<br><br>");
                        numJobsToDisplay++;
                    }
                } // foreach job
            } // foreach page


            html.Append("</ul>" + Environment.NewLine);
            
            if (numJobsToDisplay == 0)
                return "";

            return html.ToString();
        }

        /// <summary>
        /// Renders the placeholder in EditMode
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="langToRenderFor"></param>
        /// <param name="paramList"></param>
        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string placeholderId = "JobAggregatorDetails_" + page.ID.ToString() + "_" + identifier.ToString() + langToRenderFor.shortCode;
            JobPostingDb db = new JobPostingDb();

            JobPostingAggregatorData aggregatorData = db.getJobPostingAggregatorData(page, identifier, langToRenderFor, true);

            JobPostingLocation[] locations = JobPostingLocation.FetchAll();
            JobPostingLocation AllLocations = JobPostingLocation.getAllLocations(locations);

            // ------- CHECK THE FORM FOR ACTIONS
            string action = PageUtils.getFromForm(placeholderId + "_Action", "");
            if (action.Trim().ToLower() == "update")
            {
                // save the data to the database

                int newLocationId = PageUtils.getFromForm("location_" + placeholderId, AllLocations.JobLocationId);

                aggregatorData.LocationId = newLocationId;
                
                bool b = db.saveUpdatedJobPostingAggregatorData(aggregatorData);
                if (!b)
                    writer.Write("Error saving updates to database!");

            }
            StringBuilder html = new StringBuilder();
            html.Append("<div class=\"JobPostingAggregator Edit\">");
            html.Append("<table width=\"100%\" border=\"0\">");

            // -- Job Location drop-down

            html.Append("<tr>");
            html.Append("<td>Job Location To Display:</td>");
            html.Append("</tr>");
            html.Append("<tr>");

            html.Append("<td>" + PageUtils.getDropDownHtml("location_" + placeholderId, "location_" + placeholderId, JobPostingLocation.ToNameValueCollection(locations, CmsContext.currentLanguage, true), aggregatorData.LocationId.ToString()) + "</td>");
            html.Append("</tr>");

            html.Append("</table>" + Environment.NewLine);

            html.Append(PageUtils.getHiddenInputHtml(placeholderId + "_Action", "update"));

            html.Append("</div>" + Environment.NewLine);

            writer.Write(html.ToString());
        } // RenderEdit
    }
}
