namespace HatCMS.controls
{
	using System;
    using System.Collections.Specialized;
    using System.Text;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

    using Hatfield.Web.Portal;

	/// <summary>
	///		
	/// </summary>
    public partial class ViewRevisionHistoryPopup : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			
			// -- get the target page
			int targetPageId = PageUtils.getFromForm("target",Int32.MinValue);
            if (targetPageId < 0)
			{
				writer.WriteLine("Error: invalid target page!");
				return;
			}
            CmsPage page = CmsContext.getPageById(targetPageId);
            if (page.ID < 0)
            {
                writer.WriteLine("Error: invalid target page!");
                return;
            }

            if (!page.currentUserCanWrite)
            {
                writer.WriteLine("Access Denied");
                return;
            }

            string userMessage = "";

            string action = PageUtils.getFromForm("action", "");
            int VersionNumberToRevertTo = PageUtils.getFromForm("VersionNumberToRevertTo", -1);
            if (string.Compare(action, "revertToVersion", true) == 0 && VersionNumberToRevertTo > 0)
            {
                bool b = page.revertToRevision(VersionNumberToRevertTo);
                if (b)                
                    userMessage = "<p style=\"color: green;\">Revision # " + VersionNumberToRevertTo + " has been reverted to, and has been made the live version</p>";
                else
                    userMessage = "<p style=\"color: red;\">Error: could not revert to revision # " + VersionNumberToRevertTo + " - there was a database error.</p>";
            }



            string closeButtonHtml = "<input type=\"button\" value=\"close window\" onclick=\"go('"+page.Url+"'); window.close();\">";

            StringBuilder html = new StringBuilder();
            html.Append("<strong>Revisions of \"" + page.Title + "\"</strong>" + Environment.NewLine);
            html.Append(userMessage);
            html.Append("<p align=\"center\">" + closeButtonHtml + "</p>");
            html.Append("<script>"+Environment.NewLine + Environment.NewLine);
            html.Append("function go(url){" + Environment.NewLine);
            html.Append("opener.location.href = url;" + Environment.NewLine);            
            html.Append("}" + Environment.NewLine);

            html.Append("</script>" + Environment.NewLine);
            html.Append("<style>" + Environment.NewLine);
            html.Append(" form { padding: 0px; margin: 0px; } " + Environment.NewLine);
            html.Append("</style>" + Environment.NewLine);

			// -- get the data and render the table
            
            CmsPageRevisionData[] allRevs = page.getAllRevisionData();
            allRevs = CmsPageRevisionData.SortMostRecentFirst(allRevs);
            string prevTitle = "";
            if (allRevs.Length < 1)
            {
                html.Append("<p><em>Sorry, no revisions have been tracked for this page yet.</em></p>");
            }
            else
            {
                CmsPage thisPopup = CmsContext.currentPage;                

                html.Append("<table border=\"1\">" + Environment.NewLine);
                for (int i = 0; i < allRevs.Length; i++)
                {
                    CmsPageRevisionData rev = allRevs[i];

                    bool isLiveVersion = false;
                    if (i == 0)
                        isLiveVersion = true;

                    string title = "Revisions saved " + getLastModifiedTitle(rev);
                    if (prevTitle != title)
                    {
                        html.Append("<tr><td colspan=\"5\" style=\"background-color: #CCC;\">" + title + "</td></tr>" + Environment.NewLine);
                    }
                    html.Append("<tr>");
                    if (isLiveVersion)
                    {
                        html.Append("<td><strong>Live version</strong></td>");
                    }
                    else
                    {
                        html.Append("<td>Rev # " + rev.RevisionNumber.ToString() + "</td>");
                    }
                    html.Append("<td>" + rev.RevisionSavedAt.ToString("d MMM yyyy") + " at " + rev.RevisionSavedAt.ToString("%h:mm tt") + "</td>" + Environment.NewLine);
                    html.Append("<td>" + rev.RevisionSavedByUsername + "</td>" + Environment.NewLine);
                    if (CmsConfig.Languages.Length > 1)
                    {
                        NameValueCollection urlParams = new NameValueCollection();
                        urlParams.Add("revNum", rev.RevisionNumber.ToString());
                        html.Append("<td>");
                        foreach(CmsLanguage lang in CmsConfig.Languages)
                        {
                            
                            string viewUrl =page.getUrl(urlParams, lang);
                            html.Append("<input type=\"button\" value=\"view - "+lang.shortCode+"\" onclick=\"go('" + viewUrl + "');\">");
                        } // foreach Language
                        html.Append("</td>");
                    }
                    else
                    {
                        
                    }

                    html.Append("<td>");
                    if (!isLiveVersion && CmsContext.currentUserIsSuperAdmin)
                    {
                        string formId = "ViewRevision" + i.ToString();
                        html.Append(thisPopup.getFormStartHtml(formId));
                        html.Append(PageUtils.getHiddenInputHtml("target", targetPageId.ToString()));
                        html.Append(PageUtils.getHiddenInputHtml("action", "revertToVersion"));
                        html.Append(PageUtils.getHiddenInputHtml("VersionNumberToRevertTo", rev.RevisionNumber.ToString()));
                        html.Append("<input type=\"submit\" value=\"revert to this revision\">");
                        html.Append(thisPopup.getFormCloseHtml(formId));
                    }
                    html.Append("</td>"+Environment.NewLine);


                    html.Append("</tr>" + Environment.NewLine);
                    prevTitle = title;
                } // foreach        

                html.Append("</table>");
            }
            html.Append("<p align=\"center\">" + closeButtonHtml + "</p>");

			

			writer.WriteLine(html.ToString());
		}

        private string getLastModifiedTitle(CmsPageRevisionData revData)
        {
            TimeSpan timespan = TimeSpan.FromTicks(DateTime.Now.Ticks - revData.RevisionSavedAt.Ticks);
            if (timespan.TotalDays < 7)
                return "less than a week ago";
            else if (timespan.TotalDays < 31)
                return "less than a month ago";
            else if (timespan.TotalDays < 365)
            {
                int monthsAgo = Convert.ToInt32(Math.Round(timespan.TotalDays / 31));
                return monthsAgo.ToString() + " months ago";
            }
            else
            {
                int yearsAgo = Convert.ToInt32(Math.Round(timespan.TotalDays / 365));
                return yearsAgo.ToString() + " years ago";
            }

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
