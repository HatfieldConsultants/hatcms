using System;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace HatCMS.Placeholders
{
	/// <summary>
    /// Summary description for PageRedirect.
	/// </summary>
	public class PageRedirect: BaseCmsPlaceholder
	{
		public PageRedirect()
		{
			//
			// TODO: Add constructor logic here
			//
		}


        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `pageredirect` (
                  `PageRedirectId` int(11) NOT NULL AUTO_INCREMENT,
                  `PageId` int(11) NOT NULL DEFAULT '0',
                  `Identifier` int(11) NOT NULL DEFAULT '0',
                  `langShortCode` varchar(6) NOT NULL,
                  `url` varchar(1024) NOT NULL,
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`PageRedirectId`),
                  KEY `PageId` (`PageId`,`Identifier`),
                  KEY `pageredirect_secondary` (`PageId`,`Identifier`,`Deleted`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
                "));
            ret.Add(new CmsConfigItemDependency("RedirectPlaceholder_autoRedirectAfterSeconds"));
            return ret.ToArray();
        }        

        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }				

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            if (page.currentUserCanWrite)
            {
                RenderViewStatus(writer, page, identifier, langToRenderFor, paramList);
            }
            else
            {
                RenderView(writer, page, identifier, langToRenderFor, paramList);
            }
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{						
			PageRedirectDb db = new PageRedirectDb();

            string formName = "PageRedirect_" + page.Id.ToString() + identifier.ToString() + langToRenderFor.shortCode;
            string width = "100%";
            string height = "1.5em";
            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                width = PlaceholderUtils.getParameterValue("width", width, paramList);
                height = PlaceholderUtils.getParameterValue("height", height, paramList);
            }
            else
                throw new NotImplementedException("Error: invalid TemplateEngine version");

			
			string pageRedirectUrl = db.getPageRedirectUrl(page,identifier, langToRenderFor.shortCode, true);
			// string Message = "";

			// ------- CHECK THE FORM FOR ACTIONS
			string action = Hatfield.Web.Portal.PageUtils.getFromForm(formName+"_Action","");
			if (action.Trim().ToLower() == "saveUrl".ToLower())
			{
				pageRedirectUrl = Hatfield.Web.Portal.PageUtils.getFromForm(formName+"_value","");				
				if (! db.saveUpdatedPageRedirect(page,identifier, langToRenderFor.shortCode, pageRedirectUrl))
				{
					throw new Exception("Problem with database: could not set redirect url");
				}

			}

			// ------- START RENDERING
			// -- get the Javascript
			string html = "";
			// -- render the Control			
			// note: no need to put in the <form></form> tags.

			string id = formName+"_id";
			html = html + "<div style=\"background: #CCC; padding: 0.2em;\">";
			html = html + "URL to redirect to: <input style=\"font-size: "+height+"; font-weight: bold; width: "+width+"; height:"+height+" \" type=\"text\" id=\""+id+"\" name=\""+formName+"_value\" value=\""+pageRedirectUrl+"\">";
			//html = html + "<br><a href=\"#\" onclick=\"var url = document.getElementById('"+id+"').value; window.open(url,'"+id+"_window');\">test link</a>";
            html = html + "<br>hint: use a '~' for a local link (eg: '~/' links to the home page, '~" + page.ParentPage.Path + "' links to the " + page.ParentPage.Title + " page )";
			html = html + "<input type=\"hidden\" name=\""+formName+"_Action\" value=\"saveUrl\">";
			html = html + "</div>";			
			
			writer.WriteLine(html);

		} // RenderEdit

		private void RenderView(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{
			PageRedirectDb db = new PageRedirectDb();
			string url = db.getPageRedirectUrl(page,identifier, langToRenderFor.shortCode, true);

            url = resolveRedirectUrl(url);

            throw new CmsPlaceholderNeedsRedirectionException(url);
			
		} // RenderView

		public static string resolveRedirectUrl(string rawUrl)
		{
			if (rawUrl.StartsWith("~"))
			{
				string path = rawUrl.Substring(1); // remove the starting ~
				string url = CmsContext.getUrlByPagePath(path);
				return url;
			}

			return rawUrl;

		} // resolveUrl

        private void RenderViewStatus(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            PageRedirectDb db = new PageRedirectDb();
            string url = db.getPageRedirectUrl(page, identifier, langToRenderFor.shortCode, true);
            string resolvedUrl = resolveRedirectUrl(url);

            string width = "100%";
            string height = "1.5em";
            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                width = PlaceholderUtils.getParameterValue("width", width, paramList);
                height = PlaceholderUtils.getParameterValue("height", height, paramList);
            }
            else
                throw new NotImplementedException("Error: invalid TemplateEngine version");

            System.Text.StringBuilder html = new System.Text.StringBuilder();
            html.Append("<div style=\"background: #CCC; padding: 0.2em;\">");
            html.Append("Normal site viewers (people who are not site authors) will be automatically redirected to :");
            html.Append("<a href=\"" + resolvedUrl + "\" style=\"font-size: " + height + ";\">");
            html.Append(url);
            html.Append("</a>");            

            if (resolvedUrl != "")
            {
                StringBuilder js = new StringBuilder();

                js.Append("var start=new Date();" + Environment.NewLine);
                js.Append("start=Date.parse(start)/1000;" + Environment.NewLine);
                js.Append("var counts=10;" + Environment.NewLine);

                js.Append("function CountDownToRedirect(){" + Environment.NewLine);
                js.Append("	var now=new Date();" + Environment.NewLine);
                js.Append("	now=Date.parse(now)/1000;" + Environment.NewLine);
                js.Append("	var x=parseInt(counts-(now-start),10);" + Environment.NewLine);
                js.Append("	document.getElementById('redirectCountDown').value = x;" + Environment.NewLine);
                js.Append("	if(x>0){" + Environment.NewLine);
                js.Append("		timerID=setTimeout(\"CountDownToRedirect()\", 100)" + Environment.NewLine);
                js.Append("	}else{" + Environment.NewLine);
                js.Append("		location.href=\"" + resolvedUrl + "\"" + Environment.NewLine);
                js.Append("	}" + Environment.NewLine);
                js.Append("} // CountDownToRedirect" + Environment.NewLine);

                page.HeadSection.AddJSStatements(js.ToString());
                page.HeadSection.AddJSOnReady("CountDownToRedirect();");

                page.HeadSection.registerBlockForOutput("RedirectTimer");

                int numSecondsToWait = CmsConfig.getConfigValue("RedirectPlaceholder_autoRedirectAfterSeconds", 10); ;

                html.Append("<p>You are being redirected in <input type=\"text\" name=\"clock\" id=\"redirectCountDown\" size=\"2\" value=\"" + numSecondsToWait.ToString() + "\"> seconds.</p>" + Environment.NewLine);

            }

            html.Append("</div>");

            writer.WriteLine(html);

        } // RenderViewStatus

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            return new Rss.RssItem[0]; // no rss feed items to return.
        }

	}
}
