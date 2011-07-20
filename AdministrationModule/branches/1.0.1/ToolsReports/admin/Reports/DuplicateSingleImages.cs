using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Text;
using HatCMS.Placeholders;

namespace HatCMS.Admin
{
    public class DuplicateSingleImages : BaseCmsAdminTool
    {
        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Report_Image, AdminMenuTab.Reports, "Duplicate Images");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.AddRange(PlaceholderUtils.getDependencies("SingleImage"));
            return ret.ToArray();
        }

        public override string Render()
        {

            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();

            SingleImageDb db = new SingleImageDb();

            List<PageImages> pageImages = new List<PageImages>();

            foreach (int pageId in allPages.Keys)
            {
                CmsPage pageToTest = allPages[pageId];
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    SingleImageData[] imgDataArr = db.getSingleImages(new CmsPage[] { allPages[pageId] }, lang);
                    PageImages pi = new PageImages(pageToTest, lang, imgDataArr);
                    pageImages.Add(pi);
                }
            }

            Dictionary<string, List<PageLanguage>> duplicates = new Dictionary<string, List<PageLanguage>>();
            foreach (PageImages pi in pageImages)
            {
                foreach (SingleImageData img in pi.Images)
                {
                    PageLanguage[] matchingPages = PageImages.GetMatchingPagesForImage(img, pageImages.ToArray());
                    if (matchingPages.Length > 1)
                    {
                        if (!duplicates.ContainsKey(img.ImagePath))
                            duplicates[img.ImagePath] = new List<PageLanguage>();

                        duplicates[img.ImagePath].AddRange(matchingPages);
                    }
                }
            }

            // -- remove duplicate PageLanguage items
            Dictionary<string, List<PageLanguage>> toReport = new Dictionary<string, List<PageLanguage>>();
            foreach (string imgPath in duplicates.Keys)
            {
                toReport[imgPath] = PageLanguage.RemoveDuplicates(duplicates[imgPath]);
            } // foreach


            // -- display results

            StringBuilder html = new StringBuilder();
            html.Append("<p><strong>Duplicate images used on this site:</strong></p>");
            html.Append(TABLE_START_HTML);
            html.Append("<tr><th>Image</th><th>Found on pages</th></tr>");
            if (toReport.Keys.Count > 0)
            {
                foreach (string imgPath in toReport.Keys)
                {
                    html.Append("<tr>");
                    string thumbUrl = showThumbPage.getThumbDisplayUrl(imgPath, 150, 150);
                    html.Append("<td><img src=\"" + thumbUrl + "\"><br />" + imgPath + "</td>");
                    html.Append("<td>");
                    html.Append("<ul>");
                    foreach (PageLanguage targetPage in toReport[imgPath])
                    {
                        html.Append("<li><a href=\"" + targetPage.Page.getUrl(CmsUrlFormat.FullIncludingProtocolAndDomainName, targetPage.Language) + "\" target=\"_blank\">" + targetPage.Page.getTitle(targetPage.Language) + " (" + targetPage.Language.shortCode + ") </li>");
                    }
                    html.Append("</ul>");
                    html.Append("</td>");
                    html.Append("</tr>");
                } // foreach
            }
            else
            {
                html.Append("<tr><td><em>No duplicate images found</em></td></tr>");
            }
            html.Append("</table>");

            return html.ToString();
        }

        public override System.Web.UI.WebControls.GridView RenderToGridViewForOutputToExcelFile()
        {
            return null; // not implemented.
        }

        private SingleImageData[] ImgPathAlreadyExists(SingleImageData[] haystack, SingleImageData needle)
        {
            List<SingleImageData> ret = new List<SingleImageData>();
            foreach (SingleImageData img in haystack)
            {
                if (String.Compare(img.ImagePath, needle.ImagePath, true) == 0)
                    ret.Add(img);
            }
            return ret.ToArray();
        }

        private class PageLanguage
        {
            public CmsPage Page;
            public CmsLanguage Language;

            public PageLanguage(CmsPage p, CmsLanguage l)
            {
                Page = p;
                Language = l;
            }

            public static List<PageLanguage> RemoveDuplicates(List<PageLanguage> arr)
            {
                List<string> existingPageIdLangCodes = new List<string>();
                List<PageLanguage> ret = new List<PageLanguage>();
                foreach (PageLanguage pl in arr)
                {
                    // string key = pl.Page.ID.ToString() + pl.Language.shortCode;
                    // note: to show duplicates between language versions of the same page, use the language code here!
                    string key = pl.Page.ID.ToString();
                    if (existingPageIdLangCodes.IndexOf(key) < 0)
                    {
                        ret.Add(pl);
                        existingPageIdLangCodes.Add(key);
                    }
                } // foreach

                return ret;
            }
        }

        private class PageImages
        {
            public CmsPage Page;
            public CmsLanguage Language;
            public SingleImageData[] Images;

            public PageImages(CmsPage p, CmsLanguage l, SingleImageData[] images)
            {
                Page = p;
                Language = l;
                Images = images;
            }

            public static PageLanguage[] GetMatchingPagesForImage(SingleImageData needle, PageImages[] haystack)
            {
                List<PageLanguage> ret = new List<PageLanguage>();
                foreach (PageImages pi in haystack)
                {
                    foreach (SingleImageData img in pi.Images)
                    {
                        if (String.Compare(img.ImagePath, needle.ImagePath, true) == 0)
                            ret.Add(new PageLanguage(pi.Page, pi.Language));
                    } // foreach
                } // foreach
                return ret.ToArray();
            }

        }


        

    }
}
