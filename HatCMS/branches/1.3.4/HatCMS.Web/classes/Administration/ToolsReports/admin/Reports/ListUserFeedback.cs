using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HatCMS.Placeholders;
using System.Text;

namespace HatCMS.Admin
{
    public class ListUserFeedback : BaseCmsAdminTool
    {
        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Report_Feedback, AdminMenuTab.Reports, "List User Feedback");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.AddRange(PlaceholderUtils.getDependencies("UserFeedback"));
            return ret.ToArray();
        }

        public override string Render()
        {
            UserFeedbackDb db = new UserFeedbackDb();
            UserFeedbackSubmittedData[] arr = db.FetchAllUserFeedbackSubmittedData();
            if (arr.Length == 0)
            {
                return "<p><em>No User feedback has been submitted</em></p>";
            }
            StringBuilder html = new StringBuilder();
            html.Append("<p>");
            html.Append(TABLE_START_HTML);
            html.Append("<caption><h2>User feedback <a style=\"font-size: small;\" href=\"" + CmsContext.ApplicationPath + "_system/tools/download.ashx?adminTool=" + GetType().Name + "\">(download to Excel file)</a></h2></caption>");
            html.Append("<tr>");
            html.Append("<th>Submitted</th>");
            html.Append("<th>Name</th><th>Email Address</th><th>Location</th><th>Question</th><th>Answer</th><th>ReferringUrl</th>");
            html.Append("</tr>");
            foreach (UserFeedbackSubmittedData d in arr)
            {
                html.Append("<tr>");
                html.Append("<td>" + d.dateTimeSubmitted.ToString("yyyy-MM-dd") + "</td>");
                html.Append("<td>" + d.Name + "</td><td>" + d.EmailAddress + "</td><td>" + d.Location + "</td><td>" + d.TextAreaQuestion + "</td><td>" + d.TextAreaValue + "</td><td>" + d.ReferringUrl + "</td>");
                html.Append("</tr>");
            }

            html.Append("</table>");
            html.Append("</p>");
            return html.ToString();
        }

        public override System.Web.UI.WebControls.GridView RenderToGridViewForOutputToExcelFile()
        {
            return new UserFeedbackDb().FetchAllUserFeedbackSubmittedDataAsGrid();
        }


    }
}
