using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.UI.WebControls;
using Hatfield.Web.Portal.Data;
using HatCMS.Placeholders.RegisterProject;
using Hatfield.Web.Portal;
using HatCMS.Controls.Admin;
using HatCMS.Placeholders;

namespace HatCMS._system
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://hatfieldgroup.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class download : IHttpHandler
    {
        /// <summary>
        /// Read the adminTool option from URL and response with a spreadsheet
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            if (!CmsContext.currentUserIsLoggedIn)
            {
                context.Response.Write("Access Denied");
                return;
            }

            string adminTool = PageUtils.getFromForm("adminTool", "");
            try
            {
                AdminMenuControl.CmsAdminToolEnum tool = (AdminMenuControl.CmsAdminToolEnum)Enum.Parse(typeof(AdminMenuControl.CmsAdminToolEnum), adminTool);
                downloadContent(tool, context);
            }
            catch { }
        }

        /// <summary>
        /// Select appropriate database table and download the data to the client
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="context"></param>
        protected void downloadContent(AdminMenuControl.CmsAdminToolEnum tool, HttpContext context)
        {
            string fileName = tool.ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls";
            GridView gridview1 = new GridView();
            
            switch (tool)
            {
                case AdminMenuControl.CmsAdminToolEnum.ListUserFeedback:
                    gridview1 = new UserFeedbackDb().FetchAllUserFeedbackSubmittedDataAsGrid();
                    break;
                case AdminMenuControl.CmsAdminToolEnum.ListRegisteredProjects:
                    gridview1 = new RegisterProjectDb().fetchAllAsGrid();
                    break;
                default:
                    break;
            }
            OutputDataSetToExcelFile.OutputToResponse(gridview1, fileName, "", "", context.Response);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
