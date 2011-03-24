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

namespace HatCMS.Placeholders.NewsDatabase
{
    public class NewsArticleDb : PlaceholderDb
    {
        #region aggregator

        protected static string TableNameAggregator = "newsarticleaggregator";

        public class NewsArticleAggregatorData
        {

            private int yearToDisplay = -1;
            public int YearToDisplay
            {
                get { return yearToDisplay; }
                set { yearToDisplay = value; }
            }
        }

        public bool addNewsAggregator(CmsPage page, int identifier, CmsLanguage lang, NewsArticleAggregatorData entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(TableNameAggregator);
            sql.Append(" (PageId,Identifier,LangCode,DefaultYearToDisplay) VALUES (");
            sql.Append(page.ID.ToString() + ",");
            sql.Append(identifier.ToString() + ",'");
            sql.Append(dbEncode(lang.shortCode) + "',");
            sql.Append(entity.YearToDisplay.ToString() + ");");

            int affected = this.RunUpdateQuery(sql.ToString());
            if (affected > 0)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;
        }

        public NewsArticleAggregatorData fetchNewsAggregator(CmsPage page, int identifier, CmsLanguage lang, bool createIfNotExist)
        {
            if (page.ID < 0 || identifier < 0)
                return new NewsArticleAggregatorData();

            StringBuilder sql = new StringBuilder("SELECT DefaultYearToDisplay FROM ");
            sql.Append(TableNameAggregator);
            sql.Append(" WHERE PageId=" + page.ID.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "'");
            sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND Deleted is null;");

            NewsArticleAggregatorData entity = new NewsArticleAggregatorData();
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
                    if (addNewsAggregator(page, identifier, lang, entity) == false)
                        throw new Exception("fetchNewsAggregator() database error: Error creating new placeholder");
                }
                else
                    throw new Exception("fetchNewsAggregator() database error: placeholder does not exist");
            }
            return entity;
        }

        public bool updateNewsAggregator(CmsPage page, int identifier, CmsLanguage lang, NewsArticleAggregatorData entity)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(TableNameAggregator);
            sql.Append(" SET DefaultYearToDisplay = " + entity.YearToDisplay.ToString());
            sql.Append(" WHERE PageId=" + page.ID.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "'");
            sql.Append(" AND Identifier=" + identifier.ToString());

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        #endregion

        #region details
        
        protected static string TableNameDetails = "newsarticledetails";

        public class NewsArticleDetailsData
        {
            private int pageId = -1;
            public int DetailsPageId
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

            private DateTime dateOfNews = DateTime.Now;
            public DateTime DateOfNews
            {
                get { return dateOfNews; }
                set { dateOfNews = value; }
            }

            public NewsArticleDetailsData(CmsPage page, int identifier, CmsLanguage lang)
            {
                this.pageId = page.ID;
                this.identifier = identifier;
                this.lang = lang;
            }

            public NewsArticleDetailsData(int pageId, int identifier, string langCode, DateTime dateOfNews)
            {
                this.DetailsPageId = pageId;
                this.Identifier = identifier;
                this.Lang = new CmsLanguage(langCode);
                this.DateOfNews = dateOfNews;
            }

            public int CompareTo(NewsArticleDetailsData d2, NewsArticleDetailsDataComparer.CompareType field)
            {
                switch (field)
                {
                    case NewsArticleDetailsDataComparer.CompareType.DateOfNews:
                        return this.DateOfNews.CompareTo(d2.DateOfNews);
                    default:
                        return this.DetailsPageId.CompareTo(d2.DetailsPageId);
                }
            }
        }

        public class NewsArticleDetailsDataComparer : IComparer
        {
            public enum CompareType { DateOfNews } // currently can sort by DateOfNews only

            private CompareType field;
            public CompareType Field
            {
                get { return field; }
                set { field = value; }
            }

            public int Compare(object x, object y)
            {
                NewsArticleDetailsData d1, d2;

                if (x is NewsArticleDetailsData)
                    d1 = x as NewsArticleDetailsData;
                else
                    throw new ArgumentException("Object is not of type NewsArticleDetailsData");

                if (y is NewsArticleDetailsData)
                    d2 = y as NewsArticleDetailsData;
                else
                    throw new ArgumentException("Object is not of type NewsArticleDetailsData");

                return d1.CompareTo(d2, Field);
            }
        }

        public bool addNewsDetails(CmsPage page, NewsArticleDetailsData entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(TableNameDetails);
            sql.Append(" (PageId,Identifier,LangCode,DateOfNews) VALUES (");
            sql.Append(entity.DetailsPageId.ToString() + ",");
            sql.Append(entity.Identifier.ToString() + ",'");
            sql.Append(dbEncode(entity.Lang.shortCode) + "',");
            sql.Append(dbEncode(entity.DateOfNews) + ");");

            int affected = this.RunUpdateQuery(sql.ToString());
            if (affected > 0)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;
        }


        public NewsArticleDetailsData fetchNewsDetails(CmsPage page, int identifier, CmsLanguage lang, bool createIfNotExist)
        {
            if (page.ID < 0 || identifier < 0)
                return new NewsArticleDetailsData(page, identifier, lang);

            StringBuilder sql = new StringBuilder("SELECT DateOfNews FROM ");
            sql.Append(TableNameDetails);
            sql.Append(" WHERE PageId=" + page.ID.ToString());
            sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "'");
            sql.Append(" AND Deleted is null;");

            NewsArticleDetailsData entity = new NewsArticleDetailsData(page, identifier, lang);
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                entity.DateOfNews = Convert.ToDateTime(dr["DateOfNews"]);
            }
            else
            {
                if (createIfNotExist)
                {
                    if (addNewsDetails(page, entity) == false)
                        throw new Exception("fetchNewsDetails() database error: Error creating new placeholder");
                }
                else
                    throw new Exception("fetchNewsDetails() database error: placeholder does not exist");
            }
            return entity;
        }

        public List<NewsArticleDetailsData> fetchNewsDetailsByCount(CmsLanguage lang, int sequence, int count)
        {
            StringBuilder sql = new StringBuilder("SELECT N.PageId,N.Identifier,N.LangCode,N.DateOfNews FROM ");
            sql.Append(TableNameDetails + " N, pages P");
            sql.Append(" WHERE N.Deleted IS NULL");
            sql.Append(" AND P.Deleted IS NULL");
            sql.Append(" AND N.PageId=P.PageId");
            sql.Append(" ORDER BY DateOfNews desc");
            sql.Append(" LIMIT " + sequence + ", " + count + ";");

            DataSet ds = this.RunSelectQuery(sql.ToString());

            List<NewsArticleDetailsData> list = new List<NewsArticleDetailsData>();
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                    list.Add(NewsDetailsDataFromDataRow(dr));
            }
            return list;
        }

        public bool updateNewsDetails(CmsPage page, int identifier, CmsLanguage lang, NewsArticleDetailsData entity)
        {
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(TableNameDetails);
            sql.Append(" SET DateOfNews = " + dbEncode(entity.DateOfNews));
            sql.Append(" WHERE PageId=" + page.ID.ToString());
            sql.Append(" AND Identifier=" + identifier.ToString());
            sql.Append(" AND LangCode='" + lang.shortCode + "';");

            int affected = this.RunUpdateQuery(sql.ToString());
            return (affected > 0);
        }

        public NewsArticleDetailsData[] getNewsDetailsByYear( int yr, CmsLanguage lang )
        {
            List<string> wheres = new List<string>();
            if (yr > 0)
                wheres.Add(" year(N.DateOfNews)=" + yr.ToString() + ") ");

            wheres.Add(" N.LangCode='" + lang.shortCode + "' ");
            wheres.Add(" N.Deleted is null ");
            string whereClause = String.Join(" AND ", wheres.ToArray());

            StringBuilder sql = new StringBuilder("SELECT N.PageId, N.Identifier, N.LangCode, N.DateOfNews FROM ");
            sql.Append(TableNameDetails + " N");
            sql.Append(" WHERE ");
            sql.Append(whereClause);
            sql.Append(" ORDER BY N.DateOfNews desc;");

            List<NewsArticleDetailsData> arrayList = new List<NewsArticleDetailsData>();
            DataSet ds = this.RunSelectQuery(sql.ToString());
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                    arrayList.Add(NewsDetailsDataFromDataRow(dr));
            }

            return arrayList.ToArray();
        }

        protected NewsArticleDetailsData NewsDetailsDataFromDataRow(DataRow dr)
        {
            int pageId = Convert.ToInt32(dr["Pageid"]);
            int identifier = Convert.ToInt32(dr["Identifier"]);
            string langCode = dr["LangCode"].ToString();
            DateTime dateOfNews = Convert.ToDateTime(dr["DateOfNews"]);
            return new NewsArticleDetailsData(pageId, identifier, langCode, dateOfNews);
        }

        #endregion
    }
}
