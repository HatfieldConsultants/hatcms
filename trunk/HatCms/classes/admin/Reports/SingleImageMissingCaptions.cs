using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HatCMS.Placeholders;
using System.Text;
using System.Collections.Generic;

namespace HatCMS.Controls.Admin
{
    public class SingleImageMissingCaptions : CmsBaseAdminTool
    {        

        public override string Render()
        {
            StringBuilder html = new StringBuilder();

            html.Append("<p>The following images are missing captions:");

            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            List<CmsPage> pagesToGetImagesFrom = new List<CmsPage>();
            foreach (int pageId in allPages.Keys)
            {
                pagesToGetImagesFrom.Add(allPages[pageId]);
            }
            SingleImageDb singleImageDb = new SingleImageDb();
            List<SingleImageData> imageDatas = new List<SingleImageData>();
            foreach (CmsLanguage lang in CmsConfig.Languages)
            {
                imageDatas.AddRange(singleImageDb.getSingleImages(pagesToGetImagesFrom.ToArray(), lang));
            }

            List<SingleImageData> imageDatasWithoutCaptions = new List<SingleImageData>();
            foreach (SingleImageData img in imageDatas)
            {
                if (img.ImagePath.Trim() != "" && img.Caption.Trim() == "")
                    imageDatasWithoutCaptions.Add(img);
            }

            if (imageDatasWithoutCaptions.Count < 1)
                html.Append("<br><strong>No images are missing captions! (" + imageDatas.Count.ToString() + " images audited)</strong>");
            else
            {
                html.Append("<br>" + imageDatasWithoutCaptions.Count.ToString() + " images are missing captions (" + imageDatas.Count.ToString() + " images audited)");
                html.Append(TABLE_START_HTML + Environment.NewLine);
                html.Append("<tr><th>Image</th><th>On Page</th></tr>");
                foreach (SingleImageData img in imageDatasWithoutCaptions)
                {
                    html.Append("<tr>");
                    html.Append("<td>" + SingleImageHtmlDisplay(img) + "</td>");
                    html.Append("<td>");
                    CmsPage containingPage = img.getPageContainingImage(pagesToGetImagesFrom.ToArray());
                    if (containingPage != null)
                        html.Append("<a target=\"_blank\" href=\"" + containingPage.Url + "\">" + containingPage.Path + "</a>");
                    else
                        html.Append("Invalid page found!!");

                    html.Append("</td>");
                    html.Append("</tr>" + Environment.NewLine);
                }
                html.Append("</table>" + Environment.NewLine);
            }


            html.Append("</p>");

            return html.ToString();

        }


    }
}
