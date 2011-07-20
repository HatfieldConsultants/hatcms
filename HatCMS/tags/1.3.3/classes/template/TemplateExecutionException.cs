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
    /// The Exception thrown during template rendering
    /// </summary>
    public class TemplateExecutionException : Exception
    {
        public TemplateExecutionException(Exception innerException, string templateFile, string message)
            : base("Error in Template \"" + templateFile + "\": " + message, innerException)
        { }

        public TemplateExecutionException(string templateFile, string message)
            : base("Error in Template \"" + templateFile + "\": " + message)
        { }
    }
}
