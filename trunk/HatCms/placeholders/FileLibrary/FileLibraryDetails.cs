using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using HatCMS.Placeholders;
using Hatfield.Web.Portal;
using Hatfield.Web.Portal.Imaging;
using System.Collections.Specialized;
using HatCMS.Placeholders.Calendar;

namespace HatCMS.Placeholders
{
    public class FileLibraryDetails : BaseCmsPlaceholder
    {
        protected string EOL = Environment.NewLine;
        protected FileLibraryDb db = new FileLibraryDb();        
        protected List<FileLibraryCategoryData> categoryList;        

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
            ret.Add(new CmsDatabaseTableDependency("FileLibraryAggregator", new string[] {"PageId","Identifier","LangCode","NumFilesOverview","NumFilesPerPage"}));
            ret.Add(new CmsDatabaseTableDependency("FileLibraryDetails", new string[] {"PageId","Identifier","LangCode","FileName","CategoryId","Author","Description","LastModified","CreatedBy","EventPageId"}));
            ret.Add(new CmsDatabaseTableDependency("FileLibraryCategory", new string[] { "CategoryId", "LangCode", "EventRequired", "CategoryName", "SortOrdinal" }));

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

            // make sure that all files associated with FileLibraryDetails placeholder are live.
            Dictionary<CmsPage, CmsPlaceholderDefinition[]> phDefsDict = CmsContext.getAllPlaceholderDefinitions("FileLibraryDetails", CmsContext.HomePage, CmsContext.PageGatheringMode.FullRecursion);
            foreach (CmsPage page in phDefsDict.Keys)
            {
                foreach (CmsPlaceholderDefinition phDef in phDefsDict[page])
                {
                    foreach (CmsLanguage lang in CmsConfig.Languages)
                    {
                        FileLibraryDetailsData fileData = db.fetchDetailsData(page, phDef.Identifier, lang, true);
                        if (fileData.FileName != "")
                        {
                            string filenameOnDisk = FileLibraryDetailsData.getTargetNameOnDisk(page, phDef.Identifier, lang, fileData.FileName);
                            ret.Add(new CmsFileDependency(filenameOnDisk));
                            if (fileData.EventPageId >= 0) // make sure that the linked event page exists.
                                ret.Add(new CmsPageDependency(fileData.EventPageId, new CmsLanguage[] { lang }));
                        }
                    } // foreach lang
                } // foreach placeholder definition
            }// foreach page

            return ret.ToArray();
        }

        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented;
        }

        public static string DeleteThisFileDetailsPageAction(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
        {
            NameValueCollection paramList = new NameValueCollection();
            paramList.Add("target", pageToRenderFor.ID.ToString());

            string confirmText = "Do you really want to delete this file?";
            int numPagesToDelete = pageToRenderFor.getLinearizedPages().Keys.Count;
            if (numPagesToDelete > 1)
                confirmText = "Do you really want to delete this file and all " + (numPagesToDelete - 1) + " sub-pages?";

            string deleteFilesUrl = CmsContext.getUrlByPagePath(CmsConfig.getConfigValue("DeleteFileLibraryPath", "/_admin/actions/deleteFileLibrary"), paramList, langToRenderFor);
            return "<a href=\"#\" onclick=\"EditMenuConfirmModal('" + confirmText + "','" + deleteFilesUrl + "',300, 300);\"><strong>Delete</strong> this file</a>";

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

            deletePageAction.doRenderToString = DeleteThisFileDetailsPageAction;            
        }


        /// <summary>
        /// Include the JS and CSS files.
        /// </summary>
        /// <param name="page"></param>
        protected void addCssAndScript(CmsPage page)
        {
            CmsPageHeadSection h = page.HeadSection;
            h.AddJavascriptFile("js/_system/jquery/jquery-1.4.1.min.js");

            StringBuilder css = new StringBuilder();
            css.Append(".listing > div > label { margin: 5px; display: block; float: left; clear: left; text-align: right; padding-left: 1em; width: 160px; }" + EOL);
            css.Append(".listing > div > span  { margin: 5px; display: block; float: left; clear: right; text-align: left; padding-left: 1em; width: 300px; }" + EOL);
            h.AddCSSStyleStatements(css.ToString());
        }

        /// <summary>
        /// Display an error message if event is required, but no event attached.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        protected string checkEventAttached(CmsPage page, int identifier, CmsLanguage lang, FileLibraryDetailsData fileData)
        {
            bool eventRequired = fileData.isEventRequired(categoryList);
            if (eventRequired && fileData.EventPageId == -1)
                return formatErrorMsg(getEventNotAttachedText(lang));

            return "";
        }

        /// <summary>
        /// Create the FileLibraryDetailsData object
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected FileLibraryDetailsData createFileDetails(CmsLanguage lang, string controlId)
        {
            FileLibraryDetailsData submitted = new FileLibraryDetailsData();
            submitted.CategoryId = PageUtils.getFromForm(controlId + "categoryId", 0);
            submitted.Author = PageUtils.getFromForm(controlId + "author", "");
            submitted.Description = PageUtils.getFromForm(controlId + "description", "");
            submitted.EventPageId = PageUtils.getFromForm(controlId + "eventPageId", -1);
            return submitted;
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
            return "<div style=\"font-weight: bold; color: green;\">" + msg + "</div>" + EOL;
        }

        /// <summary>
        /// Get the name according to file extension
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        protected string getFileTypeName(FileLibraryDetailsData fileData)
        {
            string ext = System.IO.Path.GetExtension(fileData.FileName);
            return FileUtils.getFileTypeDescription(ext);
        }

        /// <summary>
        /// Generate HTML SELECT and OPTION tags for FileLibraryCategory
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="controlId"></param>
        /// <param name="selectedId"></param>
        /// <returns></returns>
        protected string getCategoryOption(CmsLanguage lang, string controlId, int selectedId)
        {
            StringBuilder html = new StringBuilder();
            string htmlName = controlId + "categoryId";
            string cssClass = "fileLibrary_categoryId_" + lang.shortCode;
            html.Append("<select name=\"" + htmlName + "\" id=\"" + htmlName + "\" class=\"" + cssClass + "\">" + EOL);
            html.Append(FileLibraryCategoryData.getCategoryOptionTag(lang, categoryList, selectedId));
            html.Append("</select>" + EOL);
            return html.ToString();
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
            html.Append(String.Format(tableTag, new string[]{domId, cssClass, displayStyle}));

            html.Append("<tr valign=\"top\">");
            html.Append("<td style=\"font-size: smaller;\">" + getDateTimeText(lang) + ":</td>");
            html.Append("<td style=\"font-size: smaller;\">");
            string endDateTime = (String.Format("{0:yyyy/MM/dd}", eventData.StartDateTime) == String.Format("{0:yyyy/MM/dd}", eventData.EndDateTime))? String.Format("{0:HH:mm}", eventData.EndDateTime) : "<br />" + String.Format("{0:yyyy/MM/dd HH:mm}", eventData.EndDateTime);
            html.Append(String.Format("{0:yyyy/MM/dd HH:mm}", eventData.StartDateTime) + " to " + endDateTime);
            html.Append("</td>");
            html.Append("</tr>" + EOL);

            html.Append("<tr valign=\"top\">");
            html.Append("<td style=\"font-size: smaller;\">" + getEventCategoryText(lang) + ":</td>");
            html.Append("<td style=\"font-size: smaller;\">");
            html.Append(eventData.getCategoryTitle(eventCategoryList));
            html.Append("</td>");
            html.Append("</tr>" + EOL);

            html.Append("<tr valign=\"top\">");
            html.Append("<td style=\"font-size: smaller;\">" + getDescriptionText(lang) + ":</td>");
            html.Append("<td style=\"font-size: smaller;\">");
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
        /// <param name="eventRequired"></param>
        /// <returns></returns>
        protected string getEventOption(CmsLanguage lang, string controlId, int selectedId, bool eventRequired)
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
            string htmlName = controlId + "eventPageId";
            string selectTag = "<select name=\"{0}\" id=\"{1}\" onchange=\"$('.{2}table').css('display','none'); $('#{3}table_' + this.value).fadeIn();\">" + EOL;
            html.Append(String.Format(selectTag, new string[] { htmlName, htmlName, controlId, controlId }));

            string optionTag = "<option value=\"{0}\" {1}>{2}</option>" + EOL;
            string optionSelected = (selectedId == -1) ? "selected=\"selected\"" : "";
            if (eventRequired == false) // if event not required, provide an option to select n/a
                html.Append(String.Format(optionTag, new string[]{ "-1", optionSelected, "(n/a)"}));

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

#region multi-lang text
        protected string getAttachedEventText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.AttachedEventText", "Attached event", lang);
        }

        protected string getAuthorText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.AuthorText", "Author", lang);
        }

        protected string getBackToText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.BackToText", "Back to", lang);
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

        protected string getDownloadText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.DownloadText", "Back to", lang);
        }

        protected string getEditText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.EditText", "edit", lang);
        }

        protected string getEventCategoryText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.EventCategoryText", "Event category", lang);
        }

        protected string getEventNotAttachedText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.EventNotAttachedText", "Warning: File has no event attached", lang);
        }

        protected string getFileText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.FileText", "File", lang);
        }

        protected string getImagePreviewText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.ImagePreviewText", "Image preview", lang);
        }

        protected string getLastUpdatedText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.LastUpdatedText", "Last updated", lang);
        }

        protected string getLinkOpensNewWindowText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.LinkOpensNewWindowText", "link opens new window", lang);
        }

        protected string getOverviewText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.OverviewText", "Overview", lang);
        }

        protected string getTabText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.TabText", "tab", lang);
        }

        protected string getUploadedByText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("FileLibrary.UploadedByText", "Uploaded by", lang);
        }
#endregion
        
        /// <summary>
        /// Handle the form submit for changing file details
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string handleFormSubmit(CmsPage page, int identifier, CmsLanguage lang, string controlId)
        {
            if (PageUtils.getFromForm(controlId + "action", "") != "update")
                return "";

            FileLibraryDetailsData fileData = createFileDetails(lang, controlId);
            if (db.updateDetailsData(page, identifier, lang, fileData) == false)
                return formatErrorMsg("There was a database problem while saving the record.");
            else
                return formatNormalMsg("Record updated.");
        }

        /// <summary>
        /// Render a "Back to" link
        /// </summary>
        /// <param name="pageToGoBackTo"></param>
        /// <param name="lang"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        protected string renderBackLinks(CmsPage pageToGoBackTo, CmsLanguage lang, FileLibraryDetailsData fileData)
        {
            StringBuilder html = new StringBuilder();
            string link = "<p><a class=\"backToPrev\" href=\"{0}\">{1} {2} {3}</a></p>" + EOL;
            string backTo = getBackToText(lang);
            string tab = getTabText(lang);

            string backUrl = pageToGoBackTo.getUrl(lang);
            html.Append(String.Format(link, new string[] { backUrl, backTo, getOverviewText(lang), tab }));

            Dictionary<string, string> linkParams = new Dictionary<string,string>();
            linkParams.Add("catId", fileData.CategoryId.ToString());
            backUrl = pageToGoBackTo.getUrl(linkParams, lang);
            html.Append(String.Format(link, new string[] { backUrl, backTo, fileData.getCategoryName(categoryList), tab }));

            return html.ToString();
        }

        /// <summary>
        /// Render a HTML DIV
        /// </summary>
        /// <param name="innerHtml"></param>
        /// <returns></returns>
        protected string renderDiv(string innerHtml)
        {
            StringBuilder html = new StringBuilder("<div>");
            html.Append(innerHtml);
            html.Append("</div>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Render a HTML DIV, with name on the left and value on the right
        /// </summary>
        /// <param name="labelHtml"></param>
        /// <param name="innerHtml"></param>
        /// <returns></returns>
        protected string renderDiv(string labelHtml, string innerHtml)
        {
            StringBuilder html = new StringBuilder("<div>" + EOL);
            if (labelHtml == "")
                html.Append("<label> </label>" + EOL);
            else
                html.Append("<label>" + labelHtml + ":</label>" + EOL);

            html.Append("<span>" + innerHtml + "</span>" + EOL);
            html.Append("</div>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Render left hand side of the file details page (file icon and size)
        /// </summary>
        /// <param name="detailsPage"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        protected string renderLeftPane(CmsPage detailsPage, int identifier, CmsLanguage lang, FileLibraryDetailsData fileData)
        {            
            string fName = fileData.FileName;
            StringBuilder html = new StringBuilder();
            html.Append("<div style=\"float: left; text-align: center;\">" + EOL);

            string iconTag = IconUtils.getIconTag(CmsContext.ApplicationPath, true, fileData.fileExtension);
            html.Append(renderDiv(FileLibraryDetailsData.getDownloadAnchorHtml(detailsPage, identifier, lang, fName, iconTag, "_blank", "")));

            html.Append(renderDiv(FileLibraryDetailsData.getDownloadAnchorHtml(detailsPage, identifier, lang, fName, getDownloadText(lang), "_blank", "downloadLink")));

            html.Append("<p style=\"font-style: italic; font-size: smaller;\">(" + getLinkOpensNewWindowText(lang) + ")</p>" + EOL);

            html.Append(renderDiv(getFileTypeName(fileData)));
            long size = FileLibraryDetailsData.getFileSize(detailsPage, identifier, lang, fName);
            html.Append(renderDiv(StringUtils.formatFileSize(size)));

            html.Append("</div>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Render right hand side of the file details page (all details)
        /// </summary>
        /// <param name="detailsPage"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        protected string renderRightPane(CmsPage detailsPage, int identifier, CmsLanguage lang, FileLibraryDetailsData fileData)
        {            
            string fName = fileData.FileName;
            StringBuilder html = new StringBuilder();
            html.Append("<div style=\"float: left;\" class=\"listing\">" + EOL);

            html.Append(renderDiv(getFileText(lang), FileLibraryDetailsData.getDownloadAnchorHtml(detailsPage, identifier, lang, fName)));
            html.Append(renderDiv(getCategoryText(lang), fileData.getCategoryName(categoryList)));

            html.Append(renderDiv(getAuthorText(lang), fileData.Author));
            html.Append(renderDiv(getDocumentAbstractText(lang), StringUtils.nl2br(fileData.Description)));

            if (getFileTypeName(fileData).EndsWith("graphic", StringComparison.CurrentCultureIgnoreCase))
            {
                string imgPreviewUrl = showThumbPage.getThumbDisplayUrl(FileLibraryDetailsData.getDownloadUrl(detailsPage, identifier, lang, fName), 200, -1);
                string imgTag = "<img border=\"0\" src=\"" + imgPreviewUrl + "\"></a>";
                html.Append(renderDiv(getImagePreviewText(lang), FileLibraryDetailsData.getDownloadAnchorHtml(detailsPage, identifier, lang, fName, imgTag, "_blank", "")));
            }

            bool eventRequired = FileLibraryCategoryData.isEventRequired(categoryList, fileData.CategoryId);
            if (eventRequired)
            {
                string eventHtml = "(n/a)";
                if (fileData.EventPageId > -1)
                {
                    CmsPage eventPage = CmsContext.getPageById(fileData.EventPageId);
                    eventHtml = "<a href=\"" + eventPage.getUrl(lang) + "\">" + eventPage.getTitle(lang) + "</a>" + EOL;
                }
                html.Append(renderDiv(getAttachedEventText(lang), eventHtml));
            }

            WebPortalUser u = WebPortalUser.FetchUser(fileData.CreatedBy, CmsPortalApplication.GetInstance());
            string uploadPersonName = (u == null) ? fileData.CreatedBy : u.FullName;
            html.Append(renderDiv(getUploadedByText(lang), uploadPersonName));

            html.Append(renderDiv(getLastUpdatedText(lang), detailsPage.LastUpdatedDateTime.ToString("MMMM d yyyy h:mm tt")));

            html.Append("</div>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Under edit mode, render the right hand side of the file details page as form (HTML INPUT or TEXTAREA, etc...)
        /// </summary>
        /// <param name="detailsPage"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="fileData"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string renderRightPaneForm(CmsPage detailsPage, int identifier, CmsLanguage lang, FileLibraryDetailsData fileData, string controlId)
        {            
            string fName = fileData.FileName;
            StringBuilder html = new StringBuilder();
            html.Append("<div style=\"float: left;\" class=\"listing\">" + EOL);

            html.Append(renderDiv(getFileText(lang), FileLibraryDetailsData.getDownloadAnchorHtml(detailsPage, identifier, lang, fName)));

            string cssClass = "fileLibrary_categoryId";
            string popupCategory = FileLibraryCategoryData.getEditPopupAnchor(lang, cssClass, getEditText(lang));
            string htmlId = controlId + "categoryId";
            NameValueCollection categoryColl = new NameValueCollection();
            foreach(FileLibraryCategoryData c in categoryList)
                categoryColl.Add(c.CategoryId.ToString(), c.CategoryName);
            string selectCategory = getCategoryOption(lang, controlId, fileData.CategoryId);
            html.Append(renderDiv(getCategoryText(lang) + " " + popupCategory, selectCategory));

            htmlId = controlId + "author";
            html.Append(renderDiv(getAuthorText(lang), PageUtils.getInputTextHtml(htmlId, htmlId, fileData.Author, 30, 50)));

            htmlId = controlId + "description";
            html.Append(renderDiv(getDocumentAbstractText(lang), PageUtils.getTextAreaHtml(htmlId, htmlId, fileData.Description, 25, 5)));

            if (getFileTypeName(fileData).EndsWith("graphic", StringComparison.CurrentCultureIgnoreCase))
            {
                string imgPreviewUrl = showThumbPage.getThumbDisplayUrl(FileLibraryDetailsData.getDownloadUrl(detailsPage, identifier, lang, fName), 200, -1);
                string imgTag = "<img border=\"0\" src=\"" + imgPreviewUrl + "\"></a>";
                html.Append(renderDiv(getImagePreviewText(lang), FileLibraryDetailsData.getDownloadAnchorHtml(detailsPage, identifier, lang, fName, imgTag, "_blank", "")));
            }

            bool eventRequired = FileLibraryCategoryData.isEventRequired(categoryList, fileData.CategoryId);
            if (eventRequired || fileData.EventPageId >= 0 )
            {
                htmlId = controlId + "eventPageId";
                html.Append(renderDiv(getAttachedEventText(lang), getEventOption(lang, controlId, fileData.EventPageId, eventRequired)));
            }

            WebPortalUser u = WebPortalUser.FetchUser(fileData.CreatedBy, CmsPortalApplication.GetInstance());
            string uploadPersonName = (u == null) ? fileData.CreatedBy : u.FullName;
            html.Append(renderDiv(getUploadedByText(lang), uploadPersonName));

            html.Append(renderDiv(getLastUpdatedText(lang), detailsPage.LastUpdatedDateTime.ToString("MMMM d yyyy h:mm tt")));

            html.Append("</div>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Render the file details page in view mode
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="langToRenderFor"></param>
        /// <param name="paramList"></param>
        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            UpdateFileLibraryCommandsInEditMenu(page);
            addCssAndScript(page);
            StringBuilder html = new StringBuilder();
            categoryList = db.fetchCategoryList(langToRenderFor);            

            FileLibraryDetailsData fileData = db.fetchDetailsData(page, identifier, langToRenderFor, true);

            html.Append(renderBackLinks(page, langToRenderFor, fileData));
            html.Append(checkEventAttached(page, identifier, langToRenderFor, fileData));
            html.Append("<p style=\"padding: 0.5em;\">" + EOL);
            html.Append(renderLeftPane(page, identifier, langToRenderFor, fileData));
            html.Append(renderRightPane(page, identifier, langToRenderFor, fileData));
            html.Append("</p>" + EOL);
            writer.Write(html.ToString());
        }

        /// <summary>
        /// Render the file details page in edit mode
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="langToRenderFor"></param>
        /// <param name="paramList"></param>
        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string controlId = "fileLibrary_" + page.ID.ToString() + "_" + identifier.ToString() + "_" + langToRenderFor.shortCode + "_";

            addCssAndScript(page);
            StringBuilder html = new StringBuilder();
            categoryList = db.fetchCategoryList(langToRenderFor);            

            html.Append(handleFormSubmit(page, identifier, langToRenderFor, controlId));
            FileLibraryDetailsData fileData = db.fetchDetailsData(page, identifier, langToRenderFor, false);
            
            html.Append("<p style=\"padding: 0.5em;\">" + EOL);
            html.Append(renderLeftPane(page, identifier, langToRenderFor, fileData));
            html.Append(renderRightPaneForm(page, identifier, langToRenderFor, fileData, controlId));
            html.Append(PageUtils.getHiddenInputHtml(controlId + "action", "update") + EOL);
            html.Append("</p>" + EOL);
            writer.Write(html.ToString());
        }

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            
            Rss.RssItem rssItem = CreateAndInitRssItem(page, langToRenderFor);

            FileLibraryDetailsData fileData = db.fetchDetailsData(page, placeholderDefinition.Identifier, langToRenderFor, true);
            rssItem.Description = fileData.Description;
            rssItem.Author = fileData.Author;
            string controlId = "fileLibrary_" + page.ID.ToString() + "_" + placeholderDefinition.Identifier.ToString() + "_" + langToRenderFor.shortCode + "_";
            rssItem.Guid = new Rss.RssGuid(controlId);

            return new Rss.RssItem[] { rssItem };
        }
    }
}
