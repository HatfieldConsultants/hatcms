using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS
{
    public class HtmlLinkMacroFilter : BaseCmsOutputFilter
    {
        
        /// <summary>
        /// Registers the HtmlLinkMacroFilter as a page-level filter.
        /// </summary>
        /// <returns></returns>
        public override CmsOutputFilterInfo getOutputFilterInfo()
        {
            return new CmsOutputFilterInfo(CmsOutputFilterScope.PageHtmlOutput, RunHtmlLinkMacroFilter);
        }

        
        private static bool includePageLanguageInMacro()
        {
            if (CmsConfig.Languages.Length > 1)
                return CmsConfig.getConfigValue("LinkMacrosIncludeLanguage", false);
            else
                return false;
        }

        public static string getLinkMacro(CmsPage page, CmsLanguage language)
        {
            // notes: do not use anything that will need encoding such as {} characters.
            //        start with a / so that ckeditor thinks this is a real link.
            //        should be all lower-case
            if (includePageLanguageInMacro())
                return getLinkMacroPrefix() + page.Id + "_" + language.shortCode.ToLower() + getLinkMacroSuffix();
            else
                return getLinkMacroPrefix() + page.Id + "_" + getLinkMacroSuffix();
        }

        public static string getLinkMacroPrefix()
        {
            return "/__hatcmspageurl_";
        }

        public static string getLinkMacroSuffix()
        {
            return "__/";
        }

        private string getMacroReplacement(string macro, string macroPrefix, string macroSuffix, CmsPage sourcePage)
        {

            string cleaned = macro.Substring(macroPrefix.Length, macro.Length - macroPrefix.Length - macroSuffix.Length);
            string[] parts = cleaned.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 1)
            {
                int pageId = Convert.ToInt32(parts[0]);
                CmsLanguage lang = CmsContext.currentLanguage;
                if (parts.Length >= 2 && includePageLanguageInMacro())
                {
                    string langCode = parts[1];
                    lang = CmsLanguage.GetFromHaystack(langCode, CmsConfig.Languages);
                }

                if (!lang.isInvalidLanguage)
                {
                    CmsPage p = CmsContext.getPageById(pageId);
                    if (p.Id >= 0)
                        return p.getUrl(lang);

                }
            }

            string pageNotFoundUrl = CmsContext.ApplicationPath + "PageNotFound" + macro.Replace("/", "") + "source_" + sourcePage.Id.ToString() + ".aspx";
            return pageNotFoundUrl;
        }

        public string RunHtmlLinkMacroFilter(CmsPage pageBeingFiltered, string pageHtml)
        {            
            
            // Replace all link macros with the actual links to a page.
            StringBuilder html = new StringBuilder(pageHtml); // use StringBuilder as it's way more efficient: http://dotnetperls.com/replace
            if (CmsContext.currentEditMode == CmsEditMode.View)
            {

                string macroPrefix = getLinkMacroPrefix();
                string macroSuffix = getLinkMacroSuffix();

                string s = html.ToString();
                int prefixIndex = s.IndexOf(macroPrefix);
                int suffixIndex = -1;
                if (prefixIndex >= 0)
                    suffixIndex = s.IndexOf(macroSuffix, prefixIndex);
                else
                    suffixIndex = -1;              

                while (prefixIndex >= 0 && suffixIndex > 0)
                {
                    string macro = html.ToString(prefixIndex, suffixIndex - prefixIndex + macroSuffix.Length);

                    html.Remove(prefixIndex, suffixIndex - prefixIndex + macroSuffix.Length);
                    html.Insert(prefixIndex, getMacroReplacement(macro, macroPrefix, macroSuffix, pageBeingFiltered));
                    s = html.ToString();
                    prefixIndex = s.IndexOf(macroPrefix);
                    if (prefixIndex >= 0)
                        suffixIndex = s.IndexOf(macroSuffix, prefixIndex);
                    else
                        suffixIndex = -1;
                } // while                    
            }
            return html.ToString();
        }
    }
}

