using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HatCMS.Placeholders;
using System.Collections.Generic;
using Hatfield.Web.Portal.Collections;
using System.Text;
using HatCMS.classes.dependencies;
using HatCMS.Placeholders.Calendar;

namespace HatCMS.Controls._system
{
    public partial class SimpleCalendar : System.Web.UI.UserControl
    {
        protected string EOL = Environment.NewLine;
        protected CmsLanguage lang = CmsContext.currentLanguage;

        protected void Page_Load(object sender, EventArgs e)
        {
            CmsPage page = CmsContext.currentPage;
            page.HeadSection.AddJavascriptFile("js/_system/jquery/jquery-1.4.1.min.js");
            page.HeadSection.AddCSSFile("css/_system/simpleCalendar.css");
            page.HeadSection.AddJavascriptFile("js/_system/jquery.simpleCalendar/simpleCalendar.min.js");
            addJsOnReady(page);
        }

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            // check if ##RenderControl(_system/SimpleCalendar calendarPage="...")## is defined
            CmsPage dummyPage = new CmsPage();
            dummyPage.TemplateName = "HomePage";
            string[] ctrlPaths = dummyPage.getAllControlPaths();
            for (int x = 0; x < ctrlPaths.Length; x++)
            {
                string[] parts = ctrlPaths[x].Split(new char[] { '/' });
                ctrlPaths[x] = parts[parts.Length - 1];
            }
            string currCtrlPath = this.GetType().Name;
            for (int x = 0; x < ctrlPaths.Length; x++)
            {
                string ctrlPath = ctrlPaths[x];
                string suffix = ctrlPath.Split(new char[] { ' ' })[0];
                if (currCtrlPath.EndsWith(suffix))
                    ret.Add(new CmsControlParameterDependency(ctrlPath, new string[] { "calendarpage" }));
            }

            ret.Add(CmsFileDependency.UnderAppPath("css/_system/simpleCalendar.css"));
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/jquery.simpleCalendar/simpleCalendar.min.js"));

            return ret.ToArray();
        }

        protected void addJsOnReady(CmsPage page)
        {
            if (CmsContext.currentEditMode == CmsEditMode.Edit)
            {
                page.HeadSection.AddJSOnReady("$('.SimpleCalendar').html('<div style=\"font-size: small; padding: 5px;\">Small Calendar unavailable during page edit mode.</div>');");
                return;
            }

            StringBuilder js = new StringBuilder();
            js.Append("$('.SimpleCalendar').simpleCalendar({" + EOL);
            js.Append("  buttonText: {" + EOL);
            js.Append("    prev: '&#9668;'," + EOL);
            js.Append("    next: '&#9658;'," + EOL);
            js.Append("  }," + EOL);
            js.Append("  " + getMonthNames());
            js.Append("  " + getDayNames());
            js.Append("  header: {" + EOL);
            js.Append("    left: 'prev'," + EOL);
            js.Append("    center: 'title'," + EOL);
            js.Append("    right: 'next'" + EOL);
            js.Append("  }," + EOL);
            js.Append("  events: '" + getAppPathName() + "_system/tools/Calendar/SimpleCalendarJsonData.ashx?lang=" + getCurrentLangText() + "'," + EOL);
            js.Append("  eventAfterRender: function(event,element,view) {" + EOL);
            js.Append("    simpleCalenderHighlightCell();" + EOL);
            js.Append("  }" + EOL);
            js.Append("});" + EOL);
            js.Append(EOL);
            js.Append("simpleCalenderAddFooter('" + getTodayText() + "','" + getEventsText() + "');" + EOL);
            js.Append(EOL);
            js.Append("$('.sc-button-next, .sc-button-prev').click( function() {" + EOL);
            js.Append("  simpleCalenderHighlightCell();" + EOL);
            js.Append("  simpleCalenderAddFooter('" + getTodayText() + "','" + getEventsText() + "');" + EOL);
            js.Append("});" + EOL);
            js.Append(EOL);
            js.Append("$('.sc-day-number').click( function() {" + EOL);
            js.Append("  var d = $(this).attr('title');" + EOL);
            js.Append("  var url = '" + getCalendarPageUrl() + "';" + EOL);
            js.Append("  if (url == '') {" + EOL);
            js.Append("    alert('##RenderControl(_system/SimpleCalendar calendarpage=...)## is not a EventCalendarAggregator.');" + EOL);
            js.Append("  }" + EOL);
            js.Append("  else {" + EOL);
            js.Append("    window.location = url + d.replace(/\\//gi, '-');" + EOL);
            js.Append("  }" + EOL);
            js.Append("});" + EOL);

            page.HeadSection.AddJSOnReady(js.ToString());
        }

        /// <summary>
        /// Read the "calendarPage" value from
        /// ##RenderControl(_system/SimpleCalendar calendarPage="44")##
        /// </summary>
        protected int CalendarPage
        {
            get { return CmsControlUtils.getControlParameterKeyValue(this, "calendarpage", 0); }
        }

        /// <summary>
        /// Get the current path
        /// </summary>
        /// <returns></returns>
        protected string getAppPathName()
        {
            return CmsContext.ApplicationPath;
        }

        /// <summary>
        /// Get the 12 month names in specific lang
        /// </summary>
        /// <returns></returns>
        protected string getMonthNames()
        {
            CmsLanguage lang = CmsContext.currentLanguage;
            // CmsContext.setCurrentCultureInfo(lang);
            if (lang.shortCode == "en")
                return "";

            string[] monthNames = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.MonthNames;
            StringBuilder name = new StringBuilder("monthNames: ['");
            name.Append(String.Join("','", monthNames, 0, 12)); // not sure why <<String.Join("','", monthNames)>> has 13 elements!!!
            name.Append("']," + Environment.NewLine);
            return name.ToString();
        }

        /// <summary>
        /// Get the 7 day names in specific lang
        /// </summary>
        /// <returns></returns>
        protected string getDayNames() {
            // CmsLanguage lang = CmsContext.currentLanguage;
            // CmsContext.setCurrentCultureInfo(lang);

            string[] weekdayNames = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
            for (int x = 0; x < weekdayNames.Length; x++)
                weekdayNames[x] = weekdayNames[x][0].ToString();

            StringBuilder name = new StringBuilder("dayNamesShort: ['");
            name.Append(String.Join("','", weekdayNames)); // normal, 7 elements only
            name.Append("']," + Environment.NewLine);
            return name.ToString();
        }

        /// <summary>
        /// Get the language short code
        /// </summary>
        /// <returns></returns>
        protected string getCurrentLangText()
        {
            CmsLanguage lang = CmsContext.currentLanguage;
            return lang.shortCode;
        }

        /// <summary>
        /// Get the multi-language text from config file: SimpleCalendar.EventsCalendarText
        /// </summary>
        /// <returns></returns>
        protected string getEventsCalendarText()
        {
            return CmsConfig.getConfigValue("SimpleCalendar.EventsCalendarText", "Events Calendar", lang);
        }

        /// <summary>
        /// Get the multi-language text from config file: SimpleCalendar.TodayText
        /// </summary>
        /// <returns></returns>
        protected string getTodayText()
        {
            return CmsConfig.getConfigValue("SimpleCalendar.TodayText", "Today", lang);
        }

        /// <summary>
        /// Get the multi-language text from config file: SimpleCalendar.EventsText
        /// </summary>
        /// <returns></returns>
        protected string getEventsText()
        {
            return CmsConfig.getConfigValue("SimpleCalendar.EventsText", "Event(s)", lang);
        }

        /// <summary>
        /// Retrieve the CmsPage (should be a calendar page) by reading the page ID from
        /// ##RenderControl(_system/SimpleCalendar calendarpage="...")##
        /// </summary>
        /// <returns></returns>
        protected CmsPage getCalendarPage()
        {
            int calendarPageId = CmsControlUtils.getControlParameterKeyValue(this, "calendarpage", -1);
            if (calendarPageId == -1)
                throw new Exception("CMS Control parameter for 'SimpleCalendar' not found: 'calendarpage'");

            CmsPage page = CmsContext.getPageById(calendarPageId);
            if (page == null || page.TemplateName == null || page.TemplateName != "EventCalendarAggregator")
                throw new Exception("##RenderControl(_system/SimpleCalendar calendarpage=\"...\")## is not a EventCalendarAggregator.");

            return page;
        }

        /// <summary>
        /// Get the URL with query string parameter.  Once the calendar page detected a query
        /// string date, the calender will show the specified date in "AgendaDay" view.
        /// </summary>
        /// <returns></returns>
        protected string getCalendarPageUrl()
        {
            StringBuilder url = new StringBuilder();
            try
            {
                CmsPage page = getCalendarPage();
                url.Append(page.getUrl(CmsContext.currentLanguage));
                url.Append("?simpleCalendarDate=");
            }
            catch { }

            return url.ToString();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("<div class=\"SimpleCalendar-Container\">" + EOL);
            writer.Write("<div class=\"SimpleCalendar-Title\">" + EOL);
            writer.Write(getEventsCalendarText() + EOL);
            writer.Write("</div>" + EOL);
            writer.Write("<div class=\"SimpleCalendar\"></div>" + EOL);
            writer.Write("</div>" + EOL);
        }
    }
}