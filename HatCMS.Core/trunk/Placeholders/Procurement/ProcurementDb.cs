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
using System.Collections;
using System.Collections.Generic;

namespace HatCMS.Placeholders.Procurement
{
    public class ProcurementDb : PlaceholderDb
    {
        #region aggregator

        protected static string TableNameAggregator = "procurementaggregator";

        public class ProcurementAggregatorData
        {
            /// <summary>
            /// {0} = Procurement Date in format "MMM d, yyyy"
            /// {1} = Procurement Title
            /// {2} = Full Procurement detail view URL
            /// </summary>
            public static string DisplayFormat = "<strong>{1}</strong> ({0}) &#160;<a class=\"rightArrowLink\" href=\"{2}\">{3}</a><br /><br />";

            private int yearToDisplay = -1;
            public int YearToDisplay
            {
                get { return yearToDisplay; }
                set { yearToDisplay = value; }
            }
        }

        public bool addProcurementAggregator(CmsPage page, int identifier, CmsLanguage lang, ProcurementAggregatorData entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(TableNameAggregator);
            sql.Append(" (PageId,Identifier,LangCode,DefaultYearToDisplay) VALUES (");
            sql.Append(page.Id.ToString() + ",");
            sql.Append(identifier.ToString() + ",'");
            sql.Append(dbEncode(lang.shortCode) + "',");
            sql.Append(entity.YearToDisplay.ToString() + ");");

            int affected = this.RunUpdateQuery(sql.ToString());
            if (affected > 0)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;
        }

        public ProcurementAggregatorData fetchProcurementAggregator(CmsPage page, int identifier, CmsLanguage lang, bool createIfNotExist)
        {
            if (page.Id < 0 || identifier < 0)
                return new ProcurementAggregatorData();

            StringBuilder sql = new StringBuilder("SELECT DefaultYearToDisplay FROM ");
            sql.Append(TableNameAggregator);
            sql.Append(" WHERE PageId=" + page.Id.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "'");
            sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND Deleted is null;");

            ProcurementAggregatorData entity = new ProcurementAggregatorData();
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                entity.YearToDisplay = Convert.ToInt32(dr["DefaultYearToDisplay"]);
            }
            else
            {
                if (createIfNotExist)
                {
                    if (addProcurementAggregator(page, identifier, lang, entity) == false)
                        throw new Exception("fetchProcurementAggregator() database error: Error creating new placeholder");
                }
                else
                    throw new Exception("fetchProcurementAggregator() database error: placeholder does not exist");
            }
            return entity;
        }

        public bool updateProcurementAggregator(CmsPage page, int identifier, CmsLanguage lang, ProcurementAggregatorData entity)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(TableNameAggregator);
            sql.Append(" SET DefaultYearToDisplay = " + entity.YearToDisplay.ToString());
            sql.Append(" WHERE PageId=" + page.Id.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "'");
            sql.Append(" AND Identifier=" + identifier.ToString());

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        #endregion

        #region details
        
        protected static string TableNameDetails = "procurementdetails";

        public class ProcurementDetailsData
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

            private DateTime dateOfProcurement = DateTime.Now;
            public DateTime DateOfProcurement
            {
                get { return dateOfProcurement; }
                set { dateOfProcurement = value; }
            }

            public ProcurementDetailsData(CmsPage page, int identifier, CmsLanguage lang)
            {
                this.pageId = page.Id;
                this.identifier = identifier;
                this.lang = lang;
            }

            public ProcurementDetailsData(int pageId, int identifier, string langCode, DateTime dateOfProcurement)
            {
                this.PageId = pageId;
                this.Identifier = identifier;
                this.Lang = new CmsLanguage(langCode);
                this.DateOfProcurement = dateOfProcurement;
            }

            public int CompareTo(ProcurementDetailsData d2, ProcurementDetailsDataComparer.CompareType field)
            {
                switch (field)
                {
                    case ProcurementDetailsDataComparer.CompareType.DateOfProcurement:
                        return this.DateOfProcurement.CompareTo(d2.DateOfProcurement);
                    default:
                        return this.PageId.CompareTo(d2.PageId);
                }
            }
        }

        public class ProcurementDetailsDataComparer : IComparer
        {
            public enum CompareType { DateOfProcurement } // currently can sort by DateOfProcurement only

            private CompareType field;
            public CompareType Field
            {
                get { return field; }
                set { field = value; }
            }

            public int Compare(object x, object y)
            {
                ProcurementDetailsData d1, d2;

                if (x is ProcurementDetailsData)
                    d1 = x as ProcurementDetailsData;
                else
                    throw new ArgumentException("Object is not of type ProcurementDetailsData");

                if (y is ProcurementDetailsData)
                    d2 = y as ProcurementDetailsData;
                else
                    throw new ArgumentException("Object is not of type ProcurementDetailsData");

                return d1.CompareTo(d2, Field);
            }
        }

        public bool addProcurementDetails(CmsPage page, ProcurementDetailsData entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(TableNameDetails);
            sql.Append(" (PageId,Identifier,LangCode,DateOfProcurement) VALUES (");
            sql.Append(entity.PageId.ToString() + ",");
            sql.Append(entity.Identifier.ToString() + ",'");
            sql.Append(dbEncode(entity.Lang.shortCode) + "',");
            sql.Append(dbEncode(entity.DateOfProcurement) + ");");

            int affected = this.RunUpdateQuery(sql.ToString());
            if (affected > 0)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;
        }


        public ProcurementDetailsData fetchProcurementDetails(CmsPage page, int identifier, CmsLanguage lang, bool createIfNotExist)
        {
            if (page.Id < 0 || identifier < 0)
                return new ProcurementDetailsData(page, identifier, lang);

            StringBuilder sql = new StringBuilder("SELECT DateOfProcurement FROM ");
            sql.Append(TableNameDetails);
            sql.Append(" WHERE PageId=" + page.Id.ToString());
            sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "'");
            sql.Append(" AND Deleted is null;");

            ProcurementDetailsData entity = new ProcurementDetailsData(page, identifier, lang);
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                entity.DateOfProcurement = Convert.ToDateTime(dr["DateOfProcurement"]);
            }
            else
            {
                if (createIfNotExist)
                {
                    if (addProcurementDetails(page, entity) == false)
                        throw new Exception("fetchProcurementDetails() database error: Error creating new placeholder");
                }
                else
                    throw new Exception("fetchProcurementDetails() database error: placeholder does not exist");
            }
            return entity;
        }

        /// <summary>        
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="sequence"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ProcurementDetailsData> fetchProcurementDetailsByCount(CmsLanguage lang, int sequence, int count)
        {
            StringBuilder sql = new StringBuilder("SELECT N.PageId,N.Identifier,N.LangCode,N.DateOfProcurement FROM ");
            sql.Append(TableNameDetails + " N, pages P");
            sql.Append(" WHERE N.Deleted IS NULL");
            sql.Append(" AND P.Deleted IS NULL");
            sql.Append(" AND N.PageId=P.PageId ");
            sql.Append(" AND P.Deleted is null ");
            sql.Append(" ORDER BY DateOfProcurement desc");
            sql.Append(" LIMIT " + sequence + ", " + count + ";");

            DataSet ds = this.RunSelectQuery(sql.ToString());

            List<ProcurementDetailsData> list = new List<ProcurementDetailsData>();
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                    list.Add(ProcurementDetailsDataFromDataRow(dr));
            }
            return list;
        }

        public bool updateProcurementDetails(CmsPage page, int identifier, CmsLanguage lang, ProcurementDetailsData entity)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(TableNameDetails);
            sql.Append(" SET DateOfProcurement = " + dbEncode(entity.DateOfProcurement));
            sql.Append(" WHERE PageId=" + page.Id.ToString());
            sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "';");

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        public ProcurementDetailsData[] getProcurementDetailsByYear( int yr, CmsLanguage lang )
        {
            ArrayList where = new ArrayList();
            if (yr != -1)
                where.Add(" year(N.DateOfProcurement)=" + yr.ToString() + ") ");

            where.Add(" N.LangCode='" + lang.shortCode + "' ");
            where.Add(" N.Deleted is null ");
            where.Add(" P.Deleted is null ");
            string whereClause = String.Join(" AND ", (string[])where.ToArray(typeof(string)));

            StringBuilder sql = new StringBuilder("SELECT N.PageId, N.Identifier, N.LangCode, N.DateOfProcurement FROM ");
            sql.Append(TableNameDetails + " N, , pages P");
            sql.Append(" WHERE ");
            sql.Append(whereClause);
            sql.Append(" ORDER BY N.DateOfProcurement desc;");

            ArrayList arrayList = new ArrayList();
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                    arrayList.Add(ProcurementDetailsDataFromDataRow(dr));
            }

            return (ProcurementDetailsData[])arrayList.ToArray(typeof(ProcurementDetailsData));
        }

        protected ProcurementDetailsData ProcurementDetailsDataFromDataRow(DataRow dr)
        {
            int pageId = Convert.ToInt32(dr["Pageid"]);
            int identifier = Convert.ToInt32(dr["Identifier"]);
            string langCode = dr["LangCode"].ToString();
            DateTime dateOfProcurement = Convert.ToDateTime(dr["DateOfProcurement"]);
            return new ProcurementDetailsData(pageId, identifier, langCode, dateOfProcurement);
        }

        #endregion
    }
}
