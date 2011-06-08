using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Collections.Specialized;

using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
    public class UserImageGallery : BaseCmsPlaceholder
    {
        public static string UrlParamName = "image";


        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            try
            {                
                ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `userimagegallery` (
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `LangCode` varchar(5) NOT NULL,
                  `NumThumbsPerPage` int(10) NOT NULL,
                  `NumThumbsPerRow` int(10) NOT NULL,
                  `ThumbnailDisplayBoxWidth` int(10) NOT NULL,
                  `ThumbnailDisplayBoxHeight` int(10) NOT NULL,
                  `FullSizeDisplayBoxWidth` int(10) NOT NULL,
                  `FullSizeDisplayBoxHeight` int(10) NOT NULL,
                  `FullSizeLinkMode` varchar(255) NOT NULL,
                  `CaptionDisplayLocation` varchar(255) NOT NULL,
                  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
                ) ENGINE=InnoDB  DEFAULT CHARSET=utf8;
            "));

                ret.AddRange(SWFUploadHelpers.SWFUploadDependencies);

                // -- writable directories
                ret.Add(CmsWritableDirectoryDependency.UnderAppPath("UserFiles/ImageGalleries"));

                // -- each placeholder directory should be writable.
                UserImageGalleryDb db = (new UserImageGalleryDb());
                UserImageGalleryPlaceholderData[] phs = db.getAllUserImageGalleryPlaceholderDatas();
                foreach (UserImageGalleryPlaceholderData ph in phs)
                {
                    CmsPage p = CmsContext.getPageById(ph.PageId);
                    if (p.ID >= 0)
                    {
                        string dir = ph.getImageStorageDirectory(p);
                        if (dir != String.Empty)
                        {
                            ret.Add(new CmsWritableDirectoryDependency(dir));
                            // -- check if the image gallery files on disk are in the database.
                            if (CmsLocalFileOnDisk.FetchAllFilesInDirectory(dir).Length > 0)
                            {
                                foreach (string fn in System.IO.Directory.GetFiles(dir))
                                {
                                    if (!fn.StartsWith(CmsLocalFileOnDisk.DeletedFileFilenamePrefix))
                                    {
                                        ret.Add(new CmsMessageDependency("The UserImageGallery directory '"+dir+"' has files, but are not in the database"));
                                        break;
                                    }
                                } // foreach
                            }
                        }
                    }
                }

                // -- REQUIRED config entries
                ret.Add(new CmsConfigItemDependency("UserImageGallery.PageXofYText"));
                ret.Add(new CmsConfigItemDependency("UserImageGallery.PrevLinkText"));
                ret.Add(new CmsConfigItemDependency("UserImageGallery.NextLinkText"));
                ret.Add(new CmsConfigItemDependency("UserImageGallery.ReturnToGalleryText"));
                ret.Add(new CmsConfigItemDependency("UserImageGallery.NoImageText"));
                ret.Add(new CmsConfigItemDependency("UserImageGallery.ImageRemovedText"));
                ret.Add(new CmsConfigItemDependency("UserImageGallery.SetCaptionButtonText"));
                ret.Add(new CmsConfigItemDependency("UserImageGallery.RemoveImageButtonText"));
                ret.Add(new CmsConfigItemDependency("UserImageGallery.SetCaptionButtonText"));
                ret.Add(new CmsConfigItemDependency("UserImageGallery.RemoveImageButtonText"));
                ret.Add(new CmsConfigItemDependency("UserImageGallery.UploadImageButtonText"));
            }
            catch (Exception ex)
            {
                ret.Add(new CmsMessageDependency("Error gathering UserImageGallery dependencies: "+ex.Message));
            }

            return ret.ToArray();
        }

        /// <summary>
        /// returns the number of image galleries updated.
        /// </summary>
        /// <returns></returns>
        public static int UpdateDatabaseCacheOfImageInfos()
        {
            // -- Update the CmsLocalImageOnDisk database cache from the disk source.
            int ret = 0;
            UserImageGalleryDb db = (new UserImageGalleryDb());
            UserImageGalleryPlaceholderData[] phs = db.getAllUserImageGalleryPlaceholderDatas();
            foreach (UserImageGalleryPlaceholderData ph in phs)
            {                
                CmsPage p = CmsContext.getPageById(ph.PageId);
                if (p.ID >= 0)
                {
                    string dir = ph.getImageStorageDirectory(p);
                    if (dir != String.Empty)
                    {
                        CmsLocalImageOnDisk[] updates = CmsLocalImageOnDisk.UpdateFolderInDatabase(new System.IO.DirectoryInfo(dir));
                        ret++;
                    } // if
                } // if
            } // foreach
            return ret;
        }

        protected string getPageXofYText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserImageGallery.PageXofYText", "Page {0} of {1}", lang);
        }

        protected string getEmptyGalleryText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserImageGallery.NoImageText", "There currently aren't any images to view in this gallery", lang);
        }

        protected string getReturnToGalleryText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserImageGallery.ReturnToGalleryText", "Return to gallery", lang);
        }

        protected string getPrevLinkText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserImageGallery.PrevLinkText", "Previous", lang);
        }

        protected string getNextLinkText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserImageGallery.NextLinkText", "Next", lang);
        }

        protected string getImageRemovedText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserImageGallery.ImageRemovedText", "The image has been removed.", lang);
        }

        protected string getSetCaptionText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserImageGallery.SetCaptionButtonText", "Set caption", lang);
        }

        protected string getRemoveImageText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserImageGallery.RemoveImageButtonText", "Remove this image.", lang);
        }
        
        protected string getUploadImageText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserImageGallery.UploadImageButtonText", "Upload Images to Gallery", lang);
        }

        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }

        public enum PlaceholderDisplay { MultipleItems, SelectedItem }        

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            PlaceholderDisplay display = getCurrentDisplayMode();
            if (display == PlaceholderDisplay.MultipleItems)
            {
                RenderMultipleItems_InViewMode(writer, page, identifier, langToRenderFor, paramList);
            }
            else if (display == PlaceholderDisplay.SelectedItem)
            {
                RenderSelectedItem_InViewMode(writer, page, identifier, langToRenderFor, paramList);
            }
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            PlaceholderDisplay display = getCurrentDisplayMode();
            if (display == PlaceholderDisplay.MultipleItems)
            {
                RenderMultipleItems_InEditMode(writer, page, identifier, langToRenderFor, paramList);
            }
            else if (display == PlaceholderDisplay.SelectedItem)
            {
                RenderSelectedItem_InEditMode(writer, page, identifier, langToRenderFor, paramList);
            }
        }

        /// <summary>
        /// returns a value < 0 if no imageid has been specified
        /// </summary>
        /// <returns></returns>
        private int getCurrentImageResourceId()
        {
            return PageUtils.getFromForm(UrlParamName, Int32.MinValue);
        }

        /// <summary>
        /// returns a blank (newly created) resource if not found
        /// </summary>
        /// <returns></returns>
        private CmsLocalImageOnDisk getCurrentImageResource()
        {
            int resId = getCurrentImageResourceId();
            if (resId >= 0)
            {
                return CmsLocalImageOnDisk.FetchLastRevision(resId);
            }
            return new CmsLocalImageOnDisk();
        }


        public PlaceholderDisplay getCurrentDisplayMode()
        {
            CmsLocalImageOnDisk r = getCurrentImageResource();
            if (r.ResourceId < 0)
                return PlaceholderDisplay.MultipleItems;
            else
                return PlaceholderDisplay.SelectedItem;            
        }

        public void RenderSelectedItem_InEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            UserImageGalleryDb db = (new UserImageGalleryDb());
            StringBuilder html = new StringBuilder();

            UserImageGalleryPlaceholderData placeholderData = db.getUserImageGalleryPlaceholderData(page, identifier, langToRenderFor, true);
            CmsLocalImageOnDisk currentImage = getCurrentImageResource();

            html.Append("<div class=\"UserImageGallery\">");
            html.Append(renderFullSize(placeholderData, currentImage, page, langToRenderFor));
            html.Append("</div>");

            writer.Write(html.ToString());
        }

        public void RenderSelectedItem_InViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            UserImageGalleryDb db = (new UserImageGalleryDb());
            StringBuilder html = new StringBuilder();

            UserImageGalleryPlaceholderData placeholderData = db.getUserImageGalleryPlaceholderData(page, identifier, langToRenderFor, true);
            CmsLocalImageOnDisk currentImage = getCurrentImageResource();
            CmsLocalImageOnDisk[] allResources = CmsLocalImageOnDisk.FetchAllImagesInDirectory(placeholderData.getImageStorageDirectory(page), UserImageGalleryPlaceholderData.ImageExtensionsToDisplay);

            string action = PageUtils.getFromForm("action_" + currentImage.ResourceId.ToString(), "");
            if (String.Compare(action, "updateCaption", true) == 0)
            {
                string newCaption = PageUtils.getFromForm("caption_" + currentImage.ResourceId.ToString(), "");
                currentImage.setImageCaption(newCaption);
                currentImage.SaveToDatabase();
            }
            else if (String.Compare(action, "deleteImage", true) == 0)
            {
                bool b = CmsLocalImageOnDisk.Delete(currentImage, true);
                html.Append("<div class=\"UserImageGallery\">" + getImageRemovedText(langToRenderFor) + " <a href=\"" + page.Url + "\">" + getReturnToGalleryText(langToRenderFor) + "</a></div>");
                writer.Write(html.ToString());
                return;
            }

            html.Append("<div class=\"UserImageGallery\">");
            html.Append(getSelectedItem_NextPrevLinks(placeholderData, currentImage, allResources, langToRenderFor));
            html.Append(renderFullSize(placeholderData, currentImage, page, langToRenderFor));
            html.Append("</div>");

            writer.Write(html.ToString());
        }

        private string getSelectedItem_NextPrevLinks(UserImageGalleryPlaceholderData placeholderData, CmsLocalImageOnDisk imageToShow, CmsLocalImageOnDisk[] allImagesInGallery, CmsLanguage lang)
        {
            string nextUrl = "";

            string prevUrl = "";
            for (int i = 0; i < allImagesInGallery.Length; i++)
            {
                if (imageToShow.ResourceId == allImagesInGallery[i].ResourceId)
                {
                    Dictionary<string, string> pageParams = new Dictionary<string, string>();

                    if (i > 0)
                    {
                        pageParams.Add(UrlParamName, allImagesInGallery[i - 1].ResourceId.ToString());
                        prevUrl = CmsContext.currentPage.getUrl(pageParams);
                    }
                    if (i < allImagesInGallery.Length - 1)
                    {
                        pageParams.Clear();
                        pageParams.Add(UrlParamName, allImagesInGallery[i + 1].ResourceId.ToString());
                        nextUrl = CmsContext.currentPage.getUrl(pageParams);
                    }
                    break;
                }
            } // for



            List<string> navLinks = new List<string>();
            if (prevUrl != "")
                navLinks.Add("<a href=\"" + prevUrl + "\">&#171; " + getPrevLinkText(lang) + "</a>");

            int currentPageNum = getCurrentPageNumber(allImagesInGallery, imageToShow, placeholderData);
            navLinks.Add("<a href=\"" + getPagerUrl(currentPageNum, placeholderData) + "\">" + getReturnToGalleryText(lang) + "</a>");
            if (nextUrl != "")
                navLinks.Add("<a href=\"" + nextUrl + "\">" + getNextLinkText(lang) + " &#187;</a>");

            return "<p class=\"pager\">" + (String.Join(" | ", navLinks.ToArray())) + "</p>";
        }


        public void RenderMultipleItems_InEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            UserImageGalleryDb db = (new UserImageGalleryDb());
            StringBuilder html = new StringBuilder();

            string phId = "UserGallery_" + page.ID.ToString() + "_" + identifier.ToString() + "_" + langToRenderFor.shortCode + "_";

            UserImageGalleryPlaceholderData placeholderData = db.getUserImageGalleryPlaceholderData(page, identifier, langToRenderFor, true);

            if (PageUtils.getFromForm(phId + "action", "") == "update")
            {
                placeholderData.NumThumbsPerPage = PageUtils.getFromForm(phId + "NumThumbsPerPage", placeholderData.NumThumbsPerPage);
                placeholderData.NumThumbsPerRow = PageUtils.getFromForm(phId + "NumThumbsPerRow", placeholderData.NumThumbsPerRow);
                placeholderData.ThumbnailDisplayBoxWidth = PageUtils.getFromForm(phId + "ThumbnailDisplayBoxWidth", placeholderData.ThumbnailDisplayBoxWidth);
                placeholderData.ThumbnailDisplayBoxHeight = PageUtils.getFromForm(phId + "ThumbnailDisplayBoxHeight", placeholderData.ThumbnailDisplayBoxHeight);
                placeholderData.FullSizeDisplayBoxWidth = PageUtils.getFromForm(phId + "FullSizeDisplayBoxWidth", placeholderData.FullSizeDisplayBoxWidth);
                placeholderData.FullSizeDisplayBoxHeight = PageUtils.getFromForm(phId + "FullSizeDisplayBoxHeight", placeholderData.FullSizeDisplayBoxHeight);

                placeholderData.CaptionLocation = (UserImageGalleryPlaceholderData.CaptionDisplayLocation)PageUtils.getFromForm(phId + "CaptionLocation", typeof(UserImageGalleryPlaceholderData.CaptionDisplayLocation), placeholderData.CaptionLocation);
                placeholderData.FullSizeLinkMode = (UserImageGalleryPlaceholderData.FullSizeImageDisplayMode)PageUtils.getFromForm(phId + "FullSizeLinkMode", typeof(UserImageGalleryPlaceholderData.FullSizeImageDisplayMode), placeholderData.FullSizeLinkMode);

                db.saveUpdatedUserImageGalleryPlaceholderData(page, identifier, langToRenderFor, placeholderData);
            } // if update

            
            html.Append("<table>");
            html.Append("<tr><td>Num Thumbs Per Page</td><td>" + PageUtils.getInputTextHtml(phId + "NumThumbsPerPage", phId + "NumThumbsPerPage", placeholderData.NumThumbsPerPage.ToString(), 3, 3) + "</td></tr>");
            html.Append("<tr><td>Num Thumbs Per Row</td><td>" + PageUtils.getInputTextHtml(phId + "NumThumbsPerRow", phId + "NumThumbsPerRow", placeholderData.NumThumbsPerRow.ToString(), 3, 3) + "</td></tr>");
            html.Append("<tr><td>Thumbnail Display Box Width</td><td>" + PageUtils.getInputTextHtml(phId + "ThumbnailDisplayBoxWidth", phId + "ThumbnailDisplayBoxWidth", placeholderData.ThumbnailDisplayBoxWidth.ToString(), 3, 3) + "</td></tr>");
            html.Append("<tr><td>Thumbnail Display Box Height</td><td>" + PageUtils.getInputTextHtml(phId + "ThumbnailDisplayBoxHeight", phId + "ThumbnailDisplayBoxHeight", placeholderData.ThumbnailDisplayBoxHeight.ToString(), 3, 3) + "</td></tr>");
            html.Append("<tr><td>Full Size Display Box Width</td><td>" + PageUtils.getInputTextHtml(phId + "FullSizeDisplayBoxWidth", phId + "FullSizeDisplayBoxWidth", placeholderData.FullSizeDisplayBoxWidth.ToString(), 3, 3) + "</td></tr>");
            html.Append("<tr><td>Full Size Display Box Height</td><td>" + PageUtils.getInputTextHtml(phId + "FullSizeDisplayBoxHeight", phId + "FullSizeDisplayBoxHeight", placeholderData.FullSizeDisplayBoxHeight.ToString(), 3, 3) + "</td></tr>");

            html.Append("<tr><td>Caption Location</td><td>" + PageUtils.getDropDownHtml(phId + "CaptionLocation", phId + "CaptionLocation", typeof(UserImageGalleryPlaceholderData.CaptionDisplayLocation), placeholderData.CaptionLocation) + "</td></tr>");
            html.Append("<tr><td>Full Size Link Mode</td><td>" + PageUtils.getDropDownHtml(phId + "FullSizeLinkMode", phId + "FullSizeLinkMode", typeof(UserImageGalleryPlaceholderData.FullSizeImageDisplayMode), placeholderData.FullSizeLinkMode) + "</td></tr>");
            
            html.Append("</table>");
            html.Append(PageUtils.getHiddenInputHtml(phId + "action", "update"));
            writer.Write(html.ToString());
        }

        public void RenderMultipleItems_InViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            UserImageGalleryDb db = (new UserImageGalleryDb());
            StringBuilder html = new StringBuilder();

            UserImageGalleryPlaceholderData placeholderData = db.getUserImageGalleryPlaceholderData(page, identifier, langToRenderFor, true);
            string imageStorageDir = placeholderData.getImageStorageDirectory(page);
            if (!System.IO.Directory.Exists(imageStorageDir))
            {
                bool b = placeholderData.createImageStorageDirectory(page);
                if (!b)
                {
                    if (CmsContext.currentUserIsSuperAdmin)
                    {
                        html.Append("<h1>System Error: could not create image storage directory: '" + imageStorageDir + "'</h1>");
                    }
                    else
                    {
                        html.Append("<h1>System Error: could not create image storage directory!!</h1>");                        
                    }
                    writer.Write(html.ToString());
                    return;
                }
            }

            CmsLocalImageOnDisk[] allResources = CmsLocalImageOnDisk.FetchAllImagesInDirectory(imageStorageDir, UserImageGalleryPlaceholderData.ImageExtensionsToDisplay);

            if (allResources.Length == 0 && page.currentUserCanWrite)
            {
                CmsLocalImageOnDisk.UpdateFolderInDatabase(new System.IO.DirectoryInfo(imageStorageDir));
                allResources = CmsLocalImageOnDisk.FetchAllImagesInDirectory(imageStorageDir, UserImageGalleryPlaceholderData.ImageExtensionsToDisplay);
            }

            html.Append("<div class=\"UserImageGallery\">");
            html.Append(getGalleryView(placeholderData, allResources, page, langToRenderFor));
            html.Append("</div>");

            if (page.currentUserCanWrite)
            {
                html.Append(getSwfUploadHtml(page, placeholderData, langToRenderFor));
            }

            writer.Write(html.ToString());
        }

        private string getSwfUploadHtml(CmsPage page, UserImageGalleryPlaceholderData placeholderData, CmsLanguage lang)
        {
            StringBuilder html = new StringBuilder();

            string uploadUrl = CmsContext.ApplicationPath + "_system/tools/swfUpload/SwfUploadTarget.aspx?dir=ImageGalleries/" + page.ID.ToString();

            string ControlId = "swfUpload" + page.ID.ToString() + page.ID.ToString();

            // -- SWF Upload
            SWFUploadHelpers.AddPageJavascriptStatements(page, ControlId, uploadUrl, "*.jpg", "JPG Image Files (*.jpg)");                        

            html.Append("<form action=\"" + uploadUrl + "\">");

            html.Append("<p>");
            html.Append("<strong>" + getUploadImageText(lang) + ":</strong><br>");
            html.Append("&nbsp;&nbsp;<span id=\"spanButtonPlaceHolder\"></span>");
            // html.Append("<input type=\"button\" value=\"Upload file (Max "+PageUtils.MaxUploadFileSize+")\" onclick=\"swfu.selectFiles()\" style=\"font-size: 8pt;\" />");

            html.Append("</div>");
            html.Append("<fieldset class=\"flash\" id=\"fsUploadProgress\" style=\"display:none;\">");
            html.Append("<div id=\"divStatus\">0 Files Uploaded</div>");
            html.Append("<legend>Upload Queue</legend>");
            html.Append("</fieldset>");
            html.Append("<input id=\"btnCancel\" type=\"button\" value=\"Cancel All Uploads\" onclick=\"swfu.cancelQueue();\" disabled=\"disabled\" style=\"font-size: 8pt;\" />");
            html.Append("<div>");
            html.Append("</p>");
            html.Append("</form>");

            return html.ToString();
        }

        private string renderFullSize(UserImageGalleryPlaceholderData placeholderData, CmsLocalImageOnDisk imgToShow, CmsPage page, CmsLanguage lang)
        {

            string caption = imgToShow.getImageCaption();
            
            StringBuilder html = new StringBuilder();

            html.Append("<div class=\"image\">");

            if (caption.Trim() != "" && (
                placeholderData.CaptionLocation == UserImageGalleryPlaceholderData.CaptionDisplayLocation.Top ||
                placeholderData.CaptionLocation == UserImageGalleryPlaceholderData.CaptionDisplayLocation.TopAndBottom ))
            {
                html.Append("<div class=\"caption top\">");
                html.Append(caption);
                html.Append("</div>"); // caption
            }


            string imgTag = imgToShow.getImageHtmlTag(placeholderData.FullSizeDisplayBoxWidth, placeholderData.FullSizeDisplayBoxHeight, "");
            html.Append(imgTag);

            if (page.currentUserCanWrite)
            {
                html.Append("<div class=\"caption bottom\">");
                string formId = "userImageGallery";
                html.Append(page.getFormStartHtml(formId));
                html.Append(PageUtils.getHiddenInputHtml(UserImageGallery.UrlParamName, imgToShow.ResourceId.ToString()));
                html.Append(PageUtils.getHiddenInputHtml("action_"+imgToShow.ResourceId.ToString(), "updateCaption"));
                html.Append(PageUtils.getInputTextHtml("caption_" + imgToShow.ResourceId.ToString(), "caption_" + imgToShow.ResourceId.ToString(), caption, 40, 200));
                html.Append(" <input type=\"submit\" value=\"" + getSetCaptionText(lang) + "\">");
                html.Append(page.getFormCloseHtml(formId));
                html.Append("</div>"); // caption


                html.Append(page.getFormStartHtml(formId));
                html.Append(PageUtils.getHiddenInputHtml(UserImageGallery.UrlParamName, imgToShow.ResourceId.ToString()));
                html.Append(PageUtils.getHiddenInputHtml("action_" + imgToShow.ResourceId.ToString(), "deleteImage"));
                html.Append("<p align=\"right\">");
                html.Append(" <input type=\"submit\" value=\"" + getRemoveImageText(lang) + "\">");
                html.Append("</p>");
                html.Append(page.getFormCloseHtml(formId));                
                
            }
            else if (caption.Trim() != "" && (
                placeholderData.CaptionLocation == UserImageGalleryPlaceholderData.CaptionDisplayLocation.Bottom ||
                placeholderData.CaptionLocation == UserImageGalleryPlaceholderData.CaptionDisplayLocation.TopAndBottom))
            {
                html.Append("<div class=\"caption bottom\">");
                html.Append(caption);
                html.Append("</div>"); // caption
            }

            html.Append("</div>");

            return html.ToString();
        }

        private string renderThumbnail(UserImageGalleryPlaceholderData placeholderData, CmsLocalImageOnDisk img, CmsPage page)
        {

            string caption = img.getImageCaption();

            StringBuilder html = new StringBuilder();

            html.Append("<div class=\"image\">");

            if (caption.Trim() != "" && (
                placeholderData.CaptionLocation == UserImageGalleryPlaceholderData.CaptionDisplayLocation.Top ||
                placeholderData.CaptionLocation == UserImageGalleryPlaceholderData.CaptionDisplayLocation.TopAndBottom))
            {
                html.Append("<div class=\"caption top\">");
                html.Append(caption);
                html.Append("</div>"); // caption
            }

            
            string imgTag = img.getImageHtmlTag(placeholderData.ThumbnailDisplayBoxWidth, placeholderData.ThumbnailDisplayBoxHeight,"");

            Dictionary<string, string> pageParams = new Dictionary<string,string>();
            pageParams.Add(UrlParamName, img.ResourceId.ToString());
            html.Append("<a href=\"" + page.getUrl(pageParams) + "\">");
            html.Append(imgTag);
            html.Append("</a>");


            if (caption.Trim() != "" && (
                placeholderData.CaptionLocation == UserImageGalleryPlaceholderData.CaptionDisplayLocation.Bottom ||
                placeholderData.CaptionLocation == UserImageGalleryPlaceholderData.CaptionDisplayLocation.TopAndBottom))
            {
                html.Append("<div class=\"caption bottom\">");
                html.Append(caption);
                html.Append("</div>"); // caption
            }

            html.Append("</div>");

            return html.ToString();
        }



        public string getGalleryView(UserImageGalleryPlaceholderData placeholderData, CmsLocalImageOnDisk[] imageDatas, CmsPage page, CmsLanguage lang)
        {

            if (imageDatas.Length == 0)
            {
                return ("<p><strong>" + getEmptyGalleryText(lang) + "</strong></p>");
            }

            StringBuilder html = new StringBuilder();

            string pagerHtml = getThumbnailPagerOutput(imageDatas, placeholderData, lang);
            html.Append("<p>" + pagerHtml + "</p>");

            int startAtItemNumber = PageUtils.getFromForm("ugp", 0);
            if (startAtItemNumber >= imageDatas.Length)
            {
                startAtItemNumber = imageDatas.Length - 1;
            }
            int endAt = Math.Min(startAtItemNumber + placeholderData.NumThumbsPerPage - 1, imageDatas.Length - 1);


            if (startAtItemNumber == 0 && endAt == 0 && imageDatas.Length == 1)
            {
                html.Append(renderThumbnail(placeholderData, imageDatas[0],page));
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

                    CmsLocalImageOnDisk image = imageDatas[i];
                    html.Append("<td>");
                    html.Append(renderThumbnail(placeholderData, image, page));
                    html.Append("</td>" + Environment.NewLine);
                } // for
                if (rowStarted)
                    html.Append("</tr>");
                html.Append("</table>" + Environment.NewLine);

            } // else			

            html.Append("<p>" + pagerHtml + "</p>");

            return html.ToString();

        } // RenderView

        private int getCurrentPageNumber(CmsLocalImageOnDisk[] searchResults, CmsLocalImageOnDisk imageToShow, UserImageGalleryPlaceholderData placeholderData)
        {
            int numPages = (int)Math.Ceiling((double)searchResults.Length / placeholderData.NumThumbsPerPage);
            if (numPages <= 0)
                numPages = 1;

            int startAtItemNumber = 0;
            if (imageToShow != null && imageToShow.ResourceId >= 0)
            {
                for (int i = 0; i < searchResults.Length; i++)
                {
                    if (searchResults[i].ResourceId == imageToShow.ResourceId)
                    {
                        int rem = 0;
                        Math.DivRem(i, placeholderData.NumThumbsPerPage, out rem);
                        startAtItemNumber = i - rem;
                        break;
                    }
                }
            }
            else
                startAtItemNumber = PageUtils.getFromForm("ugp", Int32.MinValue);


            if (startAtItemNumber >= searchResults.Length)
            {
                startAtItemNumber = searchResults.Length - 1;
            }
            else if (startAtItemNumber < 0)
                startAtItemNumber = 0;

            int currPageNum = (int)Math.Ceiling((double)startAtItemNumber / placeholderData.NumThumbsPerPage) + 1;

            return currPageNum;
        }

        protected string getThumbnailPagerOutput(CmsLocalImageOnDisk[] searchResults, UserImageGalleryPlaceholderData data, CmsLanguage lang)
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
                html.Append("&laquo; ");
                html.Append(getPrevLinkText(lang));
                html.Append("</a> ");
            }

            string[] parm = new string[]{currPageNum.ToString(),numPages.ToString()};
            html.Append(String.Format(getPageXofYText(lang), parm));


            if (currPageNum < numPages && numPages > 1)
            {

                html.Append(" <a href=\"" + getPagerUrl(currPageNum + 1, data) + "\">");
                html.Append(getNextLinkText(lang));
                html.Append(" &raquo;");
                html.Append("</a> ");
            }

            html.Append("</div>");

            return (html.ToString());
        } // OutputPager

        private string getPagerUrl(int pageNumber, UserImageGalleryPlaceholderData data)
        {
            int startAt = (pageNumber - 1) * data.NumThumbsPerPage;
            // string query = PageUtils.getFromForm("q", "");
            NameValueCollection urlParams = new NameValueCollection();
            urlParams.Add("ugp", startAt.ToString());

            string url = CmsContext.getUrlByPagePath(CmsContext.currentPage.Path, urlParams);
            return url;
        }

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            List<Rss.RssItem> ret = new List<Rss.RssItem>();

            UserImageGalleryDb db = (new UserImageGalleryDb());            

            UserImageGalleryPlaceholderData placeholderData = db.getUserImageGalleryPlaceholderData(page, placeholderDefinition.Identifier, langToRenderFor, true);
            string imageStorageDir = placeholderData.getImageStorageDirectory(page);

            CmsLocalImageOnDisk[] allResources = CmsLocalImageOnDisk.FetchAllImagesInDirectory(imageStorageDir, UserImageGalleryPlaceholderData.ImageExtensionsToDisplay);

            foreach (CmsLocalImageOnDisk image in allResources)
            {
                Rss.RssItem item = new Rss.RssItem();
                string title = image.getImageCaption();
                if (title == "")
                    title = image.FileName;
                item.Title = title;

                Dictionary<string, string> pageParams = new Dictionary<string, string>();
                pageParams.Add(UrlParamName, image.ResourceId.ToString());
                item.Link = new Uri(page.getUrl(pageParams));                


                item.Description = renderFullSize(placeholderData, image, page, langToRenderFor);

                ret.Add(item);
            } // foreach            


            return ret.ToArray();
        }
    }
}
