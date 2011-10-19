using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Admin
{
    public class VersionReport : BaseCmsAdminTool
    {        

        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Report_Other, AdminMenuTab.Reports, "HatCMS Version Number");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            return ret.ToArray();
        }

        public override GridView RenderToGridViewForOutputToExcelFile()
        {
            return null;
        }

        public override string Render()
        {
            StringBuilder html = new StringBuilder();
            html.Append(base.formatNormalMsg("You are running HatCMS.Core version " + CmsContext.currentHatCMSCoreVersion.ToString()));
            
            CmsModuleInfo[] moduleInfos = CmsModuleUtils.getAllModuleInfos();
            html.Append("<p>" + moduleInfos.Length + " modules are currently active: ");
            if (moduleInfos.Length > 0)
            {
                html.Append("<ul>");
                foreach (CmsModuleInfo mod in moduleInfos)
                {
                    Assembly asm = mod.GetType().Assembly;
                    html.Append("<li>");
                    html.Append(asm.GetName().Name + ": " + asm.GetName().Version.ToString());
                    html.Append("</li>");
                }
                html.Append("</ul>");
            }
            html.Append("</p>");

            return html.ToString();
        }
    }
}
