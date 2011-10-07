using System;
using System.Data;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS
{
    /// <summary>
    /// A Zone defines a security region in the web site. A page can only be in one zone - there is no zone inheritance. 
    /// If a page is not a zone-boundary, it inherits the zone of its parent page.
    /// </summary>
    public class CmsPageSecurityZone
    {
        public static int DEFAULT_ID = 1;

        private int zoneId = -1;
        public int ZoneId
        {
            get { return zoneId; }
            set { zoneId = value; }
        }

        private int startingPageId = 0;
        public int StartingPageId
        {
            get { return startingPageId; }
            set { startingPageId = value; }
        }

        private string zoneName = "";
        public string ZoneName
        {
            get { return zoneName; }
            set { zoneName = value; }
        }

        /// <summary>
        /// Checks whether a user has read access in this zone.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public bool canRead(WebPortalUser u)
        {
            if (u != null && u.inRole(CmsConfig.getConfigValue("AdminUserRole", "Administrator")))
                return true;

            WebPortalUserRole[] roleArray = new WebPortalUserRole[] { WebPortalUserRole.dummyPublicUserRole };
            if (u != null)
            {
                u.AddUserRole(WebPortalUserRole.dummyPublicUserRole); // users are always part of the "public" user role.
                roleArray = u.userRoles;
            }

            CmsZoneUserRoleDb db = new CmsZoneUserRoleDb();
            return (db.fetchRoleMatchingCountForRead(this, roleArray) > 0);
        }

        /// <summary>
        /// Checks whether a user has write access in this zone.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public bool canWrite(WebPortalUser u)
        {
            if (u != null && u.inRole(CmsConfig.getConfigValue("AdminUserRole", "Administrator")))
                return true;

            WebPortalUserRole[] roleArray = new WebPortalUserRole[] { WebPortalUserRole.dummyPublicUserRole };
            if (u != null)
            {
                u.AddUserRole(WebPortalUserRole.dummyPublicUserRole); // users are always part of the "public" user role.
                roleArray = u.userRoles;
            }

            CmsZoneUserRoleDb db = new CmsZoneUserRoleDb();
            return (db.fetchRoleMatchingCountForWrite(this, roleArray) > 0);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{");
            sb.Append(ZoneId.ToString() + ",");
            sb.Append(StartingPageId.ToString() + ",");
            sb.Append(ZoneName + "}");
            return sb.ToString();
        }
    }
}
