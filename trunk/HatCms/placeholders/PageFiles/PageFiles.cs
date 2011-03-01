using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
   
    public class PageFiles: BaseCmsPlaceholder
    {
        public static string CurrentFileIdFormName
        {
            get
            {
                return "fileid";
            }
        }
        
        public enum RenderMode { ListFiles, SingleFile };
        public static RenderMode currentViewRenderMode
        {
            get
            {
                int rm = PageUtils.getFromForm(CurrentFileIdFormName, Int32.MinValue);
                if (rm < 0)
                    return RenderMode.ListFiles;
                else
                    return RenderMode.SingleFile;
            }
        }

#region Multi-lang get method

        protected string getNoFilesText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.NoFilesText", "This area does not contain any files", lang);
        }

        protected string getNameText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.NameText", "Name", lang);
        }

        protected string getSizeText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.SizeText", "Size", lang);
        }

        protected string getTypeText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.TypeText", "Type", lang);
        }

        protected string getPostedText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.PostedText", "Posted", lang);
        }

        protected string getBackToFileListingText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.BackToFileListingText", "Back to file listing", lang);
        }

        protected string getDownloadText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.DownloadText", "Download file", lang);
        }

        protected string getLinkOpensNewWindowText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.LinkOpensNewWindow", "Link opens in new window", lang);
        }

        protected string getFileText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.FileText", "File", lang);
        }

        protected string getLastUpdatedText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.LastUpdatedText", "Last updated", lang);
        }

        protected string getTitleText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.TitleText", "Title", lang);
        }

        protected string getAuthorText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.AuthorText", "Title", lang);
        }

        protected string getDocumentAbstractText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.DocumentAbstractText", "Document abstract", lang);
        }

        protected string getImagePreviewText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.ImagePreviewText", "Image preview", lang);
        }

        protected string getAddFileText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.AddFileText", "Add a file", lang);
        }

        protected string getPageXofYText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.PageXofYText", "Page {0} of {1}", lang);
        }

        protected string getPrevLinkText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.PrevLinkText", "Previous", lang);
        }

        protected string getNextLinkText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.NextLinkText", "Next", lang);
        }

        protected string getUploadButtonText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.UploadButtonText", "Upload file", lang);
        }

        protected string getMaxFileSizeText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.MaxFileSizeText", "Maximum file size", lang);
        }

        protected string getSaveButtonText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.SaveButtonText", "Save changes to file", lang);
        }

        protected string getDeleteFileText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("PageFiles.DeleteFileText", "Delete file", lang);
        }
#endregion

        /// <summary>
        /// if not found, returns a new PageFilesItemData object with ID = -1.
        /// </summary>
        /// <returns></returns>
        public static PageFilesItemData getCurrentPageFilesItemData()
        {
            int fileId = PageUtils.getFromForm(CurrentFileIdFormName, Int32.MinValue);
            if (fileId > -1)
            {
                return (new PageFilesDb()).getPageFilesItemData(fileId);
            }
            return new PageFilesItemData();
        }

        public static bool isPageFilesPage(CmsPage page)
        {
            return (StringUtils.IndexOf(page.getAllPlaceholderNames(), "PageFiles", StringComparison.CurrentCultureIgnoreCase) > -1);
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsConfigItemDependency("DMSFileStorageLocationVersion"));

            ret.Add(new CmsDatabaseTableDependency("pagefiles", new string[] {
                "PageFilesId", "PageId", "Identifier", "langShortCode", "sortDirection", "sortColumn", "langShortCode", "tabularDisplayLinkMode",
                "numFilesToShowPerPage", "accessLevelToAddFiles", "accessLevelToEditFiles", "deleted"
                }));
            ret.Add(new CmsDatabaseTableDependency("pagefileitem", new string[] {
                "PageFileItemId", "PageId", "Identifier", "langShortCode", "Filename", "Title", "Author", "Abstract", "FileSize", "LastModified", "CreatorUsername", "Deleted"
                }));
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("_system/writable/DMSStorage"));

            ret.AddRange(SWFUploadHelpers.SWFUploadDependencies);

            ret.Add(CmsFileDependency.UnderAppPath("images/_system/fileIcons/16x16/doc.gif"));
            ret.Add(CmsFileDependency.UnderAppPath("images/_system/fileIcons/16x16/pdf.gif"));
            ret.Add(CmsFileDependency.UnderAppPath("images/_system/fileIcons/16x16/xls.gif"));

            ret.Add(CmsFileDependency.UnderAppPath("images/_system/fileIcons/32x32/doc.gif"));
            ret.Add(CmsFileDependency.UnderAppPath("images/_system/fileIcons/32x32/pdf.gif"));
            ret.Add(CmsFileDependency.UnderAppPath("images/_system/fileIcons/32x32/xls.gif"));

            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("PageFiles.NoFilesText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.NameText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.SizeText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.TypeText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.PostedText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.BackToFileListingText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.DownloadText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.LinkOpensNewWindow"));
            ret.Add(new CmsConfigItemDependency("PageFiles.FileText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.LastUpdatedText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.TitleText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.AuthorText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.DocumentAbstractText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.ImagePreviewText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.PageXofYText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.PrevLinkText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.NextLinkText"));

            ret.Add(new CmsConfigItemDependency("PageFiles.AddFileText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.UploadButtonText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.MaxFileSizeText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.SaveButtonText"));
            ret.Add(new CmsConfigItemDependency("PageFiles.DeleteFileText"));

            return ret.ToArray();
        }

        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }                

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            if (currentViewRenderMode == RenderMode.SingleFile)
                RenderViewSingleFile(writer, page, identifier, langToRenderFor, paramList);
            else
                RenderViewSummary(writer, page, identifier, langToRenderFor, paramList);	
        }

        public void RenderViewSingleFile(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            PageFilesDb db = new PageFilesDb();

                       
            int fileId = PageUtils.getFromForm(CurrentFileIdFormName, -1);
            if (fileId < 0)
            {
                writer.Write("Error: invalid fileId specified");
                return;
            }
             
            PageFilesItemData fileData = db.getPageFilesItemData(fileId);
            

            if (fileData.Id < 0)
            {
                writer.Write("Error: invalid fileId specified");
                return;
            }

            PageFilesPlaceholderData phData = db.getPageFilesData(page, identifier, langToRenderFor, true);
            if (phData.tabularDisplayLinkMode == PageFilesPlaceholderData.TabularDisplayLinkMode.LinkToFile && ! page.currentUserCanWrite)
                throw new CmsPlaceholderNeedsRedirectionException(fileData.getDownloadUrl(page, identifier, langToRenderFor));

            string ControlId = "PageFiles_"+page.ID.ToString()+"_"+identifier.ToString();
            bool userCanEditFile = page.currentUserCanWrite;

            string _userMessage = "";
            string _userErrorMessage = "";
            bool _showFileDetails = true;

            string formAction = PageUtils.getFromForm(ControlId + "action","");
            if (String.Compare(formAction, "updateFile", true) == 0 && userCanEditFile)
            {
                // -- process newly submitted values
                fileData.Title = PageUtils.getFromForm(ControlId + "Title", fileData.Title);
                fileData.Author = PageUtils.getFromForm(ControlId + "Author", fileData.Author);
                fileData.Abstract = PageUtils.getFromForm(ControlId + "Abstract", fileData.Abstract);                
                fileData.lastModified = DateTime.Now;

                bool b = db.saveUpdatedPageFilesItemData(fileData);
                if (b)
                    _userMessage = "<font color=\"green\">The changes to the file have been saved</font>";
                else
                    _userMessage = "<font color=\"red\">Error saving changes to the file - database error.</font>";
            }
            else if (String.Compare(formAction, "deleteFile", true) == 0 && userCanEditFile)
            {

                if (fileData.Id >= 0)
                {
                    string origFilename = fileData.Filename;
                    bool b = db.deletePageFilesItemData(fileData, page, identifier, langToRenderFor);
                    if (!b)
                        _userErrorMessage = "There was a problem deleting the file. Please try again.";
                    else
                    {
                        _userMessage = "The file \"" + origFilename + "\" has been deleted.";
                        _showFileDetails = false;
                    }
                } // if
                else
                {
                    _userErrorMessage = "No file was specified to delete.";
                }
            }

            // -- render HTML
            StringBuilder html = new StringBuilder();
            string fileTypeDescription = getFileTypeName(System.IO.Path.GetExtension(fileData.Filename));
            string iconUrl = getLargeIconUrl(fileData);
            

            string backUrl = page.Url;
            html.Append("<p><a class=\"backToPrev\" href=\"" + backUrl + "\">" + getBackToFileListingText(langToRenderFor) + "</a></p>");

            string formId = "PageFiles";
            if (userCanEditFile && _showFileDetails)
            {
                html.Append(page.getFormStartHtml(formId));
            }

            if (_userMessage != "")
                html.Append("<p style=\"color: green;\">" + _userMessage + "</p>");
            if (_userErrorMessage != "")
                html.Append("<p style=\"color: red;\">" + _userErrorMessage + "</p>");

            if (_showFileDetails)
            {
                html.Append("<table border=\"0\">");
                html.Append("<tr><td valign=\"top\" align=\"center\" width=\"150\">");
                html.Append("<a href=\"" + fileData.getDownloadUrl(page, identifier, langToRenderFor) + "\" target=\"_blank\">");
                html.Append("<img src=\"" + iconUrl + "\" border=\"0\" width=\"32\" height=\"32\">");
                html.Append("</a>");
                html.Append("<br>");
                html.Append("<a class=\"downloadLink\" href=\"" + fileData.getDownloadUrl(page, identifier, langToRenderFor) + "\" target=\"_blank\">");
                html.Append(getDownloadText(langToRenderFor));
                html.Append("</a>");
                html.Append("<br><span style=\"font-style: italic; font-size: smaller;\">(" + getLinkOpensNewWindowText(langToRenderFor) + ")</span>");
                html.Append("<p>");
                html.Append(fileTypeDescription);
                html.Append("<br>" + StringUtils.formatFileSize(fileData.FileSize));
                html.Append("</td>");
                html.Append("<td valign=\"top\">");

                html.Append("<table border=\"0\">");

                html.Append("<tr>");
                html.Append("<td valign=\"top\">" + getFileText(langToRenderFor) + ":</td>");
                html.Append("<td>");
                string displayName = fileData.Title;
                if (displayName.Trim() == "")
                    displayName = fileData.Filename;
                html.Append("<a href=\"" + fileData.getDownloadUrl(page, identifier, langToRenderFor) + "\" target=\"_blank\">" + displayName + "</a>");
                html.Append("</td>");
                html.Append("</tr>");

                html.Append("<tr>");
                html.Append("<td valign=\"top\">" + getLastUpdatedText(langToRenderFor) + ":</td>");
                html.Append("<td>");
                html.Append(fileData.lastModified.ToString("MMMM d yyyy h:mm tt"));
                html.Append("</td>");
                html.Append("</tr>");


                if (userCanEditFile)
                {
                    html.Append("<tr>");
                    html.Append("<td valign=\"top\">" + getTitleText(langToRenderFor) + ":</td>");
                    html.Append("<td>");
                    html.Append(PageUtils.getInputTextHtml(ControlId + "Title", ControlId + "Title", fileData.Title, 40, 255));
                    html.Append("</td>");
                    html.Append("</tr>");

                    html.Append("<tr>");
                    html.Append("<td valign=\"top\">" + getAuthorText(langToRenderFor) + ":</td>");
                    html.Append("<td>");
                    html.Append(PageUtils.getInputTextHtml(ControlId + "Author", ControlId + "Author", fileData.Author, 40, 255));
                    html.Append("</td>");
                    html.Append("</tr>");

                    html.Append("<tr>");
                    html.Append("<td valign=\"top\">" + getDocumentAbstractText(langToRenderFor) + ":</td>");
                    html.Append("<td>");
                    html.Append(PageUtils.getTextAreaHtml(ControlId + "Abstract", ControlId + "Abstract", fileData.Abstract, 30, 5));
                    html.Append("</td>");
                    html.Append("</tr>");

                }
                else
                {
                    // -- just view the data
                    html.Append("<tr>");
                    html.Append("<td valign=\"top\">" + getTitleText(langToRenderFor) + ":</td>");
                    html.Append("<td>");
                    html.Append(displayName);
                    html.Append("</td>");
                    html.Append("</tr>");


                    html.Append("<tr>");
                    html.Append("<td valign=\"top\">" + getAuthorText(langToRenderFor) + ":</td>");
                    html.Append("<td>");
                    html.Append(fileData.Author);
                    html.Append("</td>");
                    html.Append("</tr>");

                    html.Append("<tr>");
                    html.Append("<td valign=\"top\">" + getDocumentAbstractText(langToRenderFor) + ":</td>");
                    html.Append("<td>");
                    html.Append(fileData.AbstractHtml);
                    html.Append("</td>");
                    html.Append("</tr>");

                } // if user can not edit this file

                if (fileTypeDescription.EndsWith("graphic", StringComparison.CurrentCultureIgnoreCase))
                {
                    string imgPreviewUrl = showThumbPage.getThumbDisplayUrl(fileData.getDownloadUrl(page, identifier, langToRenderFor), 200, -1);
                    html.Append("<tr>");
                    html.Append("<td valign=\"top\">" + getImagePreviewText(langToRenderFor) + ":</td>");
                    html.Append("<td>");
                    html.Append("<a href=\"" + fileData.getDownloadUrl(page, identifier, langToRenderFor) + "\"><img border=\"0\" src=\"" + imgPreviewUrl + "\"></a>");
                    html.Append("</td>");
                    html.Append("</tr>");
                }
                html.Append("</table>");
                html.Append("</td></table>");

                if (userCanEditFile)
                {
                    html.Append(PageUtils.getHiddenInputHtml(CurrentFileIdFormName, fileData.Id.ToString()));
                    html.Append(PageUtils.getHiddenInputHtml(ControlId + "action", "updateFile"));
                    html.Append("<input type=\"submit\" value=\"" + getSaveButtonText(langToRenderFor) + "\">");
                    html.Append(page.getFormCloseHtml(formId));

                    formId = formId + "_Delete";
                    html.Append(page.getFormStartHtml(formId));
                    html.Append("<p align=\"right\">" + getDeleteFileText(langToRenderFor) + ":");
                    html.Append(PageUtils.getHiddenInputHtml(CurrentFileIdFormName, fileData.Id.ToString()));
                    html.Append(PageUtils.getHiddenInputHtml(ControlId + "action", "deleteFile"));
                    html.Append("<input type=\"submit\" value=\"" + getDeleteFileText(langToRenderFor) + "\">");
                    html.Append("</p>");
                    html.Append(page.getFormCloseHtml(formId));
                }
            } // if _showFileDetails

            writer.Write(html.ToString());


        } // RenderViewSingleFile

        private string getSmallIconUrl(PageFilesItemData item)
        {
            string ext = System.IO.Path.GetExtension(item.Filename).ToLower();
            string ret = "default.icon.gif";
            switch (ext)
            {
                case ".ai": ret = "ai.gif"; break;
                case ".avi": ret = "avi.gif"; break;
                case ".bmp": ret = "bmp.gif"; break;
                case ".cs": ret = "cs.gif"; break;
                case ".dll": ret = "dll.gif"; break;
                case ".doc": ret = "doc.gif"; break;
                case ".exe": ret = "exe.gif"; break;
                case ".fla": ret = "fla.gif"; break;
                case ".gif": ret = "gif.gif"; break;
                case ".htm": ret = "htm.gif"; break;
                case ".html": ret = "html.gif"; break;
                case ".jpg": ret = "jpg.gif"; break;
                case ".js": ret = "js.gif"; break;
                case ".mdb": ret = "mdb.gif"; break;
                case ".mp3": ret = "mp3.gif"; break;
                case ".pdf": ret = "pdf.gif"; break;
                case ".png": ret = "png.gif"; break;
                case ".ppt": ret = "ppt.gif"; break;
                case ".rdp": ret = "rdp.gif"; break;
                case ".swf": ret = "swf.gif"; break;
                case ".swt": ret = "swt.gif"; break;
                case ".txt": ret = "txt.gif"; break;
                case ".vsd": ret = "vsd.gif"; break;
                case ".xls": ret = "xls.gif"; break;
                case ".xml": ret = "xml.gif"; break;
                case ".zip": ret = "zip.gif"; break;

            } // switch

            string url = CmsContext.ApplicationPath + "images/_system/fileIcons/16x16/" + ret;

            return url;
        }

        private string getLargeIconUrl(PageFilesItemData item)
        {
            string ext = System.IO.Path.GetExtension(item.Filename).ToLower();
            string ret = "default.icon.gif";
            switch (ext)
            {
                case ".ai": ret = "ai.gif"; break;
                case ".avi": ret = "avi.gif"; break;
                case ".bmp": ret = "bmp.gif"; break;
                case ".cs": ret = "cs.gif"; break;
                case ".dll": ret = "dll.gif"; break;
                case ".doc": ret = "doc.gif"; break;
                case ".docx": ret = "doc.gif"; break;
                case ".docm": ret = "doc.gif"; break;
                case ".dot": ret = "doc.gif"; break;
                case ".dotx": ret = "doc.gif"; break;
                case ".dotm": ret = "doc.gif"; break;

                case ".exe": ret = "exe.gif"; break;
                case ".fla": ret = "fla.gif"; break;
                case ".gif": ret = "gif.gif"; break;
                case ".htm": ret = "htm.gif"; break;
                case ".html": ret = "html.gif"; break;
                case ".jpg": ret = "jpg.gif"; break;
                case ".js": ret = "js.gif"; break;
                case ".mdb": ret = "mdb.gif"; break;
                case ".mp3": ret = "mp3.gif"; break;
                case ".pdf": ret = "pdf.gif"; break;
                case ".png": ret = "png.gif"; break;
                case ".ppt": ret = "ppt.gif"; break;
                case ".rdp": ret = "rdp.gif"; break;
                case ".swf": ret = "swf.gif"; break;
                case ".swt": ret = "swt.gif"; break;
                case ".txt": ret = "txt.gif"; break;
                case ".vsd": ret = "vsd.gif"; break;
                case ".xls": ret = "xls.gif"; break;
                case ".xml": ret = "xml.gif"; break;
                case ".zip": ret = "zip.gif"; break;

            } // switch

            string url = CmsContext.ApplicationPath + "images/_system/fileIcons/32x32/" + ret;

            return url;
        }

        private string getFileTypeName(string fileExtension)
        {
            return FileUtils.getFileTypeDescription(fileExtension);
            
        }

        public string getSummaryViewSortedTableUrl(CmsPage page, PageFilesPlaceholderData data, PageFilesPlaceholderData.SortColumn columnToSort, int pageNumber)
        {
            NameValueCollection pageParams = new NameValueCollection();
            string sort = Enum.GetName(typeof(PageFilesPlaceholderData.SortColumn), columnToSort);
            pageParams.Add("sort", sort);
            if (columnToSort == data.sortColumn)
            {
                if (data.sortDirection == PageFilesPlaceholderData.SortDirection.Ascending)
                    pageParams.Add("sortdir", Enum.GetName(typeof(PageFilesPlaceholderData.SortDirection), PageFilesPlaceholderData.SortDirection.Descending));
                else
                    pageParams.Add("sortdir", Enum.GetName(typeof(PageFilesPlaceholderData.SortDirection), PageFilesPlaceholderData.SortDirection.Ascending));
            }

            int startAt = (pageNumber - 1) * data.numFilesToShowPerPage;
            pageParams.Add("fileStart",startAt.ToString());

            string url = CmsContext.getUrlByPagePath(page.Path, pageParams);
            return url;
        }

        public string getSummaryViewSortedTableUrl(CmsPage page, PageFilesPlaceholderData data, int pageNumber)
        {
            NameValueCollection pageParams = new NameValueCollection();

            int startAt = (pageNumber - 1) * data.numFilesToShowPerPage;
            pageParams.Add("fileStart", startAt.ToString());

            string url = CmsContext.getUrlByPagePath(page.Path, pageParams);
            return url;
        }

        protected string getPagerOutput(PageFilesPlaceholderData data,PageFilesItemData[] fileItems, CmsPage page, CmsLanguage lang)
        {
            if (data.numFilesToShowPerPage < 1 || fileItems.Length < data.numFilesToShowPerPage )
                return "";

            string html = "";

            int startAtItemNumber = PageUtils.getFromForm("fileStart", 0);

            html += "<div class=\"pager\">";
            int numPages = (int)Math.Ceiling((double)fileItems.Length / data.numFilesToShowPerPage);
            if (numPages <= 0)
                numPages = 1;

            int currPageNum = (int)Math.Ceiling((double)startAtItemNumber / data.numFilesToShowPerPage) + 1;

            if (currPageNum > 1 && numPages > 1)
            {
                html += "<a href=\"" + getSummaryViewSortedTableUrl(page, data, currPageNum - 1) + "\">";
                html += "&laquo; " + getPrevLinkText(lang);
                html += "</a> ";
            }

            string pageXofY = String.Format(getPageXofYText(lang), new string[] { currPageNum.ToString(), numPages.ToString() });
            html += pageXofY;

            if (currPageNum < numPages && numPages > 1)
            {
                html += " <a href=\"" + getSummaryViewSortedTableUrl(page, data, currPageNum + 1) + "\">";
                html += getNextLinkText(lang) + " &raquo;";
                html += "</a> ";
            }

            html += "</div>";

           return (html);
        } // OutputPager

        /*
        private bool currentUserCanEditFile(PageFilesPlaceholderData data, PageFilesItemData fileData)
        {
            if (CmsContext.currentUserIsSuperAdmin) // SuperAdmin can always edit
                return true;

            // file creator can always edit
            if (fileData.CreatorUsername != "" && CmsContext.currentWebPortalUser != null && String.Compare(CmsContext.currentWebPortalUser.UserName, fileData.CreatorUsername, true) == 0)
                return true;

            bool allowEdit = false;
            switch (data.accessLevelToEditFiles)
            {
                case BaseCmsPlaceholder.AccessLevel.Anonymous:
                    allowEdit = true;
                    break;
                case BaseCmsPlaceholder.AccessLevel.CmsAuthor:
                    if (CmsContext.currentUserCanAuthor)
                        allowEdit = true;
                    break;
                case BaseCmsPlaceholder.AccessLevel.LoggedInUser:
                    if (CmsContext.currentUserIsLoggedIn)
                        allowEdit = true;
                    break;
                default:
                    throw new ArgumentException("invalid PageFilesData.AccessLevel");
            }

            return allowEdit;
        }

        private bool currentUserCanUpload(PageFilesPlaceholderData data)
        {
            if (CmsContext.currentUserIsSuperAdmin) // SuperAdmin can always upload
                return true;

            bool allowUpload = false;
            switch (data.accessLevelToAddFiles)
            {
                case BaseCmsPlaceholder.AccessLevel.Anonymous:
                    allowUpload = true;
                    break;
                case BaseCmsPlaceholder.AccessLevel.CmsAuthor:
                    if (CmsContext.currentUserCanAuthor)
                        allowUpload = true;
                    break;
                case BaseCmsPlaceholder.AccessLevel.LoggedInUser:
                    if (CmsContext.currentUserIsLoggedIn)
                        allowUpload = true;
                    break;
                default:
                    throw new ArgumentException("invalid PageFilesData.AccessLevel");
            }

            return allowUpload;
        }
        */

        public string getFileDetailsUrl(PageFilesPlaceholderData phData, PageFilesItemData item, CmsPage page, int identifier, CmsLanguage language)
        {
            if (phData.tabularDisplayLinkMode == PageFilesPlaceholderData.TabularDisplayLinkMode.LinkToFile && !page.currentUserCanWrite)
                return item.getDownloadUrl(page, identifier, language);
            else
            {
                NameValueCollection pageParams = new NameValueCollection();

                pageParams.Add(CurrentFileIdFormName, item.Id.ToString());

                string url = CmsContext.getUrlByPagePath(page.Path, pageParams);
                return url;
            }
        } // getFileDetailsUrl

        public void RenderViewSummary(HtmlTextWriter writer, CmsPage page, int identifier,CmsLanguage langToRenderFor, string[] paramList)
        {            
            string ControlId = "PageFiles_"+page.ID.ToString()+"_"+identifier.ToString()+langToRenderFor.shortCode;

            PageFilesDb db = new PageFilesDb();
            PageFilesPlaceholderData data = db.getPageFilesData(page, identifier, langToRenderFor, true);

            string _userErrorMessage = "";
            int _swfUploadErrorCode = 200;
            string _userMessage = "";

            string swfUploadAction = PageUtils.getFromForm("swfUploadAction",""); // "
            bool isSwfUploadAction = (String.Compare(swfUploadAction,"processUpload", true) == 0);

            string action = PageUtils.getFromForm(ControlId + "_action", "");
            if ((String.Compare(action, "postFile", true) == 0 || isSwfUploadAction) && page.currentUserCanWrite)
            {

                HttpRequest req = System.Web.HttpContext.Current.Request;
                if (req.Files.Count > 0 && page.currentUserCanWrite)
                {
                    foreach (string key in req.Files.Keys)
                    {
                        //if (String.Compare(key, ControlId + "_FileUpload", true) == 0)
                        {
                            HttpPostedFile postedFile = req.Files[key];
                            if (postedFile.ContentLength < 1)
                            {
                                _userErrorMessage = "No file was received. Please try again...";
                                _swfUploadErrorCode = 503;
                            }
                            else
                            {
                                try
                                {
                                    PageFilesItemData newFileItem = new PageFilesItemData();
                                    newFileItem.Filename = System.IO.Path.GetFileName(postedFile.FileName);
                                    // newFileItem.Filename = page.ID.ToString() + identifier.ToString() + newFileItem.Filename;
                                    newFileItem.Title = System.IO.Path.GetFileName(postedFile.FileName);

                                    if (System.IO.File.Exists(newFileItem.getFilenameOnDisk(page, identifier, langToRenderFor)))
                                    {
                                        _userErrorMessage = "A file named \"" + newFileItem.Filename + "\" already exists.";
                                        _swfUploadErrorCode = 501;
                                    }
                                    else
                                    {
                                        newFileItem.FileSize = postedFile.ContentLength;
                                        newFileItem.lastModified = DateTime.Now;

                                        if (CmsContext.currentWebPortalUser != null)
                                            newFileItem.CreatorUsername = CmsContext.currentWebPortalUser.UserName;

                                        string targetFn = newFileItem.getFilenameOnDisk(page, identifier, langToRenderFor);


                                        if (!Directory.Exists(Path.GetDirectoryName(targetFn)))
                                        {
                                            Directory.CreateDirectory(Path.GetDirectoryName(targetFn));
                                        }
                                        
                                        postedFile.SaveAs(targetFn);

                                        bool b = db.saveNewPageFilesItemData(newFileItem, page, identifier, langToRenderFor);
                                        if (!b)
                                        {
                                            _userErrorMessage = "There was a database problem while saving your file.";
                                            _swfUploadErrorCode = 502;
                                        }
                                        else
                                        {
                                            string editUrl = getFileDetailsUrl(data, newFileItem, page, identifier, langToRenderFor);
                                            _userMessage = "The file \"" + newFileItem.Filename + "\" has been uploaded. <a href=\"" + editUrl + "\">Edit</a> this file's details.";
                                        }
                                    } // if file doesn't already exist
                                }
                                catch
                                {
                                    _swfUploadErrorCode = 500;
                                    _userErrorMessage = "There was a problem while saving your file.";
                                }
                            } // else
                        }
                    } // foreach
                } // if there are files
                else
                {
                    _userErrorMessage = "no file was uploaded - please try again.";
                    _swfUploadErrorCode = 503;
                }
            }

            if (isSwfUploadAction)
            {
                if (_userErrorMessage != "")
                {
                    System.Web.HttpContext.Current.Response.Clear();
                    System.Web.HttpContext.Current.Response.StatusCode = _swfUploadErrorCode;
                    System.Web.HttpContext.Current.Response.StatusDescription = _userErrorMessage;
                    System.Web.HttpContext.Current.Response.Write(_userErrorMessage);
                    System.Web.HttpContext.Current.Response.Flush();
                }

                System.Web.HttpContext.Current.Response.End();
            }



            data.sortColumn = (PageFilesPlaceholderData.SortColumn)PageUtils.getFromForm("sort", typeof(PageFilesPlaceholderData.SortColumn), data.sortColumn);
            data.sortDirection = (PageFilesPlaceholderData.SortDirection)PageUtils.getFromForm("sortdir", typeof(PageFilesPlaceholderData.SortDirection), data.sortDirection);

            PageFilesItemData[] fileItems = db.getPageFilesItemDatas(page, identifier,langToRenderFor, data);


            // make view similar to this: http://www.inforouter.com/presentation-search-document.asp
            StringBuilder html = new StringBuilder();
            if (_userErrorMessage != "")
            {
                html.Append("<p><font color=\"red\">Error: " + _userErrorMessage + "</font></p>");
            }
            else
            {
                html.Append("<p><font color=\"green\">" + _userMessage + "</font></p>");
            }

            bool displayEmptyMsg = PlaceholderUtils.getParameterValue("emptymessage", true, paramList);
            bool compactMode = PlaceholderUtils.getParameterValue("compact", false, paramList);
            if (fileItems.Length < 1 && displayEmptyMsg == true)
            {
                html.Append("<em>" + getNoFilesText(langToRenderFor) + "</em>");
                html.Append("<!-- pageId:" + page.ID + " Identifier:" + identifier + "; lang: " + langToRenderFor.shortCode + " -->");
            }
            else if (fileItems.Length < 1)
            {
                // nothing to output if displayEmptyMsg == false
                html.Append("<!-- pageId:" + page.ID + " Identifier:" + identifier + "; lang: " + langToRenderFor.shortCode + " -->");
            }
            else
            {
                html.Append("<!-- " + fileItems.Length.ToString() + " to output -->");

                if (compactMode == false)
                    html.Append(displayFullMode(data, fileItems, page, identifier, langToRenderFor));
                else
                    html.Append(displayCompactMode(data, fileItems, page, identifier, langToRenderFor));
            }

            if (page.currentUserCanWrite)
            {

                bool useSWFUpload = CmsConfig.getConfigValue("useSWFUpload", false);

                if (!useSWFUpload)
                {
                    // Traditional upload
                    html.Append("<p>");
                    string formId = "swfUpload";
                    html.Append(page.getFormStartHtml(formId));
                    html.Append("<strong>"+getAddFileText(langToRenderFor)+":</strong>");
                    html.Append(" <input type=\"file\" name=\"\">");
                    html.Append(PageUtils.getHiddenInputHtml(ControlId + "_action", "postFile"));
                    html.Append(" <input type=\"submit\" name=\"" + ControlId + "_FileUpload\" value=\"" + getUploadButtonText(langToRenderFor) + "\" onclick=\"document.getElementById('" + ControlId + "Frobber').style.display = 'inline';\">");
                    html.Append(" <img src=\"" + CmsContext.ApplicationPath + "images/_system/ajax-loader_24x24.gif\" width=\"24\" height=\"24\" align=\"absmiddle\" id=\"" + ControlId + "Frobber\" style=\"display: none;\">");
                    html.Append("<br>" + getMaxFileSizeText(langToRenderFor) + ": " + PageUtils.MaxUploadFileSize);
                    html.Append(page.getFormCloseHtml(formId));
                    html.Append("</p>");
                }
                else
                {
                    string storageUrl = PageFilesItemData.GetFileStorageDirectoryUrl(page, identifier, langToRenderFor);
                    // -- remove the ApplicationPath
                    if (storageUrl.StartsWith(CmsContext.ApplicationPath))
                        storageUrl = storageUrl.Substring(CmsContext.ApplicationPath.Length);

                    // -- SWF Upload
                    if (CmsConfig.getConfigValue("DMSFileStorageLocationVersion", "V1") == "V1")
                        throw new Exception("Error: you can not use SWFUpload for the PageFiles placeholder with DMSFileStorageLocationVersion set to V1");

                    string uploadUrl = CmsContext.ApplicationPath + "_system/tools/swfUpload/SwfUploadTarget.aspx?DMS=1&dir=" + storageUrl;
                    string fileFilters = "*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.txt;*.zip";
                    SWFUploadHelpers.AddPageJavascriptStatements(page, ControlId, uploadUrl, fileFilters, "Document Files (" + fileFilters + ")");

                    string formId = "AddFiles";
                    html.Append(page.getFormStartHtml(formId));

                    html.Append("<p>");
                    html.Append("<strong>Add files:</strong><br>");
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
                    html.Append(page.getFormCloseHtml(formId));

                } // swfUpload
            }

            writer.Write(html.ToString());
        } // RenderViewSummary

        protected string displayFullMode(PageFilesPlaceholderData data, PageFilesItemData[] fileItems, CmsPage page, int identifier, CmsLanguage langToRenderFor)
        {
            StringBuilder html = new StringBuilder();
            html.Append(getPagerOutput(data, fileItems, page, langToRenderFor));
            html.Append("<table width=\"100%\" class=\"PageFilesSummaryTable\">");
            html.Append("<tr align=\"left\">");
            html.Append("<th class=\"PageFile_Icon\"></th>"); // icon
            html.Append("<th class=\"PageFile_Name\"><a href=\"" + getSummaryViewSortedTableUrl(page, data, PageFilesPlaceholderData.SortColumn.Filename, 1) + "\">" + getNameText(langToRenderFor) + "</a></th>");
            html.Append("<th class=\"PageFile_Size\"><a href=\"" + getSummaryViewSortedTableUrl(page, data, PageFilesPlaceholderData.SortColumn.FileSize, 1) + "\">" + getSizeText(langToRenderFor) + "</a></th>");
            html.Append("<th class=\"PageFile_Type\">" + getTypeText(langToRenderFor) + "</th>");
            html.Append("<th class=\"PageFile_PostedDate\"><a href=\"" + getSummaryViewSortedTableUrl(page, data, PageFilesPlaceholderData.SortColumn.DateLastModified, 1) + "\">" + getPostedText(langToRenderFor) + "</a></th>");
            html.Append("</tr>");

            int startAt = PageUtils.getFromForm("fileStart", 0);
            int endAt = Math.Min(startAt + data.numFilesToShowPerPage, fileItems.Length);
            if (data.numFilesToShowPerPage < 1)
                endAt = fileItems.Length;

            html.Append("<!-- file output: " + startAt.ToString() + " - " + endAt.ToString() + " -->" + Environment.NewLine);

            for (int i = startAt; i < endAt; i++)
            {
                if (endAt <= 0)
                    break;


                PageFilesItemData item = fileItems[i];
                string iconUrl = getSmallIconUrl(item);
                string fileDetailsUrl = getFileDetailsUrl(data, item, page, identifier, langToRenderFor);
                string fileTypeDescription = getFileTypeName(System.IO.Path.GetExtension(item.Filename));

                string name = item.Title;
                if (name.Trim() == "")
                    name = item.Filename;

                if (CmsContext.currentUserIsSuperAdmin)
                {
                    string fnOnDisk = item.getFilenameOnDisk(page, identifier, langToRenderFor);
                    if (!System.IO.File.Exists(fnOnDisk))
                    {
                        name += " <br><span style=\"color: red\">file can not be downloaded (it doesn't exist on the server)!</span>";
                    }
                }

                html.Append("<td class=\"PageFile_Icon\"><img src=\"" + iconUrl + "\" width=\"16\" height=\"16\"></td>");
                html.Append("<td class=\"PageFile_Name\"><a href=\"" + fileDetailsUrl + "\">" + name + "</a></td>");
                html.Append("<td class=\"PageFile_Size\">" + StringUtils.formatFileSize(item.FileSize) + "</td>");
                html.Append("<td class=\"PageFile_Type\">" + fileTypeDescription + "</td>");
                html.Append("<td class=\"PageFile_PostedDate\">" + item.lastModified.ToString("MMM d yyyy") + "</td>");
                html.Append("</tr>");
                if (item.Abstract.Trim() != "")
                {
                    html.Append("<tr>");
                    html.Append("<td class=\"PageFile_Icon\"></td>"); // icon
                    html.Append("<td class=\"PageFile_Abstract\" colspan=\"4\"><font size=\"-1\"><strong>Abstract:</strong> <em>" + item.Abstract + "</em></font></td>");
                    html.Append("</tr>");
                }
            } // foreach

            html.Append("</table>");
            html.Append(getPagerOutput(data, fileItems, page, langToRenderFor));
            return html.ToString();
        }

        protected string displayCompactMode(PageFilesPlaceholderData data, PageFilesItemData[] fileItems, CmsPage page, int identifier, CmsLanguage langToRenderFor)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<table class=\"PageFilesSummaryTable\" cellspacing=\"5\">");

            int startAt = PageUtils.getFromForm("fileStart", 0);
            html.Append("<!-- file output: " + startAt.ToString() + " - " + fileItems.Length.ToString() + " -->" + Environment.NewLine);

            for (int i = startAt; i < fileItems.Length; i++)
            {
                PageFilesItemData item = fileItems[i];
                string iconUrl = getSmallIconUrl(item);
                string fileDetailsUrl = getFileDetailsUrl(data, item, page, identifier, langToRenderFor);
                string fileTypeDescription = getFileTypeName(System.IO.Path.GetExtension(item.Filename));

                string name = item.Title;
                if (name.Trim() == "")
                    name = item.Filename;

                if (CmsContext.currentUserIsSuperAdmin)
                {
                    string fnOnDisk = item.getFilenameOnDisk(page, identifier, langToRenderFor);
                    if (!System.IO.File.Exists(fnOnDisk))
                    {
                        name += " <br/ ><span style=\"font-size: smaller; color: red\">file can not be downloaded (it doesn't exist on the server)!</span>";
                    }
                }

                html.Append("<td class=\"PageFile_Icon\"><img src=\"" + iconUrl + "\" width=\"16\" height=\"16\"></td>");
                html.Append("<td class=\"PageFile_Name\"><a href=\"" + fileDetailsUrl + "\">" + name + "</a></td>");
                html.Append("<td class=\"PageFile_Size\">" + StringUtils.formatFileSize(item.FileSize) + "</td>");
                html.Append("</tr>");
            }

            html.Append("</table>");
            return html.ToString();
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string ControlId = "PageFiles_" + page.ID.ToString() + "_" + identifier.ToString() + langToRenderFor.shortCode;

            PageFilesDb db = new PageFilesDb();
            PageFilesPlaceholderData data = new PageFilesPlaceholderData();
            data = db.getPageFilesData(page, identifier, langToRenderFor, true);

            string action = PageUtils.getFromForm(ControlId + "_action", "");
            if (String.Compare(action, "saveNewValues", true) == 0)
            {
                data.sortColumn = (PageFilesPlaceholderData.SortColumn)PageUtils.getFromForm(ControlId + "SortColumn", typeof(PageFilesPlaceholderData.SortColumn), data.sortColumn);
                data.sortDirection = (PageFilesPlaceholderData.SortDirection)PageUtils.getFromForm(ControlId + "SortDirection", typeof(PageFilesPlaceholderData.SortDirection), data.sortDirection);
                data.tabularDisplayLinkMode = (PageFilesPlaceholderData.TabularDisplayLinkMode)PageUtils.getFromForm(ControlId + "tabularDisplayLinkMode", typeof(PageFilesPlaceholderData.TabularDisplayLinkMode), data.tabularDisplayLinkMode);
                data.numFilesToShowPerPage = PageUtils.getFromForm(ControlId + "numFilesToShowPerPage", data.numFilesToShowPerPage);
                data.accessLevelToAddFiles = (BaseCmsPlaceholder.AccessLevel)PageUtils.getFromForm(ControlId + "accessLevelToAddFiles", typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToAddFiles);
                data.accessLevelToEditFiles = (BaseCmsPlaceholder.AccessLevel)PageUtils.getFromForm(ControlId + "accessLevelToEditFiles", typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToEditFiles);
                db.saveUpdatedPageFilesData(page, identifier, langToRenderFor, data);
            }


            StringBuilder html = new StringBuilder();

            html.Append("<p><strong>File Listing Configuration:</strong></p>");

            html.Append("<table>");

            html.Append("<tr>");
            html.Append("<td>File link mode: </td>");
            html.Append("<td>");
            html.Append(PageUtils.getDropDownHtml(ControlId + "tabularDisplayLinkMode", ControlId + "tabularDisplayLinkMode", Enum.GetNames(typeof(PageFilesPlaceholderData.TabularDisplayLinkMode)), Enum.GetName(typeof(PageFilesPlaceholderData.TabularDisplayLinkMode), data.tabularDisplayLinkMode)));
            html.Append("</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Default column sorting: </td>");
            html.Append("<td>");
            html.Append(PageUtils.getDropDownHtml(ControlId + "sortColumn", ControlId + "sortColumn", Enum.GetNames(typeof(PageFilesPlaceholderData.SortColumn)), Enum.GetName(typeof(PageFilesPlaceholderData.SortColumn), data.sortColumn)));
            html.Append(PageUtils.getDropDownHtml(ControlId + "SortDirection", ControlId + "SortDirection", Enum.GetNames(typeof(PageFilesPlaceholderData.SortDirection)), Enum.GetName(typeof(PageFilesPlaceholderData.SortDirection), data.sortDirection)));
            html.Append("</td>");
            html.Append("</tr>");
            
            html.Append("<tr>");
            html.Append("<td># of files to show per page:</td>");            
            html.Append("<td>");
            html.Append(PageUtils.getInputTextHtml(ControlId + "numFilesToShowPerPage", ControlId + "numFilesToShowPerPage", data.numFilesToShowPerPage.ToString(), 3, 3));
            html.Append("</td>");
            html.Append("</tr>");

            // -- store obsolte values (note: access levels are now controlled by Zones)
            html.Append(PageUtils.getHiddenInputHtml(ControlId + "accessLevelToAddFiles", Enum.GetName(typeof(AccessLevel), AccessLevel.CmsAuthor)));
            html.Append(PageUtils.getHiddenInputHtml(ControlId + "accessLevelToEditFiles", Enum.GetName(typeof(AccessLevel), AccessLevel.CmsAuthor)));


            html.Append("</table>");

            html.Append(PageUtils.getHiddenInputHtml(ControlId+"_action","saveNewValues"));

            writer.Write(html.ToString());
        } // RenderEditSummary
        
        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {

            PageFilesDb db = new PageFilesDb();
            PageFilesPlaceholderData data = db.getPageFilesData(page, placeholderDefinition.Identifier, langToRenderFor, true);
            PageFilesItemData[] fileItems = db.getPageFilesItemDatas(page, placeholderDefinition.Identifier, langToRenderFor, data);

            
            List<Rss.RssItem> ret = new List<Rss.RssItem>();

            foreach (PageFilesItemData file in fileItems)
            {
                Rss.RssItem rssItem = CreateAndInitRssItem(page, langToRenderFor);

                rssItem.Title = file.Title;                
                rssItem.Link = new Uri(file.getDownloadUrl(page, placeholderDefinition.Identifier, langToRenderFor));
                rssItem.Guid = new Rss.RssGuid(rssItem.Link);

                rssItem.Description = file.AbstractHtml;

                ret.Add(rssItem);
            } // foreach

            return ret.ToArray();
        } // GetRssFeedItems

    } // class PageFiles
}
