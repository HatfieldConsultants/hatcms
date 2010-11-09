using System;
using System.Collections.Generic;
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
    /// <summary>
    /// A dependecy that requires that a CmsPage with the given path (and optional template) exists in the CMS.
    /// </summary>
    public class CmsPageDependency: CmsDependency
    {
        public string PagePath;
        public string PageTemplate;
        public CmsLanguage[] LanguagesThatMustHavePagePath;

        public CmsPageDependency(string pagePath, CmsLanguage[] languagesThatMustHavePagePath)
        {
            PagePath = pagePath;
            PageTemplate = "";
            LanguagesThatMustHavePagePath = languagesThatMustHavePagePath;
        }

        public CmsPageDependency(string pagePath, string pageTemplate, CmsLanguage[] languagesThatMustHavePagePath)
        {
            PagePath = pagePath;
            PageTemplate = pageTemplate;
            LanguagesThatMustHavePagePath = languagesThatMustHavePagePath;
        }

        public override string GetContentHash()
        {
            List<string> shortCodes = new List<string>();
            foreach (CmsLanguage lang in LanguagesThatMustHavePagePath)
                shortCodes.Add(lang.shortCode);

            return (PagePath.ToLower() + PageTemplate.ToLower()).Trim().ToLower() + string.Join(";", shortCodes.ToArray());
        }
        
        public override CmsDependencyMessage[] ValidateDependency()
        {
            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
            if (LanguagesThatMustHavePagePath.Length == 0)
                ret.Add(CmsDependencyMessage.Error("Could not run CmsPageDependency for path '" + PagePath + "' - no languages are defined!"));
            else
            {
                foreach (CmsLanguage lang in LanguagesThatMustHavePagePath)
                {
                    try
                    {
                        CmsPage page = CmsContext.getPageByPath(PagePath);
                        if (page.ID < 0)
                            ret.Add(CmsDependencyMessage.Error("could not find required page '" + PagePath + "' in language '" + lang.shortCode + "'"));
                        else if (PageTemplate != "" && String.Compare(page.TemplateName, PageTemplate, true) != 0)
                            ret.Add(CmsDependencyMessage.Error("The required page '" + PagePath + "' was found, but does not have the correct template (required: '" + PageTemplate + "'); actual: '" + page.TemplateName + "'"));
                        else
                            ret.AddRange(CmsTemplateDependency.testTemplate(page.TemplateName, System.Web.HttpContext.Current));
                    }
                    catch (Exception ex)
                    {
                        ret.Add(CmsDependencyMessage.Error("Could not find required page '" + PagePath + "' in language '" + lang.shortCode + "'"));
                    }
                } // foreach Language
            }
            return ret.ToArray();
        }
    }
}
