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

namespace HatCMS.placeholders.Calendar
{
    public enum CalendarViewMode { Month, Agenda_Week, Agenda_Day }

    public class EventCalendarDb : PlaceholderDb
    {
        #region aggregator
        protected static string TableNameAggregator = "EventCalendarAggregator";

        public class EventCalendarAggregatorData
        {
            private CalendarViewMode viewMode = CalendarViewMode.Month;
            public CalendarViewMode ViewMode
            {
                get { return viewMode; }
                set { viewMode = value; }
            }

            public string getViewModeForJQuery()
            {
                string calViewMode = ViewMode.ToString().Replace("_", "").Substring(1);
                string firstLetter = ViewMode.ToString().Substring(0, 1).ToLower();
                return firstLetter + calViewMode;
            }

            private int maxCharPerLine = CmsConfig.getConfigValue("EventCalendar.MaxCharPerLine", 10);
            public int MaxCharPerLine
            {
                get { return maxCharPerLine; }
                set { maxCharPerLine = value; }
            }

            private DateTime defaultEventStartTime = DateTime.MinValue.Date.AddHours(CmsConfig.getConfigValue("EventCalendar.DefaultEventStartHour", 8));
            public DateTime DefaultEventStartTime
            {
                get { return defaultEventStartTime; }
                set { defaultEventStartTime = value; }
            }

            private DateTime defaultEventEndTime = DateTime.MinValue.Date.AddHours(CmsConfig.getConfigValue("EventCalendar.DefaultEventEndHour", 17));
            public DateTime DefaultEventEndTime
            {
                get { return defaultEventEndTime; }
                set { defaultEventEndTime = value; }
            }
        }

        public bool insertAggregatorData(CmsPage page, int identifier, CmsLanguage lang, EventCalendarAggregatorData entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(TableNameAggregator);
            sql.Append(" (PageId,Identifier,LangCode,ViewMode) VALUES (");
            sql.Append(page.ID.ToString() + ",");
            sql.Append(identifier.ToString() + ",'");
            sql.Append(dbEncode(lang.shortCode) + "','");
            sql.Append(entity.ViewMode.ToString() + "');");

            int affected = this.RunUpdateQuery(sql.ToString());
            if (affected > 0)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;
        }

        public EventCalendarAggregatorData fetchAggregatorData(CmsPage page, int identifier, CmsLanguage lang, bool createIfNotExist)
        {
            if (page.ID < 0 || identifier < 0)
                return new EventCalendarAggregatorData();

            StringBuilder sql = new StringBuilder("SELECT ViewMode FROM ");
            sql.Append(TableNameAggregator);
            sql.Append(" WHERE PageId=" + page.ID.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "'");
            sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND Deleted IS NULL;");

            EventCalendarAggregatorData entity = new EventCalendarAggregatorData();
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                rowToData(dr, entity);
            }
            else
            {
                if (createIfNotExist)
                {
                    if (insertAggregatorData(page, identifier, lang, entity) == false)
                        throw new Exception("EventCalendarDb.fetchAggregatorData() database error: Error creating new placeholder");
                }
                else
                    throw new Exception("EventCalendarDb.fetchAggregatorData() database error: placeholder does not exist");
            }
            return entity;
        }

        public bool updateAggregatorData(CmsPage page, int identifier, CmsLanguage lang, EventCalendarAggregatorData entity)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(TableNameAggregator);
            sql.Append(" SET ViewMode = '" + entity.ViewMode.ToString() + "'");

            sql.Append(" WHERE PageId=" + page.ID.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "'");
            sql.Append(" AND Identifier=" + identifier.ToString());

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        protected void rowToData(DataRow dr, EventCalendarAggregatorData entity)
        {
            entity.ViewMode = (CalendarViewMode)Enum.Parse(typeof(CalendarViewMode), dr["ViewMode"].ToString());
        }
        #endregion

        #region details
        protected static string TableNameDetails = "EventCalendarDetails";

        public class EventCalendarDetailsData
        {
            private int pageId = -1;
            public int PageId
            {
                get { return pageId; }
                set { pageId = value; }
            }

            private int identifier = -1;
            public int Identifier
            {
                get { return identifier; }
                set { identifier = value; }
            }

            private CmsLanguage lang = CmsConfig.Languages[0];
            public CmsLanguage Lang
            {
                get { return lang; }
                set { lang = value; }
            }

            private string description = "";
            public string Description
            {
                get { return description; }
                set { description = value; }
            }

            private int categoryId = -1;
            public int CategoryId
            {
                get { return categoryId; }
                set { categoryId = value; }
            }

            private DateTime startDateTime;
            public DateTime StartDateTime
            {
                get { return startDateTime; }
                set { startDateTime = value; }
            }


            private DateTime endDateTime = DateTime.Now.AddHours(1);
            public DateTime EndDateTime
            {
                get { return endDateTime; }
                set { endDateTime = value; }
            }

            private string createdBy = "";
            public string CreatedBy
            {
                get { return createdBy; }
                set { createdBy = value; }
            }

            /// <summary>
            /// Search the category title by using the instance's category id
            /// </summary>
            /// <param name="categoryList"></param>
            /// <returns></returns>
            public string getCategoryTitle(List<EventCalendarCategoryData> categoryList)
            {
                foreach (EventCalendarCategoryData c in categoryList)
                {
                    if (c.CategoryId == CategoryId)
                        return c.Title;
                }
                return CategoryId.ToString();
            }

            public EventCalendarDetailsData()
            {
                try
                {
                    this.CreatedBy = CmsContext.currentWebPortalUser.FullName;
                }
                catch { }

                this.StartDateTime = DateTime.Now.AddDays(1);
                this.StartDateTime = StartDateTime.AddHours(-StartDateTime.Hour);
                this.StartDateTime = StartDateTime.AddMinutes(-StartDateTime.Minute);
                this.StartDateTime = StartDateTime.AddSeconds(-StartDateTime.Second);
                this.StartDateTime = StartDateTime.AddHours(CmsConfig.getConfigValue("EventCalendar.DefaultEventStartHour", 8));

                this.EndDateTime = DateTime.Now.AddDays(1);
                this.EndDateTime = EndDateTime.AddHours(-EndDateTime.Hour);
                this.EndDateTime = EndDateTime.AddMinutes(-EndDateTime.Minute);
                this.EndDateTime = EndDateTime.AddSeconds(-EndDateTime.Second);
                this.EndDateTime = EndDateTime.AddHours(CmsConfig.getConfigValue("EventCalendar.DefaultEventEndHour", 17));
            }

            public EventCalendarDetailsData(CmsPage page, int identifier, CmsLanguage lang)
                : this()
            {
                this.PageId = page.ID;
                this.identifier = identifier;
                this.lang = lang;
            }
        }

        /// <summary>
        /// Insert a event details
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool insertDetailsData(CmsPage page, int identifier, CmsLanguage lang, EventCalendarDetailsData entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(TableNameDetails);
            sql.Append(" (PageId,Identifier,LangCode,Description,CategoryId,StartDateTime,EndDateTime,CreatedBy) VALUES (");
            sql.Append(page.ID.ToString() + ",");
            sql.Append(identifier.ToString() + ",'");
            sql.Append(dbEncode(lang.shortCode) + "','");
            sql.Append(dbEncode(entity.Description) + "',");
            sql.Append(entity.CategoryId.ToString() + ",");
            sql.Append(dbEncode(entity.StartDateTime) + ",");
            sql.Append(dbEncode(entity.EndDateTime) + ",'");
            sql.Append(dbEncode(entity.CreatedBy) + "');");

            int affected = this.RunUpdateQuery(sql.ToString());
            if (affected > 0)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;
        }

        /// <summary>
        /// Select the event details by language limit by a count.  If an valid attached event id is provided,
        /// the method selects the event details as well (number of records is "count + 1" in this case).
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="attachedEventId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<EventCalendarDetailsData> fetchAllDetailsData(CmsLanguage lang, int attachedEventId, int count)
        {
            StringBuilder sql = new StringBuilder("(SELECT E.PageId,E.Identifier,E.LangCode,E.Description,E.CategoryId,E.StartDateTime,E.EndDateTime,E.CreatedBy FROM ");
            sql.Append(TableNameDetails + " E, Pages P");
            sql.Append(" WHERE E.Deleted IS NULL");
            sql.Append(" AND E.LangCode='" + dbEncode(lang.shortCode) + "'");
            sql.Append(" AND P.Deleted IS NULL");
            sql.Append(" AND E.PageId=P.PageId");
            sql.Append(" ORDER BY E.StartDateTime DESC, E.EndDateTime DESC");

            if (count > -1)
            {
                sql.Append(" LIMIT 0, " + count.ToString() + ")");
                sql.Append(" UNION (SELECT E.PageId,E.Identifier,E.LangCode,E.Description,E.CategoryId,E.StartDateTime,E.EndDateTime,E.CreatedBy FROM ");
                sql.Append(TableNameDetails + " E, Pages P");
                sql.Append(" WHERE E.Deleted IS NULL");
                sql.Append(" AND E.LangCode='" + dbEncode(lang.shortCode) + "'");
                sql.Append(" AND P.Deleted IS NULL");
                sql.Append(" AND E.PageId=P.PageId");
                sql.Append(" AND E.PageId=" + attachedEventId + ");");
            }
            else
                sql.Append(");");

            List<EventCalendarDetailsData> list = new List<EventCalendarDetailsData>();
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasRows(ds) == false)
                return list;

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                EventCalendarDetailsData entity = new EventCalendarDetailsData();
                rowToData(dr, entity);
                list.Add(entity);
            }
            return list;
        }

        /// <summary>
        /// Select a single event details data
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="createIfNotExist"></param>
        /// <returns></returns>
        public EventCalendarDetailsData fetchDetailsData(CmsPage page, int identifier, CmsLanguage lang, bool createIfNotExist)
        {
            EventCalendarDetailsData entity = new EventCalendarDetailsData(page, identifier, lang);
            if (page.ID < 0 || identifier < 0)
                return entity;

            StringBuilder sql = new StringBuilder("SELECT PageId,Identifier,LangCode,Description,CategoryId,StartDateTime,EndDateTime,CreatedBy FROM ");
            sql.Append(TableNameDetails);
            sql.Append(" WHERE PageId=" + page.ID.ToString());
            sql.Append(" AND LangCode='" + dbEncode(lang.shortCode) + "'");
            sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND Deleted IS NULL;");

            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                rowToData(dr, entity);
            }
            else
            {
                if (createIfNotExist)
                {
                    if (insertDetailsData(page, identifier, lang, entity) == false)
                        throw new Exception("EventCalendarDb.fetchDetailsData() database error: Error creating new placeholder");
                }
                else
                    throw new Exception("EventCalendarDb.fetchDetailsData() database error: placeholder does not exist");
            }
            return entity;
        }

        /// <summary>
        /// Select events which between the start/end date time values provided.
        /// </summary>
        /// <param name="strDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public List<EventCalendarDetailsData> fetchDetailsDataByRange(DateTime strDateTime, DateTime endDateTime, CmsLanguage lang)
        {
            StringBuilder sql = new StringBuilder("SELECT E.PageId,E.Identifier,E.LangCode,E.Description,E.CategoryId,E.StartDateTime,E.EndDateTime,E.CreatedBy FROM ");
            sql.Append(TableNameDetails + " E, Pages P");
            sql.Append(" WHERE ((E.EndDateTime>=" + dbEncode(strDateTime));
            sql.Append(" AND E.EndDateTime<=" + dbEncode(endDateTime) + ")");
            sql.Append(" OR (E.StartDateTime>=" + dbEncode(strDateTime));
            sql.Append(" AND E.StartDateTime<=" + dbEncode(endDateTime) + "))");
            sql.Append(" AND E.LangCode='" + dbEncode(lang.shortCode) + "'");
            sql.Append(" AND E.Deleted IS NULL");
            sql.Append(" AND P.Deleted IS NULL");
            sql.Append(" AND E.PageId=P.PageId");
            sql.Append(" ORDER BY E.StartDateTime, E.EndDateTime;");

            List<EventCalendarDetailsData> list = new List<EventCalendarDetailsData>();
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasRows(ds) == false)
                return list;

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                EventCalendarDetailsData entity = new EventCalendarDetailsData();
                rowToData(dr, entity);
                list.Add(entity);
            }
            return list;
        }

        /// <summary>
        /// Select the events from the 1st of current month to the 31st of the current month.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public List<EventCalendarDetailsData> fetchDetailsDataCurrentMonth(DateTime dt, CmsLanguage lang)
        {
            DateTime start = new DateTime(dt.Year, dt.Month, 1, 0, 0, 0);
            DateTime end = new DateTime(dt.Year, dt.Month, 1, 23, 59, 59);
            end = end.AddMonths(1).AddDays(-1);
            return fetchDetailsDataByRange(start, end, lang);
        }

        /// <summary>
        /// Select the events which are applicable since today, limit by a count.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="lang"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<EventCalendarDetailsData> fetchUpcomingEventDetails(DateTime dt, CmsLanguage lang, int count)
        {
            StringBuilder sql = new StringBuilder("SELECT E.PageId,E.Identifier,E.LangCode,E.Description,E.CategoryId,E.StartDateTime,E.EndDateTime,E.CreatedBy FROM ");
            sql.Append(TableNameDetails + " E, Pages P");
            sql.Append(" WHERE E.EndDateTime >= " + dbEncode(dt));
            sql.Append(" AND E.LangCode='" + dbEncode(lang.shortCode) + "'");
            sql.Append(" AND E.Deleted IS NULL");
            sql.Append(" AND P.Deleted IS NULL");
            sql.Append(" AND E.PageId=P.PageId");
            sql.Append(" ORDER BY E.StartDateTime, E.EndDateTime");
            sql.Append(" LIMIT 0, " + count + ";");

            DataSet ds = this.RunSelectQuery(sql.ToString());

            List<EventCalendarDetailsData> list = new List<EventCalendarDetailsData>();
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    EventCalendarDetailsData entity = new EventCalendarDetailsData();
                    rowToData(dr, entity);
                    list.Add(entity);
                }
            }
            return list;
        }

        /// <summary>
        /// Update an event details data
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="lang"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool updateDetailsData(CmsPage page, int identifier, CmsLanguage lang, EventCalendarDetailsData entity)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(TableNameDetails);
            sql.Append(" SET Description = '" + dbEncode(entity.Description) + "',");
            sql.Append(" CategoryId = " + entity.CategoryId.ToString() + ",");
            sql.Append(" StartDateTime = " + dbEncode(entity.StartDateTime) + ",");
            sql.Append(" EndDateTime = " + dbEncode(entity.EndDateTime) + ",");
            sql.Append(" CreatedBy = '" + dbEncode(entity.CreatedBy) + "'");
            sql.Append(" WHERE PageId=" + page.ID.ToString());
            sql.Append(" AND LangCode='" + dbEncode(lang.shortCode) + "'");
            sql.Append(" AND Identifier=" + identifier.ToString());

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        protected void rowToData(DataRow dr, EventCalendarDetailsData entity)
        {
            entity.PageId = Convert.ToInt32(dr["PageId"]);
            entity.Identifier = Convert.ToInt32(dr["Identifier"]);
            entity.Lang = new CmsLanguage(dr["LangCode"].ToString());
            entity.Description = dr["Description"].ToString();
            entity.CategoryId = Convert.ToInt32(dr["CategoryId"]);
            entity.StartDateTime = Convert.ToDateTime(dr["StartDateTime"]);
            entity.EndDateTime = Convert.ToDateTime(dr["EndDateTime"]);
            entity.CreatedBy = dr["CreatedBy"].ToString();
        }
        #endregion

        #region category
        protected static string TableNameCategory = "EventCalendarCategory";

        public class EventCalendarCategoryData
        {
            private int categoryId = -1;
            public int CategoryId
            {
                get { return categoryId; }
                set { categoryId = value; }
            }

            private CmsLanguage lang = CmsConfig.Languages[0];
            public CmsLanguage Lang
            {
                get { return lang; }
                set { lang = value; }
            }

            private string colorHex = "";
            public string ColorHex
            {
                get { return colorHex; }
                set { colorHex = value; }
            }

            private string title = "";
            public string Title
            {
                get { return title; }
                set { title = value; }
            }

            private string description = "";
            public string Description
            {
                get { return description; }
                set { description = value; }
            }

            /// <summary>
            /// Check if the current values in the instance is valid
            /// </summary>
            /// <returns></returns>
            public string validate()
            {
                StringBuilder sb = new StringBuilder();

                if (colorHex == "")
                    sb.Append("Color Hex is missing, ");
                    
                if (title == "")
                    sb.Append("Title is missing, ");

                if (description == "")
                    sb.Append("Description is missing, ");

                try
                {
                    sb.Remove(sb.Length - 2, 2);
                }
                catch { }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Insert event category
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool insertCategoryData(EventCalendarCategoryData entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(TableNameCategory);
            sql.Append(" (CategoryId,LangCode,ColorHex,Title,Description) VALUES (");
            sql.Append(entity.CategoryId.ToString() + ",'");
            sql.Append(dbEncode(entity.Lang.shortCode) + "','");
            sql.Append(dbEncode(entity.ColorHex) + "','");
            sql.Append(dbEncode(entity.Title) + "','");
            sql.Append(dbEncode(entity.Description) + "');");

            int affected = this.RunUpdateQuery(sql.ToString());
            if (affected > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Update event category
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool updateCategoryData(EventCalendarCategoryData entity)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(TableNameCategory);
            sql.Append(" SET ColorHex='" + dbEncode(entity.ColorHex) + "',");
            sql.Append(" Title='" + dbEncode(entity.Title) + "',");
            sql.Append(" Description='" + dbEncode(entity.Description) + "'");
            sql.Append(" WHERE CategoryId=" + entity.CategoryId.ToString());
            sql.Append(" AND LangCode='" + dbEncode(entity.Lang.shortCode) + "';");

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        /// <summary>
        /// Select the MAX() of category id of existing record, and then add 1.
        /// </summary>
        /// <returns></returns>
        public int fetchNextCategoryId()
        {
            StringBuilder sql = new StringBuilder("SELECT MAX(CategoryId) AS maxCategoryId FROM ");
            sql.Append(TableNameCategory);
            sql.Append(";");

            int returnVal = 1;
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds) == false)
                return returnVal;

            DataRow dr = ds.Tables[0].Rows[0];
            object obj = dr["maxCategoryId"];
            try
            {
                return Convert.ToInt32(obj) + 1;
            }
            catch
            {
                return returnVal;
            }
        }

        /// <summary>
        /// Select all the event categories
        /// </summary>
        /// <returns></returns>
        public List<EventCalendarCategoryData> fetchCategoryList()
        {
            StringBuilder sql = new StringBuilder("SELECT CategoryId,LangCode,ColorHex,Title,Description FROM ");
            sql.Append(TableNameCategory);
            sql.Append(" ORDER BY CategoryId, LangCode;");

            DataSet ds = this.RunSelectQuery(sql.ToString());

            List<EventCalendarCategoryData> list = new List<EventCalendarCategoryData>();
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    EventCalendarCategoryData entity = new EventCalendarCategoryData();
                    rowToData(dr, entity);
                    list.Add(entity);
                }
            }
            return list;
        }

        /// <summary>
        /// Select all event categories by language
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public List<EventCalendarCategoryData> fetchCategoryList(CmsLanguage lang)
        {
            StringBuilder sql = new StringBuilder("SELECT CategoryId,LangCode,ColorHex,Title,Description FROM ");
            sql.Append(TableNameCategory);
            sql.Append(" WHERE LangCode='" + dbEncode(lang.shortCode) + "'");
            sql.Append(" ORDER BY CategoryId;");

            DataSet ds = this.RunSelectQuery(sql.ToString());

            List<EventCalendarCategoryData> list = new List<EventCalendarCategoryData>();
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    EventCalendarCategoryData entity = new EventCalendarCategoryData();
                    rowToData(dr, entity);
                    list.Add(entity);
                }
            }
            return list;
        }

        /// <summary>
        /// Select an event category by language and category id
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public EventCalendarCategoryData fetchCategoryByIdAndLang(CmsLanguage lang, int categoryId)
        {
            StringBuilder sql = new StringBuilder("SELECT CategoryId,LangCode,ColorHex,Title,Description FROM ");
            sql.Append(TableNameCategory);
            sql.Append(" WHERE CategoryId=" + categoryId.ToString());
            sql.Append(" AND LangCode='" + dbEncode(lang.shortCode) + "';");

            DataSet ds = this.RunSelectQuery(sql.ToString());

            EventCalendarCategoryData entity = new EventCalendarCategoryData();
            if (this.hasSingleRow(ds) == false)
                return entity;

            DataRow dr = ds.Tables[0].Rows[0];
            rowToData(dr, entity);

            return entity;
        }

        protected void rowToData(DataRow dr, EventCalendarCategoryData entity)
        {
            entity.CategoryId = Convert.ToInt32(dr["CategoryId"]);
            entity.Lang = new CmsLanguage(dr["LangCode"].ToString());
            entity.ColorHex = dr["ColorHex"].ToString();
            entity.Title = dr["Title"].ToString();
            entity.Description = dr["Description"].ToString();
        }
        #endregion
    }
}
