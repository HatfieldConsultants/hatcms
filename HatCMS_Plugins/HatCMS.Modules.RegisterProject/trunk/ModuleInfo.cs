using System;
using System.Collections.Generic;
using System.Text;
using HatCMS;

namespace HatCMS.RegisterProjectModule
{
    public class RegisterProjectModuleInfo : CmsModuleInfo
    {
        /// <summary>
        /// get the dependencies for this module. Placeholder and control dependencies are called seperately.
        /// </summary>
        /// <returns></returns>
        public override CmsDependency[] getModuleDependencies()
        {
            // -- add the library dependecies
            List<CmsDependency> ret = new List<CmsDependency>();

            ret.Add(new CmsVersionDependency("RegisterProject Module", new System.Version("1.3")));

            return ret.ToArray();
        }
    }
}
