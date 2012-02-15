using System;
using System.Text;
using System.IO;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using Hatfield.Web.Portal;
// using HatCMS.WebEditor.Helpers;

namespace HatCMS.Placeholders
{
   
    /// <summary>
	/// A placeholder that contains a single image. An image can have a caption and credits line.
	/// </summary>
    public class SingleImage : BaseCmsPlaceholder
	{
        public override CmsDependency[] getDependencies()
        {
            string showLargerPagePath = CmsConfig.getConfigValue("SingleImage.FullSizeDisplayPath", "/_internal/showImage");

            List<CmsDependency> ret = new List<CmsDependency>();
            
            // -- writable directories
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("UserFiles/Image"));
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("_system/writable/ThumbnailCache"));
            ret.Add(new CmsConfigItemDependency("ThumbImageCacheDirectory", CmsDependency.ExistsMode.MustNotExist)); // removed this config entry. Thumbnail cache is always in _system/writable/ThumbnailCache
            
            // -- helpers
            ret.Add(CmsFileDependency.UnderAppPath("_system/tools/SingleImage/SingleImageEditor.aspx", new DateTime(2010,4,30)));
            ret.Add(CmsFileDependency.UnderAppPath("_system/tools/FCKHelpers/InlineImageBrowser2.aspx"));

            // -- pages
            ret.Add(new CmsPageDependency(showLargerPagePath, CmsConfig.Languages));

            // -- database tables
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `singleimage` (
                  `SingleImageId` int(10) unsigned NOT NULL AUTO_INCREMENT,
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `langShortCode` varchar(255) NOT NULL DEFAULT '',
                  `RevisionNumber` int(11) NOT NULL DEFAULT '1',
                  `ImagePath` varchar(255) NOT NULL,
                  `ThumbnailDisplayBoxWidth` int(11) NOT NULL DEFAULT '-1',
                  `ThumbnailDisplayBoxHeight` int(11) NOT NULL DEFAULT '-1',
                  `FullSizeDisplayBoxWidth` int(11) NOT NULL DEFAULT '-1',
                  `FullSizeDisplayBoxHeight` int(11) NOT NULL DEFAULT '-1',
                  `Caption` varchar(255) NOT NULL,
                  `Credits` varchar(255) NOT NULL,
                  `Tags` varchar(255) NOT NULL DEFAULT '',
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`SingleImageId`),
                  KEY `singleimage_secondary` (`PageId`,`Identifier`,`Deleted`,`langShortCode`) USING BTREE
                ) ENGINE=InnoDB  DEFAULT CHARSET=utf8;
            "));
            
            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("SingleImage.WithLinkTemplate"));
            ret.Add(new CmsConfigItemDependency("SingleImage.WithoutLinkTemplate"));

            ret.Add(new CmsConfigItemDependency("SingleImage.FullSizeDisplayPath"));
            ret.Add(new CmsConfigItemDependency("SingleImage.FullSizeDisplayWidth"));
            ret.Add(new CmsConfigItemDependency("SingleImage.FullSizeDisplayHeight"));

            ret.Add(new CmsConfigItemDependency("SingleImage.CreditsPromptPrefix")); // multilingual
            ret.Add(new CmsConfigItemDependency("SingleImage.ClickToEnlargeText")); // multilingual
            ret.Add(new CmsConfigItemDependency("SingleImage.Tags"));
            ret.Add(new CmsConfigItemDependency("SingleImage.WithLinkTemplate"));
            ret.Add(new CmsConfigItemDependency("SingleImage.WithoutLinkTemplate"));
            ret.Add(new CmsConfigItemDependency("SingleImage.PopupPaddingWidth"));
            ret.Add(new CmsConfigItemDependency("SingleImage.PopupPaddingHeight"));
            ret.Add(new CmsConfigItemDependency("SingleImage.PopupMaxWidth"));
            ret.Add(new CmsConfigItemDependency("SingleImage.PopupMaxHeight"));
            ret.Add(new CmsConfigItemDependency("SingleImage.PopupMinWidth"));
            ret.Add(new CmsConfigItemDependency("SingleImage.PopupMinHeight"));

            // -- REMOVED config entries
            //          thumbnail size is always set in the template
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceholderDefaultThumbBoxWidth", CmsDependency.ExistsMode.MustNotExist));
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceholderDefaultThumbBoxHeight", CmsDependency.ExistsMode.MustNotExist));
            //          full display size is set by SingleImage.FullSizeDisplayWidth
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceholderDefaultFullSizeBoxWidth", CmsDependency.ExistsMode.MustNotExist));
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceholderDefaultFullSizeBoxHeight", CmsDependency.ExistsMode.MustNotExist));

            //          useSubModal and useMultibox now handled using display templates (ThumbDisplayTemplate and FullDisplayTemplate)
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceHolderUseSubModal", CmsDependency.ExistsMode.MustNotExist));
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceHolderUseMultibox", CmsDependency.ExistsMode.MustNotExist));

            //          SingleImageDisplayPath is now SingleImage.FullSizeDisplayPath
            ret.Add(new CmsConfigItemDependency("SingleImageDisplayPath", CmsDependency.ExistsMode.MustNotExist));

            //          SingleImagePlaceHolderCreditsPrefix is now SingleImage.CreditsPromptPrefix
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceHolderCreditsPrefix", CmsDependency.ExistsMode.MustNotExist));
            //          SingleImagePlaceHolderClickToEnlargeText is now SingleImage.ClickToEnlargeText
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceHolderClickToEnlargeText", CmsDependency.ExistsMode.MustNotExist));

            //          SingleImagePlaceHolderPopupPaddingWidth is now SingleImage.PopupPaddingWidth
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceHolderPopupPaddingWidth", CmsDependency.ExistsMode.MustNotExist));
            //          SingleImagePlaceHolderPopupPaddingHeight is now SingleImage.PopupPaddingHeight
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceHolderPopupPaddingHeight", CmsDependency.ExistsMode.MustNotExist));
            //          SingleImagePlaceHolderPopupMaxWidth is now SingleImage.PopupMaxWidth
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceHolderPopupMaxWidth", CmsDependency.ExistsMode.MustNotExist));
            //          SingleImagePlaceHolderPopupMaxWidth is now SingleImage.PopupMaxWidth
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceHolderPopupMaxHeight", CmsDependency.ExistsMode.MustNotExist));
            //          SingleImagePlaceHolderPopupMinHeight is now SingleImage.PopupMinHeight
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceHolderPopupMinHeight", CmsDependency.ExistsMode.MustNotExist));
            ret.Add(new CmsConfigItemDependency("SingleImagePlaceHolderPopupMinWidth", CmsDependency.ExistsMode.MustNotExist));
            
            return ret.ToArray();
        }

        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            SingleImageDb imageDb = new SingleImageDb();
            foreach (int identifier in identifiers)
            {
                SingleImageData oldData = imageDb.getSingleImage(oldPage, identifier, language, false);
                bool b = imageDb.saveUpdatedSingleImage(currentPage, identifier, language, oldData);
                if (!b)
                    return RevertToRevisionResult.Failure;
            } // foreach
            return RevertToRevisionResult.Success;
        }
                
        

        public SingleImage()
		{
			// nothing to construct
		}
				             

        public static string getStandardHtmlView(SingleImageDisplayInfo displayInfo)
        {
            StringBuilder html = new StringBuilder();
            if (displayInfo.ImagePath.Trim() == "")
                return "";

            string thumbUrl = CmsContext.UserInterface.ShowThumbnailPage.getThumbDisplayUrl(displayInfo.ImagePath, displayInfo.ThumbImageDisplayBox);
            System.Drawing.Size imgThumbSize = CmsContext.UserInterface.ShowThumbnailPage.getDisplayWidthAndHeight(displayInfo.ImagePath, displayInfo.ThumbImageDisplayBox);
            System.Drawing.Size imgLargeSize = CmsContext.UserInterface.ShowThumbnailPage.getDisplayWidthAndHeight(displayInfo.ImagePath, displayInfo.FullImageDisplayBox);

            int popWidth = -1;
            int popHeight = -1;
            string popUrl = displayInfo.FullImageDisplayUrl;
            
            string template = displayInfo.ThumbDisplayWithLinkTemplate;
            string clickForLarger = "";
            
            if (imgLargeSize.IsEmpty || displayInfo.PopupDisplayBox.IsEmpty || imgThumbSize.Width > imgLargeSize.Width || imgThumbSize.Height > imgLargeSize.Height)
            {
                template = displayInfo.ThumbDisplayWithoutLinkTemplate;
            }
            else
            {
                popWidth = displayInfo.PopupDisplayBox.Width;
                popHeight = displayInfo.PopupDisplayBox.Height;
                clickForLarger = displayInfo.ClickToEnlargeText;
            }

            string creditPrefix = displayInfo.CreditsPromptPrefix;
            string credits = displayInfo.Credits;
            if (credits.Trim() == "")
                creditPrefix = "";

            /// {0} = image thumbnail width
            /// {1} = image thumbnail height
            /// {2} = image thumbnail URL
            /// {3} = popup page width
            /// {4} = popup page height
            /// {5} = popup page URL
            /// {6} = Caption
            /// {7} = CreditsPrefix
            /// {8} = Credits
            /// {9} = ClickToEnlargeText
            string txt = String.Format(template, imgThumbSize.Width, imgThumbSize.Height, thumbUrl, popWidth, popHeight, popUrl, displayInfo.Caption, creditPrefix, credits, clickForLarger);

            html.Append("<div class=\"SingleImagePlaceholder View\">");
            html.Append(txt);
            html.Append("</div>");

            return html.ToString();
        }
        /*
        public static string getStandardHtmlView_Old(SingleImageData image, FullSizeImageLinkMode fullSizeLinkMode, string fullSizeDisplayUrl)
        {
            StringBuilder html = new StringBuilder();
            if (image.ImagePath != "")
            {
                bool linkToLarger = false;
                if (image.FullSizeDisplayBoxHeight > 0 || image.FullSizeDisplayBoxWidth > 0)
                    linkToLarger = true;

                string thumbUrl = showThumbPage.getThumbDisplayUrl(image.ImagePath, image.ThumbnailDisplayBoxWidth, image.ThumbnailDisplayBoxHeight);
                System.Drawing.Size ThumbSize = showThumbPage.getDisplayWidthAndHeight(image.ImagePath, image.ThumbnailDisplayBoxWidth, image.ThumbnailDisplayBoxHeight);

                html.Append("<div class=\"SingleImagePlaceholder View\">");
                if (linkToLarger)
                {
                    bool useSubmodal = CmsConfig.getConfigValue("SingleImagePlaceHolderUseSubModal", false);
                    bool useMultibox = CmsConfig.getConfigValue("SingleImagePlaceHolderUseMultibox", false);


                    int popupPaddingWidth = CmsConfig.getConfigValue("SingleImagePlaceHolderPopupPaddingWidth", 50);
                    int popupPaddingHeight = CmsConfig.getConfigValue("SingleImagePlaceHolderPopupPaddingHeight", 60);

                    int maxPopWidth = CmsConfig.getConfigValue("SingleImagePlaceHolderPopupMaxWidth", 700 - popupPaddingWidth);
                    int maxPopHeight = CmsConfig.getConfigValue("SingleImagePlaceHolderPopupMaxHeight", 500 - popupPaddingHeight);


                    int minPopWidth = CmsConfig.getConfigValue("SingleImagePlaceHolderPopupMinWidth", 200);
                    int minPopHeight = CmsConfig.getConfigValue("SingleImagePlaceHolderPopupMinHeight", 200);


                    string showLargerPagePath = CmsConfig.getConfigValue("SingleImage.DisplayPath", "/_internal/showImage");

                    NameValueCollection largerParams = new NameValueCollection();
                    largerParams.Add("i", image.SingleImageId.ToString());
                    string showLargerPageUrl = CmsContext.getUrlByPagePath(showLargerPagePath, largerParams);

                    System.Drawing.Size imgLargeSize = showThumbPage.getDisplayWidthAndHeight(image.ImagePath, image.FullSizeDisplayBoxWidth, image.FullSizeDisplayBoxHeight);

                    if (ThumbSize.Width > imgLargeSize.Width || ThumbSize.Height > imgLargeSize.Height)
                    {
                        linkToLarger = false;
                    }
                    else
                    {

                        int popWidth = imgLargeSize.Width + popupPaddingWidth;
                        int popHeight = imgLargeSize.Height + popupPaddingHeight;

                        if (popWidth < minPopWidth)
                            popWidth = minPopWidth;
                        if (popHeight < minPopHeight)
                            popHeight = minPopHeight;

                        if (popWidth > maxPopWidth)
                            popWidth = maxPopWidth;
                        if (popHeight > maxPopHeight)
                            popHeight = maxPopHeight;

                        if (useSubmodal &&
                            (fullSizeLinkMode == FullSizeImageLinkMode.SubModalOrPopupFromConfig || fullSizeLinkMode == FullSizeImageLinkMode.SubModalWindow))
                        {
                            string submodalCssClass = "class=\"submodal-" + popWidth.ToString() + "-" + popHeight.ToString() + "\"";
                            html.Append("<a " + submodalCssClass + " href=\"" + showLargerPageUrl + "\" >");
                        }
                        else if (useMultibox && (fullSizeLinkMode == FullSizeImageLinkMode.SubModalOrPopupFromConfig || fullSizeLinkMode == FullSizeImageLinkMode.SubModalWindow))
                        {
                            string submodalCssClass = "class=\"mb\"";
                            html.Append("<a " + submodalCssClass + " href=\"" + showLargerPageUrl + "\" rel=\"width:" + popWidth + ",height:" + popHeight + "\" >");
                        }
                        else if (fullSizeLinkMode == FullSizeImageLinkMode.SingleImagePopup || fullSizeLinkMode == FullSizeImageLinkMode.SubModalOrPopupFromConfig)
                        {                            
                            string onclick = "var w = window.open(this.href, 'popupLargeImage', 'toolbar=no,menubar=no,resizable=yes,scrollbars=yes,status=yes,height=" + popWidth.ToString() + ",width=" + popWidth.ToString() + "'); ";
                            onclick += " return false;";
                            html.Append("<a href=\"" + showLargerPageUrl + "\" onclick=\"" + onclick + "\">");
                        }
                        else if (fullSizeLinkMode == FullSizeImageLinkMode.ProvidedUrl)
                            html.Append("<a href=\"" + fullSizeDisplayUrl + "\">");
                        else 
                            linkToLarger = false;
                    } // else                    
                } // if link to larger
                
                string width = "";
                string height = "";
                if (!ThumbSize.IsEmpty)
                {
                    width = " width=\"" + ThumbSize.Width + "\"";
                    height = " height=\"" + ThumbSize.Height.ToString() + "\"";
                }

                html.Append("<img src=\"" + thumbUrl + "\"" + width + "" + height + ">");
                if (linkToLarger)
                {
                    html.Append("</a>");
                }

                if (image.Caption.Trim() != "")
                {
                    html.Append("<div class=\"caption\">");
                    html.Append(image.Caption);
                    html.Append("</div>"); // caption
                }

                if (image.Credits.Trim() != "")
                {
                    html.Append("<div class=\"credits\">");
                    string creditsPrefix = CmsConfig.getConfigValue("SingleImage.CreditsPrefix", "");
                    html.Append(creditsPrefix + image.Credits);
                    html.Append("</div>"); // credits
                }

                if (linkToLarger)
                {
                    string clickToEnlargeText = CmsConfig.getConfigValue("SingleImage.ClickToEnlargeText", "");
                    if (clickToEnlargeText != "")
                    {
                        html.Append("<div class=\"clickToEnlarge\">");
                        html.Append(clickToEnlargeText);
                        html.Append("</div>"); // clickToEnlarge
                    }
                }

                html.Append("</div>");
            }

            return html.ToString();
        }
         */

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            // -- all rendering in View mode is handled by getStandardHtmlView()
            SingleImageDb db = (new SingleImageDb());
            SingleImageData image = db.getSingleImage(page, identifier, langToRenderFor, true);
            
            int fullWidth = CmsConfig.getConfigValue("SingleImage.FullSizeDisplayWidth", -1);
            int fullHeight = CmsConfig.getConfigValue("SingleImage.FullSizeDisplayHeight", -1);

            int thumbWidth = getThumbDisplayWidth(page, paramList);
            int thumbHeight = getThumbDisplayHeight(page, paramList);

            int popupPaddingWidth = CmsConfig.getConfigValue("SingleImage.PopupPaddingWidth", 50);
            int popupPaddingHeight = CmsConfig.getConfigValue("SingleImage.PopupPaddingHeight", 60);

            int maxPopWidth = CmsConfig.getConfigValue("SingleImage.PopupMaxWidth", 700 - popupPaddingWidth);
            int maxPopHeight = CmsConfig.getConfigValue("SingleImage.PopupMaxHeight", 500 - popupPaddingHeight);


            int minPopWidth = CmsConfig.getConfigValue("SingleImage.PopupMinWidth", 200);
            int minPopHeight = CmsConfig.getConfigValue("SingleImage.PopupMinHeight", 200);

            string withLinkTemplate = CmsConfig.getConfigValue("SingleImage.WithLinkTemplate", "<a href=\"{5}\"><img src=\"{2}\" width=\"{0}\" height=\"{1}\" /></a>");
            string withoutLinkTemplate = CmsConfig.getConfigValue("SingleImage.WithoutLinkTemplate", "<img src=\"{2}\" width=\"{0}\" height=\"{1}\" />");

            string showLargerPagePath = CmsConfig.getConfigValue("SingleImage.DisplayPath", "/_internal/showImage");

            NameValueCollection largerParams = new NameValueCollection();
            largerParams.Add("i", image.SingleImageId.ToString());
            string showLargerPageUrl = CmsContext.getUrlByPagePath(showLargerPagePath, largerParams);

            System.Drawing.Size imgLargeSize = CmsContext.UserInterface.ShowThumbnailPage.getDisplayWidthAndHeight(image.ImagePath, fullWidth, fullHeight);

            int popWidth = imgLargeSize.Width + popupPaddingWidth;
            int popHeight = imgLargeSize.Height + popupPaddingHeight;

            if (popWidth < minPopWidth)
                popWidth = minPopWidth;
            if (popHeight < minPopHeight)
                popHeight = minPopHeight;

            if (popWidth > maxPopWidth)
                popWidth = maxPopWidth;
            if (popHeight > maxPopHeight)
                popHeight = maxPopHeight;


            // -- create the SingleImageDisplayInfo object
            SingleImageDisplayInfo displayInfo = new SingleImageDisplayInfo();
            displayInfo.ImagePath = image.ImagePath;

            displayInfo.FullImageDisplayBox = new System.Drawing.Size(fullWidth, fullHeight);
            displayInfo.ThumbImageDisplayBox = new System.Drawing.Size(thumbWidth, thumbHeight);
            displayInfo.PopupDisplayBox = new System.Drawing.Size(popWidth, popHeight);

            displayInfo.FullImageDisplayUrl = showLargerPageUrl;

            displayInfo.ThumbDisplayWithLinkTemplate = withLinkTemplate;
            displayInfo.ThumbDisplayWithoutLinkTemplate = withoutLinkTemplate;

            displayInfo.Caption = image.Caption;
            displayInfo.Credits = image.Credits;

            // -- Multilingual CreditsPromptPrefix
            string creditPrefix = CmsConfig.getConfigValue("SingleImage.CreditsPromptPrefix", "");
            string[] creditPrefixParts = creditPrefix.Split(new char[] {CmsConfig.PerLanguageConfigSplitter},  StringSplitOptions.RemoveEmptyEntries);
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

            string html = getStandardHtmlView(displayInfo);
            writer.WriteLine(html.ToString());

        } // RenderView
        
        

        private int getThumbDisplayWidth(CmsPage page, string[] paramList)
        {
            int width = Int32.MinValue;
            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                width = PlaceholderUtils.getParameterValue("width", width, paramList);

                // -- do checks of template parameters that are no longer used (and should be removed from templates).
                string notFound = Guid.NewGuid().ToString();
                string popupWidth = PlaceholderUtils.getParameterValue("popupWidth", notFound, paramList);
                if (popupWidth != notFound)
                    throw new TemplateExecutionException(page.TemplateName, "SingleImage placeholders should no longer have a \"popupWidth\" parameter.");
                string popupHeight = PlaceholderUtils.getParameterValue("popupHeight", notFound, paramList);
                if (popupHeight != notFound)
                    throw new TemplateExecutionException(page.TemplateName, "SingleImage placeholders should no longer have a \"popupHeight\" parameter.");
                string tags = PlaceholderUtils.getParameterValue("tags", notFound, paramList);
                if (tags != notFound)
                    throw new TemplateExecutionException(page.TemplateName, "SingleImage placeholders should no longer have a \"tags\" parameter.");

            }
            else
            {
                throw new ArgumentException("Invalid CmsTemplateEngineVersion");
            }

            if (width < -1)
                throw new TemplateExecutionException(page.TemplateName, "SingleImage placeholder must have a \"width\" parameter.");

            return width;
        }

        private int getThumbDisplayHeight(CmsPage page, string[] paramList)
        {
            int forceDefaultThumbHeight = Int32.MinValue;
            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                forceDefaultThumbHeight = PlaceholderUtils.getParameterValue("height", forceDefaultThumbHeight, paramList);
            }
            else
            {
                throw new ArgumentException("Invalid CmsTemplateEngineVersion");
            }

            if (forceDefaultThumbHeight < -1)
                throw new TemplateExecutionException(page.TemplateName, "SingleImage placeholder must have a \"height\" parameter.");
            return forceDefaultThumbHeight;
        }

        private string getEditFormName(CmsPage page, int identifier, CmsLanguage langToRenderFor)
        {
            return "editSingleImage_" + page.Id.ToString() + identifier.ToString() + langToRenderFor.shortCode;           
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {

            string formName = getEditFormName(page, identifier, langToRenderFor);           

            SingleImageDb db = (new SingleImageDb());
            SingleImageData image = db.getSingleImage(page, identifier, langToRenderFor, true);

            string[] possibleTags = (new SingleImageDb()).getAllPossibleTags();

            

            StringBuilder html = new StringBuilder();
            // ------- CHECK THE FORM FOR ACTIONS
            string action = Hatfield.Web.Portal.PageUtils.getFromForm(formName + "_SingleImageAction", "");
            if (action.Trim().ToLower() == "saveimage")
            {
                image.ImagePath = PageUtils.getFromForm(formName + "ImagePath", "");
                // remove the Application path from the file location
                if (image.ImagePath.StartsWith(CmsContext.ApplicationPath))
                    image.ImagePath = image.ImagePath.Substring(CmsContext.ApplicationPath.Length);

                
                image.Caption = PageUtils.getFromForm(formName + "Caption", "");
                image.Credits = PageUtils.getFromForm(formName + "Credits", "");

                image.Tags = PageUtils.getFromForm(formName + "Tags");

                bool b = db.saveUpdatedSingleImage(page, identifier, langToRenderFor, image);
                if (!b)
                {
                    html.Append("Error: Image not saved - database error");
                }
            }

            string creditsPrefix = CmsConfig.getConfigValue("SingleImage.CreditsPrefix", "Credit:");

            int thumbWidth = getThumbDisplayWidth(page, paramList);
            int thumbHeight = getThumbDisplayHeight(page, paramList);

            // ------- START RENDERING            
                        
            // note: no need to put in the <form></form> tags.                        

            html.Append("<div class=\"SingleImagePlaceholder Edit\">"+Environment.NewLine);
            string ImageRenderAreaId = formName + "ImageRendered";

            html.Append("<div id=\"" + ImageRenderAreaId + "\"></div>");
            html.Append(PageUtils.getHiddenInputHtml(formName + "ImagePath", formName + "ImagePath", image.ImagePath));

            List<string> editParams = new List<string>();
            editParams.Add("i=" + image.SingleImageId);
            editParams.Add("formName=" + formName);
            editParams.Add("tw=" + thumbWidth.ToString());
            editParams.Add("th=" + thumbHeight.ToString());                        

            string editUrl = CmsContext.ApplicationPath + "_system/tools/SingleImage/SingleImageEditor.aspx?" + string.Join("&", editParams.ToArray()); ;
            string jsResetFunctionName = formName + "Reset";
            string jsUpdateRenderFunctionName = formName + "UpdateDisplay";

            html.Append("<table>");
            html.Append("<tr>" + Environment.NewLine);
            html.Append("<td colspan=\"2\">");            
            html.Append("<p>");
            html.Append("<a id=\""+formName+"OpenEditorLink\" href=\"" + editUrl + "\" onclick=\"window.open(this.href, 'SingleImageEdit','width=680,height=540'); return false;\">select image</a>");
            html.Append(" | ");
            html.Append("<a href=\"#\" onclick=\"" + jsResetFunctionName + "(); return false;\">clear</a>");
            if (CmsConfig.Languages.Length > 1)
            {
                html.Append(" | ");
                html.Append("copy from language: ");
                List<string> langCopyLinks = new List<string>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    
                    if (lang != langToRenderFor)
                    {
                        string langClick = "document.getElementById('" + formName + "ImagePath').value = document.getElementById('" + getEditFormName(page, identifier, lang) + "ImagePath').value;" + jsUpdateRenderFunctionName + "(); if (document.getElementById('" + formName + "ImagePath').value == '') {alert('No image is selected because the source image is blank.');} return false;";
                        langCopyLinks.Add("<a href=\"#\" onclick=\"" + langClick + "\">" + lang.shortCode + "</a>");
                    }
                } // foreach
                html.Append(string.Join("; ", langCopyLinks.ToArray()));

            }
            html.Append("</p>");
            html.Append("</td>");
            html.Append("</tr>"+Environment.NewLine);


            html.Append("<tr><td>");
            html.Append("Caption:");
            html.Append("</td><td class=\"CaptionInputTD\">");
            html.Append(PageUtils.getInputTextHtml(formName + "Caption", formName + "Caption", image.Caption.ToString(), 40, 250));
            html.Append("</td></tr>");


            html.Append("<tr><td>");
            html.Append(creditsPrefix);
            html.Append("</td><td class=\"CreditInputTD\">");
            html.Append(PageUtils.getInputTextHtml(formName + "Credits", formName + "Credits", image.Credits.ToString(), 40, 250));
            html.Append("</td></tr>" + Environment.NewLine);

            if (possibleTags.Length > 0)
            {
                html.Append("<tr><td>");
                html.Append("Tags:");
                html.Append("</td><td>");
                foreach (string t in possibleTags)
                {
                    if (t != "")
                    {
                        html.Append(PageUtils.getCheckboxHtml(t.Trim(), formName + "Tags", formName+"tag_"+t, t.Trim(), Array.IndexOf(image.Tags,t) > -1));
                        html.Append("<br />");
                    }
                } // foreach
                html.Append("</td></tr>");
            }

            html.Append("</table>");
            html.Append("</div>" + Environment.NewLine);
            // -- hidden field actions
            html.Append("<input type=\"hidden\" name=\"" + formName + "_SingleImageAction\" value=\"saveImage\">");

            // -- javascript                         

            StringBuilder js = new StringBuilder();            

            js.Append("function " + jsResetFunctionName + "() " + Environment.NewLine);
            js.Append("{ " + Environment.NewLine);
            js.Append("document.getElementById('" + formName + "ImagePath').value = '';" + Environment.NewLine);            
            js.Append("document.getElementById('" + ImageRenderAreaId + "').innerHTML = '<span style=\"margin: 10px; padding: 10px; background: #edff96; border: 1px solid #C00;\">no image is selected</span>';" + Environment.NewLine);
            js.Append("} " + Environment.NewLine);

            js.Append("function " + jsUpdateRenderFunctionName + "() " + Environment.NewLine);
            js.Append("{ " + Environment.NewLine);
            js.Append(" var ImagePath = document.getElementById('" + formName + "ImagePath').value;" + Environment.NewLine);
            js.Append(" if (ImagePath == '') { " + Environment.NewLine);
            js.Append("   document.getElementById('" + ImageRenderAreaId + "').innerHTML = '<span style=\"margin: 10px; padding: 10px; background: #edff96; border: 1px solid #C00;\">no image is selected</span>';" + Environment.NewLine);
            js.Append(" } // if " + Environment.NewLine);
            js.Append(" else { " + Environment.NewLine);
            js.Append("   document.getElementById('" + ImageRenderAreaId + "').innerHTML = '<img src=\"" + CmsContext.ApplicationPath + "_system/tools/showThumb.aspx?file='+ImagePath+'&w=" + thumbWidth + "&h="+thumbHeight+"&hatCmsNocache='+((new Date()).getTime())+'\">';" + Environment.NewLine);
            js.Append(" } // else " + Environment.NewLine);
            js.Append(" document.getElementById('" + formName + "OpenEditorLink').href = '" + editUrl + "&SelImagePath='+ImagePath;" + Environment.NewLine);
            js.Append("} " + Environment.NewLine + Environment.NewLine);

            // add javascript to head section
            page.HeadSection.AddJSStatements(js.ToString());
            page.HeadSection.AddJSOnReady(jsUpdateRenderFunctionName + "();");            
            
            

            writer.WriteLine(html.ToString());

        }

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            Rss.RssItem rssItem = base.CreateAndInitRssItem(page, langToRenderFor);
            
            string content = page.renderPlaceholderToString(placeholderDefinition, langToRenderFor, CmsPage.RenderPlaceholderFilterAction.RunAllPageAndPlaceholderFilters);
            if (content.Trim() != "")
            {
                rssItem.Description = content;

                return new Rss.RssItem[] { rssItem };
            }
            return new Rss.RssItem[0];
        }

	}
}


