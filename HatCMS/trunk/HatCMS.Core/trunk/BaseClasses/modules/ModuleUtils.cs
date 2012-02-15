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
using Hatfield.Web.Portal;

namespace HatCMS
{
    public class CmsModuleUtils
    {
        /// <summary>
        /// External modules (found in external DLL assembies) can provide a class that inherits from CmsModuleInfo to provide information on
        /// the module. 
        /// </summary>
        /// <returns></returns>
        public static CmsModuleInfo[] getAllModuleInfos()
        {
            string cacheKey = "CmsModuleUtils.getAllModuleInfos()";
            if (PerRequestCache.CacheContains(cacheKey))
            {
                return (CmsModuleInfo[])PerRequestCache.GetFromCache(cacheKey, new CmsModuleInfo[0]);
            }
            List<CmsModuleInfo> ret = new List<CmsModuleInfo>();
            Type[] moduleInfoTypes = AssemblyHelpers.LoadAllAssembliesAndGetAllSubclassesOf(typeof(CmsModuleInfo));
            foreach (Type modInfoType in moduleInfoTypes)
            {
                try
                {
                    CmsModuleInfo info = (CmsModuleInfo)modInfoType.Assembly.CreateInstance(modInfoType.FullName);
                    ret.Add(info);
                }
                catch
                {
                    Console.Write("Error: could not load module info " + modInfoType.FullName);
                }
            } // foreach

            CmsModuleInfo[] arr = ret.ToArray();
            PerRequestCache.AddToCache(cacheKey, arr);
            return arr;

        }

        /// <summary>
        /// Calls .getModuleDependencies for each module that is loaded in the system.
        /// </summary>
        /// <returns></returns>
        public static CmsDependency[] getAllModuleLevelDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            foreach (CmsModuleInfo module in getAllModuleInfos())
            {
                ret.AddRange(module.getModuleDependencies());
            } // foreach
            return ret.ToArray();
        }
    }
}
