using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;
using System.IO;

namespace HatCMS.FCKHelpers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SwfUploadTarget : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CmsContext.currentUserCanAuthor)
            {
                Response.StatusCode = 500;
                Response.Write("Authentication required - access denied");
                Response.End();
                return;
            }
            
            try
            {
                HttpPostedFile jpeg_image_upload = Request.Files["Filedata"];

                string dir = "Image/";
                if (Request.QueryString["dir"] != null && Request.QueryString["dir"] != "")
                    dir = Request.QueryString["dir"];

                string baseDir = "~/UserFiles/";
                                
                if (Request.QueryString["DMS"] != null && Request.QueryString["DMS"] == "1")
                {
                    baseDir = CmsConfig.getConfigValue("DMSFileStorageFolderUrl", "");
                    if (!baseDir.EndsWith("/"))
                        baseDir += "/";
                }

                string fullDir = Context.Server.MapPath(baseDir + dir);

                if (!fullDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    fullDir += Path.DirectorySeparatorChar;

                string fullFilename = fullDir + Path.GetFileName(jpeg_image_upload.FileName);

                jpeg_image_upload.SaveAs(fullFilename);

                CmsResource r = CmsResource.CreateFromFile(fullFilename);
                bool b = r.SaveToDatabase();
                if (b)
                {
                    Response.StatusCode = 200;
                    Response.Write(Path.GetFileName(fullFilename));
                    Response.End();
                    return;
                }

            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
            // If any kind of error occurs return a 500 Internal Server error
            Response.StatusCode = 500;
            Response.Write("An error occured");
            Response.End();

        }
    }
}
