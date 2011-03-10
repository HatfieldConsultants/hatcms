namespace HatCMS.Controls
{
	using System;
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
    public partial class ChangePageTemplatePopup : System.Web.UI.UserControl
	{

		protected string _errorMessage = "";
		protected void Page_Load(object sender, System.EventArgs e)
		{

		}

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {

            if (!CmsContext.currentUserIsSuperAdmin)
            {
                writer.WriteLine("Access Denied");
                return;
            }

            StringBuilder html = new StringBuilder();
            int PageIdToChange = PageUtils.getFromForm("target", Int32.MinValue);
            if (PageIdToChange < 0)
            {
                html.Append("<span style=\"color: red\">Invalid Target parameter. No page to change.</span>");
                writer.WriteLine(html.ToString());
                return;
            }
            else
            {
                if (!CmsContext.pageExists(PageIdToChange))
                {
                    html.Append("<span style=\"color: red\">Target page does not exist. No page to change.</span>");
                    writer.WriteLine(html.ToString());
                    return;
                }
                else
                {
                    CmsPage pageToChange = CmsContext.getPageById(PageIdToChange);
                    // -- form variable
                    string newTemplateName = PageUtils.getFromForm("template", pageToChange.TemplateName);
                    // -- process the action
                    string action = PageUtils.getFromForm("ChangeTemplatePageAction", "");
                    if (String.Compare(action.Trim(), "ChangeTemplate", true) == 0)
                    {

                        if (newTemplateName.Trim() == "")
                        {
                            _errorMessage = "The new template name was not specified";
                        }
                        else if (string.Compare(newTemplateName, pageToChange.TemplateName, true) == 0)
                        {
                            _errorMessage = "The new template name is the same as the old one!";
                        }
                        else
                        {
                            string htmlOutput = "";
                            bool success = pageToChange.setTemplateName(newTemplateName);
                            if (success)
                            {
                                string script = "<script>"+Environment.NewLine;
                                script = script + "function go(url){"+Environment.NewLine;
                                script = script + "opener.location.href = url;"+Environment.NewLine;
                                script = script + "window.close();\n}";
                                script = script + "</script>"+Environment.NewLine;
                                script = script + "<span style=\"color: green; font-weight: bold;\">The Page template has successfully been changed.</span>";
                                script = script + "<p><input type=\"button\" onclick=\"go('" + pageToChange.Url + "');\" value=\"close this window\">";
                                script = script + "<p>" + htmlOutput + "</p>";
                                writer.WriteLine(script);
                                return;
                            }

                        }

                    } // if action

                    String newLine = Environment.NewLine;

                    CmsPage page = CmsContext.currentPage;

                    html.Append("<head>" + newLine);
                    html.Append("<title>Change Page Template</title>" + newLine);
                    html.Append("<style>" + Environment.NewLine);
                    html.Append("   #fp { width: 150px; }" + Environment.NewLine);
                    html.Append("</style>" + Environment.NewLine);
                    html.Append("</head>" + newLine);
                    html.Append("<body style=\"margin: 0px; padding: 0px);\">");
                    
                    string formId = "ChangePageTemplate";
                    html.Append(page.getFormStartHtml(formId));
                    html.Append("<table width=\"100%\" cellpadding=\"1\" cellspacing=\"2\" border=\"0\">" + newLine);

                    html.Append("<tr>" + newLine);
                    html.Append("	<td colspan=\"2\" bgcolor=\"#ffffd6\"><strong>Change Page Template</strong></td>" + newLine);
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
                    html.Append("	Page to change : </td><td>\"" + pageToChange.Title + "\" <br />(" + pageToChange.Path + ")");
                    html.Append("	</td>");
                    html.Append("</tr>" + newLine);

                    html.Append("<tr>" + newLine);
                    html.Append("	<td>");
                    html.Append("	Current template : </td><td>\"" + pageToChange.TemplateName+ "\"");
                    html.Append("	</td>");
                    html.Append("</tr>" + newLine);

                    string[] templates = CmsContext.getTemplateNamesForCurrentUser();
                    
                    html.Append("<tr>" + newLine);
                    html.Append("	<td>");
                    html.Append("	New Template: </td><td>" + PageUtils.getDropDownHtml("template", "ft", templates, newTemplateName));
                    html.Append("	</td>");
                    html.Append("</tr>" + newLine);

                    html.Append("</table>");

                    html.Append(PageUtils.getHiddenInputHtml("target", PageIdToChange.ToString()));
                    html.Append(PageUtils.getHiddenInputHtml("ChangeTemplatePageAction", "ChangeTemplate"));
                    html.Append("<input type=\"submit\" value=\"change page template\"> ");
                    html.Append("<input type=\"button\" value=\"cancel\" onclick=\"window.close();\">");
                    html.Append(page.getFormCloseHtml(formId));
                    html.Append("</body>");


                } // else page exists
            }

            writer.WriteLine(html.ToString());
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

