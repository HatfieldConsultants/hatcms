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
        public int PageId;
        public string PageTemplate;
        public CmsLanguage[] LanguagesThatMustHavePagePath;
        public ExistsMode Exists;

        public CmsPageDependency(string pagePath, CmsLanguage[] languagesThatMustHavePagePath)
        {
            PagePath = pagePath;
            PageId = -1;
            PageTemplate = "";
            LanguagesThatMustHavePagePath = languagesThatMustHavePagePath;
            Exists = ExistsMode.MustExist;
        }

        public CmsPageDependency(string pagePath, CmsLanguage[] languagesThatMustHavePagePath, ExistsMode existsMode)
        {
            PagePath = pagePath;
            PageId = -1;
            PageTemplate = "";
            LanguagesThatMustHavePagePath = languagesThatMustHavePagePath;
            Exists = existsMode;
        }

        public CmsPageDependency(int pageID, CmsLanguage[] languagesThatMustHavePagePath)
        {
            PagePath = "";
            PageId = pageID;
            PageTemplate = "";
            LanguagesThatMustHavePagePath = languagesThatMustHavePagePath;
            Exists = ExistsMode.MustExist;
        }

        public CmsPageDependency(string pagePath, string pageTemplate, CmsLanguage[] languagesThatMustHavePagePath)
        {
            PagePath = pagePath;
            PageId = -1;
            PageTemplate = pageTemplate;
            LanguagesThatMustHavePagePath = languagesThatMustHavePagePath;
            Exists = ExistsMode.MustExist;
        }

        public CmsPageDependency(int pageId, string pageTemplate, CmsLanguage[] languagesThatMustHavePagePath)
        {
            PagePath = "";
            PageId = pageId;
            PageTemplate = pageTemplate;
            LanguagesThatMustHavePagePath = languagesThatMustHavePagePath;
            Exists = ExistsMode.MustExist;
        }

        

        public override string GetContentHash()
        {
            List<string> shortCodes = new List<string>();
            foreach (CmsLanguage lang in LanguagesThatMustHavePagePath)
                shortCodes.Add(lang.shortCode);

            return (PagePath.ToLower() + Exists.ToString() + PageId.ToString() + PageTemplate.ToLower()).Trim().ToLower() + string.Join(";", shortCodes.ToArray());
        }

        public override CmsDependencyMessage[] ValidateDependency()
        {
            if (PagePath != "")
                return ValidateByPagePath();
            else if (PageId >= 0)
                return ValidateByPageId();
            else
                return new CmsDependencyMessage[] { new CmsDependencyMessage(CmsDependencyMessage.MessageLevel.Error, "CmsPageDependency configured incorrectly: neither a page path nor a pageId is present.") };
        }

        public CmsDependencyMessage[] ValidateByPageId()
        {
            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
            if (LanguagesThatMustHavePagePath.Length == 0)
                ret.Add(CmsDependencyMessage.Error("Could not run CmsPageDependency for pageId '" + PageId + "' - no languages are configured!"));
            else
            {
                foreach (CmsLanguage lang in LanguagesThatMustHavePagePath)
                {
                    try
                    {
                        CmsPage page = CmsContext.getPageById(PageId);
                        if (Exists == ExistsMode.MustExist)
                        {
                            if (page.Id < 0)
                                ret.Add(CmsDependencyMessage.Error("Could not find required pageId '" + PageId + "' in language '" + lang.shortCode + "'"));
                            else if (PageTemplate != "" && String.Compare(page.TemplateName, PageTemplate, true) != 0)
                                ret.Add(CmsDependencyMessage.Error("The required page '" + PagePath + "' was found, but does not have the correct template (required: '" + PageTemplate + "'); actual: '" + page.TemplateName + "'"));
                            else
                                ret.AddRange(CmsTemplateDependency.testTemplate(page.TemplateName, "Page ID #" + PageId.ToString()));
                        }
                        else if (Exists == ExistsMode.MustNotExist)
                        {
                            if (page.Id >= 0)
                                ret.Add(CmsDependencyMessage.Error("PageId '" + PageId + "' in language '" + lang.shortCode + "' should NOT exist."));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Exists == ExistsMode.MustExist)
                            ret.Add(CmsDependencyMessage.Error("Could not find required pageId '" + PageId + "' in language '" + lang.shortCode + "'"));
                    }
                } // foreach Language
            }
            return ret.ToArray();
        }
        
        public CmsDependencyMessage[] ValidateByPagePath()
        {
            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
            if (LanguagesThatMustHavePagePath.Length == 0)
                ret.Add(CmsDependencyMessage.Error("Could not run CmsPageDependency for path '" + PagePath + "' - no languages are configured!"));
            else
            {
                foreach (CmsLanguage lang in LanguagesThatMustHavePagePath)
                {
                    try
                    {
                        CmsPage page = CmsContext.getPageByPath(PagePath);
                        if (Exists == ExistsMode.MustExist)
                        {
                            if (page.Id < 0)
                                ret.Add(CmsDependencyMessage.Error("could not find required page '" + PagePath + "' in language '" + lang.shortCode + "'"));
                            else if (PageTemplate != "" && String.Compare(page.TemplateName, PageTemplate, true) != 0)
                                ret.Add(CmsDependencyMessage.Error("The required page '" + PagePath + "' was found, but does not have the correct template (required: '" + PageTemplate + "'); actual: '" + page.TemplateName + "'"));
                            else
                                ret.AddRange(CmsTemplateDependency.testTemplate(page.TemplateName, "Page Path '" + PagePath + "'"));
                        }
                        else if (Exists == ExistsMode.MustNotExist)
                        {
                            if (page.Id >= 0)
                                ret.Add(CmsDependencyMessage.Error("The page '" + PagePath + "' in language '" + lang.shortCode + "' should NOT exist."));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Exists == ExistsMode.MustExist)
                            ret.Add(CmsDependencyMessage.Error("Could not find required page '" + PagePath + "' in language '" + lang.shortCode + "'"));
                    }
                } // foreach Language
            }
            return ret.ToArray();
        }
    }
}
