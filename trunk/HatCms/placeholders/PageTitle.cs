using System;
using System.Text;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections;

using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
	/// <summary>
	/// Summary description for PageTitle.
	/// </summary>
	public class PageTitle: BaseCmsPlaceholder
	{
		public PageTitle()
		{
			//
			// nothing to create.
			//
		}

        public override CmsDependency[] getDependencies()
        {            
            return new CmsDependency[0];
        }


        public override RevertToRevisionResult revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }
		

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
             // nothing to render in View Mode. rendering only happens in Edit mode.
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{
            string formName = "editTitle_" + page.ID.ToString() + identifier.ToString() + langToRenderFor.shortCode.ToLower();
			
            // -- get the placeholder width and height parameters
            string width = "100%";
            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v1 && paramList.Length > 0)
                width = paramList[0] as string;
            else if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
                width = PlaceholderUtils.getParameterValue("width", width, paramList);

			string height = "1.5em";
            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v1 && paramList.Length > 1)
				height = paramList[1] as string;
            else if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
                height = PlaceholderUtils.getParameterValue("height", height, paramList);
			
			string pageTitle = "";
			string menuTitle = "";
            string searchEngineDescription = "";
			string Message = "";

			// ------- CHECK THE FORM FOR ACTIONS
            string action = Hatfield.Web.Portal.PageUtils.getFromForm(formName + "_PageTitleAction", "");
			if (action.Trim().ToLower() == "savetitle")
			{
                pageTitle = PageUtils.getFromForm(formName + "_value", "");
                if (page.setTitle(pageTitle, langToRenderFor))
				{
					Message = "Page Title Updated";
				}
				else
				{
					Message = "Problem with database: could not set page title";
                    pageTitle = page.Title;
				}

                menuTitle = PageUtils.getFromForm(formName + "_menutitlevalue", "");
                if (page.setMenuTitle(menuTitle, langToRenderFor))
				{
                    Message = "Navigation Menu Text Updated";
				}
				else
				{
					Message = "Problem with database: could not set page's Menu title";
                    menuTitle = page.getMenuTitle(langToRenderFor);
				}

                searchEngineDescription = PageUtils.getFromForm(formName + "_searchEngineDescriptionvalue", "");
                searchEngineDescription = StringUtils.StripHTMLTags(searchEngineDescription);
                if (page.setSearchEngineDescription(searchEngineDescription, langToRenderFor))
                {
                    Message = "Page Search Engine Description Updated";
                }
                else
                {
                    Message = "Problem with database: could not set page's search engine description";
                    searchEngineDescription = page.SearchEngineDescription;
                }

			}
			else
			{
				pageTitle = page.getTitle(langToRenderFor);
				menuTitle = page.getMenuTitle(langToRenderFor);
                searchEngineDescription = page.getSearchEngineDescription(langToRenderFor);
			}

			// ------- START RENDERING
			// -- get the Javascript
			StringBuilder html = new StringBuilder();
            string EOL = Environment.NewLine;
			// -- render the Control			
			// note: no need to put in the <form></form> tags.
            html.Append("<div class=\"PageTitlePlaceholder\">");
			html.Append("<div class=\"PageTitleEdit\" style=\"background: #CCC; padding: 0.2em;\">");
            html.Append("Page Title: <input style=\"font-size: " + height + "; font-weight: bold; width: " + width + "; height:" + height + " \" type=\"text\" name=\"" + formName + "_value\" value=\"" + pageTitle + "\"><br>");
            html.Append("</div>" + Environment.NewLine);

            html.Append("<div class=\"PageMenuTextEdit\" style=\"background: #CCC; padding: 0.2em;\">");
            html.Append("Navigation Menu Text: <input style=\"font-size: " + height + "; font-weight: bold; width: " + width + "; height:" + height + " \" type=\"text\" name=\"" + formName + "_menutitlevalue\" value=\"" + menuTitle + "\"><br>");
            html.Append("</div>" + Environment.NewLine);

            // -- the area that allows the search engine description to be edited can be shown or hidden
            string divId = formName + "seDescDiv";
            string linkId = formName + "seDescShowHideLink";
            string showHideFuncName = formName + "toggle";

            StringBuilder js = new StringBuilder();            
            js.Append("function " + showHideFuncName + "() {" + EOL);
            js.Append("var divEl = document.getElementById('" + divId + "');" + EOL);
            js.Append("var linkEl = document.getElementById('" + linkId + "');" + EOL);
            js.Append("var showing = (divEl.style.display == 'block');" + EOL);
            js.Append("if (showing) { " + EOL);
            js.Append("   divEl.style.display = 'none';" + EOL);
            js.Append("   linkEl.innerHTML = '(edit)';" + EOL);
            js.Append("} else { " + EOL);
            js.Append("   divEl.style.display = 'block';");
            js.Append("   linkEl.innerHTML = '(hide)';" + EOL);
            js.Append("} // if showing" + EOL);
            js.Append("}" + EOL);

            page.HeadSection.AddJSStatements(js.ToString());

            html.Append("<div style=\"background: #CCC; padding: 0.2em;\">");
            html.Append("Search Engine Description:");            
            string onclick = showHideFuncName + "(); return false; ";
            html.Append(" <a id=\"" + linkId + "\" href=\"#\" onclick=\"" + onclick + "\">(edit)</a>" + Environment.NewLine);

            html.Append("<div id=\"" + divId + "\" style=\"display:none;\">"+EOL);            
            html.Append("<textarea name=\"" + formName + "_searchEngineDescriptionvalue\" id=\"" + formName + "_searchEngineDescriptionvalue\" style=\"width: " + width + ";\" rows=\"3\">");
            html.Append(searchEngineDescription);
            html.Append("</textarea>");
            html.Append("</div>" + Environment.NewLine);
            html.Append("</div>" + Environment.NewLine);


            html.Append("<input type=\"hidden\" name=\"" + formName + "_PageTitleAction\" value=\"saveTitle\">");
            html.Append("</div>" + Environment.NewLine);
			
			
			writer.WriteLine(html.ToString());

		}
		
	}
}
