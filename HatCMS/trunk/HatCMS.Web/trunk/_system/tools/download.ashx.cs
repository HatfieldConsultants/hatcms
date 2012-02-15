using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.UI.WebControls;
using Hatfield.Web.Portal.Data;
using Hatfield.Web.Portal;
using HatCMS.Admin;
using HatCMS.Placeholders;

namespace HatCMS._system
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://hatcms.net/")]
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

            string adminToolName = PageUtils.getFromForm("adminTool", "");
            if (BaseCmsAdminTool.AdminToolExists(adminToolName))
            try
            {
                BaseCmsAdminTool tool = BaseCmsAdminTool.getAdminToolInstanceByName(adminToolName);
                if (tool != null)                
                    downloadContent(tool, context);
            }
            catch { }
        }

        /// <summary>
        /// Select appropriate database table and download the data to the client
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="context"></param>
        protected void downloadContent(BaseCmsAdminTool tool, HttpContext context)
        {
            string fileName = tool.ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls";
            GridView gridview1 = new GridView();

            try
            {
                gridview1 = tool.RenderToGridViewForOutputToExcelFile();
                if (gridview1 != null)
                    OutputDataSetToExcelFile.OutputToResponse(gridview1, fileName, "", "", context.Response);
            }
            catch           
            {} // fail silently.            
            
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
