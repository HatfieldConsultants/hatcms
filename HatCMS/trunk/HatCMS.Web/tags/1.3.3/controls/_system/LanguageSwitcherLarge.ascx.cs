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
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace HatCMS.Controls._system
{
    public partial class LanguageSwitcherLarge : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(CmsFileDependency.UnderAppPath("images/_system/arrowRight_white.png", new DateTime(2011, 3, 1)));
            return ret.ToArray();
        }
        
        protected override void Render(HtmlTextWriter writer)
        {
            CultureInfo[] cultureInfoArray = CmsConfig.CultureInformation;
            CmsLanguage[] languageArray = CmsConfig.Languages;

            List<string> parts = new List<string>();
            CmsPage p = CmsContext.currentPage;

            for (int x = 0; x < languageArray.Length; x++)
            {
                CmsLanguage lang = languageArray[x];
                if (lang != CmsContext.currentLanguage)
                {
                    CultureInfo ci = cultureInfoArray[x];
                    string langName = ci.EnglishName.Split(new char[] { '(' })[0].Trim();
                    parts.Add("<a href=\"" + p.getUrl(lang) + "\">View in " + langName + "</a>");
                }
            }

            // -- only output the language switcher text in View mode multi-lang website
            StringBuilder html = new StringBuilder("<div class=\"LanguageSwitcherLarge\">");
            if (CmsContext.currentEditMode == CmsEditMode.View && languageArray.Length > 1)
                html.Append(string.Join("<br />", parts.ToArray()));
            else if (CmsContext.currentEditMode == CmsEditMode.View)
                html.Append("WARNING: LanguageSwitcherLarge is not suitable for single language website.");
            else
                html.Append("<a>View in English</a>");  // under edit mode, not a real html <a> tag

            html.Append("</div>");

            writer.Write(html.ToString());
        } // Render
    }
}