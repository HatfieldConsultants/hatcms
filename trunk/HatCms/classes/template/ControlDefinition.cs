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
    /// This class holds information gathered from a template about a control specified in that template.
    /// </summary>
    public class CmsControlDefinition
    {
        /// <summary>
        /// The lower-case Control Path.
        /// The control path never has a filename extension (ie "_system/login" is a valid control path, while "_system/login.ascx" is invalid).
        /// </summary>
        public string ControlPath;
                
        public string[] ParamList;

        public CmsControlDefinition(string controlPath, string[] paramList)
        {
            ControlPath = controlPath.ToLower();            
            ParamList = paramList;
        }
        

        public static CmsControlDefinition[] GetByControlPath(CmsControlDefinition[] haystack, string ControlPathToFind)
        {
            List<CmsControlDefinition> ret = new List<CmsControlDefinition>();
            foreach (CmsControlDefinition controlDef in haystack)
            {
                if (String.Compare(controlDef.ControlPath, ControlPathToFind, true) == 0)
                    ret.Add(controlDef);
            } // foreach
            return ret.ToArray();
        }

        public static CmsControlDefinition[] GetByControlName(CmsControlDefinition[] haystack, string ControlNameToFind)
        {
            List<CmsControlDefinition> ret = new List<CmsControlDefinition>();
            foreach (CmsControlDefinition controlDef in haystack)
            {
                if (controlDef.ControlPath.EndsWith(ControlNameToFind, StringComparison.CurrentCultureIgnoreCase))                
                    ret.Add(controlDef);
            } // foreach
            return ret.ToArray();
        }
    }
}

