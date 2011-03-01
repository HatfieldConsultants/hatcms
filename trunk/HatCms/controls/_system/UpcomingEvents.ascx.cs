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
using System.Text;
using HatCMS.placeholders.Calendar;

namespace HatCMS.controls._system
{
    public partial class UpcomingEvents : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            // -- database tables
            ret.Add(new CmsDatabaseTableDependency("EventCalendarAggregator"));
            ret.Add(new CmsDatabaseTableDependency("EventCalendarDetails"));

            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("UpcomingEvents.TitleText"));
            ret.Add(new CmsConfigItemDependency("UpcomingEvents.NoEventText"));

            return ret.ToArray();
        }

        protected string getTitleText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UpcomingEvents.TitleText", "Upcoming Events", lang);
        }

        protected string getNoEventsText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UpcomingEvents.NoEventText", "(No Events)", lang);
        }

        protected string formatDates(DateTime strDate, DateTime endDate)
        {
            bool sameYear = (strDate.Year == endDate.Year);
            bool sameMonth = (strDate.Month == endDate.Month);
            bool sameDay = (strDate.Day == endDate.Day);

            if (sameYear && sameMonth && sameDay)
                return strDate.ToString("MMMM dd, yyyy");

            if (sameYear && sameMonth)
                return strDate.ToString("MMMM dd &#8722; ") + endDate.ToString("dd, yyyy");

            if (sameYear)
                return strDate.ToString("MMMM dd &#8722; ") + endDate.ToString("MMMM dd, yyyy");

            return strDate.ToString("MMMM dd, yyyy &#8722; ") + endDate.ToString("MMMM dd, yyyy");
        }

        protected string renderUpcomingEventsHeader(CmsLanguage lang)
        {
            // CmsContext.setCurrentCultureInfo(CmsContext.currentLanguage);
            StringBuilder html = new StringBuilder("<div class=\"UpcomingEventsTitle\">");
            html.Append(getTitleText(lang));
            html.Append("</div>");
            return html.ToString();
        }

        protected string renderUpcomingEventsContent(CmsLanguage lang, List<EventCalendarDb.EventCalendarDetailsData> list)
        {
            StringBuilder html = new StringBuilder();

            if (list.Count == 0)
            {
                html.Append("<div class=\"UpcomingEventsText\">");
                html.Append(getNoEventsText(lang));
                html.Append("</div>");
                return html.ToString();
            }

            for (int x = 0; x < list.Count; x++)
            {
                CmsPage p = CmsContext.getPageById(list[x].PageId);
                string url = p.getUrl(lang);
                html.Append("<div class=\"UpcomingEventsDate\">");
                html.Append("<a href=\"" + url + "\">");
                html.Append(formatDates(list[x].StartDateTime, list[x].EndDateTime));
                html.Append("</a>");
                html.Append("</div>");

                html.Append("<div class=\"UpcomingEventsText\">");
                html.Append("<a href=\"" + url + "\">");
                html.Append(p.getTitle(lang));
                html.Append("</a>");
                html.Append("</div>");
            }

            return html.ToString();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            DateTime currDateTime = DateTime.Now;
            int count = CmsControlUtils.getControlParameterKeyValue(this, "count", 3);

            CmsLanguage lang = CmsContext.currentLanguage;
            List<EventCalendarDb.EventCalendarDetailsData> list = new EventCalendarDb().fetchUpcomingEventDetails(currDateTime, lang, count);

            StringBuilder html = new StringBuilder("<div class=\"UpcomingEvents\">");
            html.Append(renderUpcomingEventsHeader(lang));
            html.Append(renderUpcomingEventsContent(lang, list));
            html.Append("</div>");

            writer.Write(html.ToString());
        }
    }
}