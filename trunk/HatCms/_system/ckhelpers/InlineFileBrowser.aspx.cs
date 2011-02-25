using System;
using System.IO;
using System.Text;
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
using JsonFx.Json;

namespace HatCMS.ckhelpers
{
    public partial class InlineFileBrowser : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CmsContext.currentUserIsLoggedIn)
            {
                Response.Write("Access Denied");
                Response.End();
            }

            StringBuilder css = new StringBuilder();
            css.Append("<style>" + Environment.NewLine);
            css.Append("body, input { font-size: 11px; font-family: tahoma, arial; padding: 0px; margin: 0px; }" + Environment.NewLine);
            css.Append(".error { font-size: 12px; font-weight: bold; color: red; }" + Environment.NewLine);
            css.Append("</style>"+Environment.NewLine);

            Header.Controls.Add(new LiteralControl(css.ToString()));
        }

        private string uploadedFileUrl = "";

        protected void OutputCurrUrl()
        {
            StringBuilder js = new StringBuilder();
            if (uploadedFileUrl != "")
                js.Append("currUrl = '" + uploadedFileUrl + "';");
            Response.Write(js.ToString());
        }

        protected void OutputMaxFileSize()
        {
            long bytes = PageUtils.getMaxUploadFileSizeInBytes(Context);
            string sz = StringUtils.formatFileSize(bytes);
            Response.Write("(max: " + sz + ")");
        }

        protected void Submit1_ServerClick(object sender, EventArgs e)
        {
            uploadedFileUrl = "";
            if (fileUpload.PostedFile != null && fileUpload.PostedFile.ContentLength > 0)
            {
                string uploadPath = PageUtils.getFromForm("uploadPath", "");
                if (!uploadPath.StartsWith(CmsContext.ApplicationPath + "UserFiles/", StringComparison.CurrentCultureIgnoreCase))
                {
                    Response.Write("<span class=\"error\">Invalid directory to upload to...</span>");
                    return;
                }

                string finalUploadPath = Server.MapPath(uploadPath);
                if (!Directory.Exists(finalUploadPath))
                {
                    Response.Write("<span class=\"error\">Invalid directory to upload to...</span>");
                    return;
                }
                string finalFn = Path.Combine(finalUploadPath, Path.GetFileName(fileUpload.PostedFile.FileName));
                if (File.Exists(finalFn))
                {
                    Response.Write("<span class=\"error\">A file with name '"+Path.GetFileName(fileUpload.PostedFile.FileName)+"' already exists in this folder</span>");
                    return;
                }
                try
                {
                    fileUpload.PostedFile.SaveAs(finalFn);
                    uploadedFileUrl = HatCMS._system.ckhelpers.dhtmlxFiles_xml.getUrl(new FileInfo(finalFn), Context);
                }
                catch
                {
                    Response.Write("<span class=\"error\">Error saving file.</span>");
                    uploadedFileUrl = "";
                    return;
                }
            }
            else
            {
                Response.Write("<span class=\"error\">No file uploaded. Please try again.</span>");
            }
        }

        protected void b_CreateFolder_ServerClick(object sender, EventArgs e)
        {
            string uploadPath = PageUtils.getFromForm("uploadPath", "");
            if (!uploadPath.StartsWith(CmsContext.ApplicationPath + "UserFiles/", StringComparison.CurrentCultureIgnoreCase))
            {
                Response.Write("<span class=\"error\">Invalid parent directory. Select a file first!</span>");
                return;
            }
            string folderName = txt_SubFolderName.Value.Trim();
            if (folderName == "")
            {
                Response.Write("<span class=\"error\">Please enter the name of the sub-folder to create</span>");
                return;
            }
            string finalUploadPath = Server.MapPath(uploadPath);
            if (!Directory.Exists(finalUploadPath))
            {
                Response.Write("<span class=\"error\">Invalid directory to upload to...</span>");
                return;
            }
            string finalDir = Path.Combine(finalUploadPath, folderName);
            if (Directory.Exists(finalDir))
            {
                Response.Write("<span class=\"error\">A sub-directory named '" + folderName + "' already exists.</span>");
                return;
            }
            try
            {
                Directory.CreateDirectory(finalDir);
                uploadedFileUrl = HatCMS._system.ckhelpers.dhtmlxFiles_xml.getDirUrl(new DirectoryInfo(finalDir), Context);
            }
            catch (UnauthorizedAccessException uaex)
            {
                Response.Write("<span class=\"error\">Error creating directory (folder permissions problem).</span>");
                uploadedFileUrl = "";
                return;
            }
            catch
            {
                Response.Write("<span class=\"error\">Error creating directory.</span>");
                uploadedFileUrl = "";
                return;
            }
        } // Submit1_ServerClick
    }
}
