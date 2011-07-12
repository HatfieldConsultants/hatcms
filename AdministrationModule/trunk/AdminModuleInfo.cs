using System;
using System.Collections.Generic;
using System.Text;
using HatCMS;

namespace HatCMS.Admin
{
    public class AdminModuleInfo : CmsModuleInfo
    {

        /// <summary>
        /// get the dependencies for this module. Placeholder and control dependencies are called seperately.
        /// </summary>
        /// <returns></returns>
        public override CmsDependency[] getModuleDependencies()
        {            

            // -- add the library dependecies
            List<CmsDependency> ret = new List<CmsDependency>();

            // -- the internal templates directory no longer exists (they are served from this project's Assembly).
            ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("templates/internal"));

            // -- several control directories no longer exist (they are served from this project's Assembly).
            ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("controls/_system/Admin"));
            ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("controls/_system/Internal"));
            

            return ret.ToArray();
        }


    }
}
