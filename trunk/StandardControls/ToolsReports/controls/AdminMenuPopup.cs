using System;
using System.Collections.Generic;
using System.Text;
using HatCMS.Admin;

namespace HatCMS.Controls
{
    public class AdminMenuPopup: BaseCmsControl
    {
        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            return ret.ToArray();
        }

        public override string RenderToString(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
        {
            if (!CmsContext.currentPage.currentUserCanRead)
            {
                return ("Access Denied");                
            }


            StringBuilder html = new StringBuilder();
            // -- always render the admin menu
            AdminMenu adminMenu = new AdminMenu();
            html.Append(adminMenu.Render());

            BaseCmsAdminTool toolToRun = AdminMenu.getToolToRun();
            if (toolToRun.getToolInfo().Category != BaseCmsAdminTool.CmsAdminToolCategory._AdminMenu)
            {
                string toolHtml = toolToRun.Render();
                html.Append(toolHtml);
            }

            return (html.ToString());
        }
    }
}
