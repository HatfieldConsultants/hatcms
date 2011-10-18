using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HatCMS.Placeholders;

namespace HatCMS.Modules.Glossary
{
    public class GlossaryDb : PlaceholderDb
    {
        public GlossaryPlaceholderData getGlossary(CmsPage page, int identifier, CmsLanguage language, bool createNewIfDoesNotExist)
        {
            if (page.ID < 0 || identifier < 0)
                return new GlossaryPlaceholderData();

            string sql = "select glossaryid, sortOrder, viewMode from glossary c where c.pageid = " + page.ID.ToString() + " and c.identifier = " + identifier.ToString() + " and langShortCode like '" + dbEncode(language.shortCode) + "'  and deleted is null;";
            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                GlossaryPlaceholderData data = new GlossaryPlaceholderData();
                data.GlossaryId = Convert.ToInt32(dr["glossaryid"]);
                data.SortOrder = (GlossaryPlaceholderData.GlossarySortOrder)Enum.Parse(typeof(GlossaryPlaceholderData.GlossarySortOrder), dr["sortOrder"].ToString());
                data.ViewMode = (GlossaryPlaceholderData.GlossaryViewMode)Enum.Parse(typeof(GlossaryPlaceholderData.GlossaryViewMode), dr["viewMode"].ToString());
                return data;
            }
            else
            {
                if (createNewIfDoesNotExist)
                {
                    return createNewGlossary(page, identifier, language);
                }
                else
                {
                    throw new Exception("getGlossary database error: placeholder does not exist");
                }
            }
            return new GlossaryPlaceholderData();
        } // getGlossary

        public GlossaryPlaceholderData createNewGlossary(CmsPage page, int identifier, CmsLanguage language)
        {
            GlossaryPlaceholderData data = new GlossaryPlaceholderData();

            string sql = "insert into glossary (pageid, identifier, langShortCode, sortOrder, ViewMode) values (";
            sql = sql + page.ID.ToString() + "," + identifier.ToString() + ",";
            sql += "'" + dbEncode(language.shortCode) + "', ";
            sql = sql + "'" + Enum.GetName(typeof(GlossaryPlaceholderData.GlossarySortOrder), data.SortOrder) + "', ";
            sql = sql + "'" + Enum.GetName(typeof(GlossaryPlaceholderData.GlossaryViewMode), data.ViewMode) + "' ";
            sql += "); ";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {
                page.setLastUpdatedDateTimeToNow();
            }

            return data;

        } // createNewGlossary

        public bool saveUpdatedGlossary(CmsPage page, int identifier, CmsLanguage language, GlossaryPlaceholderData data, GlossaryData[] allGlossaryData)
        {
            string sql = "update glossary set ";
            sql += " sortOrder = '" + Enum.GetName(typeof(GlossaryPlaceholderData.GlossarySortOrder), data.SortOrder) + "', ";
            sql += " ViewMode = '" + Enum.GetName(typeof(GlossaryPlaceholderData.GlossaryViewMode), data.ViewMode) + "' ";
            sql += " where glossaryid = " + data.GlossaryId.ToString();
            
            int numAffected = this.RunUpdateQuery(sql);
            if (numAffected > 0)
            {
                if (RemoveAllExistingGlossaryDataAndInsertNew(data, allGlossaryData))
                    return page.setLastUpdatedDateTimeToNow();
            }

            return false;

        } // saveUpdatedGlossary

        private bool RemoveAllExistingGlossaryDataAndInsertNew(GlossaryPlaceholderData placeholder, GlossaryData[] allNewGlossaryData)
        {
            string delSql = "delete from glossarydata where phGlossaryId = "+placeholder.GlossaryId+"  ; ";
            int numUpdated = this.RunUpdateQuery(delSql);
            if (numUpdated >= 0)
            {
                if (allNewGlossaryData.Length == 0)
                    return true;

                string sql = "INSERT INTO glossarydata (phGlossaryId,isAcronym,word,description) VALUES ";
                List<string> sqlVals = new List<string>();
                foreach (GlossaryData g in allNewGlossaryData)
                {
                    
                    string Vals = ("(");
                    Vals += "" + placeholder.GlossaryId.ToString() + ", ";

                    if (g.isAcronym)
                        Vals += ("1, ");
                    else
                        Vals += ("0, ");

                    Vals += ("'" + dbEncode(g.word) + "', ");
                    Vals += ("'" + dbEncode(g.description) + "' ");
                    Vals += (")");

                    sqlVals.Add(Vals);
                } // foreach

                sql = sql + String.Join(",", sqlVals.ToArray()) + ";";
                int numInserted = this.RunUpdateQuery(sql);
                if (numInserted == allNewGlossaryData.Length)
                    return true;
            }
            return false;
        }

        private GlossaryData dataFromRow(DataRow dr)
        {
            GlossaryData d = new GlossaryData();
            d.Id = Convert.ToInt32(dr["GlossaryDataId"]);
            d.placeholderGlossaryId = Convert.ToInt32(dr["phGlossaryId"]);
            d.isAcronym = Convert.ToBoolean(dr["isAcronym"]);
            d.word = dr["word"].ToString();
            d.description = dr["description"].ToString();

            return d;
        }

        public GlossaryData[] getGlossaryData(int glossaryId)
        {
            string sql = "select * from glossarydata where Deleted IS NULL ";
            sql += " AND phGlossaryId = " + glossaryId + " ";
            sql += " ORDER BY left(word,1) "; // always order by the first character of the name
            // -- run the query
            List<GlossaryData> ret = new List<GlossaryData>();
            DataSet ds = this.RunSelectQuery(sql);

            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ret.Add(dataFromRow(dr));
                } // foreach
            }

            return ret.ToArray();

        }

        public GlossaryData[] getGlossaryData(GlossaryPlaceholderData placeholderData, string letterToDisplay)
        {
            string sql = "select * from glossarydata where Deleted IS NULL ";

            switch (placeholderData.ViewMode)
            {
                case GlossaryPlaceholderData.GlossaryViewMode.PagePerLetter:
                    if (letterToDisplay.Length != 1)
                    {
                        // throw new ArgumentException("letterToDisplay must be specified!!!");
                    }
                    else
                    {
                        sql += " AND LEFT(word,1) in ('" + dbEncode(letterToDisplay.ToString().ToLower()) + "','" + dbEncode(letterToDisplay.ToString().ToUpper()) + "') ";
                    }
                    break;
                case GlossaryPlaceholderData.GlossaryViewMode.SinglePageWithJumpList:
                    // nothing to-do
                    break;
                default:
                    throw new ArgumentException("invalid ViewMode");
            } // switch ViewMode
            sql += " AND phGlossaryId = " + placeholderData.GlossaryId + " ";
            sql += " ORDER BY left(word,1) "; // always order by the first character of the name
            
            switch (placeholderData.SortOrder)
            {
                case GlossaryPlaceholderData.GlossarySortOrder.byWord:
                    sql += ", word ";
                    break;
                case GlossaryPlaceholderData.GlossarySortOrder.byDescription:
                    sql += ", Description ";
                    break;
                case GlossaryPlaceholderData.GlossarySortOrder.byId:
                    sql += ", GlossaryDataId ";
                    break;
                default:
                    throw new ArgumentException("invalid sortOrder");
            } // switch SortOrder

            // -- run the query
            List<GlossaryData> ret = new List<GlossaryData>();
            DataSet ds = this.RunSelectQuery(sql);
            
            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ret.Add(dataFromRow(dr));
                } // foreach
            }

            return ret.ToArray();

        } // getGlossaryData

        public string[] getAllCharactersWithData(GlossaryData[] items)
        {
            List<string> ret = new List<string>();
            foreach (GlossaryData item in items)
            {
                string c = item.word[0].ToString().ToUpper();
                if (ret.IndexOf(c) < 0)
                    ret.Add(c);
            } // foreach

            return ret.ToArray();
        }


    } // GlossaryDb
}
