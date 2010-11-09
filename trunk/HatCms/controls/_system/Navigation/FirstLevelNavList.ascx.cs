using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;

namespace HatCMS.controls.Navigation
{
    public partial class FirstLevelNavList : System.Web.UI.UserControl
    {
        protected override void Render(HtmlTextWriter writer)
        {
            StringBuilder html = new StringBuilder();

            html.Append("<ul class=\"FirstLevelNavList\">");

            bool includeHomepage = CmsConfig.getConfigValue("FirstLevelNavList_IncludeHomepage", true);

            if (includeHomepage)
            {
                string homepageMenuTitle = CmsContext.HomePage.MenuTitle;
                if (homepageMenuTitle.Trim() == "")
                    homepageMenuTitle = CmsContext.HomePage.Title;

                string homeClassName = "";
                if (CmsContext.currentPage.ID == CmsContext.HomePage.ID )
                {
                    homeClassName = " class=\"selected\"";
                }

                html.Append("<li" + homeClassName + "><a href=\"" + CmsContext.HomePage.Url + "\" " + homeClassName + ">" + homepageMenuTitle + "</a></li>");
            }

            foreach (CmsPage page in CmsContext.HomePage.ChildPages)
            {
                if (page.ShowInMenu && page.isVisibleForCurrentUser)
                {
                    string className = "";
                    if (page.ID == CmsContext.currentPage.ID || page.isChildSelected())
                    {
                        className = " class=\"selected\"";
                    }
                    string menuTitle = page.MenuTitle;
                    if (menuTitle == "")
                        menuTitle = page.Title;
                    html.Append("<li" + className + "><a href=\"" + page.Url + "\" " + className + ">" + menuTitle + "</a></li>");
                }
            } // foreach
            
            html.Append("</ul>");

            writer.Write(html.ToString());

        } // Render
    }
}