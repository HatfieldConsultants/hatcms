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
    public class CmsMessageDependency: CmsDependency
    {
        string msg;
        public CmsMessageDependency(string Message)
        {
            msg = Message;
        }
        public override CmsDependencyMessage[] ValidateDependency()
        {
            return new CmsDependencyMessage[] { CmsDependencyMessage.Error(msg) };
        }

        public override string GetContentHash()
        {
            return msg;
        }
    }
}
