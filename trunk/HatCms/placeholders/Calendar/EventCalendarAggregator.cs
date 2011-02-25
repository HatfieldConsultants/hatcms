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
using System.Collections.Generic;
using System.Text;
using Hatfield.Web.Portal;
using System.Collections.Specialized;
using Hatfield.Web.Portal.Collections;
using Hatfield.Web.Portal.Imaging;

namespace HatCMS.placeholders.Calendar
{
    public class EventCalendarAggregator : BaseCmsPlaceholder
    {
        protected string EOL = Environment.NewLine;

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

        /// <summary>
        /// Render the "Add an event"
        /// </summary>
        /// <param name="action"></param>
        /// <param name="pageToRenderFor"></param>
        /// <param name="langToRenderFor"></param>
        /// <returns></returns>
        protected static string AddEventEditMenuRender(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
        {
            NameValueCollection createPageParams = action.CreateNewPageOptions.GetCreatePagePopupParams();
            if (action.CreateNewPageOptions.RequiresUserInput())
                return CmsPageEditMenu.DefaultStandardActionRenderers.RenderPopupLink("CreateNewPagePath", "/_admin/createPage", createPageParams, pageToRenderFor, langToRenderFor, "<strong>Add</strong> an event", 500, 400);
            else
                return CmsPageEditMenu.DefaultStandardActionRenderers.RenderLink("CreateNewPagePath", "/_admin/createPage", createPageParams, pageToRenderFor, langToRenderFor, "<strong>Add</strong> an event");
        }

        /// <summary>
        /// Adds the "Add an event" menu item to the Edit Menu.
        /// </summary>
        /// <param name="pageToAddCommandTo"></param>
        /// <param name="eventCalendarAggregator"></param>
        public static void AddEventCommandToEditMenu(CmsPage pageToAddCommandTo, CmsPage eventCalendarAggregator)
        {
            // -- only add the command if the user can author
            if (!pageToAddCommandTo.currentUserCanWrite)
                return;

            // -- base the command off the existing "create new sub-page" command
            CmsPageEditMenuAction createNewSubPage = pageToAddCommandTo.EditMenu.getActionItem(CmsEditMenuActionItem.CreateNewPage);

            if (createNewSubPage == null)
                throw new Exception("Fatal Error in in EventCalendarAggregator placeholder - could not get the existing CreateNewPage action");

            CmsPageEditMenuAction newAction = createNewSubPage.Copy(); // copy everything from the CreateNewPage entry            

            // -- configure this command to not prompt authors for any information.
            //    the minimum information needed to create a page is the new page's filename (page.name)
            //      -- get the next unique filename            
            string newPageName = "";
            for (int eventNum = 1; eventNum < int.MaxValue; eventNum++)
            {
                string pageNameToTest = "Event " + eventNum.ToString();
                if (!CmsContext.childPageWithNameExists(eventCalendarAggregator.ID, pageNameToTest))
                {
                    newPageName = pageNameToTest;
                    break;
                }
            }

            string newPageTitle = "";
            string newPageMenuTitle = "";
            string newPageSearchEngineDescription = "";
            bool newPageShowInMenu = false;
            string newPageTemplate = CmsConfig.getConfigValue("EventCalendar.DetailsTemplateName", "EventCalendarDetails");

            newAction.CreateNewPageOptions = CmsCreateNewPageOptions.GetInstanceWithNoUserPrompts(newPageName, newPageTitle, newPageMenuTitle, newPageSearchEngineDescription, newPageShowInMenu, newPageTemplate, eventCalendarAggregator.ID);

            newAction.CreateNewPageOptions.ParentPageId = eventCalendarAggregator.ID;
            newAction.SortOrdinal = createNewSubPage.SortOrdinal + 1;
            newAction.doRenderToString = AddEventEditMenuRender;

            pageToAddCommandTo.EditMenu.addCustomActionItem(newAction);
        }

        public override RevertToRevisionResult revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented;
        }

        /// <summary>
        /// Generate Javascript to invoke the jQuery Full Calendar plug-in.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="calDomId"></param>
        /// <param name="calViewMode"></param>
        protected void addScriptForCalendarRender(CmsPage page, CmsLanguage lang, string calDomId, string calViewMode)
        {
            string onClickUrl = "calEvent.url";

            StringBuilder simpleCalendarDate = new StringBuilder();
            string[] queryStringParm = PageUtils.getFromForm("simpleCalendarDate");
            if (queryStringParm.Length == 1)
            {
                string[] date = getDateInStringArray(queryStringParm[0]);
                calViewMode = "agendaDay";

                simpleCalendarDate.Append("  year: '" + date[0] + "'," + EOL);
                simpleCalendarDate.Append("  month: '" + date[1] + "'," + EOL);
                simpleCalendarDate.Append("  date: '" + date[2] + "'," + EOL);
            }

            StringBuilder js = new StringBuilder();
            js.Append("$('#" + calDomId + "').fullCalendar({" + EOL);
            js.Append(simpleCalendarDate.ToString());
            js.Append(getMonthAndWeekdayNames());
            js.Append(getButtonNames());
            js.Append(getAllDayText());
            js.Append("  defaultView : '" + calViewMode + "'," + EOL);
            js.Append("  header: {" + EOL);
            js.Append("    left: 'prev,next today'," + EOL);
            js.Append("    center: 'title'," + EOL);
            js.Append("    right: 'month,agendaWeek,agendaDay'" + EOL);
            js.Append("  }," + EOL);
            js.Append("  events: '" + CmsContext.ApplicationPath + "_system/tools/Calendar/SimpleCalendarJsonData.ashx?showFile=true&lang=" + lang.shortCode + "'," + EOL);
            js.Append("  eventClick: function(calEvent, jsEvent) { window.location = " + onClickUrl + "; }, " + EOL);
            js.Append("  timeFormat: { month: '', '':'h(:mm)tt'}" + EOL);
            js.Append("});" + EOL);

            page.HeadSection.AddJSOnReady(js.ToString());
        }

        /// <summary>
        /// Include the JS and CSS files for jQuery Full Calendar plug-in.
        /// </summary>
        /// <param name="page"></param>
        protected void addCssAndScriptForCalendarPlugin(CmsPage page)
        {
            page.HeadSection.AddCSSFile("js/_system/jquery.fullcalendar/fullcalendar.css");
            page.HeadSection.AddJavascriptFile("js/_system/jquery/jquery-1.4.1.min.js");
            page.HeadSection.AddJavascriptFile("js/_system/jquery.fullcalendar/fullcalendar.min.js");
        }

        /// <summary>
        /// Add CSS declarations for event category.
        /// </summary>
        /// <param name="page"></param>
        protected void addCssForEventCategory(CmsPage page, CmsLanguage lang)
        {
            EventCalendarDb db = new EventCalendarDb();
            List<EventCalendarDb.EventCalendarCategoryData> categoryList = db.fetchCategoryList(lang);
            foreach (EventCalendarDb.EventCalendarCategoryData c in categoryList)
                page.HeadSection.AddCSSStyleStatements(".EventCategory_" + c.CategoryId + " a { background-color: " + c.ColorHex + ";  }");

            string appPath = CmsContext.ApplicationPath;
            Set iconSet = IconUtils.getIconSet(CmsContext.ApplicationPath, false);
            foreach (string i in iconSet)
                page.HeadSection.AddCSSStyleStatements(IconUtils.getIconCss(appPath, false, i, "EventCategory_file"));
        }

        protected string getAllDayText()
        {
            CmsLanguage lang = CmsContext.currentLanguage;
            // CmsContext.setCurrentCultureInfo(lang);
            if (lang.shortCode == "en")
                return "";

            StringBuilder name = new StringBuilder("  allDayText: '");
            name.Append(CmsConfig.getConfigValue("EventCalendar.AllDayText", "All-day", lang) + "'," + EOL);
            return name.ToString();
        }

        /// <summary>
        /// Override the jQuery Full Calendar plug-in to use month and weekday names in order language.
        /// </summary>
        /// <returns></returns>
        protected string getMonthAndWeekdayNames()
        {
            CmsLanguage lang = CmsContext.currentLanguage;
            // CmsContext.setCurrentCultureInfo(lang);
            if (lang.shortCode == "en")
                return "";

            string[] monthNames = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.MonthNames;
            StringBuilder name = new StringBuilder("  monthNames: ['");
            name.Append(String.Join("','", monthNames, 0, 12)); // not sure why <<String.Join("','", monthNames)>> has 13 elements!!!
            name.Append("']," + EOL);

            string[] monthShortNames = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;
            name.Append("  monthNamesShort: ['");
            name.Append(String.Join("','", monthShortNames, 0, 12)); // not sure why <<String.Join("','", monthShortNames)>> has 13 elements!!!
            name.Append("']," + EOL);

            string[] weekdayNames = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.DayNames;
            name.Append("  dayNames: ['");
            name.Append(String.Join("','", weekdayNames)); // normal, 7 elements only
            name.Append("']," + EOL);

            string[] weekdayShortNames = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
            name.Append("  dayNamesShort: ['");
            name.Append(String.Join("','", weekdayShortNames)); // normal, 7 elements only
            name.Append("']," + EOL);

            return name.ToString();
        }

        /// <summary>
        /// Override the jQuery Full Calendar plug-in to use button text in order language.
        /// </summary>
        /// <returns></returns>
        protected string getButtonNames()
        {
            CmsLanguage lang = CmsContext.currentLanguage;
            // CmsContext.setCurrentCultureInfo(lang);
            if (lang.shortCode == "en")
                return "  buttonText: { today: 'Go to today' }," + EOL;

            StringBuilder name = new StringBuilder("  buttonText: {" + EOL);
            name.Append("    today: '" + CmsConfig.getConfigValue("EventCalendar.ButtonTodayText", "Go to today", lang) + "'," + EOL);
            name.Append("    month: '" + CmsConfig.getConfigValue("EventCalendar.ButtonMonthText", "Month", lang) + "'," + EOL);
            name.Append("    week: '" + CmsConfig.getConfigValue("EventCalendar.ButtonWeekText", "Week", lang) + "'," + EOL);
            name.Append("    day: '" + CmsConfig.getConfigValue("EventCalendar.ButtonDayText", "Day", lang) + "'" + EOL);
            name.Append("  }," + EOL);
            return name.ToString();
        }

        /// <summary>
        /// Generate the HTML DOM id
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        protected string getCalendarHtmlDomId(CmsPage page, int identifier)
        {
            return "cal_" + page.ID.ToString() + "_" + identifier.ToString();
        }

        /// <summary>
        /// Format the query string date to a string array [yyyy,mm,dd]
        /// </summary>
        /// <param name="queryStringDate"></param>
        /// <returns></returns>
        protected string[] getDateInStringArray(string queryStringDate)
        {
            string[] simpleCalendarDate = queryStringDate.Split(new char[] { '-' });
            string yyyy = simpleCalendarDate[0];
            string mm = (Convert.ToInt32(simpleCalendarDate[1]) - 1).ToString();
            string dd = simpleCalendarDate[2];
            return new string[] { yyyy, mm, dd };
        }

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            AddEventCommandToEditMenu(page, page);
            EventCalendarDb db = new EventCalendarDb();
            EventCalendarDb.EventCalendarAggregatorData entity = db.fetchAggregatorData(page, identifier, langToRenderFor, true);
            string calViewMode = entity.getViewModeForJQuery();

            addCssAndScriptForCalendarPlugin(page);
            addCssForEventCategory(page, langToRenderFor);

            string calDomId = getCalendarHtmlDomId(page, identifier);
            addScriptForCalendarRender(page, langToRenderFor, calDomId, calViewMode);

            StringBuilder html = new StringBuilder();
            html.Append("<div id=\""+ calDomId +"\" style=\"margin-right: 10px;\"></div>");

            writer.Write(html.ToString());
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string ControlId = "Calender_" + page.ID.ToString() + "_" + identifier.ToString() + "_" + langToRenderFor.shortCode.ToLower();
            EventCalendarDb db = new EventCalendarDb();
            EventCalendarDb.EventCalendarAggregatorData entity = db.fetchAggregatorData(page, identifier, langToRenderFor, true);

            string action = PageUtils.getFromForm(ControlId + "_action", "");
            if (String.Compare(action, "saveNewValues", true) == 0)
            {
                entity.ViewMode = (CalendarViewMode)PageUtils.getFromForm(ControlId + "_viewMode", typeof(CalendarViewMode), entity.ViewMode);
                db.updateAggregatorData(page, identifier, langToRenderFor, entity);
            }

            StringBuilder html = new StringBuilder();

            html.Append("<p>Calendar Display Configuration:</p>");

            html.Append("<table>");
            html.Append("<tr>");
            html.Append("<td>Initial view mode: </td>");
            html.Append("<td>");
            html.Append(PageUtils.getDropDownHtml(ControlId + "_viewMode", ControlId + "_viewMode", Enum.GetNames(typeof(CalendarViewMode)), Enum.GetName(typeof(CalendarViewMode), entity.ViewMode)));
            html.Append("</td>");
            html.Append("</tr>");
            html.Append("</table>");

            html.Append(PageUtils.getHiddenInputHtml(ControlId + "_action", "saveNewValues"));

            writer.Write(html.ToString());
        }
    }
}
