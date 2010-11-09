using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.UI.WebControls;
using Hatfield.Web.Portal.Data;
using HatCMS.placeholders.RegisterProject;
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
            string adminTool = PageUtils.getFromForm("adminTool", "");
            try
            {
                Audit.AdminTool tool = (Audit.AdminTool)Enum.Parse(typeof(Audit.AdminTool), adminTool);
                downloadContent(tool, context);
            }
            catch { }
        }

        /// <summary>
        /// Select appropriate database table and download the data to the client
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="context"></param>
        protected void downloadContent(Audit.AdminTool tool, HttpContext context)
        {
            string fileName = tool.ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls";
            GridView gridview1 = new GridView();
            
            switch (tool)
            {
                case Audit.AdminTool.ListUserFeedback:
                    gridview1 = new UserFeedbackDb().FetchAllUserFeedbackSubmittedDataAsGrid();
                    break;
                case Audit.AdminTool.ListRegisteredProjects:
                    gridview1 = new RegisterProjectDb().fetchAllAsGrid();
                    break;
                default:
                    break;
            }
            Hatfield.Web.Portal.Data.OutputDataSetToExcelFile.OutputToResponse(gridview1, fileName, "", "", context.Response);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
