using System;
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
    /// Any seperate project that compiles to a seperate Assembly should provide information about itself
    /// by creating a class that inherits from CmsModuleInfo.
    /// </summary>
    public abstract class CmsModuleInfo
    {
        /// <summary>
        /// get the module-level dependencies. This should only return dependencies that are NOT in placeholders or controls, as
        /// controls and placeholders in the module will have their getDependencies() method called seperately.
        /// </summary>
        /// <returns></returns>
        public abstract CmsDependency[] getModuleDependencies();
    }
}
