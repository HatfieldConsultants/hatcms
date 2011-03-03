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
using HatCMS.Placeholders;
using Hatfield.Web.Portal.Imaging;

namespace HatCMS._system.Calendar
{
    public class SimpleCalendarJsonData : IHttpHandler
    {        
        /// <summary>
        /// AJAX handler to reply a JSON containing the events for
        /// EventCalendarAggregator placeholder and SimpleCalendar control
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

            bool showFile = PageUtils.getFromForm("showFile", false); // Basic rule: event calendar shows files, simple calendar does not

            CmsLanguage lang = new CmsLanguage(PageUtils.getFromForm("lang", "en"));
            List<EventCalendarDb.EventCalendarDetailsData> list = new EventCalendarDb().fetchDetailsDataByRange(start, end, lang);
            List<FullCalendarEvent> events = new List<FullCalendarEvent>();
            foreach (EventCalendarDb.EventCalendarDetailsData c in list)
            {
                events.Add(new FullCalendarEvent(c));
                if (!showFile)
                    continue;

                CmsPage eventPage = CmsContext.getPageById(c.PageId);
                List<FileLibraryDetailsData> fileList = new FileLibraryDb().fetchDetailsData(lang, eventPage);
                foreach (FileLibraryDetailsData f in fileList)
                {
                    if (userHasAuthority(f))
                        events.Add(new FullCalendarEvent(c, f));
                }
            }

            string json = JsonWriter.Serialize(events.ToArray());
            context.Response.Write(json);
        }

        /// <summary>
        /// A user can read the calendar page does not mean he/she can read the files
        /// attached to events.  Hence, the CmsPage for a file is read in order to
        /// check the zone of the attached file.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        protected bool userHasAuthority(FileLibraryDetailsData f)
        {
            CmsPage filePage = CmsContext.getPageById(f.PageId);
            
            WebPortalUser u = CmsContext.currentWebPortalUser;
            if (filePage.Zone.canRead(u) || filePage.Zone.canWrite(u))
                return true;
            else
                return false;
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
                CmsPage page = CmsContext.getPageById(c.PageId);
                id = c.PageId.ToString();
                title = page.getTitle(c.Lang);
                start = c.StartDateTime;
                end = c.EndDateTime;
                url = page.getUrl(c.Lang);
                className = "EventCategory_" + c.CategoryId;
            }

            /// <summary>
            /// Create the jQuery FullCalendar event object (render the file attached)
            /// </summary>
            /// <param name="c"></param>
            /// <param name="f"></param>
            public FullCalendarEvent(EventCalendarDb.EventCalendarDetailsData c, FileLibraryDetailsData f)
            {
                CmsPage page = CmsContext.getPageById(f.PageId);
                id = "EventFile_" + f.PageId.ToString();
                title = f.FileName;
                start = c.StartDateTime.AddSeconds(1);  // show below the event
                end = c.EndDateTime.AddSeconds(1);      // show below the event
                allDay = true;
                url = page.getUrl(f.Lang);
                className = "EventCategory_file_" + System.IO.Path.GetExtension(f.FileName).Substring(1) + "_gif";
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
