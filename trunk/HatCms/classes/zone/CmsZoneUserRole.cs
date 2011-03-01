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
    /// Entity object for `ZoneUserRole`
    /// </summary>
    public class CmsZoneUserRole
    {
        private int zoneId = CmsPageSecurityZone.DEFAULT_ID;
        public int ZoneId
        {
            get { return zoneId; }
            set { zoneId = value; }
        }

        private int userRoleId = -1;
        public int UserRoleId
        {
            get { return userRoleId; }
            set { userRoleId = value; }
        }

        private bool readAccess = false;
        public bool ReadAccess
        {
            get { return readAccess; }
            set { readAccess = value; }
        }

        private bool writeAccess = false;
        public bool WriteAccess
        {
            get { return writeAccess; }
            set { writeAccess = value; }
        }

        public int ReadAccessAsInt
        {
            get { return Convert.ToInt32(ReadAccess); }
        }

        public int WriteAccessAsInt
        {
            get { return Convert.ToInt32(WriteAccess); }
        }

        public CmsZoneUserRole()
        {
        }

        public CmsZoneUserRole(int newZoneId, int newUserRoleId, bool newReadAccess, bool newWriteAccess)
        {
            ZoneId = newZoneId;
            UserRoleId = newUserRoleId;
            ReadAccess = newReadAccess;
            WriteAccess = newWriteAccess;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{");
            sb.Append(ZoneId.ToString() + ",");
            sb.Append(UserRoleId.ToString() + ",");
            sb.Append(ReadAccess.ToString() + ",");
            sb.Append(WriteAccess.ToString() + "}");
            return sb.ToString();
        }
    }
}
