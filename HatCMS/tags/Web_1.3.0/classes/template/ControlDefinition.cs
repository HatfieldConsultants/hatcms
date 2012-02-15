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
        /// The lower-case Control Name or Path.
        /// <para>If the control is purely a class, this parameter is the name of the class. The class must be a sub-class of BaseCmsControl.</para>
        /// <para>If the control is a file, it must be located in the "~/controls" directory.</para>
        /// <para>Note that the control path never has a filename extension (ie "_system/login" is a valid control path, while "_system/login.ascx" is invalid).</para>
        /// </summary>
        public string ControlNameOrPath;

        public string RawTemplateParameters;

        public CmsControlDefinition(string controlNameOrPath, string rawTemplateParameters)
        {
            ControlNameOrPath = controlNameOrPath.ToLower();
            RawTemplateParameters = rawTemplateParameters;
        }
                

        public static CmsControlDefinition[] GetByControlNameOrPath(CmsControlDefinition[] haystack, string ControlNameOrPathToFind)
        {
            List<CmsControlDefinition> ret = new List<CmsControlDefinition>();
            foreach (CmsControlDefinition controlDef in haystack)
            {
                if (controlDef.ControlNameOrPath.EndsWith(ControlNameOrPathToFind, StringComparison.CurrentCultureIgnoreCase))                
                    ret.Add(controlDef);
            } // foreach
            return ret.ToArray();
        }
    }
}

