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
    /// CmsTemplateEngine is an abstract class that defines the methods that a template engine must implement.
    /// </summary>
    public abstract class CmsTemplateEngine
    {
        
        public abstract void CreateChildControls(System.Web.UI.UserControl parentToAddControlsTo);
        public abstract string renderControlToString(string controlPathOrName);
        public abstract CmsControlDefinition[] getAllControlDefinitions();
        public abstract CmsPlaceholderDefinition[] getAllPlaceholderDefinitions();

        /// <summary>
        /// the templatePath does NOT have the template sub-directory, nor the .htm/.template extension
        /// </summary>
        /// <param name="templatePath"></param>
        /// <returns></returns>
        public abstract bool templateExists(string templatePath);
        

        /// <summary>
        /// returns true if the control exists.
        /// The control path does NOT have the control sub-directory, nor the .ASCX extension.
        /// Example: CmsTemplateEngine.controlExists("/_system/Breadcrumb");
        /// </summary>
        /// <param name="controlPath"></param>
        /// <returns></returns>
        public abstract bool controlExists(string controlPath);



        /// <summary>
        /// The control path does NOT have the control sub-directory, nor the .ASCX extension.
        /// Example: CmsTemplateEngine.controlLastModifiedDate("/_system/Breadcrumb");
        /// </summary>
        /// <param name="controlPath"></param>
        /// <returns></returns>
        public abstract DateTime getControlLastModifiedDate(string controlPath);

        public abstract CmsDependency[] getControlDependencies(string controlPath);


        public abstract string[] getControlParameterKeys(System.Web.UI.UserControl control);        
        public abstract int  getControlParameterKeyValue(System.Web.UI.UserControl control, string key, int defaultValue);
        public abstract bool getControlParameterKeyValue(System.Web.UI.UserControl control, string key, bool defaultValue);
        public abstract string getControlParameterKeyValue(System.Web.UI.UserControl control, string key, string defaultValue);

        public abstract string[] getControlParameterKeys(CmsControlDefinition controlDefinition);
        public abstract int getControlParameterKeyValue(CmsControlDefinition controlDefinition, string key, int defaultValue);
        public abstract bool getControlParameterKeyValue(CmsControlDefinition controlDefinition, string key, bool defaultValue);
        public abstract string getControlParameterKeyValue(CmsControlDefinition controlDefinition, string key, string defaultValue);
        /// <summary>
		/// gets the names of the currently available templates for the current user.
		/// </summary>
        public abstract string[] getTemplateNamesForCurrentUser();


    }
}
