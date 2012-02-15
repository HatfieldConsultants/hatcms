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
using SharpArch.Core.DomainModel;
using NHibernate.Validator.Constraints;
using SharpArch.Core.PersistenceSupport;
using SharpArch.Data.NHibernate;

namespace HatCMS
{
    /// <summary>
    /// Entity object for `ZoneUserRole`
    /// </summary>
    public class CmsPageSecurityZoneUserRole : Entity
    {
        private CmsPageSecurityZone zone;
        [DomainSignature]
        public virtual CmsPageSecurityZone Zone
        {
            get { return zone; }
            set { zone = value; }
        }

        private int userRoleId = -1;
        [DomainSignature]
        public virtual int UserRoleId
        {
            get { return userRoleId; }
            set { userRoleId = value; }
        }

        private bool readAccess = false;
        public virtual bool ReadAccess
        {
            get { return readAccess; }
            set { readAccess = value; }
        }

        private bool writeAccess = false;
        public virtual bool WriteAccess
        {
            get { return writeAccess; }
            set { writeAccess = value; }
        }

        public CmsPageSecurityZoneUserRole()
        {
        }//constructor

        public CmsPageSecurityZoneUserRole(int newZoneId, int newUserRoleId)
        {
            IRepository<CmsPageSecurityZone> repository = new Repository<CmsPageSecurityZone>();

            this.zone = repository.Get(newZoneId);
            UserRoleId = newUserRoleId;

        }//constructor

        public CmsPageSecurityZoneUserRole(int newZoneId, int newUserRoleId, bool newReadAccess, bool newWriteAccess)
        {
            IRepository<CmsPageSecurityZone> repository = new Repository<CmsPageSecurityZone>();

            this.zone = repository.Get(newZoneId); 
            UserRoleId = newUserRoleId;
            ReadAccess = newReadAccess;
            WriteAccess = newWriteAccess;
        }//constructor

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{");
            sb.Append(Zone.Id.ToString() + ",");
            sb.Append(UserRoleId.ToString() + ",");
            sb.Append(ReadAccess.ToString() + ",");
            sb.Append(WriteAccess.ToString() + "}");
            return sb.ToString();
        }
    }
}
