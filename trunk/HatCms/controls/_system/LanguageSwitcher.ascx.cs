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

namespace HatCMS.controls._system
{
    public partial class SwitchLanguage : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void Render(HtmlTextWriter writer)
        {
            // -- only output in View mode
            StringBuilder html = new StringBuilder();

            
            List<string> parts = new List<string>();
            CmsPage p = CmsContext.currentPage;
            foreach (CmsLanguage lang in CmsConfig.Languages)
            {
                if (lang == CmsContext.currentLanguage)
                {
                    parts.Add("<strong>" + lang.shortCode + "</strong>"); 
                }
                else
                {
                    parts.Add("<a href=\"" + p.getUrl(lang) + "\">" + lang.shortCode + "</a>");
                }
            } // foreach

            html.Append("<div class=\"LanguageSwitcher\">");
            if (CmsContext.currentEditMode == CmsEditMode.View)
            {
                html.Append(string.Join(" | ", parts.ToArray()));
            }
            html.Append("</div>");

            writer.Write(html.ToString());
        } // Render
    }
}