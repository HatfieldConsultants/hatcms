using System;
using System.Data;
using System.Collections;

namespace HatCMS.Placeholders
{
	/// <summary>
	/// A placeholder that stores HTML content
	/// </summary>
	public class HtmlContentDb: PlaceholderDb
	{
		#region HtmlContent

		public string getHtmlContent(CmsPage page, int identifier, CmsLanguage language, bool createNewIfDoesNotExist)
		{
			if (page.Id < 0 || identifier < 0)
				return "";

            string sql = "select html from htmlcontent c where c.pageid = " + page.Id.ToString() + " and c.RevisionNumber = " + page.RevisionNumber + " and c.identifier = " + identifier.ToString() + " and langShortCode like '" + language.shortCode.ToLower() + "'  and deleted is null;";
			DataSet ds = this.RunSelectQuery(sql);
			if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
			{				
				return (ds.Tables[0].Rows[0]["html"] as string);
			}
			else
			{				
				if(createNewIfDoesNotExist)
				{
                    bool b = createNewHtmlContent(page, identifier, language, "");
					
					if (!b)
					{
						throw new Exception("getHtmlContent database error: Error creating new placeholder");
					}
					else
					{
						return "";
					}
				}
				else
				{
                    throw new Exception("getHtmlContent database error: placeholder does not exist - " + page.Path + " (" + identifier.ToString() + ")");
				}
			}
			return "";
		} // getHtmlContent

        public bool createNewHtmlContent(CmsPage page, int identifier, CmsLanguage language, string html)
		{
            string sql = "insert into htmlcontent (pageid, identifier, langShortCode, RevisionNumber, html) values (";
            sql = sql + page.Id.ToString() + "," + identifier.ToString() + ", '"+dbEncode(language.shortCode.ToLower())+"', " + page.RevisionNumber + ", '" + dbEncode(html) + "'); ";

			int newId = this.RunInsertQuery(sql);
			if (newId > -1)
				return page.setLastUpdatedDateTimeToNow();
			else
				return false;

		}

        public bool saveUpdatedHtmlContent(CmsPage page, int identifier, CmsLanguage language, string html)
		{
			// with revisions, we don't update the table, we insert

            int newRevisionNumber = page.createNewRevision();
            if (newRevisionNumber < 0)
                return false;


            string sql = "insert into htmlcontent (pageid, identifier,langShortCode, RevisionNumber, html) values (";
            sql = sql + page.Id.ToString() + "," + identifier.ToString() + ", '"+dbEncode(language.shortCode.ToLower())+"', " + newRevisionNumber + ", '" + dbEncode(html) + "'); ";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;

		}
        

		#endregion
	}
}
