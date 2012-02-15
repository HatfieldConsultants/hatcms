using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Collections.Generic;
using Hatfield.Web.Portal;
using SharpArch.Core.DomainModel;
using NHibernate.Validator.Constraints;

namespace HatCMS
{
    /// <summary>
    /// CmsPageLockData stores information about a CmsPage's locks.
    /// </summary>
    public class CmsPageLockData : Entity
    {
        private int pageid = -1;

        public virtual int PageId
        {
            get { return pageid; }
            set { pageid = value; }
        }
        private string lockedbyusername = "";

        public virtual string LockedByUsername
        {
            get { return lockedbyusername; }
            set { lockedbyusername = value; }
        }
        private DateTime lockexpiresat = DateTime.MinValue;

        public virtual DateTime LockExpiresAt
        {
            get { return lockexpiresat; }
            set { lockexpiresat = value; }
        }
        /*
CREATE TABLE `PageLocks` (
  `pageid` INTEGER NOT NULL,
  `LockedByUsername` VARCHAR(255) NOT NULL,
  `LockExpiresAt` DATETIME NOT NULL,
  PRIMARY KEY (`pageid`)
)
ENGINE = InnoDB;
         */
    }
}
