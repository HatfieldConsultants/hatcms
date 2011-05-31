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
using System.Collections.Generic;
using System.Collections.Specialized;

namespace HatCMS.Admin
{
    public class LastModifiedTable : BaseCmsAdminTool
    {
        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Report_Page, AdminMenuTab.Reports, "Pages by last modified date");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            return ret.ToArray();
        }

        public override string Render()
        {
            StringBuilder html = new StringBuilder();
            Dictionary<int, CmsPage> pages = CmsContext.HomePage.getLinearizedPages();
            // change the NameObjectCollection into a sortable list
            List<CmsPage> allPages = new List<CmsPage>();
            foreach (int pageId in pages.Keys)
            {
                allPages.Add(pages[pageId]);
            }
            CmsPage[] sortedPages = CmsPage.SortPagesByLastModifiedDate(allPages.ToArray());

            html.Append(TABLE_START_HTML + Environment.NewLine);
            string rowHeader = ("<tr><th>Last Modified</th><th>Created on</th><th>Title</th><th>Path</th></tr>");
            string lastTitle = "";
            foreach (CmsPage p in sortedPages)
            {
                string title = getLastModifiedTitle(p);
                if (title != lastTitle)
                {
                    html.Append("<tr><th colspan=\"4\" style=\"background-color: #CCC;\"><strong>" + title + "</strong></th></tr>" + Environment.NewLine);
                    html.Append(rowHeader);
                }

                string tdStyle = "";
                if (DateTime.Compare(p.CreatedDateTime.Date, p.LastUpdatedDateTime.Date) == 0)
                    tdStyle = " style=\"background-color: yellow\"";


                html.Append("<tr>");
                html.Append("<td" + tdStyle + ">" + p.LastUpdatedDateTime.ToString("d MMM yyyy") + "</td>");
                html.Append("<td" + tdStyle + ">" + p.CreatedDateTime.ToString("d MMM yyyy") + "</td>");
                html.Append("<td" + tdStyle + ">" + p.Title + "</td>");
                html.Append("<td" + tdStyle + "><a target=\"_blank\" href=\"" + p.Url + "\">" + p.Path + "</a></td>");
                html.Append("</tr>" + Environment.NewLine);

                lastTitle = title;
            } // foreach
            html.Append("</table>" + Environment.NewLine);

            return html.ToString();
        } // RenderLastModifiedTable

        private string getLastModifiedTitle(CmsPage p)
        {
            TimeSpan timespan = TimeSpan.FromTicks(DateTime.Now.Ticks - p.LastUpdatedDateTime.Ticks);
            if (timespan.TotalDays < 7)
                return "Less than a week ago";
            else if (timespan.TotalDays < 31)
                return "Less than a month ago";
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



    }
}
