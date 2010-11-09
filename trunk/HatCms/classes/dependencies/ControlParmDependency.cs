using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Text;

namespace HatCMS.classes.dependencies
{
    /// <summary>
    /// Class to valid the parameters for a control.
    /// </summary>
    public class CmsControlParmDependency : CmsDependency
    {
        private string controlId;
        private string[] keys;

        /// <summary>
        /// Constructor, which accepts the Control.ID and required parameter keys.
        /// </summary>
        /// <param name="newControlId"></param>
        /// <param name="newKeys"></param>
        public CmsControlParmDependency(string newControlId, string[] newKeys)
        {
            controlId = newControlId;
            keys = newKeys;
        }

        /// <summary>
        /// Checks if the control has required parameters.
        /// Invalid:
        ///   - ##RenderControl(_system/[controlName])##
        /// Valid:
        ///   - ##RenderControl(_system/[controlName] key1="value1" key2="value2")##
        /// </summary>
        /// <returns></returns>
        public override CmsDependencyMessage[] ValidateDependency()
        {
            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
            for (int x = 0; x < keys.Length; x++)
            {
                string testString = " " + keys[x] + "=\"";
                if (controlId.Contains(testString) == false)
                {
                    ret.Add(CmsDependencyMessage.Error("CMS Control parameter for '" + controlId + "' not found: '" + keys[x] + "'"));
                }
            }
            return ret.ToArray();
        }

        /// <summary>
        /// Content in string.
        /// </summary>
        /// <returns></returns>
        public override string GetContentHash()
        {
            StringBuilder sb = new StringBuilder(controlId.GetType().ToString());
            foreach (string k in keys)
                sb.Append(k);
            return sb.ToString();
        }
    }
}
