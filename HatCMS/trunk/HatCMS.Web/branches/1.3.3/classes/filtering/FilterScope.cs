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
    /// An enum that determines the scope of a <see cref="CmsOutputFilter"/> instance.
    /// </summary>
    public enum CmsOutputFilterScope
    {
        /// <summary>
        /// filter the output of the entire page after all placeholders and controls have been rendered into the template file
        /// </summary>
        PageHtmlOutput,
        
        /// <summary>
        /// Filter the output from all placeholders. Controls and template content will not be filtered.
        /// </summary>
        AllPlaceholders,
        
        /// <summary>
        /// Filter the output from only the specified placeholder types. Controls and template content will not be filtered
        /// </summary>
        SpecifiedPlaceholderTypes
        
    }

    // note: CmsControl output filtering is not so easy - perhaps in the future (if needed?)
}
