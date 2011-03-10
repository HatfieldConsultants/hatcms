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
        public CmsBaseAdminTool.CmsAdminToolCategory Category;        
        public CmsBaseAdminTool.CmsAdminToolClass Class;
        public Dictionary<CmsLanguage, string> MenuDisplayText;

        public CmsAdminToolInfo(CmsBaseAdminTool.CmsAdminToolCategory category, CmsBaseAdminTool.CmsAdminToolClass toolClass, Dictionary<CmsLanguage, string> menuDisplayText)
        {
            Category = category;
            Class = toolClass;
            MenuDisplayText = menuDisplayText;
        } // constructor

        public CmsAdminToolInfo(CmsBaseAdminTool.CmsAdminToolCategory category, CmsBaseAdminTool.CmsAdminToolClass toolClass, string menuDisplayTextForAllLanguages)
        {
            Category = category;
            Class = toolClass;
            MenuDisplayText = new Dictionary<CmsLanguage, string>();
            foreach (CmsLanguage lang in CmsConfig.Languages)
            {
                MenuDisplayText.Add(lang, menuDisplayTextForAllLanguages);
            }
        } // constructor

        public static Dictionary<CmsBaseAdminTool.CmsAdminToolCategory, List<CmsAdminToolInfo>> ToCategoryKeyedDictionary(CmsAdminToolInfo[] arr)
        {
            Dictionary<CmsBaseAdminTool.CmsAdminToolCategory, List<CmsAdminToolInfo>> ret = new Dictionary<CmsBaseAdminTool.CmsAdminToolCategory, List<CmsAdminToolInfo>>();
            foreach (CmsAdminToolInfo toolInfo in arr)
            {
                if (!ret.ContainsKey(toolInfo.Category))
                    ret[toolInfo.Category] = new List<CmsAdminToolInfo>();

                ret[toolInfo.Category].Add(toolInfo);
            } // foreach
            return ret;
        }

    }
}
