using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Services.Protocols;
using HatCMS.Placeholders;
using HatCMS.Placeholders.Calendar;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using Hatfield.Web.Portal;

namespace HatCMS._system.tools.Calendar
{
    /// AJAX handler to reply a ICal stream containing the events for
    /// EventCalendarAggregator placeholder and SimpleCalendar control
    [WebService(Namespace = "http://hatcms.net/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class CalendarICalData : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            DateTime start = DateTime.MinValue;
            DateTime end = DateTime.MinValue;
            try
            {
                int startAddSeconds = PageUtils.getFromForm("start", Int32.MinValue);
                int endAddSeconds = PageUtils.getFromForm("end", Int32.MinValue);
                if (startAddSeconds < 0)
                    start = new DateTime(DateTime.Now.Year, 1, 1);
                else
                    start = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(startAddSeconds);

                if (endAddSeconds < 0)
                    end = new DateTime(DateTime.Now.Year, 1, 1).AddYears(1);
                else
                    end = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(endAddSeconds);

                if (end < start)
                    end = start.AddYears(1);
            }
            catch
            {
                return;
            }

            bool showFile = PageUtils.getFromForm("showFile", false); // Basic rule: event calendar shows files, simple calendar does not

            CmsLanguage lang = CmsLanguage.GetFromHaystack(PageUtils.getFromForm("lang", "en"), CmsConfig.Languages);
            List<EventCalendarDb.EventCalendarDetailsData> list = new EventCalendarDb().fetchDetailsDataByRange(start, end, lang);
            iCalendar iCal = new iCalendar();

            foreach (EventCalendarDb.EventCalendarDetailsData srcEvent in list)
            {

                // Create the event, and add it to the iCalendar
                Event evt = iCal.Create<Event>();

                // Set information about the event
                evt.Start = new iCalDateTime(srcEvent.StartDateTime);
                evt.End = new iCalDateTime(srcEvent.EndDateTime); // This also sets the duration            
                evt.Description = srcEvent.Description;
                // evt.Location = "Event location";
                CmsPage srcEventPage = CmsContext.getPageById(srcEvent.PageId);

                evt.Summary = srcEventPage.getTitle(lang);

                Uri url = new Uri(srcEventPage.getUrl(CmsUrlFormat.FullIncludingProtocolAndDomainName, lang), UriKind.Absolute);

                evt.Url = url;

                evt.UID = srcEvent.PageId + "_" + srcEvent.Lang.shortCode + "_" + srcEvent.Identifier;                
                
            }

            // Serialize (save) the iCalendar
            System.Text.Encoding outputEncoding = iCalOutputEncoding.Instance;
            context.Response.Clear();
            context.Response.ContentEncoding = outputEncoding;
            context.Response.ContentType = "text/calendar";
            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(iCal, context.Response.OutputStream, outputEncoding);

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
