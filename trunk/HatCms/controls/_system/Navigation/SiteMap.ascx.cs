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
    public partial class SiteMap : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
              

        private bool IncludeHomepage
        {
            get { return (CmsControlUtils.hasControlParameterKey(this, "IncludeHomepage")); }
        }

        private bool UsePageTitles
        {
            get { return (CmsControlUtils.hasControlParameterKey(this, "UsePageTitles")); }
        }

        private bool UseMenuTitles
        {
            get { return !UsePageTitles; }
        }

        private string ListItemIDFormat
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(this, "CellIDFormat", "SiteMapItem_{1}");
            } // get
        }

        private string OuterDivCSSClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(this, "OuterDivCSSClassName", "SiteMap");
            } // get
        }

        private string ListItemClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(this, "ListItemClassName", "SiteMapItem");
            } // get
        }


        private string ListGroupClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(this, "ListItemClassName", "SiteMapGroup_{0}");
            } // get
        }

        private int MaxLevels
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(this, "MaxLevels", 100);
            } // get
        }

        private int listItemOutputCount = 0;

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            CmsPage startRenderAtPage = CmsContext.HomePage;
            
            StringBuilder html = new StringBuilder();
            string divId = "SiteMap" + this.ID;
            divId = divId.Replace("\\","_");


            html.Append("<div class=\"" + OuterDivCSSClassName + "\" id=\"" + divId + "\">" + Environment.NewLine);

            listItemOutputCount = 0;
            string s = recursiveRender(startRenderAtPage);
            html.Append(s);

            html.Append("<br style=\"clear: left\" />");
            html.Append("</div>");
            Response.Write(html.ToString());
        } // Render


        private string recursiveRender(CmsPage page )
        {
            int currentLevel = page.Level;

            StringBuilder html = new StringBuilder();
            if (page.ID == -1 || currentLevel > this.MaxLevels)
                return "";

            if (!page.isVisibleForCurrentUser)
                return "";            

            if (!IncludeHomepage && page.Path == CmsContext.HomePage.Path)
            {
                Console.Write("not including home page");
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
                html.Append("</li>" + Environment.NewLine);

            } // else

            if (page.ChildPages.Length > 0)
            {
                string ulCssClass = String.Format(ListGroupClassName, currentLevel.ToString(), listItemOutputCount.ToString());
                if (ulCssClass != "")
                    ulCssClass = "class=\"" + ulCssClass + "\" ";
                html.Append("<ul " + ulCssClass + "\">"+Environment.NewLine);
                foreach (CmsPage subPage in page.ChildPages)
                {
                    html.Append(recursiveRender(subPage));
                }
                html.Append("</ul>"+Environment.NewLine);
            }

            return html.ToString();
        } // recursiveRender
    }
}