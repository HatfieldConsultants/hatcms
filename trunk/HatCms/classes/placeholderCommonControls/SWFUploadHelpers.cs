using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS
{
    /// <summary>
    /// A helper class that assists controls and placeholders to use the SwfUpload in a consistent manner.
    /// </summary>
    public class SWFUploadHelpers
    {
        public static CmsDependency[] SWFUploadDependencies
        {
            get
            {
                List<CmsDependency> ret = new List<CmsDependency>();                
                ret.Add(CmsFileDependency.UnderAppPath("images/_system/ajax-loader_24x24.gif"));
                ret.Add(CmsFileDependency.UnderAppPath("js/_system/swfUpload/swfupload.js"));
                ret.Add(CmsFileDependency.UnderAppPath("js/_system/swfUpload/swfupload.queue.js"));
                ret.Add(CmsFileDependency.UnderAppPath("js/_system/swfUpload/fileprogress.js"));
                ret.Add(CmsFileDependency.UnderAppPath("js/_system/swfUpload/handlers.js"));
                ret.Add(CmsFileDependency.UnderAppPath("js/_system/swfUpload/swfupload.swf"));
                ret.Add(CmsFileDependency.UnderAppPath("_system/tools/swfUpload/SwfUploadTarget.aspx"));
                return ret.ToArray();
            }
        }

        public static void AddPageJavascriptStatements(CmsPage page, string ControlId, string uploadUrl, string allowedFileTypes, string allowedFileTypesDescription)
        {
            page.HeadSection.AddJavascriptFile(JavascriptGroup.Library, "js/_system/swfUpload/swfupload.js");
            page.HeadSection.AddJavascriptFile(JavascriptGroup.Library, "js/_system/swfUpload/swfupload.queue.js");
            page.HeadSection.AddJavascriptFile(JavascriptGroup.Library, "js/_system/swfUpload/fileprogress.js");
            page.HeadSection.AddJavascriptFile(JavascriptGroup.Library, "js/_system/swfUpload/handlers.js");

            string AuthId = "";
            HttpCookie auth_cookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (auth_cookie != null)
            {
                AuthId = auth_cookie.Value;
            }

            if (allowedFileTypes == "")
            {
                allowedFileTypes = "*.jpg";
                allowedFileTypesDescription = "JPG Image Files (*.jpg)";
            }

            string onloadFuncName = ControlId + "SwfUploadLoad";
            if (!page.HeadSection.isBlockRegisteredForOutput(onloadFuncName))
            {
                page.HeadSection.registerBlockForOutput(onloadFuncName);

                StringBuilder js = new StringBuilder();
                js.Append("var swfu;" + Environment.NewLine);
                js.Append("function " + onloadFuncName + "() { " + Environment.NewLine);
                js.Append("var settings = {" + Environment.NewLine);
                js.Append("flash_url : \"" + CmsContext.ApplicationPath + "js/_system/swfUpload/swfupload.swf\"," + Environment.NewLine);
                js.Append("upload_url: \"" + uploadUrl + "\",	// Relative to the SWF file" + Environment.NewLine);
                js.Append("post_params: { \"ASPSESSID\" : \"" + System.Web.HttpContext.Current.Session.SessionID + "\", \"AUTHID\" : \"" + AuthId + "\", \"swfUploadAction\" : \"processUpload\", \"" + ControlId + "_action\": \"postFile\"}," + Environment.NewLine);
                js.Append("file_size_limit : \"" + PageUtils.MaxUploadFileSize + "\"," + Environment.NewLine);
                js.Append("file_types : \"" + allowedFileTypes + "\"," + Environment.NewLine);
                js.Append("file_types_description : \"" + allowedFileTypesDescription + "\"," + Environment.NewLine);
                js.Append("file_upload_limit : 100," + Environment.NewLine);
                js.Append("file_queue_limit : 0," + Environment.NewLine);
                js.Append("custom_settings : {" + Environment.NewLine);
                js.Append("progressTarget : \"fsUploadProgress\"," + Environment.NewLine);
                js.Append("cancelButtonId : \"btnCancel\"" + Environment.NewLine);
                js.Append("}," + Environment.NewLine);
                js.Append("debug: false," + Environment.NewLine);

                js.Append("// Button settings" + Environment.NewLine);
                js.Append("button_image_url: \"" + CmsContext.ApplicationPath + "js/_system/swfUpload/XPButtonUploadText_61x22.png\",	// Relative to the Flash file" + Environment.NewLine);
                js.Append("button_width: \"61\"," + Environment.NewLine);
                js.Append("button_height: \"22\"," + Environment.NewLine);
                js.Append("button_placeholder_id: \"spanButtonPlaceHolder\"," + Environment.NewLine);
                // js.Append("button_text: '<span class=\"theFont\">[ Select & Upload Files... ]</span>'," + Environment.NewLine);
                js.Append("button_text_style: \".theFont { font-size: 16pt; text-decoration: underline; color: #2222CC; }\"," + Environment.NewLine);
                js.Append("button_text_left_padding: 3," + Environment.NewLine);
                js.Append("button_text_top_padding: 3," + Environment.NewLine);



                js.Append("// The event handler functions are defined in handlers.js" + Environment.NewLine);
                js.Append("file_queued_handler : fileQueued," + Environment.NewLine);
                js.Append("file_queue_error_handler : fileQueueError," + Environment.NewLine);
                js.Append("file_dialog_complete_handler : fileDialogComplete," + Environment.NewLine);
                js.Append("upload_start_handler : uploadStart," + Environment.NewLine);
                js.Append("upload_progress_handler : uploadProgress," + Environment.NewLine);
                js.Append("upload_error_handler : uploadError," + Environment.NewLine);
                js.Append("upload_success_handler : uploadSuccess," + Environment.NewLine);
                js.Append("upload_complete_handler : uploadComplete," + Environment.NewLine);
                js.Append("queue_complete_handler : queueComplete	// Queue plugin event" + Environment.NewLine);
                js.Append("};" + Environment.NewLine);

                js.Append("swfu = new SWFUpload(settings);" + Environment.NewLine);
                js.Append("}" + Environment.NewLine);


                page.HeadSection.AddJSStatements(js.ToString());
                page.HeadSection.AddJSOnReady(onloadFuncName + "();");
            }
        }
    }
}
