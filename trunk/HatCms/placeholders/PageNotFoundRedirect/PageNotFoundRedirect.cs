using System;
using System.Text;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;


namespace HatCMS.Placeholders
{
    
    
    /// <summary>
	/// Summary description for PageNotFoundSuggestion.
	/// </summary>
	public class PageNotFoundRedirect: BaseCmsPlaceholder
	{
		public PageNotFoundRedirect()
		{
			//
			// TODO: Add constructor logic here
			//
		}

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsDatabaseTableDependency("pagenotfoundredirect"));
            return ret.ToArray();
        }


        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }


        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{
			// -- referer is usually filled in by the default.aspx NotFoundException handler
			//    as being the url that was requested, but not found
			string referer = Hatfield.Web.Portal.PageUtils.getFromForm("from","");
			if (referer == "")
			{
				if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_REFERER"] != null)
					referer = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_REFERER"];
			}
            else if (referer.IndexOf("?404;") > -1)
            {
                // "/hatcms/default.aspx?p=/_error404?404;http://localhost/hatcms/images/headers/stream.jpg"
                int index = referer.IndexOf("?404;") + "?404;http://".Length + System.Web.HttpContext.Current.Request.Url.Host.Length;
                referer = referer.Substring(index);

                if (referer.StartsWith(":"+System.Web.HttpContext.Current.Request.Url.Port.ToString()))
                {
                    referer = referer.Substring(":".Length + System.Web.HttpContext.Current.Request.Url.Port.ToString().Length);
                }
            }

			referer = referer.ToLower();
			
			// -- remove ApplicationPath
			if (referer.StartsWith(CmsContext.ApplicationPath.ToLower()))
				referer = referer.Substring(CmsContext.ApplicationPath.Length);


            // -- set the status code to 404 - not found.
            System.Web.HttpContext.Current.Response.StatusCode = 404;

			// -- try to find the referer in the database. If found, go to that page.
			int pageId = (new PageNotFoundRedirectDb()).getPageIdToRedirectTo(referer);
            if (pageId > -1)
            {
                string targetUrl = CmsContext.getPageById(pageId).Url;
                throw new CmsPlaceholderNeedsRedirectionException(targetUrl);
            }
            else
            {
                writer.Write("<!-- referer: \"" + referer + "\" -->");
            }
			// -- do not output anything in RenderView mode. If we are still executing here (pageId <= -1)
			//    just output the rest of the template (which should give more information on how to proceed)
		}

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{						
			
			string formName = "editPageNotFoundSuggestion_"+page.ID.ToString()+identifier.ToString();
            PageNotFoundRedirectDb db = (new PageNotFoundRedirectDb());
            PageNotFoundRedirectInfo[] infos = db.getAllRedirectInfos();

			// ------- CHECK THE FORM FOR ACTIONS
			string action = Hatfield.Web.Portal.PageUtils.getFromForm(formName+"_PageNotFoundAction","");
            StringBuilder html = new StringBuilder();
            if (action.Trim().ToLower() == "saveupdates")
			{				

			}
			else
			{
                html.Append("The edit form has not been created yet!.<p>");
                if (infos.Length < 1)
                {
                    html.Append("<strong>no redirects are currently in the database</strong>");
                }
                else
                {
                    html.Append("These are the redirects currently in the database:<br>");
                    html.Append("<table>");
                    html.Append("<tr><th></th><th>From</th><th>To</th></tr>");
                    foreach (PageNotFoundRedirectInfo info in infos)
                    {
                        html.Append("<tr>");
                        html.Append("<td>");
                        html.Append(info.PageNotFoundRedirectId.ToString());
                        html.Append("</td>");
                        html.Append("<td>");
                        html.Append("<a href=\"" + info.requestedUrl + "\" target=\"_blank\">" + info.requestedUrl + "</a>");
                        html.Append("</td>");
                        html.Append("<td>");
                        CmsPage targetPage = info.getRedirectToPageFromPageId();
                        html.Append("<a href=\"" + targetPage.Url + "\" target=\"_blank\">" + targetPage.Title + " (" + targetPage.Path + ") </a>");
                        html.Append("</td>");
                        html.Append("</tr>");
                    } // foreach
                    html.Append("</table>");
                }
			}

			// ------- START RENDERING
			
			
			// -- render the Control			
			// note: no need to put in the <form></form> tags.


            html.Append("<input type=\"hidden\" name=\"" + formName + "_PageNotFoundAction\" value=\"saveUpdates\">");
			
			
			writer.WriteLine(html.ToString());

		}

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            return new Rss.RssItem[0];
        }

	} // class
}
