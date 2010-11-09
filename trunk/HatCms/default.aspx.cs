using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Text;
using System.Diagnostics;
using Hatfield.Web.Portal;

namespace HatCMS
{
	/// <summary>
    /// The _defaultProcessingPage .ASPX page renders the CmsContext.currentPage.
	/// </summary>
	public partial class _defaultProcessingPage : System.Web.UI.Page
	{        
		protected void Page_Load(object sender, System.EventArgs e)
		{
            // -- throw an exception if something is posted and we aren't in edit mode.
            if (CmsContext.currentEditMode != CmsEditMode.Edit)
                Request.ValidateInput();

            // render nothing in Page_Load. All functions are handled in CreateChildControl
		} // page_load

		protected override void CreateChildControls()
		{
            try
            {                
                string aspxerrorpath = PageUtils.getFromForm("aspxerrorpath", "");
                if (aspxerrorpath.Trim() != "")
                    throw new CmsPageNotFoundException();
                

                CmsContext.StartNewRequest();
                // fires the CmsPage.CreateChildControls function
                this.Controls.Add(CmsContext.currentPage);
                // (do not use CmsContext.currentPage.Render)										                                

            }
            catch (CmsPageNotFoundException ex404)
            {
                Console.Write(ex404.Message);                
                CmsContext.HandleNotFoundException();
            }
            catch (NeedsAuthenticationException ex401)
            {
                Console.Write(ex401.Message);
                CmsContext.setEditModeAndRedirect(CmsEditMode.View, CmsContext.getPageByPath(CmsConfig.getConfigValue("LoginPath","/_admin/login")));
            }
                /*
#if ! DEBUG
            catch (Exception ex)
            {
                int eventId = 1309; // EVENTID_WEBREQUESTERROREVENT
                short CATEGORY_ASPNET_WEBEVENT = 3;
                EventLog.WriteEntry("ASP.NET 2.0.50727.0", ex.ToString(), EventLogEntryType.Error, eventId, CATEGORY_ASPNET_WEBEVENT);

                CmsContext.HandleFatalServerException();
            }
#endif
            */
		} // CreateChildControls


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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
		}
		#endregion
	}
}
