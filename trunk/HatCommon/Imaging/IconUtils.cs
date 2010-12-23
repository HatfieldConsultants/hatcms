using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;
using Hatfield.Web.Portal.Collections;

namespace Hatfield.Web.Portal.Imaging
{
    public class IconUtils
    {
        protected static string SMALL_ICON_URL = ConfigurationManager.AppSettings["IconUtils.SmallIconPath"];
        protected static string LARGE_ICON_URL = ConfigurationManager.AppSettings["IconUtils.LargeIconPath"];

        /// <summary>
        /// Get the icon path (there are 2 sets of icon: Large and Small)
        /// </summary>
        /// <param name="appPath"></param>
        /// <param name="largeIcon"></param>
        /// <returns></returns>
        protected static string getPath(string appPath, bool largeIcon)
        {
            return (largeIcon) ? appPath + LARGE_ICON_URL : appPath + SMALL_ICON_URL;
        }

        /// <summary>
        /// Get the icon set supported by the system (read all .gif files from disk folder).
        /// </summary>
        /// <param name="appPath"></param>
        /// <param name="largeIcon"></param>
        /// <returns></returns>
        public static Set getIconSet(string appPath, bool largeIcon)
        {
            string pathOnDisk = HttpContext.Current.Server.MapPath(getPath(appPath, largeIcon));
            DirectoryInfo di = new DirectoryInfo(pathOnDisk);
            FileInfo[] fi = di.GetFiles("*.gif");

            Set iconSet = new Set();
            foreach (FileInfo f in fi)
                iconSet.Add(f.Name);
            return iconSet;
        }

        /// <summary>
        /// Derive the html img tag to display an file icon.  If the file extension
        /// is not supported, the default.icon.gif will be used.
        /// </summary>
        /// <param name="appPath"></param>
        /// <param name="largeIcon"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static string getIconTag(string appPath, bool largeIcon, string ext)
        {
            Set iconSet = getIconSet(appPath, largeIcon);
            if (ext[0] == '.')
                ext = ext.Substring(1);

            string iconFileName = ext + ".gif";
            if (iconSet.Contains(iconFileName) == false)
                iconFileName = "default.icon.gif";

            string size = (largeIcon) ? "32" : "16";
            string imgTag = "<img src=\"{0}\" border=\"0\" width=\"{1}\" height=\"{2}\" />";
            return String.Format(imgTag, new string[] { getPath(appPath, largeIcon) + iconFileName, size, size });
        }

        /// <summary>
        /// Get the CSS statement for calendar display, e.g.
        /// .EventCalender_file_pdf_gif a {
        ///     background-color: black;
        ///     background-image: url('/images/_system/fileIcons/16x16/pdf.gif');
        ///     background-repeat: no-repeat;
        ///     text-indent: 16px;
        /// }
        /// </summary>
        /// <param name="appPath"></param>
        /// <param name="largeIcon"></param>
        /// <param name="iconFile"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        public static string getIconCss(string appPath, bool largeIcon, string iconFile, string controlId)
        {
            string size = (largeIcon) ? "32" : "16";
            string css = ".{0}_{1} a {{ background-color: #6B8E23; background-image: url('{2}images/_system/fileIcons/{3}x{3}/{4}'); background-repeat: no-repeat; text-indent: {3}px; }}";
            string[] parm = new string[] { controlId, iconFile.Replace('.', '_'), appPath, size, iconFile };
            return String.Format(css, parm);
        }
    }
}
