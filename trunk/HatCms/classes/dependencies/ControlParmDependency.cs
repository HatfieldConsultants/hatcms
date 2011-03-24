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

namespace HatCMS
{
    /// <summary>
    /// Class to valid the parameters for a control.
    /// </summary>
    public class CmsControlParameterDependency : CmsDependency
    {
        private string ControlName;
        private string[] keys;
        private ExistsMode existsMode;

        public CmsControlParameterDependency(System.Web.UI.UserControl control, string[] requiredParameterKeys)
        {
            ControlName = control.GetType().Name;
            keys = requiredParameterKeys;
            existsMode = ExistsMode.MustExist;
        }
        
        /// <summary>
        /// Constructor, which accepts the ControlName and required parameter keys.
        /// </summary>
        /// <param name="newControlId"></param>
        /// <param name="newKeys"></param>
        public CmsControlParameterDependency(string controlName, string[] requiredParameterKeys)
        {
            ControlName = controlName;
            keys = requiredParameterKeys;
            existsMode = ExistsMode.MustExist;
        }

        public CmsControlParameterDependency(System.Web.UI.UserControl control, string[] parameterKeys, ExistsMode parameterKeysExistsMode)
        {
            ControlName = control.GetType().Name;
            keys = parameterKeys;
            existsMode = parameterKeysExistsMode;
        }

        public CmsControlParameterDependency(string controlName, string[] parameterKeys, ExistsMode parameterKeysExistsMode)
        {
            ControlName = controlName;
            keys = parameterKeys;
            existsMode = parameterKeysExistsMode;
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
            string[] templates = CmsContext.getTemplateNamesForCurrentUser();
            CmsPage dummyPage = new CmsPage();
            foreach (string template in templates)
            {
                dummyPage.TemplateName = template;

                CmsControlDefinition[] controlDefs = dummyPage.getAllControlDefinitions();
                controlDefs = CmsControlDefinition.GetByControlName(controlDefs, ControlName);
                foreach (CmsControlDefinition controlDef in controlDefs)
                {
                    foreach (string keyToTest in keys)
                    {
                        bool keyExists = CmsControlUtils.hasControlParameterKey(controlDef, keyToTest);
                        if (!keyExists && existsMode == ExistsMode.MustExist)
                        {
                            ret.Add(CmsDependencyMessage.Error("CMS Control parameter '" + keyToTest + "' for control '" + controlDef.ControlPath + "' in template '" + dummyPage.TemplateName + "' is required, but was not found."));
                        }
                        else if (keyExists && existsMode == ExistsMode.MustNotExist)
                        {
                            ret.Add(CmsDependencyMessage.Error("CMS Control parameter '" + keyToTest + "' for control '" + controlDef.ControlPath + "' in template '" + dummyPage.TemplateName + "' was found, and must be removed."));
                        }
                    }
                } // foreach controlDef
            } // foreach template

            return ret.ToArray();
        }

        /// <summary>
        /// Content in string.
        /// </summary>
        /// <returns></returns>
        public override string GetContentHash()
        {
            StringBuilder sb = new StringBuilder(ControlName);
            foreach (string k in keys)
                sb.Append(k);
            return sb.ToString();
        }
    }
}
