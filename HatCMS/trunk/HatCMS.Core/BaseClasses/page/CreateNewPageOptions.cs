using System;
using System.Collections.Specialized;
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

namespace HatCMS
{

    public class CmsCreateNewPageOptions
    {
        /// <summary>
        /// Characters that should not be allowed in the Page.Name
        /// </summary>
        public static string[] InvalidNameCharacters = new string[] {
            @"\", "/", "#", "+", ":", "%"
        };
        
        
        public bool PromptUserForFilename = true;
        public bool PromptUserForTitle = true;
        public bool PromptUserForMenuTitle = true;
        public bool PromptUserForShowInMenu = true;
        public bool PromptUserForTemplate = true;
        public bool PromptUserForParentPage = true;

        public CmsPageLanguageInfo[] NewPageLanguageInfos;
        /*        
            public string languageShortCode;
            public string name;
            public string title;
            public string menuTitle;
            public string searchEngineDescription;
         */


        public bool ShowInMenu = true;
        public string Template = "";
        public int ParentPageId = -1;


        private bool AllItemsHaveAValue(string[] vals)
        {
            foreach (string v in vals)
            {
                if (v.Trim() == "")
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the options entered require user input. If user input is not required, the page should be created automatically and the user taken to EditMode of the new page.
        /// This function does some rudimentary validation of object values. If not all object values are filled in, user input is needed (returns true).
        /// </summary>
        /// <returns></returns>
        public bool RequiresUserInput()
        {
            if (!PromptUserForFilename && AllItemsHaveAValue(CmsPageLanguageInfo.GetNames(NewPageLanguageInfos))
                && !PromptUserForTitle 
                && !PromptUserForMenuTitle 
                && !PromptUserForTemplate && Template.Trim() != ""
                && !PromptUserForParentPage && ParentPageId >= 0)
                return false;
            else
                return true;                        
        }

        public NameValueCollection GetCreatePagePopupParams()
        {
            NameValueCollection ret = new NameValueCollection();
            ret.Add("pFilename", PromptUserForFilename.ToString());
            ret.Add("pTitle", PromptUserForTitle.ToString());
            ret.Add("pMenuTitle", PromptUserForMenuTitle.ToString());
            ret.Add("pShowInMenu", PromptUserForShowInMenu.ToString());
            ret.Add("pTemplate", PromptUserForTemplate.ToString());
            ret.Add("pParentPage", PromptUserForParentPage.ToString());
            foreach (CmsPageLanguageInfo l in NewPageLanguageInfos)
            {
                ret.Add("MenuTitle_" + l.LanguageShortCode, l.MenuTitle);
                ret.Add("Title_" + l.LanguageShortCode, l.Title);
                ret.Add("SEDesc_" + l.LanguageShortCode, l.SearchEngineDescription);
                ret.Add("Name_" + l.LanguageShortCode, l.Name);
            } // foreach
            ret.Add("ShowInMenu", ShowInMenu.ToString());
            ret.Add("Template", Template);
            ret.Add("ParentPageId", ParentPageId.ToString());

            return ret;
        }

        public CmsPage ToCmsPageObject()
        {
            CmsPage newPage = new CmsPage();

            newPage.LanguageInfo = NewPageLanguageInfos;        

            newPage.ShowInMenu = ShowInMenu;            
            newPage.TemplateName = Template;
            newPage.ParentID = ParentPageId;

            // -- set sortOrdinal
            CmsPage parentPage = CmsContext.getPageById(ParentPageId);
            if (parentPage.Id >= 0)
            {
                int highestSiblingSortOrdinal = -1;
                foreach (CmsPage sibling in parentPage.ChildPages)
                {
                    highestSiblingSortOrdinal = Math.Max(sibling.SortOrdinal, highestSiblingSortOrdinal);
                }
                if (highestSiblingSortOrdinal > -1)
                    newPage.SortOrdinal = highestSiblingSortOrdinal + 1;
                else
                    newPage.SortOrdinal = 0;
            }
            else
                newPage.SortOrdinal = 0;

            return newPage;
        }

        public static CmsCreateNewPageOptions ReadFromQueryString()
        {
            int parentId = PageUtils.getFromForm("ParentPageId", 0);
            CmsCreateNewPageOptions ret = new CmsCreateNewPageOptions();
            // -- modify smart defaults if the query string isn't formatted correctly. 
            if (parentId >= 0)
            {
                CmsPage parentPage = CmsContext.getPageById(parentId);
                if (parentPage.Id >= 0)
                {
                    parentId = parentPage.Id;
                    ret = CreateInstanceWithDefaultsForCurrentUser(parentPage);
                }
            }

            ret.PromptUserForFilename = PageUtils.getFromForm("pFilename", ret.PromptUserForFilename);
            ret.PromptUserForTitle = PageUtils.getFromForm("pTitle", ret.PromptUserForTitle);
            ret.PromptUserForMenuTitle = PageUtils.getFromForm("pMenuTitle", ret.PromptUserForMenuTitle);
            ret.PromptUserForShowInMenu = PageUtils.getFromForm("pShowInMenu", ret.PromptUserForShowInMenu);
            ret.PromptUserForTemplate = PageUtils.getFromForm("pTemplate", ret.PromptUserForTemplate);
            ret.PromptUserForParentPage = PageUtils.getFromForm("pParentPage", ret.PromptUserForParentPage);

            List<CmsPageLanguageInfo> langInfos = new List<CmsPageLanguageInfo>();
            foreach (CmsLanguage lang in CmsConfig.Languages)
            {
                CmsPageLanguageInfo langInfo = CmsPageLanguageInfo.GetFromHaystack(lang, ret.NewPageLanguageInfos); // returns a new CmsPageLangInfo if not found
                langInfo.LanguageShortCode = lang.shortCode;
                langInfo.MenuTitle = PageUtils.getFromForm("MenuTitle_" + lang.shortCode, langInfo.MenuTitle);
                langInfo.Title = PageUtils.getFromForm("Title_" + lang.shortCode, langInfo.Title);
                langInfo.SearchEngineDescription = PageUtils.getFromForm("SEDesc_" + lang.shortCode, langInfo.SearchEngineDescription);
                langInfo.Name = PageUtils.getFromForm("Name_" + lang.shortCode, langInfo.Name);

                langInfos.Add(langInfo);
            } // foreach lang

            ret.NewPageLanguageInfos = langInfos.ToArray();

            ret.ShowInMenu = PageUtils.getFromForm("ShowInMenu", ret.ShowInMenu);
            ret.Template = PageUtils.getFromForm("Template", ret.Template);
            ret.ParentPageId = parentId;

            return ret;
        }
        

        public static CmsCreateNewPageOptions GetInstanceWithNoUserPrompts(string newPageName, string newPageTitle, string newPageMenuTitle, string newPageSearchEngineDescription, bool newPageShowInMenu, string newPageTemplate, int newPageParentId)
        {
            List<CmsPageLanguageInfo> newPageLangInfos = new List<CmsPageLanguageInfo>();
            foreach (CmsLanguage lang in CmsConfig.Languages)
            {
                CmsPageLanguageInfo langInfo = new CmsPageLanguageInfo();
                langInfo.LanguageShortCode = lang.shortCode;
                langInfo.Name = newPageName;
                langInfo.Title = newPageTitle;
                langInfo.MenuTitle = newPageMenuTitle;
                langInfo.SearchEngineDescription = newPageSearchEngineDescription;

                newPageLangInfos.Add(langInfo);
            }

            return CreateInstanceWithNoUserPrompts(newPageLangInfos.ToArray(), newPageShowInMenu, newPageTemplate, newPageParentId);
        }

        public static CmsCreateNewPageOptions CreateInstanceWithNoUserPrompts(CmsPageLanguageInfo[] newPageLangInfos, bool newPageShowInMenu, string newPageTemplate, int newPageParentId)
        {
            CmsCreateNewPageOptions ret = new CmsCreateNewPageOptions();
            ret.PromptUserForFilename = false;
            ret.PromptUserForTitle = false;
            ret.PromptUserForMenuTitle = false;
            ret.PromptUserForShowInMenu = false;
            ret.PromptUserForTemplate = false;
            ret.PromptUserForParentPage = false;

            ret.NewPageLanguageInfos = newPageLangInfos;
            ret.ShowInMenu = newPageShowInMenu;
            ret.Template = newPageTemplate;
            ret.ParentPageId = newPageParentId;

            return ret;
        }

        public static CmsCreateNewPageOptions CreateInstanceWithDefaultsForCurrentUser(CmsPage editMenuPage)
        {
            CmsCreateNewPageOptions ret = new CmsCreateNewPageOptions();
            // CreateNewPage_DefaultTemplate; use "~~ParentTemplate~~" for the new page's parent's template 
            string defaultTemplate = CmsConfig.getConfigValue("CreateNewPage_DefaultTemplate", editMenuPage.TemplateName);
            if (string.Compare(defaultTemplate, "~~ParentTemplate~~", true) == 0)
                defaultTemplate = editMenuPage.TemplateName;

            if (CmsContext.currentUserIsSuperAdmin)
            {
                ret.PromptUserForFilename = true;
                ret.PromptUserForTitle = true;
                ret.PromptUserForMenuTitle = true;
                ret.PromptUserForShowInMenu = true;
                ret.PromptUserForTemplate = true;
                ret.PromptUserForParentPage = true;

                List<CmsPageLanguageInfo> langInfos = new List<CmsPageLanguageInfo>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    CmsPageLanguageInfo l = new CmsPageLanguageInfo();
                    l.LanguageShortCode = lang.shortCode;
                    l.Name = "";
                    l.Title = "";
                    l.MenuTitle = "";
                    l.SearchEngineDescription = "";

                    langInfos.Add(l);
                }

                ret.NewPageLanguageInfos = langInfos.ToArray();

                ret.ShowInMenu = true;
                ret.Template = defaultTemplate;
                ret.ParentPageId = editMenuPage.Id;
            }
            else if (editMenuPage.currentUserCanWrite)
            {
                ret.PromptUserForFilename = true;
                ret.PromptUserForTitle = true;
                ret.PromptUserForMenuTitle = false;
                ret.PromptUserForShowInMenu = false;
                ret.PromptUserForTemplate = false;
                ret.PromptUserForParentPage = false;

                List<CmsPageLanguageInfo> langInfos = new List<CmsPageLanguageInfo>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    CmsPageLanguageInfo l = new CmsPageLanguageInfo();
                    l.LanguageShortCode = lang.shortCode;
                    l.Name = "";
                    l.Title = "";
                    l.MenuTitle = "";
                    l.SearchEngineDescription = "";

                    langInfos.Add(l);
                }

                ret.NewPageLanguageInfos = langInfos.ToArray();

                ret.ShowInMenu = true;
                ret.Template = defaultTemplate;
                ret.ParentPageId = editMenuPage.Id;
            }
            else
            {
                ret.PromptUserForFilename = false;
                ret.PromptUserForTitle = false;
                ret.PromptUserForMenuTitle = false;
                ret.PromptUserForShowInMenu = false;
                ret.PromptUserForTemplate = false;
                ret.PromptUserForParentPage = false;

                List<CmsPageLanguageInfo> langInfos = new List<CmsPageLanguageInfo>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    CmsPageLanguageInfo l = new CmsPageLanguageInfo();
                    l.LanguageShortCode = lang.shortCode;
                    l.Name = "";
                    l.Title = "";
                    l.MenuTitle = "";
                    l.SearchEngineDescription = "";

                    langInfos.Add(l);
                }

                ret.NewPageLanguageInfos = langInfos.ToArray();

                ret.ShowInMenu = true;
                ret.Template = defaultTemplate;
                ret.ParentPageId = editMenuPage.Id;
            }

            return ret;
        }

    }
    

}
