using System;
using System.Data;
using System.Collections;

namespace HatCMS.Placeholders
{
		
	public class PageNotFoundRedirectInfo
	{
		public int PageNotFoundRedirectId = Int32.MinValue;
		public string requestedUrl = "";
		public int redirectToPageId = Int32.MinValue;

		public CmsPage getRedirectToPageFromPageId()
		{
			return CmsContext.getPageById(redirectToPageId);
		}
	}
	
	
	/// <summary>
	/// Summary description for PageNotFoundRedirectDb.
	/// </summary>
	public class PageNotFoundRedirectDb: PlaceholderDb
	{		

		/// <summary>
		/// returns the PageId, or Int32.MinValue if not found
		/// </summary>
		/// <param name="referer">a cleaned up version of the referer that does not include
		/// the hostname, and does not include the ApplicationPath
		/// </param>
		/// <returns>the PageId, or Int32.MinValue if not found</returns>
		public int getPageIdToRedirectTo(string referer)
		{
			string r = referer.Trim().ToLower();
			if (r != "")
			{
				string sql = "select redirectToPageId from pagenotfoundredirect where Deleted is null and requestedUrl like '"+dbEncode(r)+"' LIMIT 1;";
				DataSet ds = this.RunSelectQuery(sql);
				if (this.hasSingleRow(ds))
				{
					return Convert.ToInt32(ds.Tables[0].Rows[0]["redirectToPageId"]);
				}
			}
			return Int32.MinValue;
		} // getPageIdToRedirectTo

        public PageNotFoundRedirectInfo[] getAllRedirectInfos()
        {
            string sql = "select * from pagenotfoundredirect where Deleted is null;";
            DataSet ds = this.RunSelectQuery(sql);
            ArrayList ret = new ArrayList();
            if (this.hasRows(ds))
            {
                foreach(DataRow dr in ds.Tables[0].Rows)
                {
                    PageNotFoundRedirectInfo info = new PageNotFoundRedirectInfo();
                    info.PageNotFoundRedirectId = Convert.ToInt32(dr["PageNotFoundRedirectId"]);
                    info.requestedUrl = dr["requestedUrl"].ToString();
                    info.redirectToPageId = Convert.ToInt32(dr["redirectToPageId"]);
                    ret.Add(info);
                } // foreach
            }
            return (PageNotFoundRedirectInfo[])ret.ToArray(typeof(PageNotFoundRedirectInfo));
        }

	}
}
