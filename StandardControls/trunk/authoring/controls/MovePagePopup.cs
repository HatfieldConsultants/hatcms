using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Web;
using Hatfield.Web.Portal;

namespace HatCMS.Controls
{
    public class MovePagePopup : BaseCmsControl
    {

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            return ret.ToArray();
        }

        string _errorMessage = "";

        public override string RenderToString(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
        {
            StringBuilder html = new StringBuilder();
            int PageIdToMove = PageUtils.getFromForm("target", Int32.MinValue);
            if (PageIdToMove < 0)
            {
                html.Append("<span style=\"color: red\">Invalid Target parameter. No page to move.</span>");
                return (html.ToString());                
            }
            else
            {
                if (!CmsContext.pageExists(PageIdToMove))
                {
                    html.Append("<span style=\"color: red\">Target page does not exist. No page to move.</span>");
                    return (html.ToString());                    
                }
                else
                {
                    CmsPage pageToMove = CmsContext.getPageById(PageIdToMove);

                    if (!pageToMove.currentUserCanWrite)
                    {
                        return ("Access Denied");                        
                    }

                    // -- form variable
                    int parent = PageUtils.getFromForm("parent", Int32.MinValue);
                    // -- process the action
                    string action = PageUtils.getFromForm("MovePageAction", "");
                    if (String.Compare(action.Trim(), "MovePage", true) == 0)
                    {

                        if (!CmsContext.pageExists(parent))
                        {
                            _errorMessage = "No parent page specified";
                        }
                        else if (parent == pageToMove.ID)
                        {
                            _errorMessage = "can not move page to the same location!";
                        }
                        else
                        {
                            CmsPage newParentPage = CmsContext.getPageById(parent);

                            if (pageToMove.ID == CmsContext.HomePage.ID)
                            {
                                html.Append("<span style=\"color: red\">Error: you can not move the home page!</span>");
                                return (html.ToString());                                
                            }
                            else
                            {
                                bool success = MovePage(pageToMove, newParentPage);
                                if (success)
                                {
                                    string script = "<script>" + Environment.NewLine;
                                    script = script + "function go(url){" + Environment.NewLine;
                                    script = script + "opener.location.href = url;" + Environment.NewLine;
                                    script = script + "window.close();\n}";
                                    script = script + "</script>" + Environment.NewLine;
                                    script = script + "<span style=\"color: green; font-weight: bold;\">The Page has successfully been moved.</span>";
                                    script = script + "<p><input type=\"button\" onclick=\"go('" + newParentPage.Url + "');\" value=\"close this window\">";

                                    return (script);                                    
                                }
                            }
                        }

                    } // if action

                    String newLine = Environment.NewLine;

                    CmsPage page = CmsContext.currentPage;

                    html.Append("<head>" + newLine);
                    html.Append("<title>Move Page</title>" + newLine);
                    html.Append("<style>" + Environment.NewLine);
                    html.Append("   #fp { width: 150px; }" + Environment.NewLine);
                    html.Append("</style>" + Environment.NewLine);
                    html.Append("</head>" + newLine);
                    html.Append("<body style=\"margin: 0px; padding: 0px);\">");
                    string formId = "movePage";
                    html.Append(page.getFormStartHtml(formId));
                    html.Append("<table width=\"100%\" cellpadding=\"1\" cellspacing=\"2\" border=\"0\">" + newLine);

                    html.Append("<tr>" + newLine);
                    html.Append("	<td colspan=\"2\" bgcolor=\"#ffffd6\"><strong>Move page</strong></td>" + newLine);
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
                    html.Append("	Page to move : </td><td>\"" + pageToMove.Title + "\" <br />(" + pageToMove.Path + ")");
                    html.Append("	</td>");
                    html.Append("</tr>" + newLine);

                    html.Append("<tr>" + newLine);
                    html.Append("	<td>");
                    Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();

                    NameValueCollection targetDropDownVals = new NameValueCollection();

                    foreach (int pageId in allPages.Keys)
                    {
                        CmsPage pageToAdd = allPages[pageId];
                        // -- don't allow moving a page to a child of the page, or to the same location as it is now.
                        bool zoneAuthorized = pageToAdd.Zone.canWrite(CmsContext.currentWebPortalUser);
                        if (zoneAuthorized && !pageToAdd.isChildOf(pageToMove) && pageToAdd.ID != pageToMove.ParentID)
                            targetDropDownVals.Add(pageId.ToString(), pageToAdd.Path);
                    }
                    html.Append("	move page so that it is under : </td><td>" + PageUtils.getDropDownHtml("parent", "fp", targetDropDownVals, parent.ToString()));
                    html.Append("	</td>");
                    html.Append("</tr>" + newLine);

                    html.Append("</table>");

                    html.Append(PageUtils.getHiddenInputHtml("target", pageToMove.ID.ToString()));
                    html.Append(PageUtils.getHiddenInputHtml("MovePageAction", "MovePage"));
                    html.Append("<input type=\"submit\" value=\"move page\"> ");
                    html.Append("<input type=\"button\" value=\"cancel\" onclick=\"window.close()\">");
                    html.Append(page.getFormCloseHtml(formId));
                    if (CmsConfig.Languages.Length > 1)
                    {
                        html.Append("<p><em>Note: All translated versions of the page to move will be moved to the new location as well.</em></p>");
                    }
                    html.Append("</body>");


                } // else page exists
            }

            return (html.ToString());
        }

        private bool MovePage(CmsPage pageToMove, CmsPage newParentPage)
        {

            // -- move the page to its new home
            bool success = pageToMove.setParentPage(newParentPage);
            if (success)
            {
                return true;
            }
            else
            {
                _errorMessage = "Page did NOT move successfully.";
                return false;
            }

        } // MovePage
        
    }
}
