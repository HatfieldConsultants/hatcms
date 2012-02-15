using System;
using System.Collections.Generic;
using System.Text;

namespace HatCMS.Controls
{
    public abstract class BaseCmsControl 
    {
        /// <summary>
        /// the End-of-line character
        /// </summary>
        protected string EOL = Environment.NewLine;
        
        public System.Web.UI.LiteralControl ToLiteralControl(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
        {            
            return new RenderingControl(this, controlDefnToRender, langToRenderFor);
        }

        // -- use a private class so that all the functions in System.Web.UI are hidden from the user.
        private class RenderingControl : System.Web.UI.LiteralControl
        {
            BaseCmsControl ParentControl;
            CmsControlDefinition ControlDefnToRender;
            CmsLanguage LangToRenderFor;
            public RenderingControl(BaseCmsControl parentControl, CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
            {
                ParentControl = parentControl;
                ControlDefnToRender = controlDefnToRender;
                LangToRenderFor = langToRenderFor;
            }
            /// <summary>
            /// Use/Override the render function so that rendering takes place at the right time 
            /// </summary>
            /// <param name="output"></param>
            protected override void Render(System.Web.UI.HtmlTextWriter output)
            {
                output.Write(ParentControl.RenderToString(ControlDefnToRender, LangToRenderFor));
            }
        }

        public static int getParameterValue(CmsPage page, CmsControlDefinition controlDefinition, string key, int defaultValue)
        {
            return CmsControlUtils.getControlParameterKeyValue(page, controlDefinition, key, defaultValue);
        } // getParameterValue

        public static bool getParameterValue(CmsPage page, CmsControlDefinition controlDefinition, string key, bool defaultValue)
        {
            return CmsControlUtils.getControlParameterKeyValue(page, controlDefinition, key, defaultValue);
        } // getParameterValue


        public static string getParameterValue(CmsPage page, CmsControlDefinition controlDefinition, string key, string defaultValue)
        {
            return CmsControlUtils.getControlParameterKeyValue(page, controlDefinition, key, defaultValue);
        }


        public static string[] getParameterKeys(CmsPage page, CmsControlDefinition controlDefinition)
        {
            return CmsControlUtils.getControlParameterKeys(page, controlDefinition);            
        } // getControlParameterKeys



        public bool parameterKeyExists(CmsPage page, CmsControlDefinition controlDefinition, string keyNameToFind)
        {
            return CmsControlUtils.hasControlParameterKey(page, controlDefinition, keyNameToFind);
        }

        public abstract CmsDependency[] getDependencies();
        public abstract string RenderToString(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor);

        
    }
}
