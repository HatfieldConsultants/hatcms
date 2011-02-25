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
    public partial class ThirdLevelSideNavigation : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Renders the SideNavigation control to the HtmlTextWriter
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            CmsPage currentPage = CmsContext.currentPage;
            CmsPage homePage = CmsContext.HomePage;

            if (currentPage.Path == homePage.Path)
            {
                writer.Write("<div id=\"SideNav\"></div>");
                return;
            }

            // -- get the secondLevelMainPage
            CmsPage secondLevelMainPage = currentPage;
            while (secondLevelMainPage.Level > 1)
                secondLevelMainPage = secondLevelMainPage.ParentPage;

            int maxLevels = 100;
            int levels = CmsConfig.getConfigValue("SideNavMaxLevels", maxLevels);            

            StringBuilder html = new StringBuilder();
            
            html.Append("<div id=\"SideNav\">");

            if (secondLevelMainPage.ChildPages.Length > 0)
            {
                html.Append("<ul class=\"level1\">"+Environment.NewLine);
                foreach (CmsPage p in secondLevelMainPage.ChildPages)
                {
                    html.Append(recursiveRender(p, 0, maxLevels) + ""+Environment.NewLine);
                }
                html.Append("</ul>");
            }
            html.Append("</div>");

            writer.Write(html.ToString());
        } // Render



        private string recursiveRender(CmsPage page, int currentLevel, int maxLevel)
        {
            StringBuilder html = new StringBuilder();
            if (page.ID == -1 || currentLevel > maxLevel || !page.ShowInMenu)
                return "";

            if (!page.isVisibleForCurrentUser || ! page.ShowInMenu)
                return "";

            bool outputChildren = false;
            if (page.isChildSelected() || page.Path == CmsContext.currentPage.Path)
                outputChildren = true;

            string CSSClass = ""; // 
            if (page.Path == CmsContext.currentPage.Path || (currentLevel == maxLevel && page.isChildSelected()))
            {
                CSSClass = "class=\"selected\"";
            }

            string title = page.MenuTitle;
            if (title == "")
                title = page.Title;

            html.Append("<li class=\"level" + page.Level.ToString() + "\"><a " + CSSClass + " href=\"" + page.Url + "\">" + title + "</a></li>");

            if (page.ChildPages.Length > 0 && outputChildren)
            {

                html.Append("<ul class=\"level" + (currentLevel + 1).ToString() + "\">"+Environment.NewLine);
                foreach (CmsPage subPage in page.ChildPages)
                {
                    html.Append(recursiveRender(subPage, currentLevel + 1, maxLevel));
                }
                html.Append("</ul>"+Environment.NewLine);

            }

            return html.ToString();
        } // recursiveRender
    }
}