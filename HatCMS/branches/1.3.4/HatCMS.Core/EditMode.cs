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
    /// the edit mode enumeration.
    /// </summary>
    public enum CmsEditMode
    {
        /// <summary>
        /// the user is currently viewing the page
        /// </summary>
        View,
        /// <summary>
        /// the user is currently editing the page.
        /// </summary>
        Edit
    }
}
