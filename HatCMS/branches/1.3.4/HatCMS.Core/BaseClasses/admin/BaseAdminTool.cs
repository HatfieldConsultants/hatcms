using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HatCMS.Placeholders;
using Hatfield.Web.Portal;

namespace HatCMS.Admin
{
    /// <summary>
    /// Super class for the Audit tool 
    /// </summary>
    public abstract class BaseCmsAdminTool
    {
        public enum CmsAdminToolCategory
        {
            _AdminMenu,
            Report_Image, Report_Page, Report_Feedback, Report_Projects, Report_Other,
            Tool_Search, Tool_Utility, Tool_Security
        }

        public enum AdminMenuTab { _AdminMenu, Reports, Tools }


        public static string getCategoryDisplayTitle(BaseCmsAdminTool.CmsAdminToolCategory cat)
        {
            string title = "";
            switch (cat)
            {
                case BaseCmsAdminTool.CmsAdminToolCategory.Report_Image: title = "Image Reports"; break;
                case BaseCmsAdminTool.CmsAdminToolCategory.Report_Page: title = "Page Reports"; break;
                case BaseCmsAdminTool.CmsAdminToolCategory.Report_Feedback: title = "Feedback Reports"; break;
                case BaseCmsAdminTool.CmsAdminToolCategory.Report_Projects: title = "Registered Project Reports"; break;
                case BaseCmsAdminTool.CmsAdminToolCategory.Report_Other: title = "Other Reports"; break;
                case BaseCmsAdminTool.CmsAdminToolCategory.Tool_Search: title = "Search Tools"; break;
                case BaseCmsAdminTool.CmsAdminToolCategory.Tool_Utility: title = "Utilities"; break;
                case BaseCmsAdminTool.CmsAdminToolCategory.Tool_Security: title = "Security Zones"; break;
                default:
                    throw new ArgumentException("Error: invalid/unknown CmsAdminToolCategory in getCategoryDisplayTitle()");
            }
            return title;
        }

        protected static string EOL = Environment.NewLine;
        protected static string TABLE_START_HTML = "<table border=\"1\" cellspacing=\"0\" cellpadding=\"2\">";
        protected static string TABLE_END_HTML = "</table>";
        

        public static string renderAdminTool(BaseCmsAdminTool tool)
        {            
            return tool.Render();
        }

        public static BaseCmsAdminTool[] GetAllCachedAdminToolInstances()
        {
            string cacheKey = "GetAllCachedAdminToolInstances";
            if (PerRequestCache.CacheContains(cacheKey))
                return (BaseCmsAdminTool[])PerRequestCache.GetFromCache(cacheKey, new BaseCmsAdminTool[0]);

            Type[] allAdminTypes = Hatfield.Web.Portal.AssemblyHelpers.LoadAllAssembliesAndGetAllSubclassesOf(typeof(BaseCmsAdminTool));

            List<BaseCmsAdminTool> ret = new List<BaseCmsAdminTool>();

            foreach (Type t in allAdminTypes)
            {
                BaseCmsAdminTool tool = (BaseCmsAdminTool)t.Assembly.CreateInstance(t.FullName);
                if (tool != null)
                {
                    ret.Add(tool);
                }
            }

            PerRequestCache.AddToCache(cacheKey, ret.ToArray());

            return ret.ToArray();
        }

        public static bool AdminToolExists(string toolName)
        {
            BaseCmsAdminTool[] arr = GetAllCachedAdminToolInstances();
            foreach (BaseCmsAdminTool t in arr)
            {
                if (t.GetType().FullName.EndsWith("." + toolName, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// returns NULL if not found.
        /// </summary>
        /// <param name="toolName"></param>
        /// <returns></returns>
        public static BaseCmsAdminTool getAdminToolInstanceByName(string toolName)
        {
            BaseCmsAdminTool[] arr = GetAllCachedAdminToolInstances();
            foreach (BaseCmsAdminTool t in arr)
            {
                if (t.GetType().FullName.EndsWith("." + toolName, StringComparison.CurrentCultureIgnoreCase))
                    return t;
            }
            return null;
        }

        public static CmsAdminToolInfo[] getAllAdminToolInfos()
        {
            List<CmsAdminToolInfo> ret = new List<CmsAdminToolInfo>();
            BaseCmsAdminTool[] allAdminTools = GetAllCachedAdminToolInstances();
            foreach (BaseCmsAdminTool tool in allAdminTools)
            {
                ret.Add(tool.getToolInfo());
            }
            
            return ret.ToArray();
        }

        public static CmsDependency[] getAllAdminToolDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            BaseCmsAdminTool[] allAdminTools = GetAllCachedAdminToolInstances();
            foreach (BaseCmsAdminTool tool in allAdminTools)
            {
                ret.AddRange(tool.getDependencies());
            }

            return ret.ToArray();
        }

        protected string SingleImageHtmlDisplay(SingleImageData img)
        {
            StringBuilder html = new StringBuilder();
            string thumbUrl = CmsContext.UserInterface.ShowThumbnailPage.getThumbDisplayUrl(img.ImagePath, 150, 150);
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
        public abstract CmsAdminToolInfo getToolInfo();

        /// <summary>
        /// gets all dependencies for the admin tool.
        /// </summary>
        /// <returns></returns>
        public abstract CmsDependency[] getDependencies();

        /// <summary>
        /// returns a grid view of data for output as an Excel file.
        /// To return nothing, this function can return null.
        /// </summary>
        /// <returns></returns>
        public abstract System.Web.UI.WebControls.GridView RenderToGridViewForOutputToExcelFile();
        
    }
}
