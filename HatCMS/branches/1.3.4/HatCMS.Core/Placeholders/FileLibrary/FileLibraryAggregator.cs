using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using HatCMS.Placeholders;
using Hatfield.Web.Portal;
using Hatfield.Web.Portal.Imaging;
using System.Collections.Specialized;
using HatCMS.Placeholders.Calendar;
using Hatfield.Web.Portal.Html;

namespace HatCMS.Placeholders
{
    public class FileLibraryAggregator : BaseCmsPlaceholder
    {


        public class RenderParameters
        {
            public static RenderParameters fromParamList(string[] paramList)
            {
                return new RenderParameters(paramList);
            }

            public enum DisplayMode { Tabs, List }
            public DisplayMode displayMode = DisplayMode.Tabs;

            public enum FileLinkMode { LinkToPage, LinkToFile }
            public FileLinkMode fileLinkMode = FileLinkMode.LinkToPage;

            /// <summary>
            /// Set to Int32.MinValue for the current page.
            /// </summary>
            public int PageIdToGatherFilesFrom = Int32.MinValue;

            /// <summary>
            /// if set to false, only gathers files from child pages. If true, gathers from all 
            /// </summary>
            public bool RecursiveGatherFiles = false;


            public RenderParameters(string[] paramList)
            {
                if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
                {
                    string sDisplayMode = "";
                    try
                    {
                        sDisplayMode = PlaceholderUtils.getParameterValue("displaymode", Enum.GetName(typeof(DisplayMode), displayMode), paramList);
                        displayMode = (DisplayMode)Enum.Parse(typeof(DisplayMode), sDisplayMode, true);
                    }
                    catch
                    {
                        throw new Exception("Error: invalid FileLibraryAggregator displaymode parameter. Valid values: "+String.Join(", ", Enum.GetNames(typeof(DisplayMode))));
                    }

                    string sLinkMode = "";
                    try
                    {
                        sLinkMode = PlaceholderUtils.getParameterValue("filelinks", Enum.GetName(typeof(FileLinkMode), displayMode), paramList);
                        fileLinkMode = (FileLinkMode) Enum.Parse(typeof(FileLinkMode), sLinkMode, true);
                    }
                    catch
                    {
                        throw new Exception("Error: invalid FileLibraryAggregator filelinks parameter. Valid values: "+String.Join(", ", Enum.GetNames(typeof(FileLinkMode))));
                    }

                    PageIdToGatherFilesFrom = PlaceholderUtils.getParameterValue("gatherfrompageid", Int32.MinValue, paramList);
                    RecursiveGatherFiles = PlaceholderUtils.getParameterValue("gatherrecusive", RecursiveGatherFiles, paramList);
                }
                else
                {
                    throw new ArgumentException("Invalid CmsTemplateEngineVersion");
                }
            }
        } // RenderParameters        
        
        protected string EOL = Environment.NewLine;
        protected FileLibraryDb db = new FileLibraryDb();        
        protected List<FileLibraryCategoryData> categoryList;


        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented;
        }

        /// <summary>
        /// Get the dependencies for FileLibrary
        /// </summary>
        /// <returns></returns>
        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            ret.Add(CmsFileDependency.UnderAppPath("js/_system/FileLibrary/FileLibrary.js"));
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/FileLibrary/FileLibraryCategory.js"));
            ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("DeleteFileLibraryPath", "/_admin/actions/deleteFileLibrary"), CmsConfig.Languages));

            // -- database tables
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE `FileLibraryAggregator` (
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `LangCode` varchar(5) NOT NULL,
                  `NumFilesOverview` int(11) NOT NULL,
                  `NumFilesPerPage` int(11) NOT NULL,
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
            "));
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `filelibrarydetails` (
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `LangCode` varchar(5) NOT NULL,
                  `Filename` varchar(255) NOT NULL,
                  `CategoryId` int(11) NOT NULL,
                  `Author` varchar(255) NOT NULL DEFAULT '',
                  `Description` text NOT NULL,
                  `LastModified` datetime NOT NULL,
                  `CreatedBy` varchar(255) NOT NULL,
                  `EventPageId` int(11) NOT NULL DEFAULT '-1',
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
            "));
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `filelibrarycategory` (
                  `CategoryId` int(11) NOT NULL,
                  `LangCode` varchar(5) NOT NULL,
                  `EventRequired` int(1) NOT NULL DEFAULT '0',
                  `CategoryName` varchar(255) NOT NULL,
                  `SortOrdinal` int(11) NOT NULL DEFAULT '0',
                  PRIMARY KEY (`CategoryId`,`LangCode`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
            "));
            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("FileLibrary.DetailsTemplateName"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.NumEventsInList"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.OverviewText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.NewUploadText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.CategoryText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.FileNameText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.AttachedEventText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.AttachToEventText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.FileText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.SeeFileDetailsText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.BackToText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.TabText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.DownloadText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.LinkOpensNewWindowText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.AuthorText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.DocumentAbstractText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.UploadedByText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.LastUpdatedText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.ImagePreviewText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.EditText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.DateTimeText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.EventCategoryText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.DescriptionText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.AddFileText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.MaxFileSizeText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.UploadButtonText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.EventNotAttachedText"));
            ret.Add(new CmsConfigItemDependency("FileLibrary.PageText"));

            ret.Add(CmsFileDependency.UnderAppPath("images/_system/calendar/arrowRight.jpg", new DateTime(2011, 3, 1)));

            string newPageTemplateName = CmsConfig.getConfigValue("FileLibrary.DetailsTemplateName", "_FileLibraryDetails");
            ret.Add(new CmsTemplateDependency(newPageTemplateName, "FileLibrary.DetailsTemplateName configuration entry"));

            return ret.ToArray();
        }

        public static string DeleteThisFileAggregatorPageAction(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
        {
            NameValueCollection paramList = new NameValueCollection();

            CmsPage targetPage = pageToRenderFor;
            if (action.ActionPayload is CmsPage)
                targetPage = action.ActionPayload as CmsPage;

            paramList.Add("target", targetPage.ID.ToString());

            string confirmText = "Do you really want to delete this page?";

            int numPagesToDelete = targetPage.getLinearizedPages().Keys.Count;
            if (numPagesToDelete > 1)
                confirmText = "Do you really want to delete this page and all " + (numPagesToDelete - 1) + " files?";

            string deleteFilesUrl = CmsContext.getUrlByPagePath(CmsConfig.getConfigValue("DeleteFileLibraryPath", "/_admin/actions/deleteFileLibrary"), paramList, langToRenderFor);
            return "<a href=\"#\" onclick=\"EditMenuConfirmModal('" + confirmText + "','" + deleteFilesUrl + "',300, 300);\"><strong>Delete</strong> this page</a>";

        }

        /// <summary>
        /// Replaces the "Delete page" command with a custom one.
        /// </summary>
        /// <param name="pageToAddCommandTo"></param>        
        public static void UpdateFileLibraryCommandsInEditMenu(CmsPage pageToAddCommandTo)
        {
            // -- only add the command if the user can author
            if (!pageToAddCommandTo.currentUserCanWrite)
                return;

            // get the existing command
            CmsPageEditMenuAction deletePageAction = pageToAddCommandTo.EditMenu.getActionItem(CmsEditMenuActionItem.DeleteThisPage);

            if (deletePageAction == null)
                return; // do not throw an exception (note: the home page does not have a deletepageaction)
            
            deletePageAction.doRenderToString = DeleteThisFileAggregatorPageAction;
            
        }

        /// <summary>
        /// Include the JS and CSS files.
        /// </summary>
        /// <param name="page"></param>
        protected void addCssAndScript(CmsPage page, RenderParameters renderParameters)
        {
            if (renderParameters.displayMode == RenderParameters.DisplayMode.Tabs)
            {
                page.HeadSection.AddJavascriptFile(JavascriptGroup.Library, "js/_system/jquery/jquery-1.4.1.min.js");
                page.HeadSection.AddJavascriptFile(JavascriptGroup.ControlOrPlaceholder, "js/_system/FileLibrary/FileLibrary.js");
            }
        }

        /// <summary>
        /// Create a cms child page, and cms language info
        /// </summary>
        /// <param name="parentPage"></param>
        /// <param name="lang"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected CmsPage createChildPage(CmsPage parentPage, CmsLanguage lang, string fileName)
        {
            CmsPage childPage = new CmsPage();
            childPage.ParentID = parentPage.ID;
            childPage.ShowInMenu = false;
            childPage.TemplateName = CmsConfig.getConfigValue("FileLibrary.DetailsTemplateName", "_FileLibraryDetails");
            if (CmsContext.currentWebPortalUser != null)
                childPage.LastModifiedBy = CmsContext.currentWebPortalUser.UserName;

            List<CmsPageLanguageInfo> langInfoList = new List<CmsPageLanguageInfo>();
            foreach (CmsLanguage l in CmsConfig.Languages)
            {
                CmsPageLanguageInfo langInfo = new CmsPageLanguageInfo();
                langInfo.languageShortCode = l.shortCode;
                langInfo.name = fileName;
                langInfo.menuTitle = fileName;
                langInfo.title = fileName;
                langInfo.searchEngineDescription = "";

                langInfoList.Add(langInfo);
            }
            childPage.LanguageInfo = langInfoList.ToArray();

            return childPage;
        }

        /// <summary>
        /// Create the FileLibraryDetailsData object
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected FileLibraryDetailsData createFileDetails(CmsLanguage lang, string controlId, string postedFileName)
        {
            FileLibraryDetailsData fileDetails = new FileLibraryDetailsData();
            fileDetails.FileName = System.IO.Path.GetFileName(postedFileName);
            fileDetails.CategoryId = PageUtils.getFromForm(controlId + "categoryID_" + lang.shortCode, 0);
            fileDetails.EventPageId = PageUtils.getFromForm(controlId + "eventPageId_" + lang.shortCode, -1);
            if (CmsContext.currentWebPortalUser != null)
                fileDetails.CreatedBy = CmsContext.currentWebPortalUser.UserName;
            return fileDetails;
        }

        /// <summary>
        /// Format error messages in red
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected string formatErrorMsg(string msg)
        {
            return "<div style=\"font-weight: bold; color: red;\">" + msg + "</div>" + EOL;
        }

        /// <summary>
        /// Format normal messages in green
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected string formatNormalMsg(string msg)
        {
            return "<div style=\"color: green;\">" + msg + "</div>" + EOL;
        }

#region multi-lang text
        protected string getAddFileText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.AddFileText", "Add a file", lang);
        }

        protected string getAttachedEventText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.AttachedEventText", "Attached event", lang);
        }

        protected string getAttachToEventText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.AttachToEventText", "Attach to event", lang);
        }

        protected string getCategoryText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.CategoryText", "Category", lang);
        }

        protected string getDateTimeText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.DateTimeText", "Date/Time", lang);
        }

        protected string getDescriptionText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.DescriptionText", "Description", lang);
        }

        protected string getDocumentAbstractText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.DocumentAbstractText", "Document abstract", lang);
        }

        protected string getEditText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.EditText", "edit", lang);
        }

        protected string getEventCategoryText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.EventCategoryText", "Event category", lang);
        }

        protected string getFileText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.FileText", "File", lang);
        }

        protected string getFileNameText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.FileNameText", "FileName", lang);
        }

        protected string getMaxFileSizeText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.MaxFileSizeText", "Maximum file size", lang);
        }

        protected string getNewUploadText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.NewUploadText", "Newly uploaded files", lang);
        }

        protected string getOverviewText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.OverviewText", "Overview", lang);
        }

        protected string getPageText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.PageText", "Page", lang);
        }

        protected string getSeeFileDetailsText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.SeeFileDetailsText", "See file details", lang);
        }

        protected string getUploadButtonText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.UploadButtonText", "Upload file", lang);
        }
#endregion

        /// <summary>
        /// Get the category id from query string.  If not, assume to show overview
        /// </summary>
        /// <returns></returns>
        protected int getFileCategoryId()
        {
            int fileCategoryId = PageUtils.getFromForm("fcatId", -1);
            bool valid = false;
            foreach (FileLibraryCategoryData c in categoryList)
            {
                if (fileCategoryId == c.CategoryId)
                    valid = true;
            }
            return (valid) ? fileCategoryId : -1;
        }

        /// <summary>
        /// Generate HTML SELECT and OPTION tags for FileLibraryCategory
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="controlId"></param>
        /// <param name="selectedId"></param>
        /// <returns></returns>
        protected string getCategoryOption(CmsLanguage lang, string controlId)
        {
            StringBuilder html = new StringBuilder();
            string htmlName = controlId + "categoryId_" + lang.shortCode;
            html.Append("<select name=\"" + htmlName + "\" id=\"" + htmlName + "\" class=\"" + htmlName + "\">" + EOL);
            html.Append(FileLibraryCategoryData.getCategoryOptionTag(lang, categoryList, PageUtils.getFromForm("fcatId", -1)));
            html.Append("</select>" + EOL);
            html.Append(FileLibraryCategoryData.getEditPopupAnchor(lang, controlId + "categoryId", "(" + getEditText(lang) + ")") + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Under IE, the postedFileName contains local disk drive letter and path.
        /// Remove the drive letter and path by this method.
        /// </summary>
        /// <param name="postedFileName"></param>
        /// <returns></returns>
        protected string getFileNameWithoutPath( string postedFileName )
        {
            return Path.GetFileName(postedFileName);
        }

        /// <summary>
        /// Generate hidden layer to store the event details
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="eventCategoryList"></param>
        /// <param name="eventPage"></param>
        /// <param name="eventData"></param>
        /// <param name="controlId"></param>
        /// <param name="selectedId"></param>
        /// <returns></returns>
        protected string getEventHiddenLayer(CmsLanguage lang, List<EventCalendarDb.EventCalendarCategoryData> eventCategoryList, CmsPage eventPage, EventCalendarDb.EventCalendarDetailsData eventData, string controlId, int selectedId)
        {
            StringBuilder html = new StringBuilder();
            string domId = controlId + "table_" + eventData.PageId;
            string cssClass = controlId + "table";
            string tableTag = "<table cellspacing=\"0\" cellpadding=\"3\" id=\"{0}\" class=\"{1}\" {2}>" + EOL;
            string displayStyle = (eventData.PageId == selectedId) ? "style=\"display: block;\"" : "style=\"display: none;\"";
            html.Append(String.Format(tableTag, new string[] { domId, cssClass, displayStyle }));

            html.Append("<tr valign=\"top\">");
            html.Append("<td>" + getDateTimeText(lang) + ":</td>");
            html.Append("<td>");
            string endDateTime = (String.Format("{0:yyyy/MM/dd}", eventData.StartDateTime) == String.Format("{0:yyyy/MM/dd}", eventData.EndDateTime)) ? String.Format("{0:HH:mm}", eventData.EndDateTime) : "<br />" + String.Format("{0:yyyy/MM/dd HH:mm}", eventData.EndDateTime);
            html.Append(String.Format("{0:yyyy/MM/dd HH:mm}", eventData.StartDateTime) + " to " + endDateTime);
            html.Append("</td>");
            html.Append("</tr>" + EOL);

            html.Append("<tr valign=\"top\">");
            html.Append("<td>" + getEventCategoryText(lang) + ":</td>");
            html.Append("<td>");
            html.Append(eventData.getCategoryTitle(eventCategoryList));
            html.Append("</td>");
            html.Append("</tr>" + EOL);

            html.Append("<tr valign=\"top\">");
            html.Append("<td>" + getDescriptionText(lang) + ":</td>");
            html.Append("<td>");
            html.Append(StringUtils.nl2br(eventData.Description));
            html.Append("</td>");
            html.Append("</tr>" + EOL);

            html.Append("</table>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Generate HTML SELECT and OPTION tags for events selection
        /// (count limited by the config)
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="controlId"></param>
        /// <param name="selectedId"></param>
        /// <returns></returns>
        protected string getEventOption(CmsLanguage lang, string controlId, int selectedId)
        {
            EventCalendarDb eventDb = new EventCalendarDb();
            int dropdownCount = CmsConfig.getConfigValue("FileLibrary.NumEventsInList", 10);
            List<EventCalendarDb.EventCalendarDetailsData> eventList = eventDb.fetchAllDetailsData(lang, selectedId, dropdownCount);
            List<CmsPage> pageList = new List<CmsPage>(eventList.Count);
            NameValueCollection eventPageCollection = new NameValueCollection(eventList.Count);
            for (int x = 0; x < eventList.Count; x++)
            {
                EventCalendarDb.EventCalendarDetailsData e = eventList[x];
                CmsPage p = CmsContext.getPageById(e.PageId);
                pageList.Add(p);
                eventPageCollection.Add(e.PageId.ToString(), p.getTitle(lang));
            }

            StringBuilder html = new StringBuilder();
            string htmlName = controlId + "eventPageId_" + lang.shortCode;
            string selectTag = "<select name=\"{0}\" id=\"{1}\" onchange=\"$('.{2}table').css('display','none'); $('#{3}table_' + this.value).fadeIn();\">" + EOL;
            html.Append(String.Format(selectTag, new string[] { htmlName, htmlName, controlId, controlId }));

            string optionSelected = (selectedId == -1) ? "selected=\"selected\"" : "";
            string optionTag = "<option value=\"{0}\" {1}>{2}</option>" + EOL;
            html.Append(String.Format(optionTag, new string[] { "-1", optionSelected, "(n/a)" }));
            for (int x = 0; x < eventList.Count; x++)
            {
                EventCalendarDb.EventCalendarDetailsData e = eventList[x];
                optionSelected = (selectedId == e.PageId) ? "selected=\"selected\"" : "";
                html.Append(String.Format(optionTag, new string[] { e.PageId.ToString(), optionSelected, pageList[x].getTitle(lang) }));
            }

            html.Append("</select>" + EOL);

            List<EventCalendarDb.EventCalendarCategoryData> eventCategoryList = eventDb.fetchCategoryList(lang);
            for (int x = 0; x < eventList.Count; x++)
                html.Append(getEventHiddenLayer(lang, eventCategoryList, pageList[x], eventList[x], controlId, selectedId));

            return html.ToString();
        }

        /// <summary>
        /// Get the files (with page break)
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="cat"></param>
        /// <param name="aggregatorData"></param>
        /// <returns></returns>
        protected List<FileLibraryDetailsData> getFileList(CmsPage page, int identifier, CmsLanguage lang, FileLibraryCategoryData cat, FileLibraryAggregatorData aggregatorData, RenderParameters renderParameters)
        {
            int offset = PageUtils.getFromForm("offset", 0);
            int count = aggregatorData.NumFilesPerPage;

            CmsPage rootPageToGatherFrom = page;
            if (renderParameters.PageIdToGatherFilesFrom >= 0)
                rootPageToGatherFrom = CmsContext.getPageById(renderParameters.PageIdToGatherFilesFrom);

            CmsContext.PageGatheringMode gatherMode = CmsContext.PageGatheringMode.ChildPagesOnly;
            if (renderParameters.RecursiveGatherFiles)
                gatherMode = CmsContext.PageGatheringMode.FullRecursion;

            CmsPage[] pagesToGetDetailsFrom = CmsContext.getAllPagesWithPlaceholder("FileLibraryDetails", rootPageToGatherFrom, gatherMode);

            return db.fetchDetailsData(pagesToGetDetailsFrom, identifier, lang, cat, count * offset, count);
            
        }

        /// <summary>
        /// Handle a file upload, save file to disk and create record to db
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string handleUploadSubmit(CmsPage page, int identifier, CmsLanguage lang, string controlId)
        {
            if (PageUtils.getFromForm(controlId + "action", "") != "postFile")
                return "";

            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                return "";

            HttpRequest req = System.Web.HttpContext.Current.Request;
            if (req.Files.Count == 0)
                return formatErrorMsg("No file was received. Please try again...");

            StringBuilder msg = new StringBuilder();
            foreach (string key in req.Files.Keys)
            {
                HttpPostedFile postedFile = req.Files[key];
                string postedFileName = getFileNameWithoutPath(postedFile.FileName);
                if (postedFile.ContentLength < 1)
                {
                    msg.Append(formatErrorMsg("'" + postedFileName + "' is empty.  File ignored."));
                    continue;
                }

                try
                {
                    CmsPage childPage = createChildPage(page, lang, postedFileName);
                    if (CmsPage.InsertNewPage(childPage) == false)
                    {
                        msg.Append(formatErrorMsg("There was a database problem while saving '" + postedFileName + "'."));
                        continue;
                    }

                    CmsLanguage[] allLang = CmsConfig.Languages;
                    for (int x = 0; x < allLang.Length; x++)
                    {
                        FileLibraryDetailsData fileDetails = createFileDetails(lang, controlId, postedFileName);
                        if (db.insertDetailsData(childPage, identifier, allLang[x], fileDetails) == false)
                        {
                            msg.Append(formatErrorMsg("There was a database problem while saving '" + postedFileName + "'."));
                            continue;
                        }
                    }

                    string status = putUploadFileToFolder(childPage, identifier, lang, postedFile);
                    if (status != "")
                    {
                        msg.Append(status);
                        continue;
                    }

                    Dictionary<string, string> urlParams = new Dictionary<string,string>();
                    urlParams.Add(CmsContext.EditModeFormName,"1");
                    string detailsView = childPage.getUrl(urlParams, lang);
                    string m = "The file \"" + postedFileName + "\" has been uploaded. <a href=\"" + detailsView + "\">Edit</a> this file's details. <a href=\"javascript:location.reload(true);\">Refresh</a> to update the listing.";
                    msg.Append(formatNormalMsg(m));
                }
                catch (Exception ex)
                {
                    msg.Append(formatErrorMsg("Error processing file '" + postedFileName + "'. " + ex.Message));
                }
            }

            return msg.ToString();
        }

        /// <summary>
        /// Save upload file to web server's disk
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="postedFile"></param>
        /// <returns></returns>
        protected string putUploadFileToFolder(CmsPage fileDetailsPage, int identifier, CmsLanguage lang, HttpPostedFile postedFile)
        {            
            string targetFileName = getFileNameWithoutPath(postedFile.FileName);
            targetFileName = FileLibraryDetailsData.getTargetNameOnDisk(fileDetailsPage, identifier, lang, targetFileName);
            if (System.IO.File.Exists(targetFileName))
                return formatErrorMsg("A file named \"" + postedFile.FileName + "\" already exists.");

            if (!Directory.Exists(Path.GetDirectoryName(targetFileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
            postedFile.SaveAs(targetFileName);
            return "";
        }

        /// <summary>
        /// Render the tab header, i.e. overview + all categories
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="fileCategoryId"></param>
        /// <returns></returns>
        protected string renderTabHeader(CmsLanguage lang, int fileCategoryId)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<ul>" + EOL);
            html.Append(renderTabHeaderOverview(lang, fileCategoryId));

            foreach (FileLibraryCategoryData c in categoryList)
            {
                html.Append("<li>" + EOL);
                if (fileCategoryId == c.CategoryId)
                    html.Append("<a href=\"?fcatId=" + c.CategoryId + "\" class=\"tabSelected\">" + c.CategoryName + "</a>" + EOL);
                else
                    html.Append("<a href=\"?fcatId=" + c.CategoryId + "\">" + c.CategoryName + "</a>" + EOL);
                html.Append("</li>" + EOL);
            }

            html.Append("</ul>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Render the overview tab header
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="fileCategoryId"></param>
        /// <returns></returns>
        protected string renderTabHeaderOverview(CmsLanguage lang, int fileCategoryId)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<li>" + EOL);

            if (fileCategoryId == -1)
                html.Append("<a href=\"?\" class=\"tabSelected\">" + getOverviewText(lang) + "</a>" + EOL);
            else
                html.Append("<a href=\"?\">" + getOverviewText(lang) + "</a>" + EOL);

            html.Append("</li>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Render the tab content
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="fileCategoryId"></param>
        /// <param name="aggregatorData"></param>
        /// <returns></returns>
        protected string renderTabContent(CmsPage page, int identifier, CmsLanguage lang, int fileCategoryId, FileLibraryAggregatorData aggregatorData, RenderParameters renderParameters, CmsUrlFormat fileUrlFormat)
        {
            if (fileCategoryId == -1)
            {
                List<FileLibraryDetailsData> latestList = db.fetchLatestUpload(page.ChildPages, identifier, lang, aggregatorData.NumFilesForOverview);
                return renderTabContentOverview(page, identifier, lang, fileCategoryId, latestList, renderParameters, fileUrlFormat);
            }

            FileLibraryCategoryData cat = FileLibraryCategoryData.getCategoryFromList(categoryList, fileCategoryId);
            if (cat == null)
                return formatErrorMsg("File category not found.");

            StringBuilder html = new StringBuilder();
            html.Append("<div id=\"tab" + cat.CategoryId + "\" class=\"tabContentSelected\">" + EOL);
            string pageNum = renderPageNum(page, identifier, lang, cat, aggregatorData);
            if (pageNum != "")
                pageNum = "<div style=\"font-size: smaller; text-align: right;\">" + getPageText(lang) + ": " + pageNum + "</div>";

            List<FileLibraryDetailsData> fileList = getFileList(page, identifier, lang, cat, aggregatorData, renderParameters);
            html.Append(pageNum);
            html.Append(renderTableForFileList(page, identifier, lang, fileList, false, renderParameters, fileUrlFormat));
            html.Append(pageNum);
            html.Append("</div>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Render the overview tab content (latest files uploaded)
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="categoryId"></param>
        /// <param name="latestList"></param>
        /// <returns></returns>
        protected string renderTabContentOverview(CmsPage page, int identifier, CmsLanguage lang, int categoryId, List<FileLibraryDetailsData> latestList, RenderParameters renderParameters, CmsUrlFormat fileUrlFormat)
        {
            StringBuilder html = new StringBuilder();
            if (categoryId == -1)
                html.Append("<div id=\"tabOverview\" class=\"tabContentSelected\">" + EOL);
            else
                html.Append("<div id=\"tabOverview\">" + EOL);

            if (latestList.Count > 0)
            {
                html.Append("<p>" + getNewUploadText(lang) + ":</p>");
                html.Append(renderTableForFileList(page, identifier, lang, latestList, true, renderParameters, fileUrlFormat));
            }
            else
                html.Append("<p>No files yet.</p>");

            html.Append("</div>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Render the page num, and links to other pages
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="cat"></param>
        /// <param name="aggregatorData"></param>
        /// <returns></returns>
        protected string renderPageNum(CmsPage page, int identifier, CmsLanguage lang, FileLibraryCategoryData cat, FileLibraryAggregatorData aggregatorData)
        {
            int offset = PageUtils.getFromForm("offset", 0);
            int countPerPage = aggregatorData.NumFilesPerPage;
            int totalRecord = db.fetchCountByCategory(cat, page.ChildPages, identifier, lang);
            double totalPage = Math.Ceiling(((double)totalRecord) / ((double)countPerPage));

            int fileCategory = getFileCategoryId();
            string parmFileCategory = "";
            if (fileCategory != -1)
                parmFileCategory = "fcatId=" + fileCategory.ToString() + "&";

            string url = page.getUrl(lang) + "?" + parmFileCategory + "offset=";
            List<PageNumberAnchor> anchorList = new List<PageNumberAnchor>();
            for (int x = 0; x < totalPage; x++)
                anchorList.Add(new PageNumberAnchor(url+x.ToString(), x+1, ""));

            return PageNumberAnchorUtils.toHtmlString(anchorList, offset, 1, "... ");
        }

        /// <summary>
        /// Render a HTML TABLE to display the files.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="fileList"></param>
        /// <param name="showCategory"></param>
        /// <returns></returns>
        protected string renderTableForFileList(CmsPage page, int identifier, CmsLanguage lang, List<FileLibraryDetailsData> fileList, bool showCategory, RenderParameters renderParameters, CmsUrlFormat fileUrlFormat)
        {
            if (fileList.Count == 0)                
                return "<p class=\"NoResults\">There are no files in this category.</p>";

            StringBuilder html = new StringBuilder();
            html.Append("<table border=\"0\" cellpadding=\"8\" cellspacing=\"0\">" + EOL);
            html.Append("<tr>");
            if (showCategory)
                html.Append("<td>" + getCategoryText(lang) + "</td>");

            html.Append("<td>" + getFileNameText(lang) + "</td><td>" + getAttachedEventText(lang) + "</td><td> </td></tr>" + EOL);

            foreach (FileLibraryDetailsData d in fileList)
            {
                html.Append("<tr valign=\"top\">" + EOL);
                if (showCategory)
                    html.Append("<td>" + d.getCategoryName(categoryList) + "</td>" + EOL);

                string iconTag = IconUtils.getIconTag(CmsContext.ApplicationPath, false, d.fileExtension);
                string urlDetails = "";
                // -- link direct to the file only if the current user can't edit the file.
                if (renderParameters.fileLinkMode == RenderParameters.FileLinkMode.LinkToFile && ! page.currentUserCanWrite )
                {
                    urlDetails = FileLibraryDetailsData.getDownloadUrl(page, identifier, lang, d.FileName, fileUrlFormat);
                }
                else
                {
                    urlDetails = CmsContext.getPageById(d.DetailsPageId).getUrl(lang);
                }

                html.Append("<td>" + EOL);
                html.Append(iconTag + EOL);
                html.Append(FileLibraryDetailsData.getDownloadAnchorHtml(page, identifier,lang, d.FileName, fileUrlFormat) + EOL);
                html.Append("</td>" + EOL);

                string eventHtml = "(n/a)";
                if (d.EventPageId > -1)
                {
                    CmsPage eventPage = CmsContext.getPageById(d.EventPageId);
                    eventHtml = "<a href=\"" + eventPage.getUrl(lang) + "\">" + eventPage.getTitle(lang) + "</a>" + EOL;
                }
                html.Append("<td>" + eventHtml + "</td>" + EOL);

                html.Append("<td><a class=\"rightArrowLink\" href=\"" + urlDetails + "\">" + getSeeFileDetailsText(lang) + "</a></td>");
                html.Append("</tr>" + EOL);
            }
            html.Append("</table>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Render a form for file upload
        /// </summary>
        /// <param name="page"></param>
        /// <param name="lang"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string renderUploadForm(CmsPage page, CmsLanguage lang, string controlId)
        {
            // Traditional upload
            StringBuilder html = new StringBuilder();
            html.Append("<p>" + EOL);
            string formId = controlId + "uploadForm";
            html.Append(page.getFormStartHtml(formId, "return uploadFormSubmit('" + lang.shortCode + "');") + EOL);
            html.Append("<div>" + getAddFileText(lang) + ":</div>" + EOL);
            html.Append("<p>" + EOL);
            html.Append("<label>" + getCategoryText(lang) + ":</label><br />" + EOL);
            html.Append(getCategoryOption(lang, controlId));
            html.Append("</p>" + EOL);
            
            if (FileLibraryCategoryData.atLeastOneCategoryRequiresAnEvent(categoryList.ToArray()))
            {
                html.Append("<p>" + EOL);
                html.Append("<label>" + getAttachToEventText(lang) + ":</label><br />" + EOL);
                html.Append(getEventOption(lang, controlId, -1) + EOL);
                html.Append("</p>" + EOL);
            }
            else
            {
                string htmlName = controlId + "eventPageId_" + lang.shortCode;
                html.Append(PageUtils.getHiddenInputHtml(htmlName, -1));
            }

            html.Append("<p>");
            html.Append("</p>");

            html.Append("<p>" + EOL);
            html.Append("<label>" + getFileText(lang) + ":</label> (" + getMaxFileSizeText(lang) + ": " + PageUtils.MaxUploadFileSize + ")<br />" + EOL);
            html.Append("<input id=\"" + controlId + "filePath_" + lang.shortCode + "\" type=\"file\" name=\"" + controlId + "filePath_" + lang.shortCode + "\" />" + EOL);
            html.Append(PageUtils.getHiddenInputHtml(controlId + "action", "postFile") + EOL);
            html.Append("</p>" + EOL);
            html.Append("<input type=\"submit\" name=\"" + controlId + "FileUpload\" value=\"" + getUploadButtonText(lang) + "\" />" + EOL);
            html.Append(page.getFormCloseHtml(formId) + EOL);
            html.Append("</p>" + EOL);
            return html.ToString();
        }

        private string renderAsList(CmsPage page, int identifier, CmsLanguage lang, FileLibraryAggregatorData aggregatorData, RenderParameters renderParameters, CmsUrlFormat fileUrlFormat)
        {
            
            CmsPage rootPageToGatherFrom = page;
            if (renderParameters.PageIdToGatherFilesFrom >= 0)
                rootPageToGatherFrom = CmsContext.getPageById(renderParameters.PageIdToGatherFilesFrom);

            CmsContext.PageGatheringMode gatherMode = CmsContext.PageGatheringMode.ChildPagesOnly;
            if (renderParameters.RecursiveGatherFiles)
                gatherMode = CmsContext.PageGatheringMode.FullRecursion;

            CmsPage[] pagesToGetDetailsFrom = CmsContext.getAllPagesWithPlaceholder("FileLibraryDetails", rootPageToGatherFrom, gatherMode);

            List<FileLibraryDetailsData> fileDetails = db.fetchDetailsData(pagesToGetDetailsFrom, identifier, lang);

            StringBuilder html = new StringBuilder();
            if (fileDetails.Count == 0)
            {
                html.Append("<p class=\"NoResults\">No files are available</p>");
            }
            else
            {
                List<FileLibraryCategoryData> fileCategories = db.fetchCategoryList(lang);
                foreach (FileLibraryCategoryData cat in fileCategories)
                {
                    FileLibraryDetailsData[] categoryFiles = FileLibraryDetailsData.getFilesByCategory(fileDetails, cat);
                    if (categoryFiles.Length > 0)
                    {
                        html.Append("<strong>" + cat.CategoryName + "</strong>" + EOL);
                        html.Append("<ul>"+EOL);
                        foreach (FileLibraryDetailsData file in categoryFiles)
                        {
                            string urlDetails = "";
                            // -- link direct to the file only if the current user can't edit the file.
                            if (renderParameters.fileLinkMode == RenderParameters.FileLinkMode.LinkToFile && !page.SecurityZone.canWrite(CmsContext.currentWebPortalUser))
                            {
                                urlDetails = FileLibraryDetailsData.getDownloadUrl(CmsContext.getPageById(file.DetailsPageId), identifier, lang, file.FileName, fileUrlFormat);
                            }
                            else
                            {
                                urlDetails = CmsContext.getPageById(file.DetailsPageId).getUrl(lang);
                            }

                            html.Append("<li>");
                            string title = CmsContext.getPageById(file.DetailsPageId).getTitle(lang);

                            html.Append("<a target=\"_blank\" href=\"" + urlDetails + "\">" + title + "</a>");
                            html.Append("</li>" + EOL);
                        }
                        html.Append("</ul>");
                    } // if
                } // foreach
            }

            return "<div class=\"FileLibraryAggregator\">" + html.ToString() + "</div>";
        }


        /// <summary>
        /// Render the file library aggregator page in view mode
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="langToRenderFor"></param>
        /// <param name="paramList"></param>
        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string controlId = "fileLibrary_" + page.ID.ToString() + "_" + identifier.ToString() + langToRenderFor.shortCode + "_";
            UpdateFileLibraryCommandsInEditMenu(page);

            RenderParameters renderParameters = RenderParameters.fromParamList(paramList);
            CmsUrlFormat fileUrlFormat = CmsUrlFormat.RelativeToRoot;

            addCssAndScript(page, renderParameters);

            StringBuilder html = new StringBuilder();
            bool canWrite = page.SecurityZone.canWrite(CmsContext.currentWebPortalUser);
            if (canWrite)
                html.Append("<p>" + handleUploadSubmit(page, identifier, langToRenderFor, controlId) + "</p>" + EOL);

            FileLibraryAggregatorData aggregatorData = db.fetchAggregatorData(page, identifier, langToRenderFor, true);
            categoryList = db.fetchCategoryList(langToRenderFor);

            if (renderParameters.displayMode == RenderParameters.DisplayMode.Tabs)
            {
                int fileCategoryId = getFileCategoryId();
                html.Append("<div class=\"tab\">" + EOL);
                html.Append(renderTabHeader(langToRenderFor, fileCategoryId));
                html.Append(renderTabContent(page, identifier, langToRenderFor, fileCategoryId, aggregatorData, renderParameters, fileUrlFormat));
                html.Append("</div>" + EOL);
            }
            else if (renderParameters.displayMode == RenderParameters.DisplayMode.List)
            {
                html.Append(renderAsList(page, identifier, langToRenderFor, aggregatorData, renderParameters, fileUrlFormat));
            }

            if (canWrite)
                html.Append(renderUploadForm(page, langToRenderFor, controlId));

            writer.Write(html.ToString());
        }

        /// <summary>
        /// Render the file library aggregator page in edit mode
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="langToRenderFor"></param>
        /// <param name="paramList"></param>
        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            RenderParameters renderParameters = RenderParameters.fromParamList(paramList);
            
            // if we're showing files in a list, we have nothing to configure. So render that list.
            if (renderParameters.displayMode == RenderParameters.DisplayMode.List)
            {
                RenderInViewMode(writer, page, identifier, langToRenderFor, paramList);
                return;
            }
            
            string controlId = "fileLibrary_" + page.ID.ToString() + "_" + identifier.ToString() + langToRenderFor.shortCode + "_";

            FileLibraryAggregatorData aggregatorData = new FileLibraryAggregatorData();
            aggregatorData = db.fetchAggregatorData(page, identifier, langToRenderFor, true);

            if (PageUtils.getFromForm(controlId + "action", "") == "update")
            {
                aggregatorData.NumFilesForOverview = PageUtils.getFromForm(controlId + "numFilesOverview", aggregatorData.NumFilesForOverview);
                aggregatorData.NumFilesPerPage = PageUtils.getFromForm(controlId + "numFilesPerPage", aggregatorData.NumFilesPerPage);
                db.updateAggregatorData(page, identifier, langToRenderFor, aggregatorData);
            }

            StringBuilder html = new StringBuilder();
            html.Append("<table>" + EOL);

            html.Append("<tr>" + EOL);
            html.Append("<td>Number of files on the overview tab: </td>" + EOL);
            html.Append("<td>" + EOL);
            html.Append(PageUtils.getInputTextHtml(controlId + "numFilesOverview", controlId + "numFilesOverview", aggregatorData.NumFilesForOverview.ToString(), 3, 3) + EOL);
            html.Append("</td>" + EOL);
            html.Append("</tr>" + EOL);

            html.Append("<tr>" + EOL);
            html.Append("<td>Number of files to show per page:</td>" + EOL);
            html.Append("<td>" + EOL);
            html.Append(PageUtils.getInputTextHtml(controlId + "numFilesPerPage", controlId + "numFilesPerPage", aggregatorData.NumFilesPerPage.ToString(), 3, 3) + EOL);
            html.Append("</td>" + EOL);
            html.Append("</tr>" + EOL);

            html.Append("</table>" + EOL);
            html.Append(PageUtils.getHiddenInputHtml(controlId + "action", "update") + EOL);

            writer.Write(html.ToString());
        }

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            RenderParameters renderParameters = RenderParameters.fromParamList(placeholderDefinition.ParamList);
            CmsPage rootPageToGatherFrom = page;
            if (renderParameters.PageIdToGatherFilesFrom >= 0)
                rootPageToGatherFrom = CmsContext.getPageById(renderParameters.PageIdToGatherFilesFrom);

            CmsContext.PageGatheringMode gatherMode = CmsContext.PageGatheringMode.ChildPagesOnly;
            if (renderParameters.RecursiveGatherFiles)
                gatherMode = CmsContext.PageGatheringMode.FullRecursion;

            CmsPage[] pagesToGetDetailsFrom = CmsContext.getAllPagesWithPlaceholder("FileLibraryDetails", rootPageToGatherFrom, gatherMode);

            List<FileLibraryDetailsData> fileDetails = db.fetchDetailsData(pagesToGetDetailsFrom, placeholderDefinition.Identifier, langToRenderFor);
            CmsUrlFormat fileUrlFormat = CmsUrlFormat.FullIncludingProtocolAndDomainName;

            List<Rss.RssItem> ret = new List<Rss.RssItem>();

            foreach (FileLibraryDetailsData file in fileDetails)
            {
                CmsPage childPage = CmsContext.getPageById(file.DetailsPageId);
                if (childPage.isVisibleForCurrentUser)
                {
                    Rss.RssItem rssItem = CreateAndInitRssItem(childPage, langToRenderFor);
                    rssItem.Link = new Uri(FileLibraryDetailsData.getDownloadUrl(page, file.Identifier, langToRenderFor, file.FileName, fileUrlFormat), UriKind.RelativeOrAbsolute);

                    rssItem.Description = childPage.renderPlaceholdersToString("FileLibraryDetails", langToRenderFor, CmsPage.RenderPlaceholderFilterAction.RunAllPageAndPlaceholderFilters);

                    ret.Add(rssItem);
                }
            } // foreach

            return ret.ToArray();
        
        }
    }
}
