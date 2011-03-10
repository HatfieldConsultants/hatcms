using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HatCMS.Placeholders;

namespace HatCMS.Controls.Admin
{
    public class PageImageSummary : CmsBaseAdminTool
    {
        public override string Render()       
        {
            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            StringBuilder html = new StringBuilder();
            html.Append("<p><strong>Page - Image Summary</strong></p>");
            html.Append(TABLE_START_HTML);
            html.Append("<tr><th>Page</th><th>Images</th></tr>");
            SingleImageDb db = new SingleImageDb();
            foreach (int pageId in allPages.Keys)
            {
                CmsPage targetPage = allPages[pageId];
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    html.Append("<tr>");
                    html.Append("<td><a href=\"" + targetPage.getUrl(CmsUrlFormat.FullIncludingProtocolAndDomainName) + "\" target=\"_blank\">" + targetPage.Path + "</a> (\"" + targetPage.Title + "\")</td>");
                    html.Append("<td><table border=\"0\">");
                    SingleImageData[] pageImages = db.getSingleImages(new CmsPage[] { targetPage }, lang);
                    if (pageImages.Length > 0)
                    {
                        foreach (SingleImageData imgData in pageImages)
                        {
                            string thumbUrl = showThumbPage.getThumbDisplayUrl(imgData.ImagePath, 150, 150);
                            html.Append("<tr><td><img src=\"" + thumbUrl + "\"><br />Caption: " + imgData.Caption + "<br />Source:" + imgData.Credits + "<br />Path: " + imgData.ImagePath + "</td></tr>");
                        } // foreach
                    }
                    else
                    {
                        html.Append("<tr><td><em>Page doesn't have any images...</em></td></tr>");
                    }


                    html.Append("</table></td>");
                    html.Append("<tr>");
                } // foreach language
            } // foreach page
            html.Append("</table>");
            return html.ToString();
        } // Render


    }
}
