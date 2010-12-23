using System;
using System.Reflection;
using System.Text;
using HatCMS.Controls.Admin;

namespace HatCMS.controls.Admin
{
    /// <summary>
    /// Super class for the Audit tool 
    /// </summary>
    public abstract class AdminController
    {
        protected static string EOL = Environment.NewLine;
        protected static string TABLE_START_HTML = "<table border=\"1\" cellspacing=\"0\" cellpadding=\"2\">";
        protected static string TABLE_END_HTML = "</table>";

        /// <summary>
        /// Create a child object for admin tool rendering
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public static AdminController getController(Audit.AdminTool tool) {
            string className = "HatCMS.controls.Admin." + tool.ToString();
            return (AdminController)Assembly.GetExecutingAssembly().CreateInstance(className);
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
