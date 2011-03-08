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
using Hatfield.Web.Portal;
using System.Collections.Specialized;
using Hatfield.Web.Portal.Imaging;

namespace HatCMS.placeholders.Calendar
{
    public class EventCalendarDetails : BaseCmsPlaceholder
    {        
        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            ret.Add(CmsFileDependency.UnderAppPath("images/_system/calendar/calendarIcon_16x16.png"));
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/DatePicker.js"));
            ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("EditCalendarCategoryPagePath", "/_admin/EventCalendarCategory"), CmsConfig.Languages));
            ret.Add(CmsControlDependency.UnderControlDir("_system/Internal/EventCalendarCategoryPopup.ascx", new DateTime(2010, 2, 17)));

            // -- Hatfield modified version of jquery.fullcalendar -- SimpleCalendar
            ret.Add(CmsFileDependency.UnderAppPath("_system/tools/Calendar/SimpleCalendarJsonData.ashx"));
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/EventCalendar/EventCalendarCategory.js"));

            // -- Date/Time picker and jQuery UI
            ret.Add(CmsFileDependency.UnderAppPath("css/_system/jquery-ui-lightness/jquery-ui-1.8.custom.css"));
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/jquery/jquery-ui-1.8.custom.min.js"));
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/jquery/jquery-ui-timepicker-addon.min.js"));

            // -- database tables
            ret.Add(new CmsDatabaseTableDependency("EventCalendarAggregator", new string[] { "PageId", "Identifier", "LangCode", "ViewMode", "Deleted" }));
            ret.Add(new CmsDatabaseTableDependency("EventCalendarDetails", new string[] { "PageId", "Identifier", "LangCode", "Description", "CategoryId", "StartDateTime", "EndDateTime", "CreatedBy", "Deleted" }));
            ret.Add(new CmsDatabaseTableDependency("EventCalendarCategory", new string[] { "CategoryId", "LangCode", "ColorHex", "Title", "Description", "Deleted" }));

            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("EventCalendar.DefaultEventStartHour"));
            ret.Add(new CmsConfigItemDependency("EventCalendar.DefaultEventEndHour"));
            ret.Add(new CmsConfigItemDependency("EventCalendar.ButtonTodayText"));
            ret.Add(new CmsConfigItemDependency("EventCalendar.ButtonMonthText"));
            ret.Add(new CmsConfigItemDependency("EventCalendar.ButtonWeekText"));
            ret.Add(new CmsConfigItemDependency("EventCalendar.ButtonDayText"));
            ret.Add(new CmsConfigItemDependency("EventCalendar.AllDayText"));
            ret.Add(new CmsConfigItemDependency("EventCalendar.BackToText"));
            ret.Add(new CmsConfigItemDependency("EventCalendar.DescriptionText"));
            ret.Add(new CmsConfigItemDependency("EventCalendar.CategoryText"));
            ret.Add(new CmsConfigItemDependency("EventCalendar.StartDateTimeText"));
            ret.Add(new CmsConfigItemDependency("EventCalendar.EndDateTimeText"));
            ret.Add(new CmsConfigItemDependency("EventCalendar.CategoryColor"));

            return ret.ToArray();
        }

        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented;
        }

        /// <summary>
        /// Load the jQuery date/time picker
        /// </summary>
        /// <param name="page"></param>
        protected void addCssAndScriptForDateTimePicker(CmsPage page)
        {
            page.HeadSection.AddCSSFile("css/_system/jquery-ui-lightness/jquery-ui-1.8.custom.css");
            page.HeadSection.AddJavascriptFile("js/_system/jquery/jquery-1.4.1.min.js");
            page.HeadSection.AddJavascriptFile("js/_system/jquery/jquery-ui-1.8.custom.min.js");
            page.HeadSection.AddJavascriptFile("js/_system/jquery/jquery-ui-timepicker-addon.min.js");
        }

        /// <summary>
        /// Init the date/time picker
        /// </summary>
        /// <param name="page"></param>
        /// <param name="controlId"></param>
        protected void addScriptForDateTimePickerRender(CmsPage page, string controlId)
        {
            string EOL = Environment.NewLine;
            StringBuilder js = new StringBuilder();
            js.Append("$('#" + controlId + "_startDateTime').datetimepicker( { dateFormat: 'yy-mm-dd,', timeFormat: 'h:mm TT', ampm: true, hourGrid: 12, minuteGrid: 15 } );" + EOL);
            js.Append("$('#" + controlId + "_endDateTime').datetimepicker( { dateFormat: 'yy-mm-dd,', timeFormat: 'h:mm TT', ampm: true, hourGrid: 12, minuteGrid: 15 } );" + EOL);
            page.HeadSection.AddJSOnReady(js.ToString());
        }

#region multi-lang text
        private string getAttachedFilesText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("EventCalendar.AttachedFilesText", "Attached files", lang);
        }

        private string getBackToText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("EventCalendar.BackToText", "Back to", lang);
        }

        private string getDescriptionText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("EventCalendar.DescriptionText", "Description", lang);
        }

        private string getCategoryText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("EventCalendar.CategoryText", "Category", lang);
        }

        private string getSeeFileDetailsText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("EventCalendar.SeeFileDetailsText", "See file details", lang);
        }

        private string getStartDateTimeText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("EventCalendar.StartDateTimeText", "Start Date/Time", lang);
        }

        private string getEndDateTimeText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("EventCalendar.EndDateTimeText", "End Date/Time", lang);
        }
#endregion

        /// <summary>
        /// Render the html to display an attached file
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="f"></param>
        /// <param name="u"></param>
        /// <returns></returns>
        protected string renderAttachedFile(CmsLanguage lang, FileLibraryDetailsData f, WebPortalUser u)
        {
            CmsPage detailsFilePage = CmsContext.getPageById(f.DetailsPageId);
            if (detailsFilePage.Zone.canRead(u) || detailsFilePage.Zone.canWrite(u))
            {
                
                string iconTag = IconUtils.getIconTag(CmsContext.ApplicationPath, false, f.fileExtension);
                string urlDownload = FileLibraryDetailsData.getDownloadAnchorHtml(detailsFilePage, f.Identifier, lang, f.FileName, f.FileName, "_blank", "");

                string urlPage = detailsFilePage.getUrl(lang);
                urlPage = "&#160;&#160;<a href=\"" + urlPage + "\" class=\"rightArrowLink\">" + getSeeFileDetailsText(lang) + "</a>";

                return iconTag + " " + urlDownload + urlPage;
            }
            return "";
        }

        /// <summary>
        /// Get all the files attached to this event and render the html.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected string renderAttachedFileList(CmsPage page, CmsLanguage lang)
        {
            List<FileLibraryDetailsData> fileList = new FileLibraryDb().fetchDetailsData(lang, page);
            if (fileList.Count == 0)
                return "";

            WebPortalUser u = CmsContext.currentWebPortalUser;
            List<string> renderedLinks = new List<string>();
            foreach (FileLibraryDetailsData f in fileList)
            {
                string link = renderAttachedFile(lang, f, u);
                if (link != "")
                    renderedLinks.Add(link);
            }
            if (renderedLinks.Count == 0)
                return "";

            StringBuilder html = new StringBuilder();
            html.Append("<tr valign=\"top\">");
            html.Append("<td>" + getAttachedFilesText(lang) + ":</td>");
            html.Append("<td>" + renderedLinks[0] + "</td>");
            html.Append("</tr>");

            for (int x = 1; x < renderedLinks.Count; x++)
            {
                html.Append("<tr valign=\"top\">");
                html.Append("<td> </td>");
                html.Append("<td>" + renderedLinks[x] + "</td>");
                html.Append("</tr>");
            }

            return html.ToString();
        }

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            // CmsContext.setCurrentCultureInfo(langToRenderFor);
            EventCalendarDb db = new EventCalendarDb();
            EventCalendarDb.EventCalendarDetailsData entity = db.fetchDetailsData(page, identifier, langToRenderFor, true);

            CmsPage parentPage = page.ParentPage;
            StringBuilder html = new StringBuilder();
            html.Append("<table border=\"0\" cellspacing=\"10\" style=\"margin-bottom: 3em;\" >");
            html.Append("<tr valign=\"top\">");
            html.Append("<td>" + getDescriptionText(langToRenderFor) + ":</td>");
            html.Append("<td>" + StringUtils.nl2br(entity.Description) + "</td>");
            html.Append("</tr>");
            html.Append("<tr valign=\"top\">");
            html.Append("<td>" + getCategoryText(langToRenderFor) + ":</td>");
            html.Append("<td>");
            html.Append(db.fetchCategoryByIdAndLang(langToRenderFor, entity.CategoryId).Title);
            html.Append("</td>");
            html.Append("</tr>");
            html.Append("<tr valign=\"top\">");
            html.Append("<td>" + getStartDateTimeText(langToRenderFor) + ":</td>");
            html.Append("<td>" + entity.StartDateTime.ToString("yyyy-MM-dd, h:mm tt") + "</td>");
            html.Append("</tr>");
            html.Append("<tr valign=\"top\">");
            html.Append("<td>" + getEndDateTimeText(langToRenderFor) + ":</td>");
            html.Append("<td>" + entity.EndDateTime.ToString("yyyy-MM-dd, h:mm tt") + "</td>");
            html.Append("</tr>");

            html.Append(renderAttachedFileList(page, langToRenderFor));
            html.Append("</table>");

            html.Append("<a class=\"backToPrev\" href=\"");
            html.Append(parentPage.getUrl(langToRenderFor));
            html.Append("\">");
            html.Append(getBackToText(langToRenderFor));
            html.Append(" ");
            html.Append(parentPage.Title);
            html.Append("</a>");

            writer.Write(html.ToString());
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            // CmsContext.setCurrentCultureInfo(langToRenderFor);
            EventCalendarDb db = new EventCalendarDb();
            EventCalendarDb.EventCalendarDetailsData entity = db.fetchDetailsData(page, identifier, langToRenderFor, true);

            string controlId = "eventDetails_" + page.ID.ToString() + "_" + identifier.ToString() + "_" + langToRenderFor.shortCode;
            addCssAndScriptForDateTimePicker(page);
            addScriptForDateTimePickerRender(page, controlId);

            string action = PageUtils.getFromForm(controlId + "_action", "");
            if (String.Compare(action, "saveNewValues", true) == 0)
            {
                entity.Description = PageUtils.getFromForm(controlId + "_description", entity.Description);
                entity.CategoryId = PageUtils.getFromForm(controlId + "_categoryId", entity.CategoryId);
                entity.StartDateTime = PageUtils.getFromForm(controlId + "_startDateTime", entity.StartDateTime);
                entity.EndDateTime = PageUtils.getFromForm(controlId + "_endDateTime", entity.EndDateTime);
                db.updateDetailsData(page, identifier, langToRenderFor, entity);
            }

            List<EventCalendarDb.EventCalendarCategoryData> categoryList = db.fetchCategoryList(langToRenderFor);
            NameValueCollection collection = new NameValueCollection();
            foreach (EventCalendarDb.EventCalendarCategoryData c in categoryList)
                collection.Add(c.CategoryId.ToString(), c.Title);

            // ------- START RENDERING
            StringBuilder html = new StringBuilder();
            html.Append("<table border=\"0\">");
            html.Append("<tr valign=\"top\">");
            html.Append("<td>" + getDescriptionText(langToRenderFor) + ":</td>");
            html.Append("<td>" + PageUtils.getTextAreaHtml(controlId + "_description", controlId + "_description", entity.Description, 30, 4) + "</td>");
            html.Append("</tr>");
            html.Append("<tr valign=\"top\">");
            html.Append("<td>" + getCategoryText(langToRenderFor) + ":</td>");
            html.Append("<td>");
            string categoryDropDown = "CategoryDropDown";
            html.Append(PageUtils.getDropDownHtml(controlId + "_categoryId", controlId + "_categoryId", collection, entity.CategoryId.ToString(), "", categoryDropDown + "_" + langToRenderFor.shortCode));

            try
            {
                CmsPage editCategoryPage = CmsContext.getPageByPath("_admin/EventCalendarCategory");
                html.Append(" <a href=\"" + editCategoryPage.getUrl(langToRenderFor) + "\" onclick=\"window.open(this.href,'" + categoryDropDown + "','resizable=1,scrollbars=1,width=800,height=400'); return false;\">(edit)</a>");
            }
            catch (Exception ex)
            {
                html.Append(" <span>Cannot setup Edit Category Link: " + ex.Message + "</span>");
            }

            html.Append("</td>");
            html.Append("</tr>");
            html.Append("<tr valign=\"top\">");
            html.Append("<td>" + getStartDateTimeText(langToRenderFor) + ":</td>");
            html.Append("<td>" + PageUtils.getInputTextHtml(controlId + "_startDateTime", controlId + "_startDateTime", entity.StartDateTime.ToString("yyyy-MM-dd, h:mm tt"), 20, 20) + "</td>");
            html.Append("</tr>");
            html.Append("<tr valign=\"top\">");
            html.Append("<td>" + getEndDateTimeText(langToRenderFor) + ":</td>");
            html.Append("<td>" + PageUtils.getInputTextHtml(controlId + "_endDateTime", controlId + "_endDateTime", entity.EndDateTime.ToString("yyyy-MM-dd, h:mm tt"), 20, 20) + "</td>");
            html.Append("</tr>");
            html.Append("</table>");

            html.Append(PageUtils.getHiddenInputHtml(controlId + "_action", "saveNewValues"));

            writer.WriteLine(html.ToString());
        }

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            Rss.RssItem rssItem = base.CreateAndInitRssItem(page, langToRenderFor);
            rssItem.Description = page.renderPlaceholderToString(placeholderDefinition, langToRenderFor);

            return new Rss.RssItem[] { rssItem };
        }
    }
}
