using System;
using System.Data;
using System.Web;
using System.Web.Caching;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using HatCMS.Placeholders.Calendar;
using System.Collections.Generic;
using JsonFx.Json;
using Hatfield.Web.Portal;
using HatCMS.Placeholders;
using Hatfield.Web.Portal.Imaging;
using DDay.iCal;

namespace HatCMS._system.Calendar
{
    public class SimpleCalendarJsonData : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            // -- get the start and end dates.
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

            // -- if we are requesting an icalURL, go get it. Otherwise it's an internal request
            
            // -- note: iCal support currently doesn't work due to timezone parsing bugs in the DDay.iCal library. 
            //          It is currently disabled until these bugs are fixed.
            bool enableIcalSupport = false;
            string iCalUrl = PageUtils.getFromForm("icalUrl", "");
            if (enableIcalSupport && iCalUrl != "")
            {
                // -- check if the host name is in the approved list.
                //    if it's not approved, handle it as an internal request.
                Uri icalUri = new Uri(iCalUrl, UriKind.RelativeOrAbsolute);
                int index = HatCMS.Tools.cachingProxy.IndexOfHost(icalUri.Host);
                if (index >= 0)
                {
                    ProcessICalRequest(icalUri, start, end, context);
                }
                else
                {
                    ProcessInternalRequest(start, end, context);
                }
            }
            else
            {
                ProcessInternalRequest(start, end, context);
            }
        }

        public void ProcessICalRequest(Uri iCalUri, DateTime start, DateTime end, HttpContext context)
        {
            string backColor = PageUtils.getFromForm("backColour","white");
            string textColor = PageUtils.getFromForm("textColor","black");

            int cacheMinutes = HatCMS.Tools.cachingProxy.CacheDuration_Minutes;
            string cacheKey = iCalUri.ToString();
            DDay.iCal.IICalendarCollection calCollection;
            if (cacheMinutes > 0 && context.Cache[cacheKey] != null)
                calCollection = (DDay.iCal.IICalendarCollection) context.Cache[cacheKey];
            else
            {                
                calCollection = DDay.iCal.iCalendar.LoadFromUri(iCalUri);                

                // -- add the data to the cache
                if (cacheMinutes > 0)
                {
                    context.Cache.Insert(cacheKey, calCollection, null,
                                Cache.NoAbsoluteExpiration,
                                TimeSpan.FromMinutes(cacheMinutes),
                                CacheItemPriority.Normal, null);
                }
            }

            List<FullCalendarEvent> eventsToOutput = new List<FullCalendarEvent>();
            IList<DDay.iCal.Occurrence> occurrences = calCollection.GetOccurrences<DDay.iCal.IEvent>(start, end);
            foreach (DDay.iCal.Occurrence occurrence in occurrences)
            {                                
                if (occurrence.Source is DDay.iCal.IEvent)
                {
                    DDay.iCal.IEvent ievent = occurrence.Source as DDay.iCal.IEvent;
                    if (ievent.IsActive()) // make sure the event hasn't been cancelled.
                    {
                        FullCalendarEvent e = new FullCalendarEvent(ievent, backColor, textColor);
                        eventsToOutput.Add(e);
                    }
                }
            } // foreach

            string json = JsonWriter.Serialize(eventsToOutput.ToArray());
            context.Response.Write(json);
        }
        
        /// <summary>
        /// AJAX handler to reply a JSON containing the events for
        /// EventCalendarAggregator placeholder and SimpleCalendar control
        /// </summary>
        /// <param name="context"></param>
        public void ProcessInternalRequest(DateTime start, DateTime end, HttpContext context)
        {

            bool showFile = PageUtils.getFromForm("showFile", false); // Basic rule: event calendar shows files, simple calendar does not

            CmsLanguage lang = CmsLanguage.GetFromHaystack(PageUtils.getFromForm("lang", "en"), CmsConfig.Languages);
            List<EventCalendarDb.EventCalendarDetailsData> list = new EventCalendarDb().fetchDetailsDataByRange(start, end, lang);
            List<EventCalendarDb.EventCalendarCategoryData> eventCategories = new EventCalendarDb().fetchCategoryList();
            List<FullCalendarEvent> eventsToOutput = new List<FullCalendarEvent>();
            foreach (EventCalendarDb.EventCalendarDetailsData c in list)
            {
                eventsToOutput.Add(new FullCalendarEvent(c, eventCategories));
                if (!showFile)
                    continue;

                CmsPage eventPage = CmsContext.getPageById(c.PageId);
                List<FileLibraryDetailsData> fileList = new FileLibraryDb().fetchDetailsData(lang, eventPage);
                foreach (FileLibraryDetailsData f in fileList)
                {
                    if (userHasAuthority(f))
                        eventsToOutput.Add(new FullCalendarEvent(c, f, eventCategories));
                }
            }

            string json = JsonWriter.Serialize(eventsToOutput.ToArray());
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
            CmsPage filePage = CmsContext.getPageById(f.DetailsPageId);

            return filePage.currentUserCanRead;

        }

        /// <summary>
        /// FullCalendarEvent object implements the items found at http://arshaw.com/fullcalendar/docs/event_data/Event_Object/
        /// </summary>
        private class FullCalendarEvent
        {
            [JsonFx.Json.JsonName("id")]
            public string id;
            
            [JsonFx.Json.JsonName("title")]
            public string title;

            [JsonFx.Json.JsonName("allDay")]
            public bool allDay;

            [JsonFx.Json.JsonName("start")]
            public DateTime start;
            
            [JsonFx.Json.JsonName("end")]     
            public DateTime end;

            [JsonFx.Json.JsonName("url")]     
            public string url;

            [JsonFx.Json.JsonName("backgroundColor")]          
            public string backgroundColor;
            
            [JsonFx.Json.JsonName("borderColor")]          
            public string borderColor;

            [JsonFx.Json.JsonName("textColor")]          
            public string textColor;

            public FullCalendarEvent(DDay.iCal.IEvent ev, string backColour, string textColour)
            {
                id = ev.UID;
                title = ev.Name;
                allDay = ev.IsAllDay; ;
                start = ev.Start.UTC;
                end = ev.End.UTC;
                url = ev.Url.ToString();
                backgroundColor = backColour;
                borderColor = backColour;
                textColor = textColour;
            }

            public FullCalendarEvent(EventCalendarDb.EventCalendarDetailsData c, List<EventCalendarDb.EventCalendarCategoryData> categoryHaystack)
            {
                CmsPage page = CmsContext.getPageById(c.PageId);
                id = c.PageId.ToString();
                title = page.getTitle(c.Lang);
                start = c.StartDateTime;
                end = c.EndDateTime;
                url = page.getUrl(c.Lang);

                EventCalendarDb.EventCalendarCategoryData category = EventCalendarDb.EventCalendarCategoryData.GetFromHaystack(categoryHaystack, c.CategoryId);
                backgroundColor = category.ColorHex;
                borderColor = "black";
                textColor = "white";

            }

            /// <summary>
            /// Create the jQuery FullCalendar event object for an event with a file attached.
            /// </summary>
            /// <param name="c"></param>
            /// <param name="f"></param>
            public FullCalendarEvent(EventCalendarDb.EventCalendarDetailsData c, FileLibraryDetailsData attachedFile, List<EventCalendarDb.EventCalendarCategoryData> categoryHaystack)
            {
                CmsPage page = CmsContext.getPageById(attachedFile.DetailsPageId);
                id = "EventFile_" + attachedFile.DetailsPageId.ToString();
                title = attachedFile.FileName;
                start = c.StartDateTime.AddSeconds(1);  // show below the event
                end = c.EndDateTime.AddSeconds(1);      // show below the event
                allDay = true;
                url = page.getUrl(attachedFile.Lang);
                
                EventCalendarDb.EventCalendarCategoryData category = EventCalendarDb.EventCalendarCategoryData.GetFromHaystack(categoryHaystack, c.CategoryId);
                backgroundColor = category.ColorHex;
                borderColor = "black";
                textColor = "white";
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
