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
using Hatfield.Web.Portal;
using SharpArch.Core.DomainModel;
using NHibernate.Validator.Constraints;

namespace HatCMS
{
    /// <summary>
    /// The CmsPageLanguageInfo class tracks language dependent parts of a <see cref="CmsPage"/>. <seealso cref="CmsPage.LanguageInfo"/>
    /// </summary>
    /// 
    
    public class CmsPageLanguageInfo : Entity
    {
        #region domain model
        private CmsPage page;
        [DomainSignature]
        public virtual CmsPage Page
        {
            get { return page; }
            set { page = value; }
        }

        private string languageShortCode;
        [DomainSignature]
        public virtual string LanguageShortCode
        {
            get { return languageShortCode; }
            set { languageShortCode = value; }
        }
        private string name;

        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string title;

        public virtual string Title
        {
            get { return title; }
            set { title = value; }
        }
        private string menuTitle;

        public virtual string MenuTitle
        {
            get { return menuTitle; }
            set { menuTitle = value; }
        }
        private string searchEngineDescription;

        public virtual string SearchEngineDescription
        {
            get { return searchEngineDescription; }
            set { searchEngineDescription = value; }
        }
        #endregion domain model
        /// <summary>
        /// returns a newly created CmsPageLanguageInfo object if <paramref name="langToFind"/>  was not found in <paramref name="haystack"/>
        /// </summary>
        /// <param name="langToFind"></param>
        /// <param name="haystack"></param>
        /// <returns></returns>
        public static CmsPageLanguageInfo GetFromHaystack(CmsLanguage langToFind, IList<CmsPageLanguageInfo> haystack)
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
