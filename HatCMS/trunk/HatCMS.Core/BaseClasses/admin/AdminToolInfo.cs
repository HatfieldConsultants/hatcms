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

namespace HatCMS.Admin
{
    public class CmsAdminToolInfo
    {
        public BaseCmsAdminTool.CmsAdminToolCategory Category;        
        public BaseCmsAdminTool.AdminMenuTab MenuTab;
        public Dictionary<CmsLanguage, string> MenuDisplayText;

        public CmsAdminToolInfo(BaseCmsAdminTool.CmsAdminToolCategory category, BaseCmsAdminTool.AdminMenuTab menuTab, Dictionary<CmsLanguage, string> menuDisplayText)
        {
            Category = category;
            MenuTab = menuTab;
            MenuDisplayText = menuDisplayText;
        } // constructor

        public CmsAdminToolInfo(BaseCmsAdminTool.CmsAdminToolCategory category, BaseCmsAdminTool.AdminMenuTab menuTab, string menuDisplayTextForAllLanguages)
        {
            Category = category;
            MenuTab = menuTab;
            MenuDisplayText = new Dictionary<CmsLanguage, string>();
            foreach (CmsLanguage lang in CmsConfig.Languages)
            {
                MenuDisplayText.Add(lang, menuDisplayTextForAllLanguages);
            }
        } // constructor

        public static Dictionary<BaseCmsAdminTool.CmsAdminToolCategory, List<CmsAdminToolInfo>> ToCategoryKeyedDictionary(CmsAdminToolInfo[] arr)
        {
            Dictionary<BaseCmsAdminTool.CmsAdminToolCategory, List<CmsAdminToolInfo>> ret = new Dictionary<BaseCmsAdminTool.CmsAdminToolCategory, List<CmsAdminToolInfo>>();
            foreach (CmsAdminToolInfo toolInfo in arr)
            {
                if (!ret.ContainsKey(toolInfo.Category))
                    ret[toolInfo.Category] = new List<CmsAdminToolInfo>();

                ret[toolInfo.Category].Add(toolInfo);
            } // foreach
            return ret;
        }

        public static Dictionary<BaseCmsAdminTool.AdminMenuTab, List<CmsAdminToolInfo>> ToTabKeyedDictionary(CmsAdminToolInfo[] arr)
        {
            Dictionary<BaseCmsAdminTool.AdminMenuTab, List<CmsAdminToolInfo>> ret = new Dictionary<BaseCmsAdminTool.AdminMenuTab, List<CmsAdminToolInfo>>();
            foreach (CmsAdminToolInfo toolInfo in arr)
            {
                if (!ret.ContainsKey(toolInfo.MenuTab))
                    ret[toolInfo.MenuTab] = new List<CmsAdminToolInfo>();

                ret[toolInfo.MenuTab].Add(toolInfo);
            } // foreach
            return ret;
        }

    }
}
