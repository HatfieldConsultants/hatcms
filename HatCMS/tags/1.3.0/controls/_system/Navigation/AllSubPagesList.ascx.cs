using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Controls._system
{
    public partial class AllSubPagesList : System.Web.UI.UserControl
    {        

        private bool IncludeHomepage
        {
            get { return (CmsControlUtils.hasControlParameterKey(CmsContext.currentPage, this, "IncludeHomepage")); }
        }

        private bool UseMenuTitles
        {
            get { return (CmsControlUtils.hasControlParameterKey(CmsContext.currentPage, this, "UseMenuTitles")); }
        }

        private bool UsePageTitles
        {
            get { return !UseMenuTitles; }
        }

        private string ListItemIDFormat
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "CellIDFormat", "AllSubPagesListItem_{1}");
            } // get
        }

        private string OuterDivCSSClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "OuterDivCSSClassName", "AllSubPagesList");
            } // get
        }

        private string ListItemClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "ListItemClassName", "AllSubPagesListItem");
            } // get
        }


        private string ListGroupClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "ListItemClassName", "AllSubPagesListGroup_{0}");
            } // get
        }

        private int MaxLevels
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "MaxLevels", 100);
            } // get
        }

        private int listItemOutputCount = 0;
        
        
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void Render(HtmlTextWriter writer)
        {
            
            CmsPage currentPage = CmsContext.currentPage;
            CmsPage startRenderAtPage = currentPage;
            
            StringBuilder html = new StringBuilder();

            string divId = "AllSubPagesList" + UniqueID;
            divId = divId.Replace("\\", "_");
            divId = divId.Replace(":", "_");

            html.Append("<div class=\"" + OuterDivCSSClassName + "\" id=\"" + divId + "\">" + Environment.NewLine);

            listItemOutputCount = 0;

            if (currentPage.ChildPages.Length > 0)
            {
                string ulCssClass = String.Format(ListGroupClassName, currentPage.Level.ToString(), listItemOutputCount.ToString());
                if (ulCssClass != "")
                    ulCssClass = "class=\"" + ulCssClass + "\" ";
                html.Append("<ul " + ulCssClass + "\">" + Environment.NewLine);
                foreach (CmsPage subPage in currentPage.ChildPages)
                {
                    html.Append(recursiveRender(subPage));
                }
                html.Append("</ul>" + Environment.NewLine);
            }

            html.Append("<br style=\"clear: left\" />");
            html.Append("</div>"+Environment.NewLine);

            writer.Write(html.ToString());
        }

        private string recursiveRender(CmsPage page)
        {
            int currentLevel = page.Level;

            StringBuilder html = new StringBuilder();
            if (page.ID == -1 || currentLevel > this.MaxLevels)
                return "";

            if (!page.isVisibleForCurrentUser)
                return "";

            if (!IncludeHomepage && page.Path == CmsContext.HomePage.Path)
            {
                Console.WriteLine("AllSubPagesList.ascx - not including home page");
            }
            else
            {

                listItemOutputCount++;
                string listItemID = String.Format(ListItemIDFormat, currentLevel.ToString(), listItemOutputCount.ToString());

                string name = String.Format(ListItemClassName, currentLevel.ToString(), listItemOutputCount.ToString());
                string CSSClass = " class=\"" + name + "\""; // 

                string title = page.MenuTitle;
                if (title == "" || UsePageTitles)
                    title = page.Title;

                html.Append("<li" + CSSClass + " id=\"" + listItemID + "\">");
                html.Append("<a" + CSSClass + " href=\"" + page.Url + "\">");
                html.Append(title);
                html.Append("</a>");
                

            } // else

            if (page.ChildPages.Length > 0)
            {
                string ulCssClass = String.Format(ListGroupClassName, currentLevel.ToString(), listItemOutputCount.ToString());
                if (ulCssClass != "")
                    ulCssClass = "class=\"" + ulCssClass + "\" ";
                html.Append("<ul " + ulCssClass + "\">" + Environment.NewLine);
                foreach (CmsPage subPage in page.ChildPages)
                {
                    html.Append(recursiveRender(subPage));
                }
                html.Append("</ul>" + Environment.NewLine);
            }
            
            html.Append("</li>" + Environment.NewLine);

            return html.ToString();
        } // recursiveRender
    }
}