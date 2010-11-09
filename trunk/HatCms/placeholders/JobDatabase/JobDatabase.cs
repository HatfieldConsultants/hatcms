using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections;
using System.Text;
using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
#if IncludeOldJobDatabasePlaceholder

    #region Job data holding classes
    public class JobSummaryData
	{
		public int JobSummaryId;
		public string LocationToDisplay;
        public string HtmlHeader;

        public JobSummaryData()
		{
            JobSummaryId = -1;
            LocationToDisplay = "";
            HtmlHeader = "";
		}
	}

	public class JobDetailsData
	{
        public int JobDetailsId;
		public string Title;
		public string Location;
		public string HtmlJobDescription; 
		public DateTime RemoveAnonAccessAt;
        public DateTime LastUpdatedDateTime;

        public JobDetailsData()
		{
            JobDetailsId = -1;
			Title = "";
            Location = "";
            HtmlJobDescription = "";
            RemoveAnonAccessAt = DateTime.MaxValue;
            LastUpdatedDateTime = DateTime.MinValue;
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

        public static JobDetailsData[] GetJobsByLocation(string locationToFind, JobDetailsData[] haystack)
        {
            List<JobDetailsData> ret = new List<JobDetailsData>();
            foreach(JobDetailsData j in haystack)
            {
                if (string.Compare(j.Location, locationToFind, true) == 0)
                    ret.Add(j);
            } // foreach
            return ret.ToArray();
        }

        public static JobDetailsData[] SortByLocation(JobDetailsData[] jobDetailsToSort, string[] locations)
        {
            List<JobDetailsData> ret = new List<JobDetailsData>();
            foreach (string location in locations)
            {
                ret.AddRange(GetJobsByLocation(location, jobDetailsToSort));
            }

            return ret.ToArray();
        }

	}
	#endregion
	
	/// <summary>
	/// Summary description for HtmlContent.
	/// </summary>
	public class JobDatabase: BaseCmsPlaceholder
	{

        public static string AllLocationsDisplayText
        {
            get { return "All Locations"; } 
        }

        /// <summary>
        /// If no job postings available, retrieve the language-specific message from config file.
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string getNoJobMessage(CmsLanguage lang)
        {
            string[] msgArray = CmsConfig.getConfigValue("JobPosting.NoRecord", "").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            CmsLanguage[] langArray = CmsConfig.Languages;
            int x = CmsLanguage.IndexOf(lang.shortCode, langArray);

            if (msgArray.Length < langArray.Length || x < 0)
                throw new Exception("Missing entry for JobPosting.NoRecord!");
            
            return msgArray[x];
        }

        /// <summary>
        /// If no job postings available, retrieve the language-specific message from config file.
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string getNoJobForMessage(CmsLanguage lang)
        {
            string[] msgArray = CmsConfig.getConfigValue("JobPosting.NoRecordFor", "").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            CmsLanguage[] langArray = CmsConfig.Languages;
            int x = CmsLanguage.IndexOf(lang.shortCode, langArray);

            if (msgArray.Length < langArray.Length || x < 0)
                throw new Exception("Missing entry for JobPosting.NoRecordFor!");

            return msgArray[x];
        }

        public static string[] getJobPostingLocations(bool includeAllOption)
        {
            ArrayList ret = new ArrayList();
            if (includeAllOption)
                ret.Add(AllLocationsDisplayText);
            if (CmsConfig.getConfigValue("JobPostingLocations","") != "")
            {
                ret.AddRange(CmsConfig.getConfigValue("JobPostingLocations", "").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            }
            return (string[])ret.ToArray(typeof(string));
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            // -- CKEditor dependencies
            ret.AddRange(CKEditorHelpers.CKEditorDependencies);

            // -- config entries
            ret.Add(new CmsConfigItemDependency("JobPostingLocations"));
            ret.Add(new CmsConfigItemDependency("JobDatabaseTemplate", CmsDependency.ExistsMode.MustNotExist));

            // -- pages
            string deleteJobPagePath = CmsConfig.getConfigValue("DeleteJobPath", "/_admin/actions/deleteJob");
            ret.Add(new CmsPageDependency(deleteJobPagePath, CmsConfig.Languages));

            // -- database tables
            ret.Add(new CmsDatabaseTableDependency("jobsummary"));
            ret.Add(new CmsDatabaseTableDependency("jobdetails"));
            
            return ret.ToArray();
        }

        public override bool contentContainsLinks(CmsPage page, int[] identifiers, CmsLanguage[] languages, ref string[] links)
        {
            // @@ TODO: actually implement this!
            return false;
        }

        public override string getPlaceholderValue(CmsPage page, int identifier, CmsLanguage language)
        {
            try
            {
                JobDatabaseDb db = new JobDatabaseDb();
                JobSummaryData summary = db.getJobSummary(page, identifier, language, false);
                return summary.HtmlHeader;
            }
            catch
            { }
            return string.Empty;
        }
        

        public override CmsAlternateView[] getAlternateViews(CmsPage page, int[] identifiers, CmsLanguage pageLanguage)
        {
            // @@ TODO: actually implement this!
            return new CmsAlternateView[0];
        }

        public override bool revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return true; // this placeholder doesn't implement revisions
        }
        
        /// <summary>
        /// if not found, returns a new JobDetailsData object with ID = -1.
        /// </summary>
        /// <returns></returns>
        public static JobDetailsData getCurrentJobDetailsData()
        {
            if (currentJobId > -1)
            {
                return (new JobDatabaseDb()).getJobDetails(currentJobId, false);
            }
            return new JobDetailsData();
        }

        public static bool isJobDatabasePage(CmsPage page)
        {
            return (StringUtils.IndexOf(page.getAllPlaceholderNames(), "JobDatabase", StringComparison.CurrentCultureIgnoreCase) > -1);
        }
        
        public static string CurrentJobIdFormName
		{
			get
			{
				return "job";
			}
		}
		
		public JobDatabase()
		{
			//
			// nothing to construct
			//
		}
				

		public static int currentJobId
		{
			get
			{
                return PageUtils.getFromForm(CurrentJobIdFormName, -1);
                }
			}

		public enum RenderMode { Summaries, IndividualJob};

		/// <summary>
		///  the currentViewRenderMode is derived from the currentProjectId.
		///  If the currentProjectId is set and > -1, the individual project will be rendered.
		///  Otherwise, Summaries will be rendered.
		/// </summary>
		public static RenderMode currentViewRenderMode
		{
			get
			{
				// if pid is set, and > -1, render as IndividualProject
                int pid = currentJobId;
				if (pid > -1)
                    return RenderMode.IndividualJob;
				else
					return RenderMode.Summaries;
			}
		}

		/// <summary>
		/// Render the JobDatabase placeholder.
		/// This placeholder renders in both CmsEditMode.Edit and CmsMode.View,
		/// and in each CmsEditMode, can either be in IndividualProject mode, or Project Summaries Mode.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="page"></param>
		/// <param name="identifier"></param>
		/// <param name="paramList"></param>
		public override void Render(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{
            switch(CmsContext.currentEditMode)
			{
				case CmsEditMode.Edit:
                    if (currentViewRenderMode == RenderMode.IndividualJob)					
						RenderEditIndividualJob(writer, page, identifier, langToRenderFor, paramList);					
					else					
						RenderEditSummaries(writer, page, identifier, langToRenderFor, paramList);						
					break;
				case CmsEditMode.View:
                    if (currentViewRenderMode == RenderMode.IndividualJob)					
						RenderViewIndividualJob(writer, page, identifier, paramList);					
					else					
						RenderViewSummaries(writer, page, identifier, langToRenderFor, paramList);
					break;
			}
			
		} // Render		

		/// <summary>
		/// Render in Edit / Individual Project Mode
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="page"></param>
		/// <param name="identifier"></param>
		/// <param name="paramList"></param>
		private void RenderEditIndividualJob(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{
            string JobInfoId = "JobSummary_" + page.ID.ToString() + "_" + identifier.ToString() + langToRenderFor.shortCode;
            string editorId = JobInfoId;

			string width = "100%";
			string height = "400px";


			StringBuilder html = new StringBuilder();
			
			JobDatabaseDb db = new JobDatabaseDb();
			JobDetailsData job = db.getJobDetails(currentJobId, false);

            string action = Hatfield.Web.Portal.PageUtils.getFromForm(JobInfoId + "_Action", "");
			if (action.Trim().ToLower() == "update")
			{
                job.JobDetailsId = currentJobId;

                job.Location = PageUtils.getFromForm("location_" + JobInfoId, "");                

                job.HtmlJobDescription = PageUtils.getFromForm("name_" + editorId, "");

                string dateStr = PageUtils.getFromForm("RemoveAnonAccessAt_" + JobInfoId,"");

                job.RemoveAnonAccessAt = CmsConfig.parseDateInDateInputFormat(dateStr, job.RemoveAnonAccessAt);
				
				bool b = db.saveUpdatedJobDetails(job);
				if (!b)
					writer.Write("<h1>error saving updates to database</h1>");								
			}
						
			
			// -- setup the inline HTML Editor 
            CKEditorHelpers.AddPageJavascriptStatements(page, editorId, width, height, langToRenderFor);
						
			// -- render the form elements	
			html.Append("<table width=\"100%\" border=\"0\">");
			
			/* -- title is handled in JobTitle Placeholder */
			
			html.Append("<tr>");
			html.Append("<td>Date to remove public access:</td>");
			html.Append("</tr>");
			html.Append("<tr>");
            html.Append("<td>" + PageUtils.getInputTextHtml("RemoveAnonAccessAt_" + JobInfoId, "RemoveAnonAccessAt_" + JobInfoId, job.RemoveAnonAccessAt.ToString(CmsConfig.InputDateTimeFormatInfo), 12, 10) + " <em>format: " + CmsConfig.InputDateTimeFormatInfo.ToUpper() + ". Enter '" + DateTime.MaxValue.ToString(CmsConfig.InputDateTimeFormatInfo) + "' for no auto-removal</em>.</td>");
            html.Append("</tr>");

            // -- Job Location drop-down
            
            html.Append("<tr>");
            html.Append("<td>Job Location:</td>");
            html.Append("</tr>");
            html.Append("<tr>");
            html.Append("<td>" + PageUtils.getDropDownHtml("location_" + JobInfoId, "location_" + JobInfoId, getJobPostingLocations(false), job.Location) + "</td>");
            html.Append("</tr>");

            											
			html.Append("<tr>");
			html.Append("<td>Full Job Posting: </td>");
			html.Append("</tr>");
			html.Append("<tr>");
			html.Append("<td>");
            html.Append("<textarea name=\"name_" + editorId + "\" id=\"" + editorId + "\" style=\"WIDTH: " + width + "; HEIGHT: " + height + ";\">");
			html.Append(job.HtmlJobDescription);
			html.Append("</textarea>");	
			html.Append("</td>");
			html.Append("<tr>");
			
			html.Append("</table>");
			
            // -- hidden fields to make all the form submit voodoo work.
            html.Append(PageUtils.getHiddenInputHtml(JobInfoId+"_Action", "update"));
			html.Append(PageUtils.getHiddenInputHtml(JobDatabase.CurrentJobIdFormName, currentJobId.ToString()));
            html.Append(PageUtils.getHiddenInputHtml("appendToTargetUrl", JobDatabase.CurrentJobIdFormName+"="+currentJobId));
            			
            // -- output
			writer.Write(html.ToString());
		} // RenderEditIndividualProject

		/// <summary>
		/// Render in Edit / Summary Mode
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="page"></param>
		/// <param name="identifier"></param>
		/// <param name="paramList"></param>
		private void RenderEditSummaries(HtmlTextWriter writer, CmsPage page, int identifier,CmsLanguage langToRenderFor, string[] paramList)
		{
			JobDatabaseDb db = new JobDatabaseDb();
			
			JobSummaryData data = new JobSummaryData();

            data = db.getJobSummary(page, identifier, langToRenderFor, true);			

            string width = "100%";
            string height = "400px";


			string JobSummaryId = "JobSummary_"+page.ID.ToString()+"_"+identifier.ToString() + langToRenderFor.shortCode;
            string editorId = JobSummaryId;

			// ------- CHECK THE FORM FOR ACTIONS
            string action = Hatfield.Web.Portal.PageUtils.getFromForm(JobSummaryId + "_Action", "");
			if (action.Trim().ToLower() == "update")
			{
				// save the data to the database
                int id = PageUtils.getFromForm(JobSummaryId + "_JobSummaryId", -1);
                string newDefaultLocation = PageUtils.getFromForm("defaultLocation_" + JobSummaryId, AllLocationsDisplayText);
				
				data.JobSummaryId = id;
				data.LocationToDisplay = newDefaultLocation;	
				data.HtmlHeader = PageUtils.getFromForm("name_" + editorId, "");                

				bool b = db.saveUpdatedJobSummary(data);
				if (!b)
					writer.Write("Error saving updates to database!");
				
			}			

			// ------- START RENDERING
			// note: no need to put in the <form></form> tags.
			
			StringBuilder html = new StringBuilder();
            // -- setup the inline HTML Editor 
            CKEditorHelpers.AddPageJavascriptStatements(page, editorId, width, height, langToRenderFor);							

			html.Append("<strong>Job Summary Display Settings:</strong><br>");

            html.Append("<table>" + Environment.NewLine);
            // -- HTML Header
            html.Append("<tr>");
            html.Append("<td colspan=\"2\">");
            html.Append("<textarea name=\"name_" + editorId + "\" id=\"" + editorId + "\" style=\"WIDTH: " + width + "; HEIGHT: " + height + ";\">");
			html.Append(data.HtmlHeader);
			html.Append("</textarea>");	            
            html.Append("</td>");
            html.Append("</tr>");
            
            // -- Default Location
            string s = PageUtils.getDropDownHtml("defaultLocation_" + JobSummaryId, "defaultLocation_" + JobSummaryId, getJobPostingLocations(true), data.LocationToDisplay);

            html.Append("<tr><td>Location to display summaries for:</td>");
            html.Append("<td>" + s + "</td></tr>");

            html.Append("</table>" + Environment.NewLine);
			
            html.Append(PageUtils.getHiddenInputHtml(JobSummaryId + "_Action", "update"));
            html.Append(PageUtils.getHiddenInputHtml(JobSummaryId + "_JobSummaryId", data.JobSummaryId.ToString()));
			
			writer.WriteLine(html.ToString());

		}

        private string getHtmlForSummaryView(JobDetailsData[] jobDetails, string displayLocation, CmsLanguage lang)
        {
            StringBuilder html = new StringBuilder();

            if (jobDetails.Length == 0)
            {
                if (displayLocation == "" || String.Compare(displayLocation, AllLocationsDisplayText, true) == 0)
                {
                    html.Append("<p><strong>" + getNoJobMessage(lang) + "</strong>");
                }
                else
                {
                    html.Append("<p><strong>" + getNoJobForMessage(lang) + " " + displayLocation.ToString() + ".</strong>");
                }
            }
            else
            {
                bool showLocationTitles = false;
                if (displayLocation == "" || String.Compare(displayLocation, AllLocationsDisplayText, true) == 0)
                {
                    showLocationTitles = true;
                    jobDetails = JobDetailsData.SortByLocation(jobDetails, getJobPostingLocations(false));
                }

                string previousLocationTitle = "";
                bool locationULStarted = false;
                
                foreach (JobDetailsData job in jobDetails)
                {
                    if (showLocationTitles && (previousLocationTitle == "" || job.Location != previousLocationTitle))
                    {
                        if (locationULStarted)
                            html.Append("</ul>");

                        html.Append("<h2 class=\"JobLocation\">");
                        html.Append(job.Location);
                        html.Append("</h2>");
                        previousLocationTitle = job.Location;
                        html.Append("<ul>");
                        locationULStarted = true;
                    }


                    // -- create the details url
                    NameValueCollection paramList = new NameValueCollection();
                    paramList.Add(JobDatabase.CurrentJobIdFormName, job.JobDetailsId.ToString());

                    string detailsUrl = CmsContext.getUrlByPagePath(CmsContext.currentPage.Path, paramList);
                    if (job.IsExpired)
                    {
                        html.Append("<span class=\"jobExpiredNotice\" style=\"background-color: yellow; font-weight: bold\">EXPIRED:</span> ");
                    }
                    html.Append("<strong>");
                    html.Append(job.Title);
                    html.Append("</strong>");
                    html.Append(" <a href=\"" + detailsUrl + "\">full job description &raquo;</a>");
                    html.Append("<br><br>");
                } // foreach
                if (locationULStarted)
                    html.Append("</ul>");
            }
            return html.ToString();
        }
		

		/// <summary>
		/// Render in View / Individual Project Mode
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="page"></param>
		/// <param name="identifier"></param>
		/// <param name="paramList"></param>
		private void RenderViewIndividualJob(HtmlTextWriter writer, CmsPage page, int identifier, string[] paramList)
		{
			StringBuilder html = new StringBuilder();
			
			JobDatabaseDb db = new JobDatabaseDb();
			JobDetailsData job = db.getJobDetails(currentJobId, false);
            
            // -- don't show expired jobs to non-authors
            if (job.JobDetailsId == -1 || (job.IsExpired && !CmsContext.currentUserCanAuthor))
            {                
                throw new CmsPageNotFoundException();
            }
			
			string summaryUrl = page.Url;

            html.Append("<strong><a href=\"" + summaryUrl + "\">&laquo; back to job listings</a></strong><p>");

            // html.Append("<h1>"+job.Title+"</h1>");	-- the title is now output by the PageTitle control.					
			
			html.Append(job.HtmlJobDescription);

			writer.Write(html.ToString());
        } // RenderViewIndividualJob

		/// <summary>
		/// Render in View / Summary Mode
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="page"></param>
		/// <param name="identifier"></param>
		/// <param name="paramList"></param>
        private void RenderViewSummaries(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage language, string[] paramList)
		{
			CmsPage currentPage = CmsContext.currentPage;
			StringBuilder html = new StringBuilder();

			JobDatabaseDb db = new JobDatabaseDb();
			JobSummaryData jobSummary = db.getJobSummary(page, identifier, language, true);

            // html.Append("<h1>" + page.Title + "</h1>"); -- the title is now output by the PageTitle control.
            html.Append(Environment.NewLine);
            html.Append(jobSummary.HtmlHeader );
            html.Append(Environment.NewLine);
			// -- display results
            bool includeExpired = includeExpiredListings();
            JobDetailsData[] jobs = db.getJobDetailsByLocation(jobSummary.LocationToDisplay, includeExpired);
            html.Append(getHtmlForSummaryView(jobs, jobSummary.LocationToDisplay, language));

			writer.Write(html.ToString());
		} // RenderViewSummaries

        private bool includeExpiredListings()
        {
            if (CmsContext.currentWebPortalUser != null)
            {
                if (CmsContext.currentUserCanAuthor)
                {
                    return true;
                }
            }
            return false;
        }


	}
#endif
}
