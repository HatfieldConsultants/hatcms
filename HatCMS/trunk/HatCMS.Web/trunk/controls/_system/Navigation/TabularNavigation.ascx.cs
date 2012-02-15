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
    public partial class TabularNavigation : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }        

        private bool RenderVertical
        {
            get { return CmsControlUtils.hasControlParameterKey(CmsContext.currentPage, this, "RenderVertical"); }
        }

        private bool RenderHorizontal
        {
            get { return (!RenderVertical); } 
        }

        private bool IncludeHomepage
        {
            get { return (CmsControlUtils.hasControlParameterKey(CmsContext.currentPage, this, "IncludeHomepage")); }
        }

  

        private int MaxLevels
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "MaxLevels", 100);
            } // get
        }



        /// <summary>
        /// defaults to the current page's path.
        /// </summary>
        private string StartRenderAtPagePath
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "StartRenderAtPagePath", CmsContext.currentPage.Path);
            } // get
        }

        private string CellIDFormat
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "CellIDFormat", "TabularNavigationCell_{1}");
            } // get
        }

        private string TableCSSClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "TableCSSClassName", "TabularNavigation");
            } // get
        }



        private string UnSelectedCellOnMouseOverJS
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "UnSelectedCellOnMouseOverJS", "");
            } // get
        }

        private string UnSelectedCellOnMouseOutJS
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "UnSelectedCellOnMouseOutJS", "");
            } // get
        }

        private string SelectedCellClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "SelectedCellClassName", "Level{0}_Selected");
            } // get
        }

        private string UnSelectedCellClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "UnSelectedCellClassName", "Level{0}");
            } // get
        }

        private string ChildIsSelectedCellClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, "ChildIsSelectedCellClassName", "Level{0}_ChildSelected");
            } // get
        }

        private bool RenderOnlyChildrenUnderCurrentPage
        {
            get { return (CmsControlUtils.hasControlParameterKey(CmsContext.currentPage, this, "RenderOnlyChildrenUnderCurrentPage")); }
        }

        private bool RenderAllChildren
        {
            get { return (!RenderOnlyChildrenUnderCurrentPage); }
        }

        private int cellOutputCount = 0;

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            CmsPage startRenderAtPage = CmsContext.getPageByPath(StartRenderAtPagePath);
            
            StringBuilder html = new StringBuilder();

            html.Append("<table class=\"" + TableCSSClassName + "\">" + Environment.NewLine);
            if (this.RenderHorizontal)
                html.Append("<tr>" + Environment.NewLine);

            cellOutputCount = 0;
            string s = recursiveRender(startRenderAtPage);
            html.Append(s);

            if (this.RenderHorizontal)
                html.Append("</tr>" + Environment.NewLine);
            html.Append("</table>");
            writer.Write(html.ToString());
        } // Render


        private string recursiveRender(CmsPage page )
        {
            int currentLevel = page.Level;

            StringBuilder html = new StringBuilder();
            if (page.Id == -1 || currentLevel > this.MaxLevels)
                return "";

            if (!page.isVisibleForCurrentUser)
                return "";            

            if (!IncludeHomepage && page.Path == CmsContext.HomePage.Path)
            {
                Console.Write("not including home page");
            }
            else
            {
                if (this.RenderVertical)
                    html.Append("<tr>");

                cellOutputCount++;
                string cellId = String.Format(CellIDFormat, currentLevel.ToString(), cellOutputCount.ToString());

                string name = String.Format(UnSelectedCellClassName, currentLevel.ToString(), cellOutputCount.ToString());
                string CSSClass = " class=\"" + name + "\""; // 
                if (page.Path == CmsContext.currentPage.Path)
                {
                    name = String.Format(SelectedCellClassName, currentLevel.ToString(), cellOutputCount.ToString());
                    CSSClass = " class=\"" + name + "\""; // 
                }
                else if (page.isChildSelected())
                {
                    name = String.Format(ChildIsSelectedCellClassName, currentLevel.ToString(), cellOutputCount.ToString());
                    CSSClass = " class=\"" + name + "\""; // 
                }

                string title = page.MenuTitle;
                if (title == "")
                    title = page.Title;

                string onMouseOver = "";
                string onMouseOut = "";
                if (! page.isChildOrSelfSelected())
                {
                    if (UnSelectedCellOnMouseOverJS != "")
                    {
                        name = String.Format(UnSelectedCellOnMouseOverJS, currentLevel.ToString(), cellOutputCount.ToString());
                        onMouseOver = " onmouseover=\"" + name + "\"";
                    }

                    if (UnSelectedCellOnMouseOutJS != "")
                    {
                        name = String.Format(UnSelectedCellOnMouseOutJS, currentLevel.ToString(), cellOutputCount.ToString());
                        onMouseOut = " onmouseout=\"" + name + "\"";
                    }
                }


                html.Append("<td" + CSSClass + " id=\"" + cellId + "\"" + onMouseOver + "" + onMouseOut + ">");
                html.Append("<a" + CSSClass + " href=\"" + page.Url + "\">");
                html.Append(title);
                html.Append("</a>");
                html.Append("</td>" + Environment.NewLine);

                if (this.RenderVertical)
                    html.Append("</tr>"+Environment.NewLine);
            }
            
            if (page.ChildPages.Length > 0 && (RenderAllChildren || (RenderOnlyChildrenUnderCurrentPage && page.isChildOrSelfSelected())))
            {
                // html.Append("<ul class=\"level" + (currentLevel + 1).ToString() + "\">"+Environment.NewLine);
                foreach (CmsPage subPage in page.ChildPages)
                {
                    html.Append(recursiveRender(subPage));
                }
                // html.Append("</ul>"+Environment.NewLine);
            }

            return html.ToString();
        } // recursiveRender
    }
}