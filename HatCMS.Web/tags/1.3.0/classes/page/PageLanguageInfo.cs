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
    /// The CmsPageLanguageInfo class tracks language dependent parts of a <see cref="CmsPage"/>. <seealso cref="CmsPage.LanguageInfo"/>
    /// </summary>
    public class CmsPageLanguageInfo
    {
        public string languageShortCode;
        public string name;
        public string title;
        public string menuTitle;
        public string searchEngineDescription;

        /// <summary>
        /// returns a newly created CmsPageLanguageInfo object if <paramref name="langToFind"/>  was not found in <paramref name="haystack"/>
        /// </summary>
        /// <param name="langToFind"></param>
        /// <param name="haystack"></param>
        /// <returns></returns>
        public static CmsPageLanguageInfo GetFromHaystack(CmsLanguage langToFind, CmsPageLanguageInfo[] haystack)
        {
            foreach (CmsPageLanguageInfo l in haystack)
            {
                if (string.Compare(l.languageShortCode, langToFind.shortCode, true) == 0)
                    return l;
            } // foreach

            return new CmsPageLanguageInfo();
        }

        public static string[] GetNames(CmsPageLanguageInfo[] langInfos)
        {
            List<string> ret = new List<string>();
            foreach (CmsPageLanguageInfo l in langInfos)
                ret.Add(l.name);
            return ret.ToArray();

        }

        public static string[] GetTitles(CmsPageLanguageInfo[] langInfos)
        {
            List<string> ret = new List<string>();
            foreach (CmsPageLanguageInfo l in langInfos)
                ret.Add(l.title);
            return ret.ToArray();

        }

        public static string[] GetMenuTitles(CmsPageLanguageInfo[] langInfos)
        {
            List<string> ret = new List<string>();
            foreach (CmsPageLanguageInfo l in langInfos)
                ret.Add(l.menuTitle);
            return ret.ToArray();

        }

        public static string[] GetSearchEngineDescriptions(CmsPageLanguageInfo[] langInfos)
        {
            List<string> ret = new List<string>();
            foreach (CmsPageLanguageInfo l in langInfos)
                ret.Add(l.searchEngineDescription);
            return ret.ToArray();

        }

    }
}
