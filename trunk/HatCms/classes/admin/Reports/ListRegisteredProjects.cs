using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using HatCMS.Placeholders;
using HatCMS.Placeholders.RegisterProject;
using System.Collections.Generic;

namespace HatCMS.Controls.Admin
{
    public class ListRegisteredProjects : CmsBaseAdminTool
    {

        /// <summary>
        /// List the registered projects in descending order;
        /// Provide a download link.
        /// </summary>
        /// <returns></returns>
        public override string Render()
        {
            RegisterProjectDb db = new RegisterProjectDb();
            List<RegisterProjectDb.RegisterProjectData> list = db.fetchAll();
            if (list.Count == 0)
                return "<p><em>No registered project</em></p>";

            StringBuilder html = new StringBuilder();
            html.Append("<p>");
            html.Append("<table border=\"1\" cellspacing=\"0\" cellpadding=\"2\" style=\"border-collapse: collapse;\">");
            html.Append("<caption><h2>Registered Projects <a style=\"font-size: small;\" href=\"" + CmsContext.ApplicationPath + "_system/tools/download.ashx?adminTool=" + Enum.GetName(typeof(AdminMenuControl.CmsAdminToolEnum), AdminMenuControl.CmsAdminToolEnum.ListRegisteredProjects) + "\">(download)</a></h2></caption>");
            html.Append("<tr>");
            html.Append("<th>Name</th><th>Location</th><th>Description</th><th>Contact Person</th><th>Email</th><th>Telephone</th><th>Cellphone</th><th>Website</th><th>Funding Source</th><th>Date/Time Created</th><th>IP Address</th>");
            html.Append("</tr>");

            foreach (RegisterProjectDb.RegisterProjectData d in list)
            {
                html.Append("<tr>");
                html.Append("<td>" + d.Name + "</td>");
                html.Append("<td>" + d.Location + "</td>");
                html.Append("<td>" + d.Description + "</td>");
                html.Append("<td>" + d.ContactPerson + "</td>");
                html.Append("<td><a href=\"mailto:" + d.Email + "\">" + d.Email + "</a></td>");
                html.Append("<td>" + d.Telephone + "</td>");
                html.Append("<td>" + d.Cellphone + "</td>");
                html.Append("<td>" + d.Website + "</td>");
                html.Append("<td>" + d.FundingSource + "</td>");
                html.Append("<td>" + d.CreatedDateTime.ToString() + "</td>");
                html.Append("<td>" + d.ClientIp + "</td>");
                html.Append("</tr>");
            }

            html.Append("</table>");
            html.Append("</p>");
            return html.ToString();
        }
    }
}
