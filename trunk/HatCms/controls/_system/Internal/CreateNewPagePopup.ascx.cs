namespace HatCMS.controls
{
	using System;
    using System.Collections.Generic;
	using System.Text;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Collections.Specialized;

    using Hatfield.Web.Portal;

	/// <summary>
	///		Summary description for CreateNewPagePopup.
	/// </summary>
	public partial class CreateNewPagePopup : System.Web.UI.UserControl
	{

		protected string _errorMessage = "";
		protected void Page_Load(object sender, System.EventArgs e)
		{

		}

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("GotoEditModePath", "/_admin/action/gotoEdit"), CmsConfig.Languages));
            return ret.ToArray();
        }


		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{

            if (!CmsContext.currentUserCanAuthor)
            {
                writer.WriteLine("Access Denied");
                return;
            }

            string action = PageUtils.getFromForm("CreateNewPageAction", "");

            CmsCreateNewPageOptions options = CmsCreateNewPageOptions.ReadFromQueryString();
            if (!options.RequiresUserInput() && (String.Compare(action, "createnew", true) != 0))
            {
                CmsPage pageToCreate = options.ToCmsPageObject();
                if (CmsContext.childPageWithNameExists(pageToCreate.ParentID, pageToCreate.LanguageInfo))
                {
                    _errorMessage = "a page with the specified filename already exists!";
                }
                else
                {
                    // -- page does not already exist, so create it
                    CmsPageDb db = new CmsPageDb();
                    bool success = db.createNewPage(pageToCreate);
                    if (!success)
                    {
                        _errorMessage = "database error: could not create new page.";
                    }
                    else
                    {
                        CmsContext.setEditModeAndRedirect(CmsEditMode.Edit, pageToCreate);
                    }
                }
            }

            
			
			// -- get the form variables
            string name = PageUtils.getFromForm("_name", ""); name = name.Trim();
            string title = PageUtils.getFromForm("_title", ""); title = title.Trim();
            string menuTitle = PageUtils.getFromForm("_menuTitle", ""); menuTitle = menuTitle.Trim();
			bool showInMenu = PageUtils.getFromForm("_showInMenu",true);
			string template = PageUtils.getFromForm("_template","");
			string parent = PageUtils.getFromForm("target","");
			
            // -- process the action			
			if (String.Compare(action,"createnew", true) == 0)
			{
				
				if ((!options.PromptUserForFilename || isNotEmpty(name, "Please enter in the page's filename")) &&
					(!options.PromptUserForTemplate || isNotEmpty(template, "Please enter in the page's template")) &&
					(!options.PromptUserForParentPage || isNotEmpty(parent, "Please enter in the page's parent")) &&
                    (!options.PromptUserForTitle || isNotEmpty(title, "Please enter in the page's title")) &&
                    (!options.PromptUserForMenuTitle || isNotEmpty(menuTitle, "Please enter in the page's navigation menu text")) &&
                    doesNotContain(name, @"\", "the filename can not contain a \"\\\" character.") &&
                    doesNotContain(name,"/", "the filename can not contain a \"/\" character.") &&
                    doesNotContain(name, "#", "the filename can not contain a \"#\" character.") &&
                    doesNotContain(name, "+", "the filename can not contain a \"+\" character.") &&
                    doesNotContain(name, ":", "the filename can not contain a \":\" character.") &&
                    doesNotStartWithUnderscoreForNonSuperAdmin(name, "the filename can not start with an \"_\" character.")
                    ) // note when adding new restrictions for the filename, add them also to the RenamePagePopup control!
				{					
					int parentId = Convert.ToInt32(parent);
					CmsPage newPage = new CmsPage();

                    // -- setup the page's language info
                    /*
                     newPage.Name = name;
					 newPage.MenuTitle = menuTitle;
                     newPage.Title = title;
                     */
                    List<CmsPageLanguageInfo> langInfos= new List<CmsPageLanguageInfo>();
                    foreach (CmsLanguage lang in CmsConfig.Languages)
                    {
                        CmsPageLanguageInfo langInfo = new CmsPageLanguageInfo();
                        langInfo.languageShortCode = lang.shortCode;
                        langInfo.name = name;
                        langInfo.menuTitle = menuTitle;
                        langInfo.title = title;
                        langInfo.searchEngineDescription = "";

                        langInfos.Add(langInfo);
                    } // foreach languages
                    newPage.LanguageInfo = langInfos.ToArray();

                    newPage.ShowInMenu = showInMenu;
					newPage.ParentID = parentId;					
					newPage.TemplateName = template;
					newPage.ShowInMenu = showInMenu;

                    // -- set sortOrdinal
                    CmsPage parentPage = CmsContext.getPageById(parentId);
                    int highestSiblingSortOrdinal = -1;
                    foreach (CmsPage sibling in parentPage.ChildPages)
                    {
                        highestSiblingSortOrdinal = Math.Max(sibling.SortOrdinal, highestSiblingSortOrdinal);
                    }
                    if (highestSiblingSortOrdinal > -1)
                        newPage.SortOrdinal = highestSiblingSortOrdinal + 1;
                    else
                        newPage.SortOrdinal = 0;

                    if (CmsContext.childPageWithNameExists(parentId, name))
					{
						_errorMessage = "a page with the specified filename and parent already exists!";
					}
					else
					{
						// -- page does not already exist, so create it
						CmsPageDb db = new CmsPageDb();
						bool success = db.createNewPage(newPage);
						if (!success)
						{
							_errorMessage = "database could not create new page.";
						}
						else
						{
							// -- success: redirect main page to the new page, and close this window.
							StringBuilder script = new StringBuilder();
							script.Append("<script>"+Environment.NewLine);
							script.Append("function go(url){"+Environment.NewLine);
							script.Append("opener.location.href = url;"+Environment.NewLine);
							script.Append("window.close();}"+Environment.NewLine);
							script.Append("</script>"+Environment.NewLine);
							
							// -- Switch to Edit Mode
							NameValueCollection paramList = new NameValueCollection();
							paramList.Add("target",newPage.ID.ToString());
                            string toggleEditUrl = CmsContext.getUrlByPagePath(CmsConfig.getConfigValue("GotoEditModePath","/_admin/action/gotoEdit"), paramList);							

							script.Append("<center>");
							script.Append("<b>Your new page has been created.</b><p>");
                            script.Append("<a href=\"#\" onclick=\"go('" + newPage.Url + "')\">");
							script.Append("take me to this new page");
							script.Append("</a><p>");

                            script.Append("<a href=\"#\" onclick=\"go('" + toggleEditUrl + "')\">");
							script.Append("edit this new page");
							script.Append("</a><p>");

							script.Append("<a href=\"#\" onclick=\"opener.location.reload(); window.close()\">");
							script.Append("close this window");
							script.Append("</a><br>");
							script.Append("</center>");
							

							writer.WriteLine(script.ToString());
							return;
						}
					}
				}
				
			} // if action is set
			
			// -- Render the page
			CmsPage page = CmsContext.currentPage;			

			StringBuilder html = new StringBuilder();
            string newLine = Environment.NewLine;
            html.Append("<head>" + newLine);
            html.Append("<title>Create a New Page</title>" + newLine);
            html.Append("<style>" + Environment.NewLine);
            html.Append("   #fp { width: 150px; }" + Environment.NewLine);
            html.Append("</style>" + Environment.NewLine);
            html.Append("</head>" + newLine);
			html.Append( "<body style=\"margin: 0px; padding: 0px);\">");
            string formId = "createPage";
            html.Append(page.getFormStartHtml(formId));
            html.Append("<table width=\"100%\" cellpadding=\"1\" cellspacing=\"2\" border=\"0\">" + newLine);

            html.Append("<tr>" + newLine);
            html.Append("	<td colspan=\"2\" bgcolor=\"#ffffd6\"><strong>Create a new page</strong></td>" + newLine);
            html.Append("</tr>" + newLine);
			if (_errorMessage != "")
			{
                html.Append("<tr>" + newLine);
				html.Append( "	<td colspan=\"2\">");
				html.Append( "<span style=\"color: red;\">"+_errorMessage+"</span>");
				html.Append( "	</td>");
                html.Append("</tr>" + newLine);				
			}

            if (options.PromptUserForFilename)
            {

                html.Append("<tr>" + newLine);
                html.Append("	<td>");
                html.Append("	Filename: </td><td>" + PageUtils.getInputTextHtml("_name", "fn", name, 20, 255));
                html.Append("	</td>");
                html.Append("</tr>" + newLine);
            }
            else
            {
                html.Append(PageUtils.getHiddenInputHtml("_name", options.NewPageLanguageInfos[0].name));
            }

            if (options.PromptUserForTitle)
            {
                html.Append("<tr>" + newLine);
                html.Append("	<td>");
                html.Append("	Title: </td><td>" + PageUtils.getInputTextHtml("_title", "ft", title, 20, 255));
                html.Append("	</td>");
                html.Append("</tr>" + newLine);
            }
            else
            {
                html.Append(PageUtils.getHiddenInputHtml("_title", options.NewPageLanguageInfos[0].title));
            }

            if (options.PromptUserForMenuTitle)
            {
                html.Append("<tr>" + newLine);
                html.Append("	<td>");
                html.Append("	Navigation Menu Text: </td><td>" + PageUtils.getInputTextHtml("_menutitle", "mt", title, 20, 255));
                html.Append("	</td>");
                html.Append("</tr>" + newLine);
            }
            else
            {
                html.Append(PageUtils.getHiddenInputHtml("_menutitle", options.NewPageLanguageInfos[0].menuTitle));
            }

            if (options.PromptUserForShowInMenu)
            {
                html.Append("<tr>");
                html.Append("	<td>");
                NameValueCollection ynOptions = new NameValueCollection();
                ynOptions.Add("1", "Yes");
                ynOptions.Add("0", "No");
                html.Append("	Show In Menu: </td><td>" + PageUtils.getRadioListHtml("_showInMenu", "mt", ynOptions, Convert.ToInt32(showInMenu).ToString(), "", " "));
                html.Append("	</td>");
                html.Append("</tr>");
            }
            else
            {
                html.Append(PageUtils.getHiddenInputHtml("_showInMenu", Convert.ToInt32(options.ShowInMenu))); 
            }

            if (options.PromptUserForTemplate)
            {
                string[] templates = CmsContext.getTemplateNamesForCurrentUser();
                html.Append("<tr>" + newLine);
                html.Append("	<td>");
                html.Append("	Template: </td><td>" + PageUtils.getDropDownHtml("_template", "ft", templates, template));
                html.Append("	</td>");
                html.Append("</tr>" + newLine);
            }
            else
            {
                html.Append(PageUtils.getHiddenInputHtml("_template", options.Template)); 
            }

            if (options.PromptUserForParentPage)
            {
                html.Append("<tr>" + newLine);
                html.Append("	<td>");
                html.Append("	Parent Page: </td><td>" + PageUtils.getDropDownHtml("target", "fp", getParentPageOptions(CmsContext.HomePage), parent));
                html.Append("	</td>");
                html.Append("</tr>" + newLine);
            }
            else
            {
                html.Append("<tr>" + newLine);
                html.Append("	<td>");
                html.Append("	Parent Page: </td><td>" +CmsContext.getPageById(Convert.ToInt32(parent)).Path);
                html.Append(PageUtils.getHiddenInputHtml("target", "fp", parent ));
                html.Append("	</td>");
                html.Append("</tr>" + newLine);
            }
			html.Append( "</table>");
            html.Append("<p><em>All fields are required</em></p>");

            NameValueCollection optionParams = options.GetCreatePagePopupParams();
            foreach (string key in optionParams.AllKeys)
            {
                html.Append(PageUtils.getHiddenInputHtml(key, "option_" + key, optionParams[key]));
            }

			
			html.Append( PageUtils.getHiddenInputHtml("CreateNewPageAction","createNew") );
			html.Append( "<input type=\"submit\" value=\"create new page\">");
			html.Append( "<input type=\"button\" value=\"cancel\" onclick=\"window.close()\">");
			html.Append( page.getFormCloseHtml(formId));
			html.Append( "</body>");
			
			writer.WriteLine(html.ToString());
		} // Render

		
		private NameValueCollection getParentPageOptions(CmsPage page)
		{
			if (page == null || page.ID == -1)
				return new NameValueCollection();

			NameValueCollection nvc = new NameValueCollection();
			if (page.isVisibleForCurrentUser)
			{
				nvc.Add(page.ID.ToString(), page.Path);
				foreach(CmsPage subPage in page.ChildPages)
				{
					NameValueCollection ret = getParentPageOptions(subPage);
					foreach(string key in ret.Keys)
					{					
						nvc.Add(key, ret[key]);
					}				
				} // foreach
			}							
			return nvc;			
		}

        private bool doesNotStartWithUnderscoreForNonSuperAdmin(string valToSearch, string onContainsMessage)
        {
            if (valToSearch.StartsWith("_") && ! CmsContext.currentUserIsSuperAdmin)
            {
                _errorMessage = onContainsMessage;
                return false;
            }
            return true;
        }

        private bool doesNotContain(string valToSearch, string toFind, string onContainsMessage)
        {
            if (valToSearch.IndexOf(toFind, StringComparison.CurrentCultureIgnoreCase) > -1)
            {
                _errorMessage = onContainsMessage;
                return false;
            }
            return true;
        }

		private bool isNotEmpty(string val, string onEmptyMessage)
		{
			if (val.Trim() != "")
			{
				
				return true;
			}
			_errorMessage = onEmptyMessage;
			return false;
		}



		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			// Response.Write("onInit!!!");
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}
		#endregion
	}
}

