using System;
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
    public class CmsPortalApplication : PortalApplication
    {
        public override string GetApplicationName()
        {
            string siteName = CmsConfig.getConfigValue("SiteName", "Unknown hatCms installation");
                        
            return siteName+" (hatCms)";
        }

        public override PortalApplicationPermission[] GetAllPermissionsForApplication()
        {
            string appName = GetApplicationName();

            return new PortalApplicationPermission[] {
                // new PortalApplicationPermission(appName, "Login to Portal Admin", "Allow the user to log into the Portal Administration system")
            };
        }

        public static CmsPortalApplication GetInstance()
        {
            string key = (new CmsPortalApplication()).GetApplicationName();
            if (PerRequestCache.CacheContains(key))
                return (CmsPortalApplication)PerRequestCache.GetFromCache(key, null);
            else
            {
                CmsPortalApplication p = new CmsPortalApplication();
                PerRequestCache.AddToCache(key, p);
                p.EnsurePermissionsInDatabase();
                return p;
            }
        }

    } // PortalAdminApplication
}
