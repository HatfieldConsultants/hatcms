using System;
using System.Reflection;
using System.Text;
using HatCMS.Placeholders;

namespace HatCMS.Controls.Admin
{
    /// <summary>
    /// Super class for the Audit tool 
    /// </summary>
    public abstract class CmsBaseAdminTool
    {
        protected static string EOL = Environment.NewLine;
        protected static string TABLE_START_HTML = "<table border=\"1\" cellspacing=\"0\" cellpadding=\"2\">";
        protected static string TABLE_END_HTML = "</table>";

        /// <summary>
        /// Create a child object for admin tool rendering
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public static CmsBaseAdminTool getAdminTool(AdminMenuControl.CmsAdminToolEnum tool)
        {
            string className = "HatCMS.Controls.Admin." + tool.ToString();
            return (CmsBaseAdminTool)Assembly.GetExecutingAssembly().CreateInstance(className);
        }

        protected string SingleImageHtmlDisplay(SingleImageData img)
        {
            StringBuilder html = new StringBuilder();
            string thumbUrl = showThumbPage.getThumbDisplayUrl(img.ImagePath, 150, 150);
            html.Append("<img src=\"" + thumbUrl + "\">");
            html.Append("<br />" + img.ImagePath + "");
            html.Append("<br />Caption: " + img.Caption + "");
            html.Append("<br />Credits: " + img.Credits + "");

            return html.ToString();
        }


        /// <summary>
        /// Set the color to green for a normal message
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected string formatNormalMsg(string msg)
        {
            StringBuilder sb = new StringBuilder("<p style=\"color: green; font-weight: bold;\">");
            sb.Append(msg);
            sb.Append("</p>");
            sb.Append(EOL);
            return sb.ToString();
        }

        /// <summary>
        /// Set the color to red for an error message
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected string formatErrorMsg(string msg)
        {
            StringBuilder sb = new StringBuilder("<p style=\"color: red; font-weight: bold;\">");
            sb.Append(msg);
            sb.Append("</p>");
            sb.Append(EOL);
            return sb.ToString(); 
        }

        /// <summary>
        /// Method to implement in child class, output html content
        /// </summary>
        /// <returns></returns>
        public abstract string Render();
    }
}
