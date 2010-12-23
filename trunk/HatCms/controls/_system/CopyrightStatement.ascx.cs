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
using System.Collections.Generic;

namespace HatCMS.controls._system
{
    public partial class CopyrightStatement : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("CopyrightStatement.CopyrightText"));
            ret.Add(new CmsConfigItemDependency("CopyrightStatement.CssStyle"));

            return ret.ToArray();
        }

        protected string getCopyrightText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("CopyrightStatement.CopyrightText", "Copyright &#169; {0}.", lang);
        }

        protected string getCopyrightCssStyle()
        {
            return CmsConfig.getConfigValue("CopyrightStatement.CssStyle", "");
        }

        protected override void Render(HtmlTextWriter writer)
        {
            int yyyy = DateTime.Now.Year;
            CmsLanguage lang = CmsContext.currentLanguage;
            string text = String.Format(getCopyrightText(lang), new string[] { yyyy.ToString() });

            StringBuilder html = new StringBuilder("<div class=\"copyrightStatement\" style=\"" + getCopyrightCssStyle() + "\">");
            html.Append(text);
            html.Append("</div>");

            writer.Write(html.ToString());
        }
    }
}