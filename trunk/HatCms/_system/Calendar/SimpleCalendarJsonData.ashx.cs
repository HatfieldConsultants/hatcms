using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using HatCMS.placeholders.Calendar;
using System.Collections.Generic;
using JsonFx.Json;
using Hatfield.Web.Portal;

namespace HatCMS._system.Calendar
{
    public class SimpleCalendarJsonData : IHttpHandler
    {
        /// <summary>
        /// AJAX handler to reply a JSON containing the events for
        /// EventCalendarAggregator and SimpleCalendar
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            DateTime start = DateTime.MinValue;
            DateTime end = DateTime.MinValue;
            try
            {
                start = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(PageUtils.getFromForm("start", 0));
                end = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(PageUtils.getFromForm("end", 0));
            }
            catch
            {
                return;
            }

            CmsLanguage lang = new CmsLanguage(PageUtils.getFromForm("lang", "en"));
            List<EventCalendarDb.EventCalendarDetailsData> list = new EventCalendarDb().fetchDetailsDataByRange(start, end, lang);
            List<FullCalendarEvent> events = new List<FullCalendarEvent>();
            foreach (EventCalendarDb.EventCalendarDetailsData c in list)
                events.Add(new FullCalendarEvent(c));

            string json = JsonWriter.Serialize(events.ToArray());
            context.Response.Write(json);
        }

        /// <summary>
        /// FullCalendarEvent object implements the items found at http://arshaw.com/fullcalendar/docs/event_data/Event_Object/
        /// </summary>
        private class FullCalendarEvent
        {
            public string id;
            public string title;
            public bool allDay;
            public DateTime start;
            public DateTime end;
            public string url;
            public string className;

            public FullCalendarEvent()
            {
                id = "";
                title = "";
                allDay = false;
                start = DateTime.MinValue;
                end = DateTime.MaxValue;
                url = "";
                className = "";
            }

            public FullCalendarEvent(EventCalendarDb.EventCalendarDetailsData c)
            {
                CmsPage page = new CmsPageDb().getPage(c.PageId);
                id = c.PageId.ToString();
                title = page.getTitle(c.Lang);
                start = c.StartDateTime;
                end = c.EndDateTime;
                url = page.getUrl(c.Lang);
                className = "EventCategory_" + c.CategoryId;
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
