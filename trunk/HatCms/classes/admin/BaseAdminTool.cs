using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HatCMS.Placeholders;

namespace HatCMS.Admin
{
    /// <summary>
    /// Super class for the Audit tool 
    /// </summary>
    public abstract class CmsBaseAdminTool
    {
        public enum CmsAdminToolCategory
        {
            Report_Image, Report_Page, Report_Feedback, Report_Projects, Report_Other,
            Tool_Search, Tool_Utility, Tool_Security
        }

        // Notes: the name of the class must match EXACTLY (case-sensitive) the name listed here,
        // and be in the HatCMS.Admin namespace.
        public enum CmsAdminToolClass
        {
            AdminMenu,
            SearchAndReplace,
            SearchHtmlContent,
            ListUserFeedback,
            SearchSingleImagesByCaption,
            LastModifiedTable,
            DuplicateSingleImages,
            SingleImageMissingCaptions,
            PageImageSummary,
            UnusedFiles,
            ValidateConfig,
            PagesByTemplate,
            EmptyThumbnailCache,
            PageUrlsById,
            ListRegisteredProjects,
            ZoneManagement,
            ZoneAuthority
        }

        
        protected static string EOL = Environment.NewLine;
        protected static string TABLE_START_HTML = "<table border=\"1\" cellspacing=\"0\" cellpadding=\"2\">";
        protected static string TABLE_END_HTML = "</table>";

        /// <summary>
        /// Create a child object for admin tool rendering
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        private static CmsBaseAdminTool createAdminToolInstance(CmsBaseAdminTool.CmsAdminToolClass tool)
        {
            string className = "HatCMS.Admin." + tool.ToString();
            return (CmsBaseAdminTool)Assembly.GetExecutingAssembly().CreateInstance(className); 
        }

        public static string renderAdminTool(CmsBaseAdminTool.CmsAdminToolClass tool)
        {
            CmsBaseAdminTool instance = createAdminToolInstance(tool);
            return instance.Render();
        }

        public static CmsAdminToolInfo[] getAllAdminToolInfos()
        {
            List<CmsAdminToolInfo> ret = new List<CmsAdminToolInfo>();
            foreach (CmsBaseAdminTool.CmsAdminToolClass toolClass in Enum.GetValues(typeof(CmsBaseAdminTool.CmsAdminToolClass)))
            {
                if (toolClass != CmsAdminToolClass.AdminMenu)
                {
                    CmsBaseAdminTool tool = createAdminToolInstance(toolClass);
                    ret.Add(tool.GetToolInfo());
                }
            }
            return ret.ToArray();
        }

        public static CmsDependency[] getAllAdminToolDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            foreach (CmsBaseAdminTool.CmsAdminToolClass toolClass in Enum.GetValues(typeof(CmsBaseAdminTool.CmsAdminToolClass)))
            {
                if (toolClass != CmsAdminToolClass.AdminMenu)
                {
                    CmsBaseAdminTool tool = createAdminToolInstance(toolClass);
                    ret.AddRange(tool.getDependencies());
                }
            }
            return ret.ToArray();
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

        /// <summary>
        /// Method to get the tool's information
        /// </summary>
        /// <returns></returns>
        public abstract CmsAdminToolInfo GetToolInfo();

        /// <summary>
        /// gets all dependencies for the admin tool.
        /// </summary>
        /// <returns></returns>
        public abstract CmsDependency[] getDependencies();

    }
}
