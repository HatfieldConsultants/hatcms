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
    /// When a URL is constructed, it can either be a Full URL, or a relative one.
    /// </summary>
    public enum CmsUrlFormat
    {
        /// <remarks>
        /// Relative URL format - relative to the root of the domain name (default)
        /// </remarks>
        RelativeToRoot,
        ///<remarks>        
        /// Full URL, including http:// and domain name
        /// </remarks>
        FullIncludingProtocolAndDomainName
    }
}
