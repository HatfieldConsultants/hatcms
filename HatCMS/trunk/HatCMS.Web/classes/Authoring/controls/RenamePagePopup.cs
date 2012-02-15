using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using Hatfield.Web.Portal;

namespace HatCMS.Controls
{
    public class RenamePagePopup : BaseCmsControl
    {

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            return ret.ToArray();
        }

        string _errorMessage = "";

        public override string RenderToString(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
        {
            if (!CmsContext.currentUserIsLoggedIn)
            {
                return("Access Denied");                
            }

            StringBuilder html = new StringBuilder();
            int PageIdToRename = PageUtils.getFromForm("target", Int32.MinValue);
            if (PageIdToRename < 0)
            {
                html.Append("<span style=\"color: red\">Invalid Target parameter. No page to rename.</span>");
                return(html.ToString());
                
            }
            else
            {
                if (!CmsContext.pageExists(PageIdToRename))
                {
                    html.Append("<span style=\"color: red\">Target page does not exist. No page to rename.</span>");
                    return(html.ToString());                    
                }
                else
                {
                    CmsPage pageToRename = CmsContext.getPageById(PageIdToRename);
                    if (!pageToRename.currentUserCanWrite)
                    {
                        html.Append("<span style=\"color: red\">Access denied. You do not have write-access to this page.</span>");
                        return(html.ToString());                        
                    }
                    if (pageToRename.Id == CmsContext.HomePage.Id)
                    {
                        html.Append("<span style=\"color: red\">Error: you can not rename the home page!</span>");
                        return (html.ToString());                        
                    }


                    // -- process the action
                    string action = PageUtils.getFromForm("RenamePageAction", "");
                    if (String.Compare(action.Trim(), "RenamePage", true) == 0)
                    {
                        string newPageName = PageUtils.getFromForm("newName", pageToRename.Name);
                        newPageName = newPageName.Trim().ToLower();
                        if (newPageName == "")
                        {
                            _errorMessage = "Please specify a new name for the page";
                        }
                        else
                        {
                            // -- check for invalid characters.
                            foreach (string invalidChar in CmsPage.InvalidPageNameChars)
                            {
                                if (newPageName.IndexOf(invalidChar) >= 0)
                                    _errorMessage = "The page name can not include the \"" + invalidChar + "\" character.";
                            }

                            if (_errorMessage == "")
                            {
                                bool nameAlreadyExists = CmsContext.childPageWithNameExists(pageToRename.ParentID, newPageName);
                                if (nameAlreadyExists)
                                {
                                    _errorMessage = "A page named \"" + newPageName + "\" already exists.";
                                }
                                else
                                {
                                    bool success = RenamePage(pageToRename, newPageName);
                                    if (success)
                                    {
                                        string script = "<script>" + Environment.NewLine;
                                        script = script + "function go(url){" + Environment.NewLine;
                                        script = script + "opener.location.href = url;" + Environment.NewLine;
                                        script = script + "window.close();\n}";
                                        script = script + "</script>" + Environment.NewLine;
                                        script = script + "<span style=\"color: green; font-weight: bold;\">The Page has successfully been renamed.</span>";
                                        script = script + "<p><input type=\"button\" onclick=\"go('" + pageToRename.Url + "');\" value=\"close this window\">";
                                        // script = script + "<p>" + htmlOutput + "</p>";
                                        return (script);                                        
                                    }
                                }
                            }
                        } // else


                    } // if action

                    String newLine = Environment.NewLine;

                    CmsPage page = CmsContext.currentPage;

                    html.Append("<head>" + newLine);
                    html.Append("<title>Rename Page</title>" + newLine);
                    html.Append("</head>" + newLine);
                    html.Append("<body style=\"margin: 0px; padding: 0px);\">");
                    string formId = "RenamePage";
                    html.Append(page.getFormStartHtml(formId));
                    html.Append("<table width=\"100%\" cellpadding=\"1\" cellspacing=\"2\" border=\"0\">" + newLine);

                    html.Append("<tr>" + newLine);
                    html.Append("	<td colspan=\"2\" bgcolor=\"#ffffd6\"><strong>Rename page</strong></td>" + newLine);
                    html.Append("</tr>" + newLine);
                    if (_errorMessage != "")
                    {
                        html.Append("<tr>" + newLine);
                        html.Append("	<td colspan=\"2\">");
                        html.Append("<span style=\"color: red;\">" + _errorMessage + "</span>");
                        html.Append("	</td>");
                        html.Append("</tr>" + newLine);
                    }

                    html.Append("<tr>" + newLine);
                    html.Append("	<td>");
                    html.Append("	Page to rename : </td><td>\"" + pageToRename.Title + "\" <br />(" + pageToRename.Path + ")");
                    html.Append("	</td>");
                    html.Append("</tr>" + newLine);

                    if (CmsConfig.Languages.Length > 1)
                    {
                        html.Append("<tr>" + newLine);
                        html.Append("	<td>");
                        html.Append("	For language : </td><td>" + CmsContext.currentLanguage.shortCode);
                        html.Append("	</td>");
                        html.Append("</tr>" + newLine);
                    }

                    html.Append("<tr>" + newLine);
                    html.Append("	<td colspan=\"2\">New name:");
                    html.Append(PageUtils.getInputTextHtml("newName", "newName", pageToRename.Name, 50, 255));
                    html.Append("	</td>");
                    html.Append("</tr>" + newLine);
                    html.Append("</table>");

                    html.Append(PageUtils.getHiddenInputHtml("target", PageIdToRename.ToString()));
                    html.Append(PageUtils.getHiddenInputHtml("RenamePageAction", "RenamePage"));
                    html.Append("<input type=\"submit\" value=\"rename page\"> ");
                    html.Append("<input type=\"button\" value=\"cancel\" onclick=\"window.close()\">");
                    html.Append(page.getFormCloseHtml(formId));
                    html.Append("</body>");


                } // else page exists
            }

            return (html.ToString());
        }

        private bool RenamePage(CmsPage pageToRename, string newPageName)
        {
            if (String.Compare(pageToRename.Name, newPageName, true) == 0)
            {
                _errorMessage = "nothing renamed (origional name is the same as specified name).";
                return false;
            }


            // -- rename the page to its new name
            bool success = pageToRename.setName(newPageName, CmsContext.currentLanguage);
            if (success)
            {
                return true;
            }
            else
            {
                _errorMessage = "Page did NOT rename successfully. There was a database error.";
                return false;
            }

        } // RenamePage
        
    }
}
