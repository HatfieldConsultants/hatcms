using System;
using System.Data;
using System.Collections;

namespace HatCMS.Placeholders
{
	/// <summary>
	/// Summary description for PageCommentsDb.
	/// </summary>
	public class PageCommentsDb: PlaceholderDb
	{
		#region PageComments

		//------------------------------------------------------------------------------------------
		// PageComments placeholder database functions
		//------------------------------------------------------------------------------------------
		/// <summary>
		/// gets the stored URL for the PageRedirect placeholder
		/// </summary>
		/// <param name="page"></param>
		/// <param name="identifier"></param>
		/// <param name="createNewIfDoesNotExist"></param>
		/// <returns></returns>
		public PageCommentData[] getPageComments(CmsPage page, int identifier, bool createNewIfDoesNotExist)
		{
			if (page.ID < 0 || identifier < 0)
				return new PageCommentData[0];
			
			string sql = "select * from pagecomments c where c.pageid = "+page.ID.ToString()+" and c.identifier = "+identifier.ToString()+" and c.deleted is null;";
			DataSet ds = this.RunSelectQuery(sql);
			if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
			{
				ArrayList arrayList = new ArrayList();
				foreach(DataRow dr in ds.Tables[0].Rows)
				{
					arrayList.Add(PageCommentDataFromDataRow(dr));
				}
				PageCommentData[] ret = new PageCommentData[arrayList.Count];
				arrayList.CopyTo(ret);
				return ret;
			}

			return new PageCommentData[0];
		} // getPageRedirect

		/// <summary>
		/// creates a new PageRedirect entry in the database
		/// </summary>
		/// <param name="page"></param>
		/// <param name="identifier"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public bool createNewPageComment(CmsPage page, int identifier, PageCommentData pageComment)
		{			
			string sql = "insert into pagecomments (pageid, identifier, CommentText, PostedDate, PostedBy) values (";
			sql = sql +page.ID.ToString()+","+identifier.ToString()+",'"+dbEncode(pageComment.CommentText)+"', NOW(), '"+dbEncode(pageComment.PostedBy)+"'); ";
			// sql = sql + " SELECT @@IDENTITY as newId;";

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
		public bool saveUpdatedPageComment(CmsPage page, int identifier, PageCommentData pageComment)
		{			
			string sql = "update pagecomments set CommentText= '"+dbEncode(pageComment.CommentText)+"' where PageCommentId= "+pageComment.PageCommentId.ToString();
			sql = sql +  " AND identifier = "+identifier.ToString()+"; ";
			// sql = sql + " SELECT @@IDENTITY as newId;";

			int numAffected = this.RunUpdateQuery(sql);
			if (numAffected > 0)
				return page.setLastUpdatedDateTimeToNow();
			else
				return false;

		}

		public bool deletePageComment(CmsPage page, int identifier, PageCommentData pageComment)
		{			
			string sql = "update pagecomments c set c.Deleted = NOW() where PageCommentId= "+pageComment.PageCommentId.ToString();
			sql = sql +  " AND identifier = "+identifier.ToString()+"; ";
			// sql = sql + " SELECT @@IDENTITY as newId;";

			int numAffected = this.RunUpdateQuery(sql);
			if (numAffected > 0)
				return page.setLastUpdatedDateTimeToNow();
			else
				return false;

		}

		private PageCommentData PageCommentDataFromDataRow(DataRow dr)
		{
			PageCommentData p = new PageCommentData();
			p.PageCommentId = Convert.ToInt32(dr["PageCommentId"]);
			p.PostedBy = dr["PostedBy"].ToString();
			p.postedDate = Convert.ToDateTime(dr["PostedDate"].ToString());
			p.CommentText = dr["CommentText"].ToString();
			return p;
		}
		#endregion

	}
}
