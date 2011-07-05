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
    public class CmsPlaceholderDefinition
    {
        /// <summary>
        /// the lower-case placeholder type.
        /// </summary>
        public string PlaceholderType;
        
        public int Identifier;
        public string[] ParamList;

        public CmsPlaceholderDefinition(string placeholderType, int identifier, string[] paramList)
        {
            PlaceholderType = placeholderType.ToLower();
            Identifier = identifier;
            ParamList = paramList;
        }

        public static Dictionary<string, List<int>> ToNameIdentifierDictionary(CmsPlaceholderDefinition[] haystack)
        {
            Dictionary<string, List<int>> ret = new  Dictionary<string,List<int>>();
            foreach (CmsPlaceholderDefinition phDef in haystack)
            {
                if (!ret.ContainsKey(phDef.PlaceholderType))
                    ret[phDef.PlaceholderType] = new List<int>();

                ret[phDef.PlaceholderType].Add(phDef.Identifier);
            }
            return ret;
        }

        public static CmsPlaceholderDefinition[] GetByPlaceholderType(CmsPlaceholderDefinition[] haystack, string PlaceholderType)
        {
            List<CmsPlaceholderDefinition> ret = new List<CmsPlaceholderDefinition>();
            foreach (CmsPlaceholderDefinition phDef in haystack)
            {
                if (String.Compare(phDef.PlaceholderType, PlaceholderType, true) == 0)
                    ret.Add(phDef);
            } // foreach
            return ret.ToArray();
        }
    }
}

