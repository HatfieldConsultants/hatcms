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
using Hatfield.Web.Portal;
using System.Collections.Generic;
using HatCMS.Placeholders;

namespace HatCMS.Admin
{
    public class SearchSingleImagesByCaption : BaseCmsAdminTool
    {
        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Tool_Search, AdminMenuTab.Tools, "Search Images by caption");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.AddRange(PlaceholderUtils.getDependencies("SingleImage"));
            return ret.ToArray();
        }

        public override string Render()
        {
            CmsPage currentPage = CmsContext.currentPage;
            string searchText = PageUtils.getFromForm("AuditSearch", "");
            searchText = searchText.Trim();

            StringBuilder html = new StringBuilder();

            // -- start query form
            string formId = "searchImageCaptions";
            html.Append(currentPage.getFormStartHtml(formId));
            html.Append("<strong>Search Image Captions: </strong> ");
            html.Append(PageUtils.getInputTextHtml("AuditSearch", "AuditSearch", searchText, 40, 1024));            
            html.Append("<input type=\"submit\" value=\"search\">");
            html.Append(PageUtils.getHiddenInputHtml("AdminTool", GetType().Name ));
            html.Append(currentPage.getFormCloseHtml(formId));

            if (searchText != "")
            {
                Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
                List<CmsPage> pages = new List<CmsPage>(allPages.Values);

                SingleImageDb db = new SingleImageDb();
                List<SingleImageData> imgDatas = new List<SingleImageData>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    imgDatas.AddRange(db.getSingleImages(pages.ToArray(), lang));
                }

                html.Append("<p><strong>Images containing \"" + searchText + "\":</strong></p>");
                html.Append("<table border=\"1\">");
                html.Append("<tr><th>Image</th><th>Page Link</th></tr>");
                foreach (SingleImageData img in imgDatas)
                {


                    if (img.Caption.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) > -1 ||
                        img.Credits.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) > -1)
                    {
                        html.Append("<tr>");
                        html.Append("<td>");
                        html.Append(SingleImageHtmlDisplay(img));
                        html.Append("</td>");
                        CmsPage targetPage = CmsContext.getPageById(img.PageId);
                        html.Append("<td><a target=\"_blank\" href=\"" + targetPage.getUrl(CmsUrlFormat.FullIncludingProtocolAndDomainName) + "\">" + targetPage.Title + "</a></td>");
                        html.Append("</tr>");
                    } // if

                } // foreach
                html.Append("</table>");
            }

            return html.ToString();
        }

        public override System.Web.UI.WebControls.GridView RenderToGridViewForOutputToExcelFile()
        {
            return null; // not implemented.
        }


    }
}
