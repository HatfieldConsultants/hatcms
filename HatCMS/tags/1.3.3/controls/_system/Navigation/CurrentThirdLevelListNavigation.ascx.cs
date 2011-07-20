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

namespace HatCMS.Controls.Navigation
{
    public partial class CurrentThirdLevelListNavigation : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }        

        private int MaxLevels
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "MaxLevels", 100);
            } // get
        }

        private bool RenderOnlyPagesInCurrentPath
        {
            get { return (CmsControlUtils.hasControlParameterKey(CmsContext.currentPage, this, "RenderOnlyPagesInCurrentPath")); }
        }

        private bool RenderAllChildren
        {
            get { return (!RenderOnlyPagesInCurrentPath); }
        }

        private string SelectedLinkClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "SelectedLinkClassName", "Level{0}_Selected");
            } // get
        }

        private string ChildIsSelectedLinkClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "ChildIsSelectedLinkClassName", "Level{0}_ChildSelected");
            } // get
        }

        private string UnSelectedLinkClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "UnSelectedLinkClassName", "Level{0}");
            } // get
        }


        /// <summary>
        /// Renders the ThirdLevelListNavigation control to the HtmlTextWriter
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            CmsPage currentPage = CmsContext.currentPage;
            CmsPage homePage = CmsContext.HomePage;

            if (currentPage.Path == homePage.Path)
            {
                writer.Write("<div class=\"ThirdLevelListNavigation\"></div>");
                return;
            }

            // -- get the current secondLevelMainPage
            CmsPage secondLevelMainPage = currentPage;
            while (secondLevelMainPage.Level > 1)
                secondLevelMainPage = secondLevelMainPage.ParentPage;

            int maxLevels = MaxLevels;            

            StringBuilder html = new StringBuilder();

            html.Append("<div class=\"ThirdLevelListNavigation\">");

            if (secondLevelMainPage.ChildPages.Length > 0)
            {
                html.Append("<ul class=\"level2\">"+Environment.NewLine);
                foreach (CmsPage p in secondLevelMainPage.ChildPages)
                {
                    if (RenderAllChildren || (RenderOnlyPagesInCurrentPath && p.isChildOrSelfSelected()))
                    {
                        html.Append(recursiveRender(p, maxLevels) + Environment.NewLine);
                    }
                }
                html.Append("</ul>");
            }
            html.Append("</div>");

            writer.Write(html.ToString());
        } // Render



        private string recursiveRender(CmsPage page, int maxLevel)
        {
            StringBuilder html = new StringBuilder();

            int currentLevel = page.Level;
            if (page.ID == -1 || currentLevel > maxLevel)
                return "";

            if (!page.isVisibleForCurrentUser || ! page.ShowInMenu)
                return "";

            

            bool outputChildren = (RenderAllChildren || (RenderOnlyPagesInCurrentPath && page.isChildOrSelfSelected()));

            string LinkCSSClass = "";
            if (UnSelectedLinkClassName != "")
                LinkCSSClass = " class=\""+String.Format(UnSelectedLinkClassName, currentLevel)+"\"";
            if (page.isSelfSelected() && SelectedLinkClassName != "")
            {
                LinkCSSClass = " class=\"" + String.Format(SelectedLinkClassName, currentLevel) + "\""; 
            }
            else if (page.isChildSelected())
            {
                LinkCSSClass = " class=\"" + String.Format(ChildIsSelectedLinkClassName, currentLevel) + "\"";
            }

            string title = page.MenuTitle;
            if (title == "")
                title = page.Title;

            html.Append("<li class=\"level" + currentLevel.ToString() + "\"><a" + LinkCSSClass + " href=\"" + page.Url + "\">" + title + "</a></li>");

            if (page.ChildPages.Length > 0 && outputChildren && (page.Level + 1) < maxLevel)
            {

                html.Append("<ul class=\"level" + (currentLevel + 1).ToString() + "\">"+Environment.NewLine);
                foreach (CmsPage subPage in page.ChildPages)
                {
                    html.Append(recursiveRender(subPage, maxLevel));
                }
                html.Append("</ul>"+Environment.NewLine);

            }

            return html.ToString();
        } // recursiveRender
    }
}