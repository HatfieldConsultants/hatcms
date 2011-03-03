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
    public class CmsPlaceholderContentDependency : CmsDependency
    {
        public string placeholderTypeToMatch;
        public ExistsMode existsMode;
        public string contentToFind;
        StringComparison comparisonMode;

        public CmsPlaceholderContentDependency(string ContentToFind, ExistsMode ContentExistsMode, StringComparison ComparisonMode)
        {
            placeholderTypeToMatch = "";
            contentToFind = ContentToFind;
            existsMode = ContentExistsMode;
            comparisonMode = ComparisonMode;            
        }

        public CmsPlaceholderContentDependency(string PlaceholderType, string ContentToFind, ExistsMode ContentExistsMode, StringComparison ComparisonMode)
        {
            placeholderTypeToMatch = PlaceholderType;
            contentToFind = ContentToFind;
            existsMode = ContentExistsMode;
            comparisonMode = ComparisonMode;
        }

        private CmsDependencyMessage[] contentMatches(string ContentToSearch, CmsPage inPage, CmsLanguage inLang)
        {
            if (ContentToSearch.Trim() == "")
                return new CmsDependencyMessage[0]; // short circuit if ContentToSearch is blank

            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
            string errMsg = errMsg = "<a href=\""+inPage.getUrl(inLang)+"\" target=\"_blank\">This page</a> {0} contain the text '"+contentToFind+"'";;
            if (placeholderTypeToMatch == "")
                errMsg = "The "+placeholderTypeToMatch+" placeholder on <a href=\""+inPage.getUrl(inLang)+"\" target=\"_blank\">this page</a> {0} contain the text '"+contentToFind+"'";
            switch (existsMode)
            {
                case ExistsMode.MustExist:
                    if (ContentToSearch.IndexOf(contentToFind, comparisonMode) < 0)
                        ret.Add(new CmsDependencyMessage(CmsDependencyMessage.MessageLevel.Error, String.Format(errMsg, "must")));
                    break;
                case ExistsMode.MustNotExist:
                    if (ContentToSearch.IndexOf(contentToFind, comparisonMode) >= 0)
                        ret.Add(new CmsDependencyMessage(CmsDependencyMessage.MessageLevel.Error, String.Format(errMsg, "must NOT")));
                    break;
            }

            return ret.ToArray();
        }
        
        public override CmsDependencyMessage[] ValidateDependency()
        {
            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
            if (placeholderTypeToMatch == "")
            {
                // -- no placeholderType specified, so render all placeholders.
                Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
                foreach (CmsPage page in allPages.Values)
                {
                    foreach (CmsLanguage lang in CmsConfig.Languages)
                    {
                        string allPhContent = page.renderAllPlaceholdersToString(lang);
                        ret.AddRange(contentMatches(allPhContent, page, lang));
                    } // foreach lang
                } // foreach page
            }
            else
            {
                // -- get all placeholders of the type specified recursively.
                Dictionary<CmsPage, CmsPlaceholderDefinition[]> dict = CmsContext.getAllPlaceholderDefinitions(placeholderTypeToMatch, CmsContext.HomePage, CmsContext.PageGatheringMode.FullRecursion);
                foreach (CmsPage page in dict.Keys)
                {
                    foreach (CmsPlaceholderDefinition phDef in dict[page])
                    {
                        foreach (CmsLanguage lang in CmsConfig.Languages)
                        {
                            string phContent = page.renderPlaceholderToString(phDef, lang);
                            ret.AddRange(contentMatches(phContent, page, lang));
                        }
                    } // foreach phDef
                } // foreach page
            }

            return ret.ToArray();
        }

        public override string GetContentHash()
        {
            List<string> hashParts = new List<string>();

            hashParts.Add(Enum.GetName(typeof(ExistsMode), existsMode));
            hashParts.Add(contentToFind);
            hashParts.Add(Enum.GetName(typeof(StringComparison), comparisonMode));

            return string.Join("_", hashParts.ToArray());
        }
    }
}
