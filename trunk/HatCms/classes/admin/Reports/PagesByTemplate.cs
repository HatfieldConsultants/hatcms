using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Collections.Generic;

namespace HatCMS.Controls.Admin
{
    public class PagesByTemplate : CmsBaseAdminTool
    {        
        public override string Render()
        {
            Dictionary<string, List<CmsPage>> reportStorage = new Dictionary<string, List<CmsPage>>();

            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            foreach (int pageId in allPages.Keys)
            {
                CmsPage targetPage = allPages[pageId];
                if (!reportStorage.ContainsKey(targetPage.TemplateName.ToLower()))
                    reportStorage[targetPage.TemplateName.ToLower()] = new List<CmsPage>();

                reportStorage[targetPage.TemplateName.ToLower()].Add(targetPage);
            } // foreach

            StringBuilder html = new StringBuilder();
            html.Append("<table border=\"0\">");
            foreach (string templateName in reportStorage.Keys)
            {
                html.Append("<tr><td style=\"background-color: #CCC;\"><strong>" + templateName + "</strong><td></tr>");
                foreach (CmsPage targetPage in reportStorage[templateName])
                {
                    html.Append("<tr><td><a href=\"" + targetPage.getUrl(CmsUrlFormat.FullIncludingProtocolAndDomainName) + "\" target=\"_blank\">" + targetPage.Title + "</td></tr>");
                } // foreach
            } // foreach
            html.Append("</table>");
            return html.ToString();

        }
        

    }
}
