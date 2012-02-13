using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Controls._system
{
    public partial class TopMultiLevelDropDownMenu : System.Web.UI.UserControl
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
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "CellIDFormat", "TopMultiLevelDropDownMenuItem_{1}");
            } // get
        }

        private string OuterDivCSSClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "OuterDivCSSClassName", "TopMultiLevelDropDownMenu");
            } // get
        }

        private string ListItemClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "ListItemClassName", "TopMultiLevelDropDownMenuItem");
            } // get
        }


        private string ListGroupClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "ListItemClassName", "TopMultiLevelDropDownMenuGroup_{0}");
            } // get
        }

        private int MaxLevels
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "MaxLevels", 100);
            } // get
        }

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(CmsFileDependency.UnderAppPath("css/_system/TopMultiLevelDropDownMenu.css"));
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/jquery/jquery-1.4.1.min.js"));
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/TopMultiLevelDropDownMenu.js"));
            return ret.ToArray();
        }


        private int listItemOutputCount = 0;
        
        
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void Render(HtmlTextWriter writer)
        {
            // -- modified from: http://www.dynamicdrive.com/style/csslibrary/item/jquery_multi_level_css_menu_2/
            CmsPage startRenderAtPage = CmsContext.HomePage;
            CmsPage currentPage = CmsContext.currentPage;
            StringBuilder html = new StringBuilder();
            /*
            if (!currentPage.HeadSectionallowScriptToOutput("TopMultiLevelDropDownMenu"))
                throw new Exception("Error: only one TopMultiLevelDropDownMenu control can be on a page at a single time");
            */
            string divId = "TopMultiLevelDropDownMenu"; // note: this.UniqueID is not javascript save under mono!
            divId = divId.Replace("\\", "_");
            divId = divId.Replace(":", "_");
            divId = divId.Replace(",", "_");
            divId = divId.Replace("\"", "_");
            divId = divId.Replace("'", "_");
            divId = divId.Replace("/", "_");
            divId = divId.Replace(" ", "_");
            if (divId.Length > 255)
                divId = divId.Substring(0, 255);


            html.Append("<div class=\"" + OuterDivCSSClassName + "\" id=\"" + divId + "\">" + Environment.NewLine);

            listItemOutputCount = 0;
            
            string s = recursiveRender(startRenderAtPage);
            html.Append(s);
            
            html.Append("<br style=\"clear: left\" />");
            html.Append("</div>"+Environment.NewLine);

            currentPage.HeadSection.AddCSSFile(CSSGroup.ControlOrPlaceholder, "css/_system/TopMultiLevelDropDownMenu.css");
            currentPage.HeadSection.AddCSSStyleStatements("<!--[if lte IE 7]>html .TopMultiLevelDropDownMenu{height: 1%;} <![endif]-->");
            currentPage.HeadSection.AddJavascriptFile(JavascriptGroup.Library, "js/_system/jquery/jquery-1.4.1.min.js");
            currentPage.HeadSection.AddJavascriptFile(JavascriptGroup.ControlOrPlaceholder, "js/_system/TopMultiLevelDropDownMenu.js");

            currentPage.HeadSection.AddJSOnReady("var arrowimages={down:['downarrowclass', '" + CmsContext.ApplicationPath + "images/_system/TopMultiLevelDropDownMenu/down.gif', 23], right:['rightarrowclass', '" + CmsContext.ApplicationPath + "images/_system/TopMultiLevelDropDownMenu/right.gif']}");
            currentPage.HeadSection.AddJSOnReady("TopMultiLevelDropDownMenu.buildmenu('" + divId + "', arrowimages);");
            

            writer.Write(html.ToString());
        }

        private string recursiveRender(CmsPage page)
        {
            int currentLevel = page.Level;

            StringBuilder html = new StringBuilder();
            if (page.Id == -1 || currentLevel > this.MaxLevels || ! page.ShowInMenu)
                return "";

            if (!page.isVisibleForCurrentUser)
                return "";

            bool pageOutput = false;
            string ulCssClass = String.Format(ListGroupClassName, currentLevel.ToString(), listItemOutputCount.ToString());
            if (ulCssClass != "")
                ulCssClass = "class=\"" + ulCssClass + "\" ";
            bool ulStartedAtPage = false;

            if (!IncludeHomepage && page.Id == CmsContext.HomePage.Id)
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

                if (page.Id == CmsContext.HomePage.Id)
                {
                    html.Append("<ul " + ulCssClass + ">" + Environment.NewLine);
                    ulStartedAtPage = true;
                }

                html.Append("<li" + CSSClass + " id=\"" + listItemID + "\">");
                html.Append("<a" + CSSClass + " href=\"" + page.Url + "\">");
                html.Append(title);
                html.Append("</a>");

                pageOutput = true;
                

            } // else

            bool atLeastOne = atLeastOneToShow(page.ChildPages);
            if (page.ChildPages.Length > 0 && atLeastOne == true)
            {
                if (!ulStartedAtPage)
                    html.Append("<ul " + ulCssClass + ">" + Environment.NewLine);
                foreach (CmsPage subPage in page.ChildPages)
                {
                    html.Append(recursiveRender(subPage));
                }
                if (!ulStartedAtPage)
                    html.Append("</ul>" + Environment.NewLine);
            }

            if (pageOutput)
            {
                html.Append("</li>" + Environment.NewLine);
            }

            if (ulStartedAtPage)
                html.Append("</ul>" + Environment.NewLine);

            return html.ToString();
        } // recursiveRender

        /// <summary>
        /// If there are child pages, but all of them are hidden, there is
        /// no need to generate a html UL tag for the display.
        /// </summary>
        /// <param name="childPages"></param>
        /// <returns></returns>
        protected bool atLeastOneToShow(CmsPage[] childPages)
        {
            bool atLeastOne = false;
            foreach (CmsPage subPage in childPages)
            {
                if (subPage.ShowInMenu == true)
                    return true;
            }
            return atLeastOne;
        }
    }
}