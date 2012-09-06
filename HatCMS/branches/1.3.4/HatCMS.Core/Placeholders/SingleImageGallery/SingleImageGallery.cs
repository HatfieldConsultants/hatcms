using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal.Collections;

using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
    public class SingleImageGallery: BaseCmsPlaceholder
    {
        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `singleimagegallery` (
                  `SingleImageGalleryId` int(10) unsigned NOT NULL AUTO_INCREMENT,
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `langShortCode` varchar(255) NOT NULL,
                  `PageIdToGatherImagesFrom` int(10) NOT NULL,
                  `RecursiveGatherImages` int(10) unsigned NOT NULL,
                  `ThumbnailDisplayBoxWidth` int(11) NOT NULL,
                  `ThumbnailDisplayBoxHeight` int(11) NOT NULL,
                  `OverrideFullDisplayBoxSize` int(10) unsigned NOT NULL,
                  `FullSizeDisplayBoxWidth` int(11) NOT NULL,
                  `FullSizeDisplayBoxHeight` int(11) NOT NULL,
                  `NumThumbsPerRow` int(10) unsigned NOT NULL,
                  `NumThumbsPerPage` int(11) NOT NULL,
                  `ShowOnlyTags` varchar(255) NOT NULL DEFAULT '',
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`SingleImageGalleryId`)
                ) ENGINE=InnoDB  DEFAULT CHARSET=utf8;
            "));

            ret.AddRange(new SingleImage().getDependencies());
            
            return ret.ToArray();
        }


        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }                

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string controlId = "SingleImageGallery_" + page.ID.ToString() + identifier.ToString() + langToRenderFor.shortCode;
            SingleImageGalleryDb db = new SingleImageGalleryDb();
            SingleImageGalleryPlaceholderData placeholderData = db.getSingleImageGallery(page, identifier, langToRenderFor, true);

            string[] possibleTags = (new SingleImageDb()).getAllPossibleTags();           

            string action = PageUtils.getFromForm(controlId + "_action","");
            if (action == "updateSingleImageGallery")
            {
                placeholderData.PageIdToGatherImagesFrom = PageUtils.getFromForm(controlId + "PageIdToGatherImagesFrom", placeholderData.PageIdToGatherImagesFrom);
                placeholderData.RecursiveGatherImages = PageUtils.getFromForm(controlId+"RecursiveGatherImages",placeholderData.RecursiveGatherImages);

                placeholderData.ThumbImageDisplayBoxWidth = PageUtils.getFromForm(controlId + "ThumbnailDisplayBoxWidth", placeholderData.ThumbImageDisplayBoxWidth);

                placeholderData.ThumbImageDisplayBoxHeight = PageUtils.getFromForm(controlId + "ThumbnailDisplayBoxHeight", placeholderData.ThumbImageDisplayBoxHeight);
                
                placeholderData.OverrideFullDisplayBoxSize = PageUtils.getFromForm(controlId + "OverrideFullDisplayBoxSize", false);

                placeholderData.FullSizeDisplayBoxWidth = PageUtils.getFromForm(controlId + "FullSizeDisplayBoxWidth", placeholderData.FullSizeDisplayBoxWidth);

                placeholderData.FullSizeDisplayBoxHeight = PageUtils.getFromForm(controlId+"FullSizeDisplayBoxHeight",placeholderData.FullSizeDisplayBoxHeight);

                placeholderData.NumThumbsPerRow = PageUtils.getFromForm(controlId + "NumThumbsPerRow", placeholderData.NumThumbsPerRow);

                placeholderData.NumThumbsPerPage = PageUtils.getFromForm(controlId + "NumThumbsPerPage", placeholderData.NumThumbsPerPage);

                placeholderData.TagsImagesMustHave = PageUtils.getFromForm(controlId + "Tags");

                db.saveUpdatedSingleImageGallery(page, identifier, langToRenderFor, placeholderData);
            }

            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();

            NameValueCollection pageSelection = new NameValueCollection();
            foreach (int pageId in allPages.Keys)
            {
                pageSelection.Add(pageId.ToString(), allPages[pageId].getPath(langToRenderFor));
            }                

            
            StringBuilder html = new StringBuilder();

            html.Append("<table>" + Environment.NewLine);

            html.Append("<tr>");
            html.Append("<td>Gather images from page:</td>");
            html.Append("<td>" + PageUtils.getDropDownHtml(controlId + "PageIdToGatherImagesFrom", controlId + "PageIdToGatherImagesFrom", pageSelection, placeholderData.PageIdToGatherImagesFrom.ToString()) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Recursively gather images from sub-pages?</td>");
            NameValueCollection recursiveOptions = new NameValueCollection();
            recursiveOptions.Add(true.ToString(), "from the selected page and all its children");
            recursiveOptions.Add(false.ToString(), "only the selected page");
            html.Append("<td>"+PageUtils.getDropDownHtml(controlId+"RecursiveGatherImages", controlId+"RecursiveGatherImages", recursiveOptions, placeholderData.RecursiveGatherImages.ToString())+"</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Number of thumbnails per-row:</td>");
            html.Append("<td>"+PageUtils.getInputTextHtml(controlId+"NumThumbsPerRow",controlId+"NumThumbsPerRow", placeholderData.NumThumbsPerRow.ToString(), 3, 3)+"</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Number of thumbnails per-page:</td>");
            html.Append("<td>" + PageUtils.getInputTextHtml(controlId + "NumThumbsPerPage", controlId + "NumThumbsPerPage", placeholderData.NumThumbsPerPage.ToString(), 3, 3) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Thumbnail size:</td>");
            html.Append("<td>");                        
            html.Append(PageUtils.getInputTextHtml(controlId + "ThumbnailDisplayBoxWidth", controlId + "ThumbnailDisplayBoxWidth", placeholderData.ThumbImageDisplayBoxWidth.ToString(), 3, 4));
            html.Append(" x ");
            html.Append(PageUtils.getInputTextHtml(controlId + "ThumbnailDisplayBoxHeight", controlId + "ThumbnailDisplayBoxHeight", placeholderData.ThumbImageDisplayBoxHeight.ToString(), 3, 4));
            html.Append("</td>");
            html.Append("</tr>");

             
            html.Append("<tr>");
            html.Append("<td>Full-size display:</td>");
            html.Append("<td>");
            html.Append(PageUtils.getCheckboxHtml("Override full-sized display size", controlId + "OverrideFullDisplayBoxSize", controlId + "OverrideFullDisplayBoxSize", true.ToString(), placeholderData.OverrideFullDisplayBoxSize));
            html.Append("<br />");
            html.Append("Size: ");
            html.Append(PageUtils.getInputTextHtml(controlId + "FullSizeDisplayBoxWidth", controlId + "FullSizeDisplayBoxWidth", placeholderData.FullSizeDisplayBoxWidth.ToString(), 3, 4));
            html.Append(" x ");
            html.Append(PageUtils.getInputTextHtml(controlId + "FullSizeDisplayBoxHeight", controlId + "FullSizeDisplayBoxHeight", placeholderData.FullSizeDisplayBoxHeight.ToString(), 3, 4));
            html.Append("</td>");
            html.Append("</tr>");

            if (possibleTags.Length > 0)
            {
                html.Append("<tr><td>Images must be tagged with:<br />(when no tags are selected, all images are displayed)</td><td>");
                foreach (string t in possibleTags)
                {
                    if (t != "")
                    {
                        html.Append(PageUtils.getCheckboxHtml(t.Trim(), controlId + "Tags", "tag_" + t, t.Trim(), Array.IndexOf(placeholderData.TagsImagesMustHave, t) > -1));
                        html.Append("<br />");
                    }
                } // foreach

                html.Append("</td></tr>");
            }


            html.Append("</table>" + Environment.NewLine);

            html.Append(PageUtils.getHiddenInputHtml(controlId + "_action", "updateSingleImageGallery"));

            writer.Write(html.ToString());

        } // RenderEdit

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string controlId = "SingleImageGallery_" + page.ID.ToString() + identifier.ToString() + langToRenderFor.shortCode;
            SingleImageGalleryDb db = new SingleImageGalleryDb();
            SingleImageGalleryPlaceholderData placeholderData = db.getSingleImageGallery(page, identifier, langToRenderFor, true);
            SingleImageData[] imageDatas = new SingleImageData[0];
            SingleImageDb singleImageDb = new SingleImageDb();
            List<CmsPage> pagesToGatherImagesFrom = new List<CmsPage>();
           
            // -- gather images to display from selected pages

            CmsPage startPage = CmsContext.getPageById(placeholderData.PageIdToGatherImagesFrom);

            if (placeholderData.RecursiveGatherImages)
            {
                Dictionary<int, CmsPage> allPages = startPage.getLinearizedPages();
                pagesToGatherImagesFrom = new List<CmsPage>(allPages.Values);
            }
            else
            {
                pagesToGatherImagesFrom.Add(startPage);
            }

            imageDatas = singleImageDb.getSingleImages(pagesToGatherImagesFrom.ToArray(), langToRenderFor);

            imageDatas = filterOutDuplicateImagePaths(placeholderData, imageDatas);
            imageDatas = filterImagesByRequiredTags(placeholderData, imageDatas);
            imageDatas = sortByPageOrder(placeholderData, imageDatas, pagesToGatherImagesFrom.ToArray());
            

            // -- override the full-sized display            

            int imgIdToView = PageUtils.getFromForm("galleryimage", Int32.MinValue);
            if (imgIdToView >= 0)
            {
                foreach (SingleImageData i in imageDatas)
                {
                    if (i.SingleImageId == imgIdToView)
                    {
                        writer.Write(renderSinglePageDisplayOfImage(placeholderData, i, imageDatas, pagesToGatherImagesFrom.ToArray()));
                        return;
                    }
                } // foreach

            }
            else
            {
                writer.Write(getGalleryView(placeholderData, imageDatas, pagesToGatherImagesFrom.ToArray(), langToRenderFor));
            }
        } // RenderView

        private string renderSinglePageDisplayOfImage(SingleImageGalleryPlaceholderData placeholderData,SingleImageData imageToShow, SingleImageData[] imageDatas, CmsPage[] pagesToGatherImagesFrom)
        {
            string nextUrl = "";
            
            string prevUrl = "";
            for (int i = 0; i < imageDatas.Length; i++)
            {
                if (imageToShow.SingleImageId == imageDatas[i].SingleImageId)
                {
                    Dictionary<string, string> pageParams = new Dictionary<string,string>();
                
                    if (i > 0)
                    {
                        pageParams.Add("galleryimage", imageDatas[i-1].SingleImageId.ToString());
                        prevUrl = CmsContext.currentPage.getUrl(pageParams);
                    }
                    if (i < imageDatas.Length - 1)
                    {
                        pageParams.Clear();
                        pageParams.Add("galleryimage", imageDatas[i + 1].SingleImageId.ToString());
                        nextUrl = CmsContext.currentPage.getUrl(pageParams);
                    }
                    break;
                }
            } // for

            
            
            List<string> navLinks = new List<string>();
            if (prevUrl != "")
                navLinks.Add("<a href=\"" + prevUrl + "\">&#171; prev</a>");

            int currentPageNum = getCurrentPageNumber(imageDatas, imageToShow, placeholderData);
            navLinks.Add("<a href=\"" + getPagerUrl(currentPageNum, placeholderData) + "\">return to gallery</a>");  
            if (nextUrl != "")
                navLinks.Add("<a href=\"" + nextUrl + "\">next &#187;</a>");

            StringBuilder html = new StringBuilder();
            html.Append("<div class=\"SingleImageGallery SingleImageDisplay\">");

            html.Append("<p class=\"pager\">"); 
            html.Append(String.Join(" | ", navLinks.ToArray()));

            if (imageToShow.Caption.Trim() != "")
            {
                html.Append("<div class=\"caption top\">");
                html.Append(imageToShow.Caption);
                html.Append("</div>"); // caption
            }
            html.Append("</p>");

            int fullImageBoxWidth = CmsConfig.getConfigValue("SingleImage.FullSizeDisplayWidth", -1);
            int fullImageBoxHeight = CmsConfig.getConfigValue("SingleImage.FullSizeDisplayHeight", -1);
            if (placeholderData.OverrideFullDisplayBoxSize)
            {
                fullImageBoxWidth = placeholderData.FullSizeDisplayBoxWidth;
                fullImageBoxHeight = placeholderData.FullSizeDisplayBoxHeight;
            }

            string imgUrl = CmsContext.UserInterface.ShowThumbnailPage.getThumbDisplayUrl(imageToShow.ImagePath, fullImageBoxWidth, fullImageBoxHeight);
            System.Drawing.Size imgSize = CmsContext.UserInterface.ShowThumbnailPage.getDisplayWidthAndHeight(imageToShow.ImagePath, fullImageBoxWidth, fullImageBoxHeight);

            string width = "";
            string height = "";
            if (!imgSize.IsEmpty)
            {
                width = " width=\"" + imgSize.Width + "\"";
                height = " height=\"" + imgSize.Height.ToString() + "\"";
            }

            html.Append("<img src=\"" + imgUrl + "\"" + width + "" + height + ">");

            if (imageToShow.Caption.Trim() != "")
            {
                html.Append("<div class=\"caption bottom\">");
                html.Append(imageToShow.Caption);
                html.Append("</div>"); // caption
            }

            if (imageToShow.Credits.Trim() != "")
            {
                html.Append("<div class=\"credits\">");
                string creditsPrefix = CmsConfig.getConfigValue("SingleImage.CreditsPrefix", "");
                html.Append(creditsPrefix + imageToShow.Credits);
                html.Append("</div>"); // credits
            }

            html.Append("</div>");

            return html.ToString();
        }

        private string renderThumbnail(SingleImageGalleryPlaceholderData placeholderData, SingleImageData img, CmsLanguage langToRenderFor)
        {
            string fullDisplayUrl = "";
            Dictionary<string, string> pageParams = new Dictionary<string, string>();
            pageParams.Add("galleryimage", img.SingleImageId.ToString());
            fullDisplayUrl = CmsContext.currentPage.getUrl(pageParams);

            SingleImageDisplayInfo displayInfo = new SingleImageDisplayInfo();
            displayInfo.FullImageDisplayUrl = fullDisplayUrl;
            
            displayInfo.PopupDisplayBox = new System.Drawing.Size(-1, -1);
            displayInfo.ImagePath = img.ImagePath;

            displayInfo.ThumbImageDisplayBox = new System.Drawing.Size(placeholderData.ThumbImageDisplayBoxWidth, placeholderData.ThumbImageDisplayBoxHeight);
            displayInfo.Caption = img.Caption;
            displayInfo.Credits = img.Credits;

            int fullImageBoxWidth = -1;
            int fullImageBoxHeight = -1;
            if (placeholderData.OverrideFullDisplayBoxSize)
            {
                fullImageBoxWidth = placeholderData.FullSizeDisplayBoxWidth;
                fullImageBoxHeight = placeholderData.FullSizeDisplayBoxHeight;
            }
            else
            {
                fullImageBoxWidth = CmsConfig.getConfigValue("SingleImage.FullSizeDisplayWidth", -1);
                fullImageBoxHeight = CmsConfig.getConfigValue("SingleImage.FullSizeDisplayHeight", -1);            
            }
            displayInfo.FullImageDisplayBox = new System.Drawing.Size(fullImageBoxWidth, fullImageBoxHeight);

            // -- Multilingual CreditsPromptPrefix
            string creditPrefix = CmsConfig.getConfigValue("SingleImage.CreditsPromptPrefix", "");
            string[] creditPrefixParts = creditPrefix.Split(new char[] { CmsConfig.PerLanguageConfigSplitter }, StringSplitOptions.RemoveEmptyEntries);
            if (creditPrefixParts.Length >= CmsConfig.Languages.Length)
            {
                int index = CmsLanguage.IndexOf(langToRenderFor.shortCode, CmsConfig.Languages);
                if (index >= 0)
                    creditPrefix = creditPrefixParts[index];
            }

            // -- Multilingual ClickToEnlargeText
            string clickToEnlargeText = CmsConfig.getConfigValue("SingleImage.ClickToEnlargeText", "");
            string[] clickToEnlargeTextParts = clickToEnlargeText.Split(new char[] { CmsConfig.PerLanguageConfigSplitter }, StringSplitOptions.RemoveEmptyEntries);
            if (clickToEnlargeTextParts.Length >= CmsConfig.Languages.Length)
            {
                int index = CmsLanguage.IndexOf(langToRenderFor.shortCode, CmsConfig.Languages);
                if (index >= 0)
                    clickToEnlargeText = clickToEnlargeTextParts[index];
            }

            displayInfo.CreditsPromptPrefix = creditPrefix;
            displayInfo.ClickToEnlargeText = clickToEnlargeText;

            string displayTemplate = "<a href=\"{5}\"><img src=\"{2}\" width=\"{0}\" height=\"{1}\" /></a>";
            displayTemplate = CmsConfig.getConfigValue("SingleImage.WithLinkTemplate", displayTemplate);

            displayInfo.ThumbDisplayWithLinkTemplate = displayTemplate;
            displayInfo.ThumbDisplayWithoutLinkTemplate = displayTemplate;

            return SingleImage.getStandardHtmlView(displayInfo);
        }

        public string getGalleryView(SingleImageGalleryPlaceholderData placeholderData, SingleImageData[] imageDatas, CmsPage[] pagesToGatherImagesFrom, CmsLanguage langToRenderFor)
        {

            if (imageDatas.Length == 0)
            {
                return ("<p><strong>There currently aren't any images to view in this gallery</strong></p>");                
            }

            StringBuilder html = new StringBuilder();

            string pagerHtml = getThumbnailPagerOutput(imageDatas, placeholderData);
            html.Append("<p>" + pagerHtml + "</p>");

            int startAtItemNumber = PageUtils.getFromForm("gn", 0);
            if (startAtItemNumber >= imageDatas.Length)
            {
                startAtItemNumber = imageDatas.Length - 1;
            }
            int endAt = Math.Min(startAtItemNumber + placeholderData.NumThumbsPerPage - 1, imageDatas.Length - 1);

            
            if (startAtItemNumber == 0 && endAt == 0 && imageDatas.Length == 1)
            {
                html.Append(renderThumbnail(placeholderData, imageDatas[0], langToRenderFor));
            }
            else
            {
                html.Append("<table>" + Environment.NewLine);
                bool rowStarted = false;

                for (int i = startAtItemNumber; i <= endAt; i++)
                {
                    if (endAt <= 0)
                        break;

                    if (i % placeholderData.NumThumbsPerRow == 0)
                    {
                        if (rowStarted)
                            html.Append("</tr>" + Environment.NewLine);
                        html.Append("<tr>" + Environment.NewLine);
                        rowStarted = true;
                    }

                    SingleImageData image = imageDatas[i];
                    html.Append("<td>");
                    html.Append(renderThumbnail(placeholderData, image, langToRenderFor));
                    if (CmsContext.currentPage.currentUserCanWrite)
                    {
                        CmsPage pageContainingImage = image.getPageContainingImage(pagesToGatherImagesFrom);
                        if (pageContainingImage != null)
                        {
                            html.Append("<br />");
                            html.Append("<a title=\"found on page '" + pageContainingImage.Title.Replace("\"", "") + "'\" href=\"" + pageContainingImage.Url + "\">( found here )</a>");
                        }
                    }
                    html.Append("</td>" + Environment.NewLine);
                } // for
                if (rowStarted)
                    html.Append("</tr>");
                html.Append("</table>" + Environment.NewLine);

            } // else			

            html.Append("<p>" + pagerHtml + "</p>");

            return html.ToString();

        } // RenderView


        private SingleImageData[] filterOutDuplicateImagePaths(SingleImageGalleryPlaceholderData placeholderData, SingleImageData[] imageDatas)
        {
            List<string> existingPagePaths = new List<string>();
            List<SingleImageData> ret = new List<SingleImageData>();
            foreach (SingleImageData img in imageDatas)
            {
                if (existingPagePaths.IndexOf(img.ImagePath.Trim().ToLower()) == -1)
                {
                    ret.Add(img);
                    existingPagePaths.Add(img.ImagePath.Trim().ToLower());
                }
            }
            return ret.ToArray();
        }
        private SingleImageData[] filterImagesByRequiredTags(SingleImageGalleryPlaceholderData placeholderData, SingleImageData[] imageDatas)
        {
            // if nothing to filter by, show them all
            if (placeholderData.TagsImagesMustHave.Length == 0)
                return imageDatas;

            List<SingleImageData> ret = new List<SingleImageData>();
            foreach (SingleImageData img in imageDatas)
            {
                foreach (string reqiredTag in placeholderData.TagsImagesMustHave)
                {
                    if (img.containsTag(reqiredTag))
                    {
                        ret.Add(img);
                        break;
                    }
                } // foreach
            }
            return ret.ToArray();
        }

        private SingleImageData[] getImagesForPage(CmsPage page, SingleImageData[] allImages)
        {
            List<SingleImageData> ret = new List<SingleImageData>();
            foreach (SingleImageData img in allImages)
            {
                if (img.PageId == page.ID)
                    ret.Add(img);
            } // foreach
            return ret.ToArray();
        }

        private SingleImageData[] sortByPageOrder(SingleImageGalleryPlaceholderData placeholderData, SingleImageData[] imageDatas, CmsPage[] orderedPages)
        {
            List<SingleImageData> ret = new List<SingleImageData>();

            foreach (CmsPage p in orderedPages)
            {
                ret.AddRange(getImagesForPage(p, imageDatas));    
            }

            return ret.ToArray();
        }

        private int getCurrentPageNumber(SingleImageData[] searchResults, SingleImageData imageToShow, SingleImageGalleryPlaceholderData placeholderData)
        {
            int numPages = (int)Math.Ceiling((double)searchResults.Length / placeholderData.NumThumbsPerPage);
            if (numPages <= 0)
                numPages = 1;

            int startAtItemNumber = 0;
            if (imageToShow != null)
            {
                for (int i = 0; i < searchResults.Length; i++)
                {
                    if (searchResults[i].SingleImageId == imageToShow.SingleImageId)
                    {
                        int rem = 0;
                        Math.DivRem(i,placeholderData.NumThumbsPerPage, out rem);
                        startAtItemNumber = i - rem;
                        break;
                    }
                }
            }
            else
                startAtItemNumber = PageUtils.getFromForm("gn", Int32.MinValue);


            if (startAtItemNumber >= searchResults.Length)
            {
                startAtItemNumber = searchResults.Length - 1;
            }
            else if (startAtItemNumber < 0)
                startAtItemNumber = 0;

            int currPageNum = (int)Math.Ceiling((double)startAtItemNumber / placeholderData.NumThumbsPerPage) + 1;

            return currPageNum;
        }

        protected string getThumbnailPagerOutput(SingleImageData[] searchResults, SingleImageGalleryPlaceholderData data)
        {
            StringBuilder html = new StringBuilder();

            html.Append("<div class=\"pager\">");
            int numPages = (int)Math.Ceiling((double)searchResults.Length / data.NumThumbsPerPage);
            if (numPages <= 0)
                numPages = 1;            


            int currPageNum = getCurrentPageNumber(searchResults, null, data);

            if (currPageNum > 1 && numPages > 1)
            {
                html.Append("<a href=\"" + getPagerUrl(currPageNum - 1, data) + "\">");
                html.Append("&laquo; prev");
                html.Append("</a> ");
            }

            html.Append("Page " + currPageNum.ToString() + " of " + numPages.ToString());

            if (currPageNum < numPages && numPages > 1)
            {
                
                html.Append(" <a href=\"" + getPagerUrl(currPageNum + 1, data) + "\">");
                html.Append("next &raquo;");
                html.Append("</a> ");
            }

            html.Append("</div>");

            return (html.ToString());
        } // OutputPager

        private string getPagerUrl(int pageNumber, SingleImageGalleryPlaceholderData data)
        {
            int startAt = (pageNumber - 1) * data.NumThumbsPerPage;
            // string query = PageUtils.getFromForm("q", "");
            NameValueCollection urlParams = new NameValueCollection();
            urlParams.Add("gn", startAt.ToString());
            
            string url = CmsContext.getUrlByPagePath(CmsContext.currentPage.Path, urlParams);
            return url;
        }

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            return new Rss.RssItem[0]; // no RSS items to return (at this time).
        }

    } // placeholder
}
