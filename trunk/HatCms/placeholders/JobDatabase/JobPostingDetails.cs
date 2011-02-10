using System;
using System.Text;
using System.Collections.Generic;
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

    public class JobPostingDetailsData
    {
        public int JobId;
        public int LocationId;     
        public DateTime RemoveAnonAccessAt;

        public static DateTime NoExpiryDateTime = DateTime.MaxValue;

        public JobPostingDetailsData()
        {
            JobId = -1;
            LocationId = -1;
            RemoveAnonAccessAt = NoExpiryDateTime;
        }

        public bool IsExpired
        {
            get
            {
                if (DateTime.Compare(RemoveAnonAccessAt, DateTime.Now) <= 0)
                    return true;
                else
                    return false;
            }
        }

        public static JobPostingDetailsData[] GetForLocation(JobPostingDetailsData[] haystack, JobPostingLocation locationToFind)
        {
            List<JobPostingDetailsData> ret = new List<JobPostingDetailsData>();

            foreach (JobPostingDetailsData job in haystack)
            {
                if (job.LocationId == locationToFind.JobLocationId)
                    ret.Add(job);
            } // foreach
            return ret.ToArray();
        }

    }

    public class JobPostingDetails : BaseCmsPlaceholder
    {
        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            // -- Database tables:
            string[] requiredCols = new string[] { "JobId", "PageId", "Identifier", "langShortCode", "Location", "RemoveAnonAccessAt" };
            string[] colsToRemove = new string[] { "Title", "Location", "HtmlJobDescription", "LastUpdatedDateTime" };
            ret.Add(new CmsDatabaseTableDependency("jobdetails", requiredCols, colsToRemove));
            
            ret.Add(new CmsDatabaseTableDependency("joblocations", new string[] { "JobLocationId", "LocationText", "IsAllLocations", "SortOrdinal" }));

            // Config entries:
            ret.Add(new CmsConfigItemDependency("JobPostingDetails.AllowPostingToAllLocations", CmsDependency.ExistsMode.MustExist));
            ret.Add(new CmsConfigItemDependency("JobPostingDetails.IncludeLocationInDisplay", CmsDependency.ExistsMode.MustExist));
            ret.Add(new CmsConfigItemDependency("JobPostingDetails.IncludeBackLinkInDisplay", CmsDependency.ExistsMode.MustExist));
            ret.Add(new CmsConfigItemDependency("JobPostingDetails.BackToJobListingText", CmsDependency.ExistsMode.MustExist));
            ret.Add(new CmsConfigItemDependency("JobPostingDetails.LocationText", CmsDependency.ExistsMode.MustExist));
            
            // -- obsolete config entries:
            ret.Add(new CmsConfigItemDependency("JobPostingLocations", CmsDependency.ExistsMode.MustNotExist));            
            ret.Add(new CmsConfigItemDependency("DeleteJobPath", CmsDependency.ExistsMode.MustNotExist));
            return ret.ToArray();
        }



        public override RevertToRevisionResult revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented;
        }        

        private bool AllowPostingToAllLocations
        {
            get
            {
                return CmsConfig.getConfigValue("JobPostingDetails.AllowPostingToAllLocations", false);
            }
        }

        private bool IncludeLocationInDisplay
        {
            get
            {
                return CmsConfig.getConfigValue("JobPostingDetails.IncludeLocationInDisplay", true);
            }
        }


        private bool IncludeBackLinkInDisplay
        {
            get
            {
                return CmsConfig.getConfigValue("JobPostingDetails.IncludeBackLinkInDisplay", true);
            }
        }

        protected string getBackToJobListingText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("JobPostingDetails.BackToJobListingText", "Back to job listing page", lang);
        }

        protected string getLocationText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("JobPostingDetails.LocationText", "Location", lang);
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string placeholderId = "JobPostingDetails_" + page.ID.ToString() + "_" + identifier.ToString() + langToRenderFor.shortCode;
            string placeholderIdWithoutLang = "location_JobPostingDetails_" + page.ID.ToString() + "_" + identifier.ToString();
            JobPostingDb db = new JobPostingDb();
            JobPostingDetailsData postingDetails = db.getJobPostingDetailsData(page, identifier, langToRenderFor, true);

            JobPostingLocation[] locations = JobPostingLocation.FetchAll();
            JobPostingLocation AllLocations = JobPostingLocation.getAllLocations(locations);

            // ------- CHECK THE FORM FOR ACTIONS
            string action = PageUtils.getFromForm(placeholderId + "_Action", "");
            if (action.Trim().ToLower() == "update")
            {
                // save the data to the database

                int newLocationId = PageUtils.getFromForm("location_" + placeholderId, AllLocations.JobLocationId);
                string dateStr = PageUtils.getFromForm("RemoveAnonAccessAt_" + placeholderId,"");

                postingDetails.LocationId = newLocationId;
                postingDetails.RemoveAnonAccessAt = CmsConfig.parseDateInDateInputFormat(dateStr, postingDetails.RemoveAnonAccessAt);

                bool b = db.saveUpdatedJobPostingDetailsData(postingDetails);
                if (!b)
                    writer.Write("Error saving updates to database!");

            }
            StringBuilder html = new StringBuilder();
            html.Append("<div class=\"JobPostingDetails Edit\">");
            html.Append("<table width=\"100%\" border=\"0\">");
									
			html.Append("<tr>");
			html.Append("<td>Date to remove public access:</td>");
			html.Append("</tr>");
			html.Append("<tr>");
            html.Append("<td>");
            html.Append(PageUtils.getInputTextHtml("RemoveAnonAccessAt_" + placeholderId, "RemoveAnonAccessAt_" + placeholderId, postingDetails.RemoveAnonAccessAt.ToString(CmsConfig.InputDateTimeFormatInfo), 12, 10));
            html.Append(" <em>format: " + CmsConfig.InputDateTimeFormatInfo.ToUpper() + ". ");
            html.Append("Enter '" + DateTime.MaxValue.ToString(CmsConfig.InputDateTimeFormatInfo) + "' for no auto-removal</em>.</td>");
            html.Append("</tr>");

            // -- Job Location drop-down
            
            html.Append("<tr>");
            html.Append("<td>Job Location:</td>");
            html.Append("</tr>");
            html.Append("<tr>");
            
            html.Append("<td>" + PageUtils.getDropDownHtml("location_" + placeholderId, "location_" + placeholderId, JobPostingLocation.ToNameValueCollection(locations, langToRenderFor, AllowPostingToAllLocations), postingDetails.LocationId.ToString()));

            try
            {
                CmsPage editLocationPage = new CmsPageDb().getPage("_admin/JobLocation");
                html.Append(" <a href=\"" + editLocationPage.getUrl(langToRenderFor) + "\" onclick=\"window.open(this.href,'" + placeholderIdWithoutLang + "','resizable=1,scrollbars=1,width=800,height=400'); return false;\">(edit)</a>");
            }
            catch (Exception ex)
            {
                html.Append(" <span>Cannot setup Edit Category Link: " + ex.Message + "</span>");
            }

            html.Append("</td>");
            html.Append("</tr>");

            html.Append("</table>"+Environment.NewLine);

            html.Append(PageUtils.getHiddenInputHtml(placeholderId + "_Action", "update"));

            html.Append("</div>" + Environment.NewLine);

            writer.Write(html.ToString());
        } // RenderEdit

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            // -- revise the Edit Menu
            CmsPage aggregatorPage = page.ParentPage;
            JobPostingAggregator.AddJobPostingCommandToEditMenu(page, aggregatorPage);
            
            JobPostingDb db = new JobPostingDb();
            JobPostingDetailsData postingDetails = db.getJobPostingDetailsData(page, identifier, langToRenderFor, true);
            JobPostingLocation location = JobPostingLocation.Fetch(postingDetails.LocationId);
            StringBuilder html = new StringBuilder();

            html.Append("<div class=\"JobPostingDetails View\">");

            if (IncludeBackLinkInDisplay)
            {
                string aggregatorUrl = aggregatorPage.Url;
                html.Append("<p class=\"jobBackToAggregator\"><strong><a class=\"backToPrev\" href=\"" + aggregatorUrl + "\">" + getBackToJobListingText(langToRenderFor) + "</a></strong><p>");
            }

            // -- if the posting is expired, don't let non-authors view it.            
            if (postingDetails.IsExpired && ! CmsContext.currentUserCanAuthor)
            {
                throw new CmsPageNotFoundException();
            }
            
            if (CmsContext.currentUserCanAuthor && postingDetails.IsExpired)
            {
                html.Append("<div class=\"jobExpiredNotice\" style=\"background-color: yellow; font-weight: bold\">This job posting expired on " + postingDetails.RemoveAnonAccessAt.ToString("d MMM yyyy") + "</div> ");
            }
            else if (CmsContext.currentUserCanAuthor && postingDetails.RemoveAnonAccessAt.Date != JobPostingDetailsData.NoExpiryDateTime.Date)
            {
                html.Append("<p class=\"jobExpiredNotice\" style=\"background-color: yellow; font-weight: bold\">This job will be removed from public access on " + postingDetails.RemoveAnonAccessAt.ToString("d MMM yyyy") + "</p>");
            }

            if (IncludeLocationInDisplay)
            {
                html.Append("<p class=\"jobLocation\">" + getLocationText(langToRenderFor) + ": " + location.getLocationText(langToRenderFor) + "</p>");
            }

            html.Append("</div>" + Environment.NewLine);

            writer.Write(html.ToString());

        } // RenderView
    }
}
