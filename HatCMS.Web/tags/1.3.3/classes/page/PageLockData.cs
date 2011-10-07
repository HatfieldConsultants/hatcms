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
    /// CmsPageLockData stores information about a CmsPage's locks.
    /// </summary>
    public class CmsPageLockData
    {
        public int PageId = -1;
        public string LockedByUsername = "";
        public DateTime LockExpiresAt = DateTime.MinValue;
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
