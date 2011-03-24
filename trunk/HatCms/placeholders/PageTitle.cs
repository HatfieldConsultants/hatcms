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


        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }
		

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            // Nothing to render in View Mode. rendering only happens in Edit mode.
            // To view the title of the page, use the _system/PageTitle control.
            // View mode rendering is not done in this placeholder so that the template can precisely control where the
            // Title is edited, and where it is displayed.
        }

        private string getPageNameFromTitle(string newTitle, CmsPage oldPageWithValidName)
        {
            string returnTitle = newTitle;
            // -- remove invalid characters            
            foreach (string invalidChar in HatCMS.Controls.RenamePagePopup.InvalidPageNameChars)
            {
                returnTitle = returnTitle.Replace(invalidChar, " ");
            }

            // -- remove whitespace and multiple spaces. Source: http://www.dotnetperls.com/remove-whitespace
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"\s+");

            returnTitle = r.Replace(returnTitle, @" ");

            returnTitle = returnTitle.Trim();

            if (returnTitle.Length > 0)
            {
                bool pageNameAlreadyExists = CmsContext.childPageWithNameExists(oldPageWithValidName.ParentID, returnTitle);
                if (!pageNameAlreadyExists)
                    return returnTitle;
            }

            // -- there was a problem, so use the old name.
            return oldPageWithValidName.Name;

        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{
            string formName = "editTitle_" + page.ID.ToString() + identifier.ToString() + langToRenderFor.shortCode.ToLower();
			
            // -- get the placeholder width and height parameters
            string width = "100%";
            string height = "1.5em";
            bool renamePageBasedOnTitle = false;
            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                width = PlaceholderUtils.getParameterValue("width", width, paramList);
                height = PlaceholderUtils.getParameterValue("height", height, paramList);
                renamePageBasedOnTitle = PlaceholderUtils.getParameterValue("RenamePageBasedOnTitle", renamePageBasedOnTitle, paramList);
            }
            else
                throw new ArgumentException("Invalid CmsTemplateEngineVersion");            

			            
			
			string pageTitle = "";
			string menuTitle = "";
            string searchEngineDescription = "";
			string Message = "";

			// ------- CHECK THE FORM FOR ACTIONS
            string action = Hatfield.Web.Portal.PageUtils.getFromForm(formName + "_PageTitleAction", "");
			if (action.Trim().ToLower() == "savetitle")
			{                
                // -- save the page title
                pageTitle = PageUtils.getFromForm(formName + "_value", "");
                if (pageTitle.CompareTo(page.Title) != 0 && page.setTitle(pageTitle, langToRenderFor))
				{                    
                    Message = "Page Title Updated";
                    // -- save the name based on the page title
                    if (renamePageBasedOnTitle && page.ID != CmsContext.HomePage.ID)
                    {
                        string newPageName = getPageNameFromTitle(pageTitle, page);
                        // -- rename the page to its new name
                        bool renameSuccess = page.setName(newPageName, langToRenderFor);
                        if (!renameSuccess)                        
                        {
                            Message = "Page did NOT rename successfully.";                         
                        }
                    }
				}
				else
				{
					Message = "Problem with database: could not set page title";
                    pageTitle = page.Title;
				}

                // -- save the menu title

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

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            return new Rss.RssItem[0]; // no RSS items.
        }

		
	}
}
