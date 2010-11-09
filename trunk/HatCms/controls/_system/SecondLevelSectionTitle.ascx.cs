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

namespace HatCMS.controls
{
    public partial class SecondLevelSectionTitle : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// default: true.
        /// </summary>
        private bool UsePageTitle
        {
            get { return !UseMenuTitle; }
        }

        /// <summary>
        /// default: false
        /// </summary>
        private bool UseMenuTitle
        {
            get 
            {
                return CmsControlUtils.getControlParameterKeyValue(this, "UseMenuTitle", false);
            }
        }

        /// <summary>
        /// default value: "SecondLevelSectionTitle"
        /// </summary>
        private string OuterDivCSSClassName
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(this, "OuterDivCSSClassName", "SecondLevelSectionTitle");
            } // get
        }

        protected override void Render(HtmlTextWriter writer)
        {
            StringBuilder html = new StringBuilder();

            CmsPage currentPage = CmsContext.currentPage;
            CmsPage homePage = CmsContext.HomePage;

            if (currentPage.Path == homePage.Path)
            {
                writer.Write("<div class=\"" + OuterDivCSSClassName + "\"></div>");
                return;
            }


            // -- get the secondLevelMainPage
            CmsPage secondLevelMainPage = currentPage;
            while (secondLevelMainPage.Level > 1)
                secondLevelMainPage = secondLevelMainPage.ParentPage;

            string title = secondLevelMainPage.MenuTitle;
            if (title == "" || UsePageTitle)
                title = secondLevelMainPage.Title;


            html.Append("<div class=\"" + OuterDivCSSClassName + "\">");
            html.Append("<a href=\"" + secondLevelMainPage.Url + "\">" + title + "</a>");
            html.Append("</div>");

            writer.Write(html.ToString());
        } // Render
    }
}