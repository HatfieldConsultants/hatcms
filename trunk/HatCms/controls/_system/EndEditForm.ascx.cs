namespace HatCMS.controls
{
	using System;
    using System.Text;
    using System.Collections.Generic;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Collections.Specialized;

    using Hatfield.Web.Portal;

	/// <summary>
	///		Summary description for EndEditForm.
	/// </summary>
	public partial class EndEditForm : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{            
            // -- render based on the current edit mode
            if (CmsContext.currentEditMode == CmsEditMode.Edit)
            {
                if (PageUtils.getFromForm("EndEditForm", "") == "submit")
                {                    

                    NameValueCollection paramList = new NameValueCollection();
                    string appendToTargetUrl = PageUtils.getFromForm("appendToTargetUrl", "");
                    if (appendToTargetUrl != "")
                    {
                        string[] p1 = appendToTargetUrl.Split(new char[] { '|', ',' });
                        foreach (string s in p1)
                        {
                            string[] p2 = s.Split(new char[] { '=' });
                            if (p2.Length == 2)
                                paramList.Add(p2[0], p2[1]);
                        }

                    }

                    CmsContext.setEditModeAndRedirect(CmsEditMode.View, CmsContext.currentPage, paramList);
                    // -- setEditModeAndRedirect ends response
                } // if submit

                StringBuilder html = new StringBuilder();
                html.Append(PageUtils.getHiddenInputHtml("EndEditForm", "submit"));
                html.Append(PageUtils.getHiddenInputHtml(CmsContext.EditModeFormName, "1")); // track the edit mode
                

                html.Append(CmsContext.currentPage.getFormCloseHtml(StartEditForm.FormId));
                writer.WriteLine(html.ToString());
            } // if in edit mode
            else if (CmsContext.currentEditMode == CmsEditMode.View && CmsContext.currentUserCanAuthor)
            {
                // -- we are in view mode, so let's kill all edit mode cookies and release all page locks
                List<HttpCookie> respCookiesToSet = new List<HttpCookie>();
                for (int i = 0; i < Request.Cookies.Count; i++)
                {
                    HttpCookie c = Request.Cookies[i];
                    if (c.Name.StartsWith("currentEditMode_", StringComparison.CurrentCultureIgnoreCase) && c.Value == "1")
                    {
                        // value=="1" for EditMode
                                                
                        string pagePath = c.Name.Substring("currentEditMode_".Length);
                        try
                        {
                            if (CmsContext.pageExists(pagePath))
                            {
                                // -- expire the cookie
                                c.Expires = DateTime.Now.AddDays(-10);
                                c.Value = "2";
                                respCookiesToSet.Add(c);
                                // -- kill the page lock (if it exists)
                                CmsPage p = CmsContext.getPageByPath(pagePath);
                                if (p.ID >= 0)
                                {
                                    p.clearCurrentPageLock();
                                } // if
                            }
                        }
                        catch
                        { }
                    }
                        
                } // foreach Request.Cookie
                
                // -- set cookies outside of the loop so that it doens't go on infinitely
                foreach (HttpCookie c in respCookiesToSet)
                    Response.Cookies.Add(c);
            } // else
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
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
