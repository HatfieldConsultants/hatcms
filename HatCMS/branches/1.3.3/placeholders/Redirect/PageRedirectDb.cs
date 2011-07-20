using System;
using System.Data;
using System.Collections;

namespace HatCMS.Placeholders
{
	/// <summary>
	/// Summary description for PageRedirectDb.
	/// </summary>
	public class PageRedirectDb: PlaceholderDb
	{

		//------------------------------------------------------------------------------------------
		// PageRedirect placeholder database functions
		//------------------------------------------------------------------------------------------

		/// <summary>
		/// gets the stored URL for the PageRedirect placeholder
		/// </summary>
		/// <param name="page"></param>
		/// <param name="identifier"></param>
		/// <param name="createNewIfDoesNotExist"></param>
		/// <returns></returns>
		public string getPageRedirectUrl(CmsPage page, int identifier, string langShortCode, bool createNewIfDoesNotExist)
		{
			if (page.ID < 0 || identifier < 0)
				return "";

            string sql = "select url from pageredirect c where c.pageid = " + page.ID.ToString() + " and c.identifier = " + identifier.ToString() + " and langShortCode = '"+dbEncode(langShortCode)+"' and deleted is null;";
			DataSet ds = this.RunSelectQuery(sql);
			if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
			{
				string url = (ds.Tables[0].Rows[0]["url"] as string);
				return url;
			}
			else
			{				
				if(createNewIfDoesNotExist)
				{
					bool b = createNewPageRedirect(page,identifier, langShortCode, "");	
					
					if (!b)
					{
						throw new Exception("getPageRedirect database error: Error creating new placeholder");
					}
					else
					{
						return "";
					}
				}
				else
				{
					throw new Exception("getPageRedirect database error: placeholder does not exist");
				}
			}
			return "";
		} // getPageRedirect

		/// <summary>
		/// creates a new PageRedirect entry in the database
		/// </summary>
		/// <param name="page"></param>
		/// <param name="identifier"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public bool createNewPageRedirect(CmsPage page, int identifier, string langShortCode, string url)
		{
			url = this.dbEncode(url);
			string sql = "insert into pageredirect (pageid, identifier, langShortCode, url) values (";
			sql = sql +page.ID.ToString()+","+identifier.ToString()+",'"+dbEncode(langShortCode)+"', '"+dbEncode(url)+"'); ";			

			int newId = this.RunInsertQuery(sql);
			if (newId > -1)
				return page.setLastUpdatedDateTimeToNow();
			else
				return false;

		} // createNewPageRedirect

		/// <summary>
		/// saves an updated PageRedirect url to the database
		/// </summary>
		/// <param name="page"></param>
		/// <param name="identifier"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public bool saveUpdatedPageRedirect(CmsPage page, int identifier, string langShortCode, string url)
		{
			url = this.dbEncode(url);
			string sql = "update pageredirect set url= '"+dbEncode(url)+"' where pageid= "+page.ID.ToString();
			sql = sql +  " AND identifier = "+identifier.ToString()+" ";
            sql = sql + " AND langShortCode = '" + dbEncode(langShortCode) + "'; ";

			int numAffected = this.RunUpdateQuery(sql);
			if (numAffected > 0)
				return page.setLastUpdatedDateTimeToNow();
			else
				return false;

		}


	}
}
