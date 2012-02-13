using System;
using System.Data;
using System.Collections;

namespace HatCMS.Placeholders
{
	/// <summary>
	/// Summary description for PlainTextContentDb.
	/// </summary>
	public class PlainTextContentDb: PlaceholderDb
	{
		#region PlainTextContent

		public string getPlainTextContent(CmsPage page, int identifier, CmsLanguage language, bool createNewIfDoesNotExist)
		{
			if (page.Id < 0 || identifier < 0)
				return "";
			
			string sql = "select PlainText from plaintextcontent c where c.pageid = "+page.Id.ToString()+" and c.identifier = "+identifier.ToString()+" and langShortCode like '"+dbEncode(language.shortCode)+"' and deleted is null;";
			DataSet ds = this.RunSelectQuery(sql);
			if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
			{
                return (ds.Tables[0].Rows[0]["PlainText"] as string);
			}
			else
			{				
				if(createNewIfDoesNotExist)
				{
                    bool b = createNewPlainTextContent(page, identifier, language, "");
					
					if (!b)
					{
                        throw new Exception("getPlainTextContent database error: Error creating new placeholder");
					}
					else
					{
						return "";
					}
				}
				else
				{
                    throw new Exception("getPlainTextContent database error: placeholder does not exist");
				}
			}
			return "";
        } // getPlainTextContent

		public bool createNewPlainTextContent(CmsPage page, int identifier, CmsLanguage language, string PlainText)
		{
            PlainText = this.dbEncode(PlainText);
            string sql = "insert into plaintextcontent (pageid, identifier, langShortCode, PlainText) values (";
            sql = sql + page.Id.ToString() + "," + identifier.ToString() + ",'" + language.shortCode + "','" + PlainText + "'); ";

			int newId = this.RunInsertQuery(sql);
			if (newId > -1)
				return page.setLastUpdatedDateTimeToNow();
			else
				return false;

		}

		public bool saveUpdatedPlainTextContent(CmsPage page, int identifier, CmsLanguage language, string PlainText)
		{
            PlainText = this.dbEncode(PlainText);
            string sql = "update plaintextcontent set PlainText= '" + PlainText + "' where pageid= " + page.Id.ToString();
            sql += " and langShortCode like '" + dbEncode(language.shortCode) + "' ";
            sql = sql + " AND identifier = " + identifier.ToString() + "; ";			

			int numAffected = this.RunUpdateQuery(sql);
			if (numAffected > 0)
				return page.setLastUpdatedDateTimeToNow();
			else
				return false;

		}

		#endregion
	}
}
