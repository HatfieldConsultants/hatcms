using System;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Text;


namespace HatCMS.Placeholders
{
	#region PageComment data holding classes
	public class PageCommentData
	{
		public int PageCommentId;
		// public int ParentPageId;
		public string CommentText;
		public DateTime postedDate;
		public string PostedBy;

		public PageCommentData()
		{
			PageCommentId = -1;
			CommentText = "";
			postedDate = DateTime.MinValue;
			PostedBy = "";
		}
	} // PageCommentData
	#endregion

	/// <summary>
	/// Summary description for HtmlContent.
	/// </summary>
	public class CommentsPlaceholder: BaseCmsPlaceholder
	{
		public CommentsPlaceholder()
		{
			//
			// TODO: Add constructor logic here
			//
		}

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsDatabaseTableDependency("pagecomments"));
            return ret.ToArray();
        }


        public override bool revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return true; // this placeholder doesn't implement revisions
        }
		

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{									
            // nothing to edit right now.
		}

		private string RenderPageCommentData(PageCommentData data)
		{
			string html = "";
			html += "<div class=\"PageComment\">";
			// -- title
			html += "<div class=\"PageCommentTitle\">";
			html += "Entered on "+data.postedDate.ToString("ddd MMM dd yyyy")+" by "+data.PostedBy;				
			html += "</div>";
			// -- contents
			html += "<div class=\"PageCommentText\">";
			html += data.CommentText;
			html += "</div>";
			html += "</div>";
			return html;
		}

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{
			throw new NotImplementedException("PageComments placeholder is not ready for use!");
			// -- get the values from the database
			PageCommentsDb db = new PageCommentsDb();
			PageCommentData[] comments= db.getPageComments(page, identifier, false);
			// -- output the comments			
			string html = "";				
			html += "<div class=\"PageCommentSection\">";
			if (comments.Length > 0)
			{
				foreach(PageCommentData comment in comments)
				{
					// html += RenderPageCommentData(comment);
				}
				
			}
			else
			{
				// no comments on this page
			}
			html += "</div>";
			writer.WriteLine(html);
		} // RenderView
	}
}
